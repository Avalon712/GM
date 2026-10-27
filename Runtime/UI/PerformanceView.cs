using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniGM
{
    /// <summary>
    /// 运行时性能页。每秒刷新平均帧率，并用最近一段帧时间计算 1% Low、0.1% Low。内存每 10 秒刷新。
    /// </summary>
    internal sealed class PerformanceView
    {
        private const int Capacity = 1000;
        private const float MemoryRefreshSeconds = 10f;

        private readonly float[] _times = new float[Capacity];
        private readonly float[] _sort = new float[Capacity];

        private int _count;
        private int _cursor;
        private int _lastFrame = -1;
        private int _secondFrames;
        private float _secondElapsed;
        private float _memoryElapsed;
        private bool _ready;

        private float _avgFps;
        private float _avgFrameMs;
        private float _low1Fps;
        private float _low01Fps;
        private float _low1Ms;
        private float _low01Ms;

        private long _allocated = -1;
        private long _reserved = -1;
        private long _monoUsed = -1;
        private long _monoHeap = -1;
        private long _graphics = -1;
        private long _managed = -1;

        private Vector2 _scroll;
        private readonly GUIContent _content = new GUIContent();

        public void Draw(Rect rect, GmSkin skin)
        {
            float row = skin.Px(26);
            float width = Mathf.Max(10f, rect.width - skin.Px(14));
            float height = row * 16f + skin.Px(28);
            _scroll = GUI.BeginScrollView(rect, _scroll, new Rect(0f, 0f, width, Mathf.Max(rect.height, height)), false, false);

            float y = skin.Px(4);
            y = Section(skin, y, width, "帧率");
            y = Metric(skin, y, width, row, "平均帧率", _ready ? FormatFps(_avgFps) : "采样中", FpsColor(_avgFps, skin));
            y = Metric(skin, y, width, row, "平均帧时间", _ready ? FormatMs(_avgFrameMs) : "—", skin.Text);
            y = Metric(skin, y, width, row, "1% Low", _ready ? FormatFps(_low1Fps) : "—", FpsColor(_low1Fps, skin));
            y = Metric(skin, y, width, row, "1% Low 帧时间", _ready ? FormatMs(_low1Ms) : "—", skin.Text);
            y = Metric(skin, y, width, row, "0.1% Low", _ready ? FormatFps(_low01Fps) : "—", FpsColor(_low01Fps, skin));
            y = Metric(skin, y, width, row, "0.1% Low 帧时间", _ready ? FormatMs(_low01Ms) : "—", skin.Text);
            y += skin.Px(8);
            y = Section(skin, y, width, "内存");
            y = Metric(skin, y, width, row, "总分配", FormatBytes(_allocated), skin.Text);
            y = Metric(skin, y, width, row, "总保留", FormatBytes(_reserved), skin.Text);
            y = Metric(skin, y, width, row, "Mono 已用", FormatBytes(_monoUsed), skin.Text);
            y = Metric(skin, y, width, row, "Mono 堆", FormatBytes(_monoHeap), skin.Text);
            y = Metric(skin, y, width, row, "图形驱动", FormatBytes(_graphics), skin.Text);
            y = Metric(skin, y, width, row, "托管内存", FormatBytes(_managed), skin.Text);
            y += skin.Px(10);
            skin.MutedStyle.normal.textColor = skin.Muted;
            GUI.Label(new Rect(skin.Px(4), y, width - skin.Px(8), row * 2f), "平均帧率按最近 1 秒统计。1% Low 和 0.1% Low 取最近 1000 帧里最慢的帧。内存每 10 秒刷新。", skin.MutedStyle);

            GUI.EndScrollView();
        }

        internal void Sample()
        {
            if (_lastFrame == Time.frameCount)
                return;

            _lastFrame = Time.frameCount;
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;

            _times[_cursor] = dt;
            _cursor = (_cursor + 1) % Capacity;
            if (_count < Capacity)
                _count++;

            _secondFrames++;
            _secondElapsed += dt;
            _memoryElapsed += dt;
            if (_allocated < 0 || _memoryElapsed >= MemoryRefreshSeconds)
            {
                ReadMemory();
                _memoryElapsed = 0f;
            }

            if (_secondElapsed < 1f)
                return;

            _avgFps = _secondFrames / _secondElapsed;
            _avgFrameMs = _secondElapsed / _secondFrames * 1000f;
            ComputeLows();
            _ready = true;
            _secondFrames = 0;
            _secondElapsed = 0f;
        }

        private void ComputeLows()
        {
            if (_count == 0)
                return;

            Array.Copy(_times, _sort, _count);
            Array.Sort(_sort, 0, _count);
            _low1Ms = AverageSlowest(_count, Mathf.Max(1, Mathf.CeilToInt(_count * 0.01f)));
            _low01Ms = AverageSlowest(_count, Mathf.Max(1, Mathf.CeilToInt(_count * 0.001f)));
            _low1Fps = _low1Ms > 0.001f ? 1000f / _low1Ms : 0f;
            _low01Fps = _low01Ms > 0.001f ? 1000f / _low01Ms : 0f;
        }

        private float AverageSlowest(int count, int take)
        {
            double sum = 0d;
            int start = count - take;
            for (int i = start; i < count; i++)
                sum += _sort[i];

            return (float)(sum / take * 1000d);
        }

        private void ReadMemory()
        {
            _allocated = Profiler.GetTotalAllocatedMemoryLong();
            _reserved = Profiler.GetTotalReservedMemoryLong();
            _monoUsed = Profiler.GetMonoUsedSizeLong();
            _monoHeap = Profiler.GetMonoHeapSizeLong();
            _graphics = Profiler.GetAllocatedMemoryForGraphicsDriver();
            _managed = GC.GetTotalMemory(false);
        }

        private float Section(GmSkin skin, float y, float width, string title)
        {
            skin.Tab.alignment = TextAnchor.MiddleLeft;
            skin.Tab.normal.textColor = skin.Stats;
            GUI.Label(new Rect(skin.Px(4), y, width - skin.Px(8), skin.Px(24)), title, skin.Tab);
            skin.Tab.alignment = TextAnchor.MiddleCenter;
            skin.Tab.normal.textColor = skin.Text;
            return y + skin.Px(26);
        }

        private float Metric(GmSkin skin, float y, float width, float row, string label, string value, Color valueColor)
        {
            if (Event.current.type == EventType.Repaint && ((int)(y / row) % 2 == 0))
                skin.DrawSolid(new Rect(0f, y, width, row), skin.Row);

            skin.MutedStyle.normal.textColor = skin.Muted;
            GUI.Label(new Rect(skin.Px(8), y, width * 0.48f, row), label, skin.MutedStyle);
            skin.Log.normal.textColor = valueColor;
            skin.Log.alignment = TextAnchor.MiddleRight;
            _content.text = value;
            GUI.Label(new Rect(width * 0.48f, y, width * 0.48f, row), _content, skin.Log);
            skin.Log.alignment = TextAnchor.MiddleLeft;
            skin.Log.normal.textColor = skin.Text;
            return y + row;
        }

        private Color FpsColor(float fps, GmSkin skin)
        {
            if (!_ready)
                return skin.Muted;
            if (fps >= 55f)
                return skin.Stats;
            if (fps >= 30f)
                return skin.Warn;
            return skin.Error;
        }

        private static string FormatFps(float fps)
        {
            return fps.ToString("0.0") + " fps";
        }

        private static string FormatMs(float ms)
        {
            return ms.ToString("0.00") + " ms";
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 0)
                return "—";

            double mb = bytes / (1024d * 1024d);
            if (mb >= 1024d)
                return (mb / 1024d).ToString("0.00") + " GB";

            return mb.ToString("0.0") + " MB";
        }
    }
}
