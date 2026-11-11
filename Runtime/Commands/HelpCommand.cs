using System;
using System.Collections.Generic;

namespace UniGM
{
    /// <summary>
    /// 内置帮助命令。选项和参数都走统一的解析器。
    /// </summary>
    public sealed class HelpCommand : GMCommand
    {
        public override string Name => "help";

        private const string Usage =
            "help #同时列出参数写法和全部命令。\n" +
            "help -arg #列出全部参数类型。\n" +
            "help -arg [TypeName...] #查询一个到所有的参数类型。\n" +
            "help -gm #列出全部命令。\n" +
            "help -gm [CmdName...] #查询一个到所有的注册命令。";

        public override string Hint => Usage;

        public override void Execute(IReadOnlyDictionary<string, object> args)
        {
            if (args == null || !args.TryGetValue(Name, out object packed) || !(packed is List<object> all))
            {
                PrintUsage();
                return;
            }

            bool hasArg = args.TryGetValue("-arg", out object argValue);
            bool hasGm = args.TryGetValue("-gm", out object gmValue);
            if (!hasArg && !hasGm)
            {
                if (all.Count == 0)
                {
                    GM.Print("参数");
                    PrintArgs(null);
                    GM.Print(string.Empty);
                    GM.Print("命令");
                    PrintCommands(null);
                    return;
                }

                PrintUsage();
                return;
            }

            if (!MatchesShape(args, all))
            {
                PrintUsage();
                return;
            }

            List<string> argNames = null;
            List<string> commandNames = null;
            if (hasArg && !TryReadNames(argValue, out argNames))
            {
                PrintUsage();
                return;
            }

            if (hasGm && !TryReadNames(gmValue, out commandNames))
            {
                PrintUsage();
                return;
            }

            if (hasArg)
                PrintArgs(argValue == null ? null : argNames);
            if (hasGm)
                PrintCommands(gmValue == null ? null : commandNames);
        }

        private static bool TryReadNames(object value, out List<string> names)
        {
            names = null;
            if (value == null)
                return true;

            if (value is string one)
            {
                names = new List<string> { one };
                return true;
            }

            if (!(value is List<object> many))
                return false;

            names = new List<string>(many.Count);
            for (int i = 0; i < many.Count; i++)
            {
                if (!(many[i] is string text))
                    return false;

                names.Add(text);
            }

            return true;
        }

        private bool MatchesShape(IReadOnlyDictionary<string, object> args, List<object> all)
        {
            int expected = 0;
            foreach (KeyValuePair<string, object> pair in args)
            {
                if (pair.Key.Equals(Name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!pair.Key.Equals("-arg", StringComparison.OrdinalIgnoreCase)
                    && !pair.Key.Equals("-gm", StringComparison.OrdinalIgnoreCase))
                    return false;

                expected++;
                if (pair.Value is List<object> many)
                    expected += many.Count;
                else if (pair.Value != null)
                    expected++;
            }

            return all.Count == expected;
        }

        private static void PrintArgs(List<string> typeNames)
        {
            if (typeNames == null)
            {
                List<IArgParser> parsers = GM.CopyParsers();
                if (parsers.Count == 0)
                {
                    GM.Print("没有已注册的参数解析器。");
                    return;
                }

                int width = 0;
                for (int i = 0; i < parsers.Count; i++)
                    width = Math.Max(width, GM.DisplayName(parsers[i].ArgType).Length);

                for (int i = 0; i < parsers.Count; i++)
                {
                    string name = GM.DisplayName(parsers[i].ArgType);
                    GM.Print(name.PadRight(width + 2) + parsers[i].Hint);
                }

                return;
            }

            for (int n = 0; n < typeNames.Count; n++)
                PrintArg(typeNames[n]);
        }

        private static void PrintArg(string typeName)
        {
            List<IArgParser> found = GM.FindParsers(typeName);
            if (found.Count == 0)
            {
                GM.Fail($"未注册类型 \"{typeName}\"。输入 help -arg 查看全部类型。");
                return;
            }

            if (found.Count > 1)
            {
                GM.Fail($"类型名 \"{typeName}\" 匹配到多个解析器，请使用完整类型名。");
                return;
            }

            GM.Print(found[0].Hint);
        }

        private static void PrintCommands(List<string> commandNames)
        {
            if (commandNames == null)
            {
                List<GMCommand> commands = GM.CopyCommands();
                if (commands.Count == 0)
                {
                    GM.Print("没有已注册的命令。");
                    return;
                }

                for (int i = 0; i < commands.Count; i++)
                    PrintHint(commands[i]);

                return;
            }

            for (int i = 0; i < commandNames.Count; i++)
            {
                if (!GM.TryGetCommand(commandNames[i], out GMCommand command))
                {
                    GM.Fail($"未注册命令 \"{commandNames[i]}\"。输入 help -gm 查看全部命令。");
                    continue;
                }

                PrintHint(command);
            }
        }

        private static void PrintHint(GMCommand command)
        {
            string hint = command.Hint ?? string.Empty;
            string[] lines = hint.Split('\n');
            bool any = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                any = true;
                GM.Print(lines[i]);
            }

            if (!any)
                GM.Print(command.Name);
        }

        private static void PrintUsage()
        {
            string[] lines = Usage.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                    GM.Fail(lines[i]);
            }
        }
    }
}
