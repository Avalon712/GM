using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UniGM
{
    internal sealed class DebugConsoleView
    {
        private readonly List<GmLogEntry> _raw = new List<GmLogEntry>(256);
        private readonly List<GmLogEntry> _view = new List<GmLogEntry>(256);
        private readonly Dictionary<string, GmLogEntry> _groups = new Dictionary<string, GmLogEntry>(128);
        private readonly GUIContent _content = new GUIContent();

        private Vector2 _scroll;
        private Vector2 _detailScroll;
        private string _search = string.Empty;
        private string _appliedSearch = string.Empty;
        private string _searchPattern;
        private Regex _searchRegex;
        private bool _info = true;
        private bool _warn = true;
        private bool _error = true;
        private bool _collapse;
        private bool _appliedInfo = true;
        private bool _appliedWarn = true;
        private bool _appliedError = true;
        private bool _appliedCollapse;
        private bool _clearRequested;
        private bool _dirty = true;
        private bool _forceScroll;
        private int _expandedId;
        private int _pendingExpandId;
        private bool _hasExpandRequest;

        public void Draw(Rect rect, GmSkin skin)
        {
            if (Event.current.type == EventType.Layout)
                ApplyChanges();

            float tool = skin.Px(30);
            float gap = skin.Px(6);
            DrawToolbar(new Rect(rect.x, rect.y, rect.width, tool), skin);
            float bodyTop = rect.y + tool + gap;
            float bodyHeight = Mathf.Max(0f, rect.height - tool - gap);
            bool showDetail = FindSelected() != null;
            float detailHeight = 0f;
            float listHeight = bodyHeight;
            if (showDetail)
            {
                detailHeight = Mathf.Clamp(bodyHeight * 0.34f, skin.Px(72), bodyHeight * 0.5f);
                listHeight = Mathf.Max(0f, bodyHeight - detailHeight - gap);
            }

            DrawList(new Rect(rect.x, bodyTop, rect.width, listHeight), skin);
            if (showDetail)
                DrawDetail(new Rect(rect.x, bodyTop + listHeight + gap, rect.width, detailHeight), skin);
        }

        private void ApplyChanges()
        {
            if (_clearRequested)
            {
                _raw.Clear();
                _expandedId = 0;
                _clearRequested = false;
                _dirty = true;
                _forceScroll = true;
            }

            int added = 0;
            while (GmLogHub.TryDequeue(out GmLogEntry entry))
            {
                _raw.Add(entry);
                added++;
            }

            if (_raw.Count > GmLogHub.Capacity)
            {
                _raw.RemoveRange(0, _raw.Count - GmLogHub.Capacity);
                _dirty = true;
            }

            if (added > 0)
            {
                _dirty = true;
                _forceScroll = true;
            }

            if (_search != _appliedSearch
                || _info != _appliedInfo
                || _warn != _appliedWarn
                || _error != _appliedError
                || _collapse != _appliedCollapse)
            {
                _appliedSearch = _search;
                _appliedInfo = _info;
                _appliedWarn = _warn;
                _appliedError = _error;
                _appliedCollapse = _collapse;
                _dirty = true;
            }

            if (_hasExpandRequest)
            {
                if (_expandedId == _pendingExpandId)
                    _expandedId = 0;
                else
                {
                    _expandedId = _pendingExpandId;
                    _detailScroll = Vector2.zero;
                }

                _hasExpandRequest = false;
            }

            if (_dirty)
            {
                Rebuild();
                _dirty = false;
            }

            if (_forceScroll)
            {
                _scroll.y = 1000000f;
                _forceScroll = false;
            }
        }

        private void Rebuild()
        {
            _view.Clear();
            if (!_appliedCollapse)
            {
                for (int i = 0; i < _raw.Count; i++)
                {
                    if (Pass(_raw[i]))
                        _view.Add(_raw[i]);
                }

                return;
            }

            _groups.Clear();
            for (int i = 0; i < _raw.Count; i++)
            {
                GmLogEntry source = _raw[i];
                if (!Pass(source))
                    continue;

                string key = ((int)source.Type) + "\n" + source.Message + "\n" + source.Stack;
                if (_groups.TryGetValue(key, out GmLogEntry existing))
                {
                    existing.Count += source.Count;
                    existing.Time = source.Time;
                    continue;
                }

                GmLogEntry copy = new GmLogEntry
                {
                    Id = source.Id,
                    Message = source.Message,
                    Stack = source.Stack,
                    Type = source.Type,
                    Count = source.Count,
                    Time = source.Time
                };
                _groups.Add(key, copy);
                _view.Add(copy);
            }
        }

        private bool Pass(GmLogEntry entry)
        {
            switch (entry.Type)
            {
                case LogType.Warning:
                    if (!_appliedWarn)
                        return false;
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    if (!_appliedError)
                        return false;
                    break;
                default:
                    if (!_appliedInfo)
                        return false;
                    break;
            }

            if (string.IsNullOrEmpty(_appliedSearch))
                return true;

            return MatchSearch(entry.Message);
        }

        private bool MatchSearch(string message)
        {
            if (_searchPattern != _appliedSearch)
            {
                _searchPattern = _appliedSearch;
                try
                {
                    _searchRegex = new Regex(_appliedSearch, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                catch (ArgumentException)
                {
                    _searchRegex = null;
                }
            }

            if (string.IsNullOrEmpty(message))
                return false;

            if (_searchRegex == null)
                return message.IndexOf(_appliedSearch, StringComparison.OrdinalIgnoreCase) >= 0;

            return _searchRegex.IsMatch(message);
        }

        private void DrawToolbar(Rect rect, GmSkin skin)
        {
            float height = rect.height;
            float gap = skin.Px(2);
            float pad = skin.Px(6);
            float buttonHeight = Mathf.Max(skin.Px(18), height - skin.Px(4));
            float y = rect.y + (height - buttonHeight) * 0.5f;
            float x = rect.x;
            x = FilterButton(skin, x, y, buttonHeight, pad, "Info", _info, skin.Console, ref _info);
            x += gap;
            x = FilterButton(skin, x, y, buttonHeight, pad, "Warning", _warn, skin.Warn, ref _warn);
            x += gap;
            x = FilterButton(skin, x, y, buttonHeight, pad, "Error", _error, skin.Error, ref _error);

            float sideGap = skin.Px(6);
            float side = skin.Px(64);
            Rect clear = new Rect(rect.xMax - side, rect.y, side, height);
            Rect collapse = new Rect(clear.x - sideGap - side, rect.y, side, height);
            float searchWidth = collapse.x - sideGap - (x + sideGap);
            Rect search = new Rect(x + sideGap, rect.y, Mathf.Max(0f, searchWidth), height);

            if (search.width >= skin.Px(36))
            {
                Color previous = GUI.backgroundColor;
                GUI.backgroundColor = skin.Input;
                _search = GUI.TextField(search, _search, skin.InputStyle);
                GUI.backgroundColor = previous;
            }

            if (search.width >= skin.Px(36) && Event.current.type == EventType.Repaint && string.IsNullOrEmpty(_search))
            {
                skin.DrawText(new Rect(search.x + skin.Px(10), search.y, search.width - skin.Px(12), search.height), skin.MutedStyle, "正则搜索");
            }

            if (skin.Button(collapse, "折叠", _collapse, skin.EditorBlue))
                _collapse = !_collapse;
            if (skin.Button(clear, "清空", false, skin.ButtonFace))
                _clearRequested = true;
        }

        private static float FilterButton(GmSkin skin, float x, float y, float height, float pad, string label, bool selected, Color accent, ref bool value)
        {
            float width = skin.Measure(skin.Tab, label).x + pad * 2f;
            if (skin.Button(new Rect(x, y, width, height), label, selected, accent))
                value = !value;
            return x + width;
        }

        private void DrawList(Rect rect, GmSkin skin)
        {
            float width = Mathf.Max(10f, rect.width - skin.Px(14));
            float rowHeight = skin.Px(26);
            float height = _view.Count == 0 ? skin.Px(40) : skin.Px(4) + _view.Count * (rowHeight + skin.Px(1)) + skin.Px(8);
            if (Event.current.type == EventType.Layout && _scroll.y > 900000f)
                _scroll.y = Mathf.Max(0f, height - rect.height);

            _scroll = GUI.BeginScrollView(rect, _scroll, new Rect(0f, 0f, width, Mathf.Max(rect.height, height)), false, false);
            if (_view.Count == 0)
            {
                skin.MutedStyle.alignment = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(skin.Px(8), skin.Px(8), width - skin.Px(16), skin.Px(24)), _raw.Count == 0 ? "没有日志" : "没有符合筛选的日志", skin.MutedStyle);
            }
            else
            {
                float y = skin.Px(2);
                for (int i = 0; i < _view.Count; i++)
                {
                    GmLogEntry entry = _view[i];
                    string first = FirstLine(entry.Message);
                    string time = "[" + entry.Time + "]";
                    _content.text = entry.Count > 1
                        ? time + "    " + first + "    ×" + entry.Count
                        : time + "    " + first;
                    Rect row = new Rect(0f, y, width, rowHeight);
                    Color level = LevelColor(entry.Type, skin);
                    bool selected = entry.Id == _expandedId;
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (selected)
                            skin.DrawSolid(row, skin.Select);
                        else if (i % 2 == 1)
                            skin.DrawSolid(row, skin.Row);
                        skin.DrawSolid(new Rect(row.x, row.y, skin.Px(3), row.height), level);
                    }

                    skin.Log.normal.textColor = level;
                    skin.Log.hover.textColor = level;
                    skin.Log.active.textColor = level;
                    if (GUI.Button(row, _content, skin.Log))
                    {
                        _pendingExpandId = entry.Id;
                        _hasExpandRequest = true;
                    }

                    y += rowHeight + skin.Px(1);
                }
            }

            GUI.EndScrollView();
        }

        private void DrawDetail(Rect rect, GmSkin skin)
        {
            if (Event.current.type == EventType.Repaint)
            {
                skin.DrawSolid(rect, skin.DetailBack);
                skin.DrawSolid(new Rect(rect.x, rect.y, rect.width, 1f), skin.Line);
            }

            GmLogEntry selected = FindSelected();
            float width = Mathf.Max(10f, rect.width - skin.Px(14));
            string stack = selected == null ? null : selected.Stack;
            float stackHeight = 0f;
            if (!string.IsNullOrEmpty(stack))
            {
                _content.text = stack;
                stackHeight = skin.Stack.CalcHeight(_content, width - skin.Px(4));
            }

            string body = selected == null ? "点击一条日志，在这里查看堆栈。" : DetailText(selected);
            GUIStyle style = selected == null ? skin.MutedStyle : skin.Detail;
            if (selected == null)
                skin.MutedStyle.normal.textColor = skin.Muted;
            else
                skin.Detail.normal.textColor = LevelColor(selected.Type, skin);

            _content.text = body;
            float textHeight = style.CalcHeight(_content, width - skin.Px(8));
            float extra = stackHeight > 0f ? stackHeight + skin.Px(8) : 0f;
            float contentHeight = Mathf.Max(rect.height, textHeight + extra + skin.Px(12));
            Rect view = new Rect(rect.x + skin.Px(4), rect.y + skin.Px(4), rect.width - skin.Px(8), Mathf.Max(0f, rect.height - skin.Px(8)));
            _detailScroll = GUI.BeginScrollView(view, _detailScroll, new Rect(0f, 0f, width, contentHeight), false, false);
            GUI.Label(new Rect(0f, skin.Px(4), width - skin.Px(4), textHeight), _content, style);
            if (stackHeight > 0f)
            {
                _content.text = stack;
                GUI.Label(new Rect(0f, textHeight + skin.Px(8), width - skin.Px(4), stackHeight), _content, skin.Stack);
            }

            GUI.EndScrollView();
        }

        private GmLogEntry FindSelected()
        {
            for (int i = 0; i < _view.Count; i++)
            {
                if (_view[i].Id == _expandedId)
                    return _view[i];
            }

            return null;
        }

        private static string DetailText(GmLogEntry entry)
        {
            return LevelName(entry.Type) + "    [" + entry.Time + "]\n" + (entry.Message ?? string.Empty);
        }

        private static string FirstLine(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            int line = message.IndexOf('\n');
            int carriage = message.IndexOf('\r');
            int cut = line < 0 ? carriage : carriage < 0 ? line : Math.Min(line, carriage);
            return cut < 0 ? message : message.Substring(0, cut);
        }

        private static string LevelName(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return "Warning";
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return "Error";
                default:
                    return "Info";
            }
        }

        private static Color LevelColor(LogType type, GmSkin skin)
        {
            switch (type)
            {
                case LogType.Warning:
                    return skin.Warn;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return skin.Error;
                default:
                    return skin.Text;
            }
        }
    }
}
