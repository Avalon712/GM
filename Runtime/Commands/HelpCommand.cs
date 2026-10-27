using System;
using System.Collections.Generic;

namespace UniGM
{
    /// <summary>
    /// 内置帮助命令。参数是关键字，不经过类型解析器。
    /// </summary>
    public sealed class HelpCommand : GMCommand
    {
        public override string Name => "help";

        public override string Hint => "eg. help, help arg Vector3, help gm cube";

        internal const string UsageText =
            "eg. help\n" +
            "eg. help arg Vector3\n" +
            "eg. help gm cube";

        public override void Execute(IReadOnlyList<object> args)
        {
            if (args == null || args.Count == 0)
            {
                GM.Print("参数");
                PrintArgs(null);
                GM.Print(string.Empty);
                GM.Print("命令");
                PrintCommands(null);
                return;
            }

            string topic = args[0] as string ?? string.Empty;
            if (topic.Equals("arg", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Count == 1)
                    PrintArgs(null);
                else if (args.Count == 2)
                    PrintArgs(args[1] as string ?? string.Empty);
                else
                    PrintUsage();
                return;
            }

            if (topic.Equals("gm", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Count == 1)
                    PrintCommands(null);
                else if (args.Count == 2)
                    PrintCommands(args[1] as string ?? string.Empty);
                else
                    PrintUsage();
                return;
            }

            PrintUsage();
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
                GM.Fail($"未注册类型 \"{typeName}\"。输入 help arg 查看全部类型。");
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
                GM.Fail($"未注册命令 \"{commandName}\"。输入 help gm 查看全部命令。");
                return;
            }

            GM.Print(command.Hint);
        }

        private static void PrintUsage()
        {
            GM.Fail("用法:");
            GM.Fail("  help");
            GM.Fail("  help arg [TypeName]");
            GM.Fail("  help gm [CommandName]");
        }
    }
}
