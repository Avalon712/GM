using System;
using System.Collections.Generic;

namespace UniGM
{
    /// <summary>
    /// 内置命令。清空 GM 控制台输出，或清空命令历史。
    /// </summary>
    public sealed class ClearCommand : GMCommand
    {
        public override string Name => "clear";

        public override string Hint =>
            "clear #清空控制台。\n" +
            "clear -console #清空控制台。\n" +
            "clear -history #清空命令历史。";

        public override bool DisplayUsage => true;

        public override void Execute(IReadOnlyDictionary<string, object> args)
        {
            List<object> all = args != null && args.TryGetValue(Name, out object packed) ? packed as List<object> : null;
            if (all == null)
            {
                PrintHint();
                return;
            }

            if (all.Count == 0)
            {
                GM.ClearOutput();
                return;
            }

            bool console = false;
            bool history = false;
            for (int i = 0; i < all.Count; i++)
            {
                if (!(all[i] is GmOption option))
                {
                    PrintHint();
                    return;
                }

                if (!args.TryGetValue(option.Token, out object value) || value != null)
                {
                    PrintHint();
                    return;
                }

                if (option.Token.Equals("-console", StringComparison.OrdinalIgnoreCase))
                    console = true;
                else if (option.Token.Equals("-history", StringComparison.OrdinalIgnoreCase))
                    history = true;
                else
                {
                    PrintHint();
                    return;
                }
            }

            if (console)
                GM.ClearOutput();
            if (history)
                GM.RequestClearHistory();
        }

        private void PrintHint()
        {
            string[] lines = Hint.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                    GM.Print(lines[i]);
            }
        }
    }
}
