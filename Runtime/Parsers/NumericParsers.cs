using System.Globalization;
using System.Text.RegularExpressions;

namespace Game.GM
{
    internal sealed class IntArgParser : ArgParser<int>
    {
        private static readonly Regex _template = ArgText.Compile(@"^[+-]?[0-9]+$");

        public override Regex Template => _template;

        public override string Hint => "十进制整数，例如 42、-3、+8";

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

        public override string Hint => "无符号整数，后缀 u，例如 42u、+8u";

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

        public override string Hint => "长整数，后缀 L，例如 42L、-1L";

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

        public override string Hint => "无符号长整数，后缀 ul 或 lu，例如 42ul、42lu";

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

        public override string Hint => "小数、科学计数或后缀 f，例如 1.5、.5、1e-3、1.5f、1f";

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

        public override string Hint => "双精度小数，后缀 d，例如 1.5d、1d、1e-3d";

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

        public override string Hint => "小数，后缀 m，例如 1.5m、1m";

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
