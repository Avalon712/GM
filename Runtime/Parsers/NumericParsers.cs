using System.Globalization;
using System.Text.RegularExpressions;

namespace UniGM
{
    internal sealed class IntArgParser : ArgParser<int>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+-]?[0-9]+$");

        public override Regex Template => _template;

        public override string Hint => "eg. 42, -3";

        public override bool TryParse(string arg, out object argObj)
        {
            if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class UIntArgParser : ArgParser<uint>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+]?[0-9]+[uU]$");

        public override Regex Template => _template;

        public override string Hint => "eg. 42u";

        public override bool TryParse(string arg, out object argObj)
        {
            string body = arg.Substring(0, arg.Length - 1);
            if (uint.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class LongArgParser : ArgParser<long>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+-]?[0-9]+[lL]$");

        public override Regex Template => _template;

        public override string Hint => "eg. 42L";

        public override bool TryParse(string arg, out object argObj)
        {
            string body = arg.Substring(0, arg.Length - 1);
            if (long.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class ULongArgParser : ArgParser<ulong>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+]?[0-9]+(?:[uU][lL]|[lL][uU])$");

        public override Regex Template => _template;

        public override string Hint => "eg. 42ul, 42lu";

        public override bool TryParse(string arg, out object argObj)
        {
            string body = arg.Substring(0, arg.Length - 2);
            if (ulong.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class FloatArgParser : ArgParser<float>
    {
        private static readonly Regex _template = ArgText.Compile(
            @"^(?:[+-]?(?:[0-9]+\.[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]+)?[fF]?|[+-]?[0-9]+[eE][+-]?[0-9]+[fF]?|[+-]?(?:[0-9]+\.?[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]+)?[fF])$");

        public override Regex Template => _template;

        public override string Hint => "eg. 1.5, 1.5f";

        public override bool TryParse(string arg, out object argObj)
        {
            string body = ArgText.StripNumericSuffix(arg, out _);
            if (float.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class DoubleArgParser : ArgParser<double>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+-]?(?:[0-9]+\.?[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]+)?[dD]$");

        public override Regex Template => _template;

        public override string Hint => "eg. 1.5d";

        public override bool TryParse(string arg, out object argObj)
        {
            string body = ArgText.StripNumericSuffix(arg, out _);
            if (double.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class DecimalArgParser : ArgParser<decimal>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+-]?(?:[0-9]+\.?[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]+)?[mM]$");

        public override Regex Template => _template;

        public override string Hint => "eg. 1.5m";

        public override bool TryParse(string arg, out object argObj)
        {
            string body = ArgText.StripNumericSuffix(arg, out _);
            if (decimal.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value))
            {
                argObj = value;
                return true;
            }

            argObj = null;
            return false;
        }
    }
}
