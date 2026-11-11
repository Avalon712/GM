using System.Collections.Generic;

namespace UniGM
{
    /// <summary>
    /// 内置命令。清空 GM 控制台已经输出的内容。
    /// </summary>
    public sealed class ClearCommand : GMCommand
    {
        public override string Name => "clear";

        public override string Hint => "eg. clear";

        public override void Execute(IReadOnlyDictionary<string, object> args)
        {
            List<object> all = args != null && args.TryGetValue(Name, out object packed) ? packed as List<object> : null;
            if (all != null && all.Count > 0)
            {
                GM.Print(Hint);
                return;
            }

            GM.ClearOutput();
        }
    }
}
