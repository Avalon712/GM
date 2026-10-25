using UnityEngine;

namespace UniGM
{
    internal sealed class GmSkin
    {
        public readonly Color Window = new Color(0.220f, 0.220f, 0.220f, 1f);
        public readonly Color Header = new Color(0.176f, 0.176f, 0.176f, 1f);
        public readonly Color Panel = new Color(0.196f, 0.196f, 0.196f, 1f);
        public readonly Color Line = new Color(0.110f, 0.110f, 0.110f, 1f);
        public readonly Color Text = new Color(0.820f, 0.820f, 0.820f, 1f);
        public readonly Color Muted = new Color(0.550f, 0.550f, 0.550f, 1f);
        public readonly Color EditorBlue = new Color(0.173f, 0.365f, 0.529f, 1f);
        public readonly Color Console = new Color(0.620f, 0.780f, 1f, 1f);
        public readonly Color Command = new Color(0.820f, 0.820f, 0.820f, 1f);
        public readonly Color Warn = new Color(1f, 0.820f, 0.310f, 1f);
        public readonly Color Error = new Color(1f, 0.420f, 0.380f, 1f);
        public readonly Color Echo = new Color(0.910f, 0.910f, 0.910f, 1f);
        public readonly Color Input = new Color(0.157f, 0.157f, 0.157f, 1f);
        public readonly Color ButtonFace = new Color(0.275f, 0.275f, 0.275f, 1f);
        public readonly Color ButtonHover = new Color(0.330f, 0.330f, 0.330f, 1f);
        public readonly Color Row = new Color(1f, 1f, 1f, 0.028f);
        public readonly Color RowHover = new Color(1f, 1f, 1f, 0.050f);
        public readonly Color Select = new Color(0.173f, 0.365f, 0.529f, 1f);
        public readonly Color DetailBack = new Color(0.157f, 0.157f, 0.157f, 1f);
        public readonly Color CmdBack = new Color(0.157f, 0.157f, 0.157f, 1f);
        public readonly Color CmdText = new Color(0.820f, 0.820f, 0.820f, 1f);
        public readonly Color CmdEcho = new Color(0.930f, 0.930f, 0.930f, 1f);
        public readonly Color CmdDim = new Color(0.550f, 0.550f, 0.550f, 1f);
        public readonly Color CmdError = new Color(1f, 0.420f, 0.380f, 1f);
        public readonly Color CmdButton = new Color(0.275f, 0.275f, 0.275f, 1f);
        public readonly Color Stats = new Color(0.550f, 0.780f, 0.480f, 1f);
        public readonly Color PillFace = new Color(0.45f, 0.45f, 0.45f, 1f);
        public readonly Color PillLight = new Color(0.72f, 0.72f, 0.72f, 1f);
        public readonly Color PillDark = new Color(0.18f, 0.18f, 0.18f, 1f);
        public readonly Color PillText = new Color(0.12f, 0.12f, 0.12f, 1f);

        public GUIStyle Title;
        public GUIStyle Tab;
        public GUIStyle MutedStyle;
        public GUIStyle InputStyle;
        public GUIStyle Log;
        public GUIStyle Stack;
        public GUIStyle Detail;
        public GUIStyle Cmd;
        public GUIStyle CmdInput;

        public Texture2D White;
        public Texture2D Bar;
        public Texture2D Thumb;
        public GUISkin Skin;

        public float Scale { get; private set; } = 1f;

        private readonly GUIContent _content = new GUIContent();
        private bool _ready;

        public float Px(float value)
        {
            return Mathf.Round(value * Scale);
        }

        public void Ensure(float scale)
        {
            if (_ready && Mathf.Abs(scale - Scale) < 0.02f)
                return;

            Scale = scale;
            if (White == null)
                White = SolidTexture(Color.white);
            if (Bar == null)
                Bar = SolidTexture(new Color(0.16f, 0.16f, 0.16f, 1f));
            if (Thumb == null)
                Thumb = SolidTexture(new Color(0.40f, 0.40f, 0.40f, 1f));

            if (Skin == null)
            {
                Skin = Object.Instantiate(GUI.skin);
                Skin.hideFlags = HideFlags.HideAndDontSave;
                Skin.verticalScrollbar = new GUIStyle(Skin.verticalScrollbar);
                Skin.verticalScrollbarThumb = new GUIStyle(Skin.verticalScrollbarThumb);
                Skin.horizontalScrollbar = new GUIStyle(Skin.horizontalScrollbar);
                Skin.horizontalScrollbarThumb = new GUIStyle(Skin.horizontalScrollbarThumb);
                if (Skin.scrollView != null)
                {
                    Skin.scrollView = new GUIStyle(Skin.scrollView);
                    Skin.scrollView.normal.background = null;
                }
            }

            StyleScrollbar(Skin.verticalScrollbar, Skin.verticalScrollbarThumb);
            StyleScrollbar(Skin.horizontalScrollbar, Skin.horizontalScrollbarThumb);

            Title = Make(13, FontStyle.Bold, Text, TextAnchor.MiddleLeft);
            Tab = Make(11, FontStyle.Bold, Text, TextAnchor.MiddleCenter);
            MutedStyle = Make(10, FontStyle.Normal, Muted, TextAnchor.MiddleLeft);
            InputStyle = Make(11, FontStyle.Normal, Text, TextAnchor.MiddleLeft);
            Log = Make(11, FontStyle.Normal, Text, TextAnchor.MiddleLeft);
            Stack = Make(10, FontStyle.Normal, Muted, TextAnchor.UpperLeft);
            Detail = Make(11, FontStyle.Normal, Text, TextAnchor.UpperLeft);
            Cmd = Make(11, FontStyle.Normal, CmdText, TextAnchor.UpperLeft);
            CmdInput = Make(12, FontStyle.Normal, CmdEcho, TextAnchor.MiddleLeft);

            Log.wordWrap = false;
            Log.clipping = TextClipping.Clip;
            Log.padding = new RectOffset((int)Px(10), (int)Px(8), 0, 0);
            Log.normal.background = null;
            Log.hover.background = null;
            Log.active.background = null;
            Log.onNormal.background = null;
            Log.onHover.background = null;

            Stack.wordWrap = true;
            Stack.padding = new RectOffset((int)Px(10), (int)Px(8), (int)Px(2), (int)Px(4));
            Detail.wordWrap = true;
            Detail.padding = new RectOffset((int)Px(10), (int)Px(8), (int)Px(4), (int)Px(2));
            Cmd.wordWrap = true;
            Cmd.padding = new RectOffset((int)Px(2), (int)Px(2), (int)Px(1), (int)Px(1));
            CmdInput.padding = new RectOffset((int)Px(8), (int)Px(8), (int)Px(6), (int)Px(4));
            CmdInput.normal.background = White;
            CmdInput.focused.background = White;
            CmdInput.hover.background = White;
            CmdInput.active.background = White;
            CmdInput.normal.textColor = CmdEcho;
            CmdInput.focused.textColor = CmdEcho;
            CmdInput.clipping = TextClipping.Clip;
            InputStyle.padding = new RectOffset((int)Px(10), (int)Px(8), (int)Px(6), (int)Px(4));
            InputStyle.normal.background = White;
            InputStyle.focused.background = White;
            InputStyle.hover.background = White;
            InputStyle.active.background = White;
            InputStyle.normal.textColor = Text;
            InputStyle.focused.textColor = Color.white;
            InputStyle.clipping = TextClipping.Clip;

            Title.padding = new RectOffset((int)Px(12), 0, 0, 0);
            _ready = true;
        }

        public void DrawSolid(Rect rect, Color color)
        {
            if (Event.current.type != EventType.Repaint || White == null)
                return;

            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, White);
            GUI.color = previous;
        }

        public Vector2 Measure(GUIStyle style, string text)
        {
            _content.text = text;
            return style.CalcSize(_content);
        }

        public void DrawText(Rect rect, GUIStyle style, string text)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            _content.text = text;
            style.Draw(rect, _content, false, false, false, false);
        }

        public void DrawBorder(Rect rect)
        {
            DrawSolid(new Rect(rect.x, rect.y, rect.width, 1f), Line);
            DrawSolid(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), Line);
            DrawSolid(new Rect(rect.x, rect.y, 1f, rect.height), Line);
            DrawSolid(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), Line);
        }

        public bool Button(Rect rect, string text, bool selected, Color accent)
        {
            bool hover = rect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.Repaint)
            {
                bool tint = selected && accent != EditorBlue;
                Color background = tint || !selected
                    ? (hover ? ButtonHover : ButtonFace)
                    : EditorBlue;
                DrawSolid(rect, background);
                if (selected)
                    DrawSolid(new Rect(rect.x, rect.y, rect.width, 2f), tint ? accent : EditorBlue);
                _content.text = text;
                Tab.normal.textColor = selected ? (tint ? accent : Color.white) : Text;
                Tab.Draw(rect, _content, false, false, false, false);
            }

            bool click = Event.current.type == EventType.MouseDown
                         && Event.current.button == 0
                         && hover;
            if (click)
                Event.current.Use();
            return click;
        }

        public void Dispose()
        {
            Destroy(White);
            Destroy(Bar);
            Destroy(Thumb);
            Destroy(Skin);
            White = null;
            Bar = null;
            Thumb = null;
            Skin = null;
        }

        private GUIStyle Make(int size, FontStyle fontStyle, Color color, TextAnchor anchor)
        {
            GUIStyle style = new GUIStyle
            {
                font = GUI.skin.font,
                fontSize = Mathf.Max(11, Mathf.RoundToInt(size * Scale)),
                fontStyle = fontStyle,
                alignment = anchor,
                richText = false,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            return style;
        }

        private void StyleScrollbar(GUIStyle bar, GUIStyle thumb)
        {
            if (bar == null || thumb == null || Bar == null || Thumb == null)
                return;

            bar.normal.background = Bar;
            bar.fixedWidth = Px(10);
            bar.margin = new RectOffset(0, 0, 0, 0);
            thumb.normal.background = Thumb;
            thumb.hover.background = Thumb;
            thumb.active.background = Thumb;
            thumb.fixedWidth = Px(8);
        }

        private static void Destroy(Object asset)
        {
            if (asset != null)
                Object.Destroy(asset);
        }

        private static Texture2D SolidTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            return texture;
        }
    }
}
