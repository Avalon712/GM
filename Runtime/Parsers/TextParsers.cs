using System;
using System.Text.RegularExpressions;

namespace UniGM
{
    internal sealed class BoolArgParser : ArgParser<bool>
    {
        private static readonly Regex _template = ArgText.CompileIgnoreCase(@"^(?:true|false)$");

        public override Regex Template => _template;

        public override string Hint => "eg. true, false";

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

        public override string Hint => "eg. 'a'";

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

        public override string Hint => "eg. \"hello\"";

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
