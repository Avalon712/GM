using System;
using System.Collections.Generic;

namespace UniGM
{
    internal enum GmLineKind
    {
        Echo,
        Info,
        Error
    }

    internal sealed class GmLine
    {
        public int Id;
        public string Text;
        public GmLineKind Kind;
        public string Time;
    }

    internal static class GmOutput
    {
        public const int Capacity = 1000;

        private static readonly object _gate = new object();
        private static readonly List<GmLine> _lines = new List<GmLine>(128);
        private static int _version;
        private static int _nextId = 1;

        public static int Version
        {
            get
            {
                lock (_gate)
                    return _version;
            }
        }

        public static int Add(string text, GmLineKind kind)
        {
            text = Trim(text);
            var line = new GmLine
            {
                Text = text,
                Kind = kind,
                Time = DateTime.Now.ToString("HH:mm:ss")
            };

            lock (_gate)
            {
                line.Id = _nextId++;
                _lines.Add(line);
                if (_lines.Count > Capacity)
                    _lines.RemoveRange(0, _lines.Count - Capacity);
                _version++;
                return line.Id;
            }
        }

        public static int Set(int line, string text, GmLineKind kind)
        {
            text = Trim(text);
            lock (_gate)
            {
                for (int i = 0; i < _lines.Count; i++)
                {
                    if (_lines[i].Id != line)
                        continue;

                    _lines[i].Text = text;
                    _lines[i].Kind = kind;
                    _version++;
                    return line;
                }
            }

            return Add(text, kind);
        }

        private static string Trim(string text)
        {
            if (text != null && text.Length > 8000)
                return text.Substring(0, 8000) + "…";

            return text ?? string.Empty;
        }

        public static void Clear()
        {
            lock (_gate)
            {
                if (_lines.Count == 0)
                    return;
                _lines.Clear();
                _version++;
            }
        }

        public static void CopyTo(List<GmLine> destination)
        {
            if (destination == null)
                return;

            lock (_gate)
            {
                destination.Clear();
                destination.AddRange(_lines);
            }
        }
    }
}
