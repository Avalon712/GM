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

        public override string Hint => "eg. help, help -arg \"Vector3\", help -gm \"cube\"";

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

            if (hasArg && argValue != null && !(argValue is string))
            {
                PrintUsage();
                return;
            }

            if (hasGm && gmValue != null && !(gmValue is string))
            {
                PrintUsage();
                return;
            }

            if (hasArg)
                PrintArgs(argValue as string);
            if (hasGm)
                PrintCommands(gmValue as string);
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

        private static void PrintArgs(string typeName)
        {
            if (typeName == null)
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

        private static void PrintCommands(string commandName)
        {
            if (commandName == null)
            {
                List<GMCommand> commands = GM.CopyCommands();
                if (commands.Count == 0)
                {
                    GM.Print("没有已注册的命令。");
                    return;
                }

                int width = 0;
                for (int i = 0; i < commands.Count; i++)
                    width = Math.Max(width, commands[i].Name.Length);

                for (int i = 0; i < commands.Count; i++)
                    GM.Print(commands[i].Name.PadRight(width + 2) + commands[i].Hint);

                return;
            }

            if (!GM.TryGetCommand(commandName, out GMCommand command))
            {
                GM.Fail($"未注册命令 \"{commandName}\"。输入 help -gm 查看全部命令。");
                return;
            }

            GM.Print(command.Hint);
        }

        private static void PrintUsage()
        {
            GM.Fail("用法:");
            GM.Fail("  help");
            GM.Fail("  help -arg [\"TypeName\"]");
            GM.Fail("  help -gm [\"CommandName\"]");
        }
    }
}
