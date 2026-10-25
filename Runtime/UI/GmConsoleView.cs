using System.Collections.Generic;
using UnityEngine;

namespace Game.GM
{
    internal sealed class GmConsoleView
    {
        public const string InputControl = "GM.CommandInput";

        private readonly List<GmLine> _lines = new List<GmLine>(128);
        private readonly List<string> _history = new List<string>(32);
        private readonly GUIContent _content = new GUIContent();

        private Vector2 _scroll;
        private string _input = string.Empty;
        private string _draft = string.Empty;
        private string _pendingLine;
        private int _historyIndex = -1;
        private int _historyMove;
        private bool _submit;
        private bool _clearRequested;
        private bool _enterHeld;
        private bool _forceScroll;
        private int _seenVersion = -1;
        private float[] _rowHeights = System.Array.Empty<float>();
        private float _contentHeight;
        private float _contentWidth = -1f;
        private float _contentScale = -1f;
        private int _contentVersion = -2;
        private float _usageTitle;
        private float _usageBody;

        public void Focus()
        {
            GUI.FocusControl(InputControl);
        }

        public void Draw(Rect rect, GmSkin skin)
        {
            HandleKeys();
            if (Event.current.type == EventType.Layout)
                ApplyChanges();

            if (Event.current.type == EventType.Repaint)
                skin.DrawSolid(rect, skin.CmdBack);

            float tool = skin.Px(26);
            float input = skin.Px(34);
            DrawToolbar(new Rect(rect.x, rect.y, rect.width, tool), skin);
            DrawList(new Rect(rect.x, rect.y + tool + skin.Px(4), rect.width, rect.height - tool - input - skin.Px(12)), skin);
            DrawInput(new Rect(rect.x, rect.yMax - input, rect.width, input), skin);
        }

        private void HandleKeys()
        {
            Event current = Event.current;
            if (current.type == EventType.KeyUp && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter))
                _enterHeld = false;

            if (current.type != EventType.KeyDown)
                return;

            if (GUI.GetNameOfFocusedControl() != InputControl)
                return;

            if (current.keyCode == KeyCode.UpArrow)
            {
                _historyMove = -1;
                current.Use();
                return;
            }

            if (current.keyCode == KeyCode.DownArrow)
            {
                _historyMove = 1;
                current.Use();
                return;
            }

            if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && !_enterHeld)
            {
                _enterHeld = true;
                _pendingLine = _input;
                _input = string.Empty;
                _historyIndex = -1;
                _submit = true;
                current.Use();
            }
        }

        private void ApplyChanges()
        {
            if (_historyMove != 0)
            {
                MoveHistory(_historyMove);
                _historyMove = 0;
            }

            if (!GmInput.GetKey(KeyCode.Return) && !GmInput.GetKey(KeyCode.KeypadEnter))
                _enterHeld = false;

            if (_clearRequested)
            {
                GM.ClearOutput();
                _clearRequested = false;
            }

            if (_submit)
            {
                _submit = false;
                string line = _pendingLine ?? string.Empty;
                _pendingLine = null;
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (_history.Count == 0 || _history[_history.Count - 1] != line)
                        _history.Add(line);
                    if (_history.Count > 50)
                        _history.RemoveAt(0);
                    GM.Execute(line);
                }
            }

            int version = GM.OutputVersion;
            if (version != _seenVersion)
            {
                _seenVersion = version;
                _forceScroll = true;
            }

            GM.CopyOutput(_lines);
            if (_forceScroll)
            {
                _scroll.y = 1000000f;
                _forceScroll = false;
            }
        }

        private void MoveHistory(int direction)
        {
            if (_history.Count == 0)
                return;

            if (direction < 0)
            {
                if (_historyIndex < 0)
                {
                    _draft = _input;
                    _historyIndex = _history.Count - 1;
                }
                else if (_historyIndex > 0)
                {
                    _historyIndex--;
                }

                _input = _history[_historyIndex];
                return;
            }

            if (_historyIndex < 0)
                return;

            _historyIndex++;
            if (_historyIndex >= _history.Count)
            {
                _historyIndex = -1;
                _input = _draft ?? string.Empty;
                return;
            }

            _input = _history[_historyIndex];
        }

        private void DrawToolbar(Rect rect, GmSkin skin)
        {
            if (Event.current.type == EventType.Repaint)
            {
                skin.MutedStyle.alignment = TextAnchor.MiddleLeft;
                skin.MutedStyle.normal.textColor = skin.CmdDim;
                skin.DrawText(new Rect(rect.x, rect.y, rect.width - skin.Px(90), rect.height), skin.MutedStyle, "Enter 执行    ↑↓ 历史");
                skin.MutedStyle.normal.textColor = skin.Muted;
            }

            if (skin.Button(new Rect(rect.xMax - skin.Px(64), rect.y, skin.Px(64), rect.height), "清空", false, skin.ButtonFace))
                _clearRequested = true;
        }

        private void DrawList(Rect rect, GmSkin skin)
        {
            float width = Mathf.Max(10f, rect.width - skin.Px(14));
            EnsureHeights(width, skin);
            if (Event.current.type == EventType.Layout && _scroll.y > 900000f)
                _scroll.y = Mathf.Max(0f, _contentHeight - rect.height);

            _scroll = GUI.BeginScrollView(rect, _scroll, new Rect(0f, 0f, width, Mathf.Max(rect.height, _contentHeight)), false, false);
            if (_lines.Count == 0)
                DrawUsage(width, skin);
            else
            {
                float y = skin.Px(2);
                for (int i = 0; i < _lines.Count; i++)
                {
                    GmLine line = _lines[i];
                    if (line.Kind == GmLineKind.Echo)
                        y += skin.Px(8);
                    ApplyLineStyle(line.Kind, skin);
                    _content.text = LineText(line);
                    float rowHeight = _rowHeights[i];
                    GUI.Label(new Rect(skin.Px(2), y, width - skin.Px(6), rowHeight), _content, skin.Cmd);
                    y += rowHeight + skin.Px(1);
                }
            }

            GUI.EndScrollView();
        }

        private void DrawUsage(float width, GmSkin skin)
        {
            skin.Cmd.fontStyle = FontStyle.Bold;
            skin.Cmd.normal.textColor = skin.CmdEcho;
            _content.text = "help";
            GUI.Label(new Rect(skin.Px(2), skin.Px(4), width - skin.Px(6), _usageTitle), _content, skin.Cmd);

            skin.Cmd.fontStyle = FontStyle.Normal;
            skin.Cmd.normal.textColor = skin.CmdText;
            _content.text = HelpCommand.UsageText;
            GUI.Label(new Rect(skin.Px(2), _usageTitle + skin.Px(10), width - skin.Px(6), _usageBody), _content, skin.Cmd);
        }

        private void EnsureHeights(float width, GmSkin skin)
        {
            int version = GM.OutputVersion;
            if (version == _contentVersion && Mathf.Abs(width - _contentWidth) < 0.5f && Mathf.Abs(skin.Scale - _contentScale) < 0.001f)
                return;

            _contentVersion = version;
            _contentWidth = width;
            _contentScale = skin.Scale;
            if (_lines.Count == 0)
            {
                skin.Cmd.fontStyle = FontStyle.Bold;
                _content.text = "help";
                _usageTitle = skin.Cmd.CalcHeight(_content, width - skin.Px(4));
                skin.Cmd.fontStyle = FontStyle.Normal;
                _content.text = HelpCommand.UsageText;
                _usageBody = skin.Cmd.CalcHeight(_content, width - skin.Px(4));
                _contentHeight = _usageTitle + _usageBody + skin.Px(20);
                return;
            }

            if (_rowHeights.Length < _lines.Count)
                _rowHeights = new float[Mathf.NextPowerOfTwo(_lines.Count)];

            float y = skin.Px(2);
            float textWidth = width - skin.Px(4);
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_lines[i].Kind == GmLineKind.Echo)
                    y += skin.Px(8);
                ApplyLineStyle(_lines[i].Kind, skin);
                _content.text = LineText(_lines[i]);
                float rowHeight = Mathf.Max(skin.Px(20), skin.Cmd.CalcHeight(_content, textWidth));
                _rowHeights[i] = rowHeight;
                y += rowHeight + skin.Px(1);
            }

            _contentHeight = y + skin.Px(8);
        }

        private static string LineText(GmLine line)
        {
            string text = line.Text ?? string.Empty;
            return line.Kind == GmLineKind.Echo ? text : "    " + text;
        }

        private void DrawInput(Rect rect, GmSkin skin)
        {
            Rect prompt = new Rect(rect.x, rect.y, skin.Px(28), rect.height);
            Rect field = new Rect(prompt.xMax, rect.y, rect.width - skin.Px(28) - skin.Px(92), rect.height);
            Rect run = new Rect(field.xMax + skin.Px(8), rect.y, skin.Px(84), rect.height);

            if (Event.current.type == EventType.Repaint)
            {
                skin.DrawSolid(new Rect(field.x - 1f, field.y - 1f, field.width + 2f, field.height + 2f), skin.Line);
                skin.DrawSolid(field, skin.Input);
                skin.MutedStyle.alignment = TextAnchor.MiddleCenter;
                skin.MutedStyle.normal.textColor = skin.Text;
                skin.DrawText(prompt, skin.MutedStyle, ">");
                skin.MutedStyle.normal.textColor = skin.Muted;
                skin.MutedStyle.alignment = TextAnchor.MiddleLeft;
            }

            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = skin.Input;
            GUI.SetNextControlName(InputControl);
            _input = GUI.TextField(field, _input ?? string.Empty, skin.CmdInput);
            GUI.backgroundColor = previous;

            if (Event.current.type == EventType.Repaint
                && string.IsNullOrEmpty(_input)
                && GUI.GetNameOfFocusedControl() != InputControl)
            {
                skin.MutedStyle.normal.textColor = skin.CmdDim;
                skin.DrawText(new Rect(field.x + skin.Px(10), field.y, field.width - skin.Px(12), field.height), skin.MutedStyle, "输入命令");
                skin.MutedStyle.normal.textColor = skin.Muted;
            }

            if (skin.Button(run, "执行", true, skin.EditorBlue))
            {
                _pendingLine = _input;
                _input = string.Empty;
                _historyIndex = -1;
                _submit = true;
            }
        }

        private static void ApplyLineStyle(GmLineKind kind, GmSkin skin)
        {
            switch (kind)
            {
                case GmLineKind.Echo:
                    skin.Cmd.fontStyle = FontStyle.Bold;
                    skin.Cmd.normal.textColor = skin.CmdEcho;
                    break;
                case GmLineKind.Error:
                    skin.Cmd.fontStyle = FontStyle.Normal;
                    skin.Cmd.normal.textColor = skin.CmdError;
                    break;
                default:
                    skin.Cmd.fontStyle = FontStyle.Normal;
                    skin.Cmd.normal.textColor = skin.CmdText;
                    break;
            }
        }
    }
}
