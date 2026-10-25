using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace UniGM
{
    internal sealed class GmLogEntry
    {
        public int Id;
        public string Message;
        public string Stack;
        public LogType Type;
        public int Count;
        public string Time;
    }

    internal static class GmLogHub
    {
        public const int Capacity = 1000;

        private static readonly ConcurrentQueue<GmLogEntry> _pending = new ConcurrentQueue<GmLogEntry>();
        private static int _nextId;
        private static bool _subscribed;

        internal static void Subscribe()
        {
            if (_subscribed)
                return;

            Application.logMessageReceivedThreaded += OnLog;
            Application.quitting += Unsubscribe;
            _subscribed = true;
        }

        public static void Unsubscribe()
        {
            if (!_subscribed)
                return;

            Application.logMessageReceivedThreaded -= OnLog;
            Application.quitting -= Unsubscribe;
            _subscribed = false;
        }

        public static bool TryDequeue(out GmLogEntry entry)
        {
            return _pending.TryDequeue(out entry);
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            _pending.Enqueue(new GmLogEntry
            {
                Id = System.Threading.Interlocked.Increment(ref _nextId),
                Message = Trim(condition, 4000),
                Stack = Trim(stackTrace, 6000),
                Type = type,
                Count = 1,
                Time = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        private static string Trim(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
                return value ?? string.Empty;

            return value.Substring(0, max) + "…";
        }
    }
}
