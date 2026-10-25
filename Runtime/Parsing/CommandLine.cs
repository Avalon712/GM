using System.Collections.Generic;

namespace UniGM
{
    /// <summary>
    /// 把一行命令拆成命令名和参数文本。引号和括号内部的空格不会被切开。
    /// </summary>
    internal static class CommandLine
    {
        public static bool TrySplit(string line, out string commandName, out List<string> args, out string error)
        {
            commandName = null;
            args = null;
            error = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            int index = 0;
            SkipWhite(line, ref index);
            int nameStart = index;
            while (index < line.Length && !char.IsWhiteSpace(line[index]))
                index++;

            if (index == nameStart)
                return false;

            commandName = line.Substring(nameStart, index - nameStart);
            args = new List<string>();

            while (index < line.Length)
            {
                SkipWhite(line, ref index);
                if (index >= line.Length)
                    break;

                if (!TryReadToken(line, ref index, out string token, out error))
                {
                    args = null;
                    return false;
                }

                args.Add(token);
            }

            return true;
        }

        private static void SkipWhite(string line, ref int index)
        {
            while (index < line.Length && char.IsWhiteSpace(line[index]))
                index++;
        }

        private static bool TryReadToken(string line, ref int index, out string token, out string error)
        {
            error = null;
            if (line[index] == '"')
                return TryReadQuoted(line, ref index, out token, out error);

            int start = index;
            int depth = 0;
            while (index < line.Length)
            {
                char current = line[index];
                if (depth == 0 && char.IsWhiteSpace(current))
                    break;

                if (current == '(')
                {
                    depth++;
                }
                else if (current == ')')
                {
                    depth--;
                    if (depth < 0)
                    {
                        token = null;
                        error = "括号不匹配。";
                        return false;
                    }
                }

                index++;
            }

            if (depth != 0)
            {
                token = null;
                error = "括号未闭合。";
                return false;
            }

            token = line.Substring(start, index - start);
            if (token.Length == 0)
            {
                error = "存在空参数。";
                return false;
            }

            return true;
        }

        private static bool TryReadQuoted(string line, ref int index, out string token, out string error)
        {
            int start = index;
            index++;
            while (index < line.Length)
            {
                char current = line[index];
                if (current == '\\')
                {
                    index++;
                    if (index >= line.Length)
                        break;
                    index++;
                    continue;
                }

                index++;
                if (current == '"')
                {
                    token = line.Substring(start, index - start);
                    error = null;
                    return true;
                }
            }

            token = null;
            error = "字符串缺少结束引号。";
            return false;
        }
    }
}
