using System;
using System.Collections.Generic;

namespace Game.GM
{
    internal enum GmLineKind
    {
        Echo,
        Info,
        Error
    }

    internal sealed class GmLine
    {
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

        public static int Version
        {
            get
            {
                lock (_gate)
                    return _version;
            }
        }

        public static void Add(string text, GmLineKind kind)
        {
            if (text != null && text.Length > 8000)
                text = text.Substring(0, 8000) + "…";

            GmLine line = new GmLine
            {
                Text = text ?? string.Empty,
                Kind = kind,
                Time = DateTime.Now.ToString("HH:mm:ss")
            };

            lock (_gate)
            {
                _lines.Add(line);
                if (_lines.Count > Capacity)
                    _lines.RemoveRange(0, _lines.Count - Capacity);
                _version++;
            }
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
