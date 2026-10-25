using System;
using System.Text.RegularExpressions;

namespace Game.GM
{
    internal sealed class BoolArgParser : ArgParser<bool>
    {
        private static readonly Regex _template = ArgText.CompileIgnoreCase(@"^(?:true|false)$");

        public override Regex Template => _template;

        public override string Hint => "true 或 false，忽略大小写";

        public override bool TryParse(string arg, out object argObj)
        {
            if (arg.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                argObj = true;
                return true;
            }

            if (arg.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                argObj = false;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class CharArgParser : ArgParser<char>
    {
        private static readonly Regex _template = ArgText.Compile(@"^'(?:\\.|[^'\\])'$");

        public override Regex Template => _template;

        public override string Hint => "英文单引号包裹的一个字符，例如 'a'、'\\n'、'\\t'、'\\''";

        public override bool TryParse(string arg, out object argObj)
        {
            if (arg == null || arg.Length < 3 || arg[0] != '\'' || arg[arg.Length - 1] != '\'')
            {
                argObj = null;
                return false;
            }

            string inner = ArgText.Unescape(arg.Substring(1, arg.Length - 2));
            if (inner.Length != 1)
            {
                argObj = null;
                return false;
            }

            argObj = inner[0];
            return true;
        }
    }

    internal sealed class StringArgParser : ArgParser<string>
    {
        private static readonly Regex _template = ArgText.Compile(@"^""(?:\\.|[^""\\])*""$");

        public override Regex Template => _template;

        public override string Hint => "英文双引号字符串，例如 \"hello\"。转义：\\\\、\\\"、\\n、\\t";

        public override bool TryParse(string arg, out object argObj)
        {
            if (arg == null || arg.Length < 2 || arg[0] != '"' || arg[arg.Length - 1] != '"')
            {
                argObj = null;
                return false;
            }

            argObj = ArgText.Unescape(arg.Substring(1, arg.Length - 2));
            return true;
        }
    }
}
