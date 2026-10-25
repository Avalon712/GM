using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Game.GM
{
    internal static class ArgText
    {
        public static readonly RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;

        public static readonly RegexOptions IgnoreCaseOptions = Options | RegexOptions.IgnoreCase;

        public const string FloatComponent = @"[+-]?(?:[0-9]+\.?[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]+)?[fFdDmM]?";

        public const string IntComponent = @"[+-]?[0-9]+";

        public static Regex Compile(string pattern)
        {
            return new Regex(pattern, Options);
        }

        public static Regex CompileIgnoreCase(string pattern)
        {
            return new Regex(pattern, IgnoreCaseOptions);
        }

        public static Regex Tuple(string prefix, string component, int count)
        {
            return Tuple(prefix, component, count, count);
        }

        public static Regex Tuple(string prefix, string component, int minCount, int maxCount)
        {
            int minExtra = minCount - 1;
            int maxExtra = maxCount - 1;
            string repeat = minExtra == maxExtra
                ? "{" + minExtra + "}"
                : "{" + minExtra + "," + maxExtra + "}";
            string pattern = "^" + prefix + "\\(\\s*" + component + "(?:\\s*,\\s*" + component + ")" + repeat + "\\s*\\)$";
            return CompileIgnoreCase(pattern);
        }

        public static bool TrySplitTuple(string arg, out string[] parts)
        {
            parts = null;
            if (string.IsNullOrEmpty(arg))
                return false;

            int open = arg.IndexOf('(');
            int close = arg.LastIndexOf(')');
            if (open < 0 || close <= open)
                return false;

            string inner = arg.Substring(open + 1, close - open - 1);
            parts = inner.Split(',');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();
            return true;
        }

        public static bool TryParseFloat(string text, out float value)
        {
            text = StripNumericSuffix(text, out char suffix);
            if (suffix != '\0' && suffix != 'f' && suffix != 'F' && suffix != 'd' && suffix != 'D' && suffix != 'm' && suffix != 'M')
            {
                value = 0f;
                return false;
            }

            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParseInt(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static string StripNumericSuffix(string text, out char suffix)
        {
            suffix = '\0';
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Trim();
            if (text.Length == 0)
                return text;

            char last = text[text.Length - 1];
            if (last == 'f' || last == 'F' || last == 'd' || last == 'D' || last == 'm' || last == 'M')
            {
                suffix = last;
                return text.Substring(0, text.Length - 1);
            }

            return text;
        }

        public static string Unescape(string inner)
        {
            if (string.IsNullOrEmpty(inner))
                return string.Empty;

            StringBuilder builder = new StringBuilder(inner.Length);
            for (int i = 0; i < inner.Length; i++)
            {
                char current = inner[i];
                if (current != '\\' || i + 1 >= inner.Length)
                {
                    builder.Append(current);
                    continue;
                }

                i++;
                switch (inner[i])
                {
                    case '\\':
                        builder.Append('\\');
                        break;
                    case '"':
                        builder.Append('"');
                        break;
                    case '\'':
                        builder.Append('\'');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    default:
                        builder.Append(inner[i]);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
