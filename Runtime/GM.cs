using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UniGM
{
    /// <summary>
    /// GM 命令和参数解析器的注册入口。命令输出走 <see cref="Print"/>，只出现在 GM 控制台。
    /// </summary>
    public static class GM
    {
        private static readonly Dictionary<string, GMCommand> _commands =
            new Dictionary<string, GMCommand>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<Type, IArgParser> _parsers = new Dictionary<Type, IArgParser>();

        private static readonly Dictionary<Type, string> _keywords = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" }
        };

        /// <summary>
        /// 命令输入框正在接收键盘输入。游戏逻辑可据此忽略自己的快捷键。
        /// </summary>
        public static bool IsTyping { get; private set; }

        /// <summary>
        /// 创建 GM 窗口并开始接收 Debug 日志。由游戏启动流程调用。
        /// <paramref name="input"/> 提供按键和鼠标状态，重复调用会换成新的实现。
        /// </summary>
        public static void Initialize(IGmInput input)
        {
            GmInput.Bind(input);
            GmLogHub.Subscribe();
            GMManager.Ensure();
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// 旧输入系统可用时，用内置的 F1、鼠标左键、回车和小键盘回车打开 GM。
        /// </summary>
        public static void Initialize()
        {
            Initialize(new LegacyGmInput());
        }
#endif

        static GM()
        {
            RegisterBuiltins();
        }

        /// <summary>
        /// 注册命令。同名命令已存在时失败，并在 GM 控制台提示。
        /// </summary>
        public static bool Register(GMCommand command)
        {
            if (command == null)
            {
                Fail("不能注册空命令。");
                return false;
            }

            if (string.IsNullOrWhiteSpace(command.Name) || command.Name.IndexOf(' ') >= 0 || command.Name.IndexOf('\t') >= 0)
            {
                Fail("命令名不能为空，也不能包含空白。");
                return false;
            }

            if (_commands.ContainsKey(command.Name))
            {
                Fail($"命令 \"{command.Name}\" 已存在。请先 Unregister 再注册。");
                return false;
            }

            _commands.Add(command.Name, command);
            return true;
        }

        /// <summary>
        /// 按名称注销命令，忽略大小写。
        /// </summary>
        public static bool Unregister(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return _commands.Remove(name);
        }

        /// <summary>
        /// 注册参数解析器。同一类型再次注册会覆盖旧解析器。
        /// </summary>
        public static void Register<T>(ArgParser<T> parser)
        {
            if (parser == null)
            {
                Fail("不能注册空的参数解析器。");
                return;
            }

            _parsers[typeof(T)] = parser;
        }

        /// <summary>
        /// 注销某个类型的参数解析器。
        /// </summary>
        public static bool Unregister<T>()
        {
            return _parsers.Remove(typeof(T));
        }

        /// <summary>
        /// 执行一行命令。空行忽略。解析错误和异常写入 GM 控制台。
        /// </summary>
        public static bool Execute(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return false;

            Echo("> " + commandLine.Trim());

            try
            {
                if (!CommandLine.TrySplit(commandLine, out string commandName, out List<string> tokens, out string error))
                {
                    if (!string.IsNullOrEmpty(error))
                        Fail(error);
                    return false;
                }

                if (!_commands.TryGetValue(commandName, out GMCommand command))
                {
                    Fail($"未找到命令 \"{commandName}\"。输入 help gm 查看全部命令。");
                    return false;
                }

                List<object> values = new List<object>(tokens.Count);
                if (command is HelpCommand)
                {
                    for (int i = 0; i < tokens.Count; i++)
                        values.Add(tokens[i]);
                }
                else if (!TryBind(tokens, values, out error))
                {
                    Fail(error);
                    return false;
                }

                command.SetArgs(values);
                command.Execute(values);
                return true;
            }
            catch (Exception exception)
            {
                Fail("执行命令时出现异常。\n" + exception);
                return false;
            }
        }

        /// <summary>
        /// 向 GM 控制台写一行普通文本，不会进入 Debug 控制台。
        /// </summary>
        public static void Print(string message)
        {
            GmOutput.Add(message ?? string.Empty, GmLineKind.Info);
        }

        internal static void Echo(string message)
        {
            GmOutput.Add(message ?? string.Empty, GmLineKind.Echo);
        }

        internal static void Fail(string message)
        {
            GmOutput.Add(message ?? string.Empty, GmLineKind.Error);
        }

        internal static void ClearOutput()
        {
            GmOutput.Clear();
        }

        internal static int OutputVersion => GmOutput.Version;

        internal static void CopyOutput(List<GmLine> destination)
        {
            GmOutput.CopyTo(destination);
        }

        internal static void SetTyping(bool typing)
        {
            IsTyping = typing;
        }

        internal static string DisplayName(Type type)
        {
            if (type == null)
                return "?";

            if (_keywords.TryGetValue(type, out string keyword))
                return keyword;

            return type.Name;
        }

        internal static List<GMCommand> CopyCommands()
        {
            List<GMCommand> list = new List<GMCommand>(_commands.Values);
            list.Sort(CompareCommand);
            return list;
        }

        internal static List<IArgParser> CopyParsers()
        {
            List<IArgParser> list = new List<IArgParser>(_parsers.Values);
            list.Sort(CompareParser);
            return list;
        }

        internal static bool TryGetCommand(string name, out GMCommand command)
        {
            if (string.IsNullOrEmpty(name))
            {
                command = null;
                return false;
            }

            return _commands.TryGetValue(name, out command);
        }

        internal static List<IArgParser> FindParsers(string typeName)
        {
            List<IArgParser> found = new List<IArgParser>();
            if (string.IsNullOrEmpty(typeName))
                return found;

            foreach (KeyValuePair<Type, IArgParser> pair in _parsers)
            {
                if (TypeNameEquals(pair.Key, typeName))
                    found.Add(pair.Value);
            }

            return found;
        }

        private static void RegisterBuiltins()
        {
            Register(new IntArgParser());
            Register(new UIntArgParser());
            Register(new LongArgParser());
            Register(new ULongArgParser());
            Register(new FloatArgParser());
            Register(new DoubleArgParser());
            Register(new DecimalArgParser());
            Register(new BoolArgParser());
            Register(new CharArgParser());
            Register(new StringArgParser());
            Register(new Vector2ArgParser());
            Register(new Vector3ArgParser());
            Register(new Vector4ArgParser());
            Register(new Vector2IntArgParser());
            Register(new Vector3IntArgParser());
            Register(new QuaternionArgParser());
            Register(new ColorArgParser());
            Register(new RectArgParser());
            Register(new HelpCommand());
        }

        private static bool TryBind(List<string> tokens, List<object> values, out string error)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];
                List<IArgParser> matchedParsers = new List<IArgParser>(2);
                foreach (IArgParser parser in _parsers.Values)
                {
                    Regex template = parser.Template;
                    if (template != null && template.IsMatch(token))
                        matchedParsers.Add(parser);
                }

                if (matchedParsers.Count == 0)
                {
                    error = $"无法解析参数 \"{token}\"。输入 help arg 查看写法。";
                    return false;
                }

                if (matchedParsers.Count > 1)
                {
                    error = $"参数 \"{token}\" 同时匹配 {JoinNames(matchedParsers)}。";
                    return false;
                }

                IArgParser matched = matchedParsers[0];
                if (!matched.TryParse(token, out object value))
                {
                    error = $"参数 \"{token}\" 不能按 {DisplayName(matched.ArgType)} 解析。{matched.Hint}";
                    return false;
                }

                values.Add(value);
            }

            error = null;
            return true;
        }

        private static string JoinNames(List<IArgParser> parsers)
        {
            string names = DisplayName(parsers[0].ArgType);
            for (int i = 1; i < parsers.Count; i++)
                names += "、" + DisplayName(parsers[i].ArgType);
            return names;
        }

        private static bool TypeNameEquals(Type type, string typeName)
        {
            if (type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                return true;

            if (type.FullName != null && type.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                return true;

            return _keywords.TryGetValue(type, out string keyword)
                   && keyword.Equals(typeName, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareCommand(GMCommand left, GMCommand right)
        {
            return string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareParser(IArgParser left, IArgParser right)
        {
            return string.Compare(DisplayName(left.ArgType), DisplayName(right.ArgType), StringComparison.OrdinalIgnoreCase);
        }
    }
}
