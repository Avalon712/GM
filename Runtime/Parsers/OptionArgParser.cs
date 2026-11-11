using System.Text.RegularExpressions;

namespace UniGM
{
    internal sealed class OptionArgParser : ArgParser<GmOption>
    {
        private static readonly Regex _template = ArgText.Compile(@"^--?[A-Za-z][A-Za-z0-9_-]*$");

        public override Regex Template => _template;

        public override string Hint => "eg. -gm, --file";

        public override bool TryParse(string arg, out object argObj)
        {
            if (arg == null || !_template.IsMatch(arg))
            {
                argObj = null;
                return false;
            }

            argObj = new GmOption(arg);
            return true;
        }
    }
}
