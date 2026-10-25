using UnityEngine;

namespace UniGM
{
    /// <summary>
    /// 运行时 GM 窗口。Console 与 GM 命令使用各自的缓冲和输入。
    /// </summary>
    internal sealed class GMManager : MonoBehaviour
    {
        private const string PrefX = "Game.GM.WinX";
        private const string PrefY = "Game.GM.WinY";
        private const string PrefW = "Game.GM.WinW";
        private const string PrefH = "Game.GM.WinH";
        private const string PrefTab = "Game.GM.Tab";
        private const string PrefPillX = "Game.GM.PillX";
        private const string PrefPillY = "Game.GM.PillY";

        private static GMManager _instance;

        private readonly GmSkin _skin = new GmSkin();
        private readonly DebugConsoleView _debug = new DebugConsoleView();
        private readonly GmConsoleView _command = new GmConsoleView();
        private readonly PerformanceView _stats = new PerformanceView();

        private Rect _window;
        private float _scale = 1f;
        private float _titleH = 44f;
        private float _tabH = 36f;
        private float _pad = 12f;
        private int _tab;
        private bool _visible;
        private bool _ready;
        private bool _toggleRequested;
        private bool _showRequested;
        private bool _hideRequested;
        private int _tabRequest = -1;
        private bool _f1Held;
        private bool _dragging;
        private bool _resizing;
        private bool _focusInput;
        private Vector2 _pointer;
        private Vector2 _dragOffset;
        private Vector2 _resizeSlack;
        private Rect _pill;
        private bool _pillReady;
        private bool _pillDrag;
        private bool _pillMoved;
        private Vector2 _pillDown;
        private Vector2 _pillOffset;

        internal static void Ensure()
        {
            if (!Application.isPlaying || _instance != null)
                return;

            GameObject host = new GameObject("[GM]");
            GMManager manager = host.AddComponent<GMManager>();
            DontDestroyOnLoad(host);
            MarkRuntimeOnly(host, manager);
        }

        private static void MarkRuntimeOnly(GameObject host, GMManager manager)
        {
            host.hideFlags = HideFlags.HideAndDontSave;
            manager.hideFlags = HideFlags.HideAndDontSave;
        }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }

            MarkRuntimeOnly(gameObject, this);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            bool owner = _instance == this;
            if (owner)
                _instance = null;

            _skin.Dispose();
            if (!owner)
                return;

            GmLogHub.Unsubscribe();
            GM.SetTyping(false);
        }

        private void Update()
        {
            if (Application.isPlaying)
                _stats.Sample();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }

            _scale = Mathf.Clamp(Mathf.Min(Screen.width / 1920f, Screen.height / 1080f), 1.35f, 2f);
            _skin.Ensure(_scale);
            _titleH = _skin.Px(40f);
            _tabH = _skin.Px(34f);
            _pad = _skin.Px(14f);
            if (!_ready)
            {
                LoadWindow();
                _ready = true;
            }

            TrackPointer();
            if (Event.current.type == EventType.Layout)
                ApplyWindowChanges();

            TrackHotkeys();

            GUISkin previousSkin = GUI.skin;
            Color previousColor = GUI.color;
            if (_skin.Skin != null)
                GUI.skin = _skin.Skin;

            try
            {
                if (!_visible)
                {
                    DrawPill();
                    GM.SetTyping(false);
                    return;
                }

                DrawWindow();
                bool typing = _tab == 1 && GUI.GetNameOfFocusedControl() == GmConsoleView.InputControl;
                GM.SetTyping(typing);
            }
            finally
            {
                GUI.skin = previousSkin;
                GUI.color = previousColor;
            }
        }

        private void ApplyWindowChanges()
        {
            if ((_dragging || _resizing) && !GmInput.GetMouseButton(0))
            {
                _dragging = false;
                _resizing = false;
                SaveWindow();
            }

            if (_pillDrag && !GmInput.GetMouseButton(0))
                FinishPillGesture();

            if (!GmInput.GetKey(KeyCode.F1))
                _f1Held = false;

            if (_hideRequested)
            {
                _visible = false;
                _hideRequested = false;
                _showRequested = false;
                _toggleRequested = false;
                GM.SetTyping(false);
                SaveWindow();
            }
            else if (_showRequested)
            {
                _visible = true;
                _showRequested = false;
                _toggleRequested = false;
                if (_tab == 1)
                    _focusInput = true;
            }
            else if (_toggleRequested)
            {
                _visible = !_visible;
                _toggleRequested = false;
                if (_visible && _tab == 1)
                    _focusInput = true;
                if (!_visible)
                    GM.SetTyping(false);
            }

            if (_tabRequest >= 0)
            {
                _tab = _tabRequest;
                _tabRequest = -1;
                if (_tab == 1)
                    _focusInput = true;
                else
                {
                    GUI.FocusControl(null);
                    GM.SetTyping(false);
                }

                SaveWindow();
            }

            if (_dragging)
                _window.position = _pointer - _dragOffset;

            if (_resizing)
            {
                _window.width = _pointer.x - _window.x + _resizeSlack.x;
                _window.height = _pointer.y - _window.y + _resizeSlack.y;
            }

            ClampWindow();
        }

        private void TrackPointer()
        {
            if (Event.current.isMouse)
                _pointer = Event.current.mousePosition;
        }

        private void TrackHotkeys()
        {
            Event current = Event.current;
            if (current.type == EventType.KeyUp && current.keyCode == KeyCode.F1)
                _f1Held = false;

            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.F1 && !_f1Held)
            {
                _f1Held = true;
                _toggleRequested = true;
                current.Use();
                return;
            }

            if (current.keyCode == KeyCode.Escape && _visible)
            {
                if (GUI.GetNameOfFocusedControl() == GmConsoleView.InputControl)
                    GUI.FocusControl(null);
                else
                    _hideRequested = true;
                current.Use();
            }
        }

        private void DrawPill()
        {
            EnsurePill();
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && _pill.Contains(current.mousePosition))
            {
                _pillDrag = true;
                _pillMoved = false;
                _pillDown = current.mousePosition;
                _pillOffset = current.mousePosition - _pill.position;
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && _pillDrag)
            {
                if ((current.mousePosition - _pillDown).sqrMagnitude > 36f)
                    _pillMoved = true;
                if (_pillMoved)
                {
                    _pill.position = current.mousePosition - _pillOffset;
                    ClampPill();
                }

                current.Use();
            }
            else if (current.type == EventType.MouseUp && current.button == 0 && _pillDrag)
            {
                FinishPillGesture();
                current.Use();
            }

            if (current.type == EventType.Repaint)
                DrawPillFace();
        }

        private void DrawPillFace()
        {
            _skin.DrawSolid(_pill, _skin.PillDark);
            _skin.DrawSolid(new Rect(_pill.x, _pill.y, _pill.width - 1f, _pill.height - 1f), _skin.PillLight);
            _skin.DrawSolid(new Rect(_pill.x + 1f, _pill.y + 1f, _pill.width - 2f, _pill.height - 2f), _skin.PillFace);
            _skin.Tab.normal.textColor = _skin.PillText;
            _skin.DrawText(_pill, _skin.Tab, "GM");
        }

        private void FinishPillGesture()
        {
            bool moved = _pillMoved;
            _pillDrag = false;
            _pillMoved = false;
            if (moved)
                SavePill();
            else
                _showRequested = true;
        }

        private void EnsurePill()
        {
            Vector2 text = _skin.Measure(_skin.Tab, "GM");
            float width = Mathf.Max(8f, text.x * 1.5f);
            float height = Mathf.Max(8f, text.y * 1.5f);
            if (!_pillReady)
            {
                float x = PlayerPrefs.GetFloat(PrefPillX, _skin.Px(18));
                float y = PlayerPrefs.GetFloat(PrefPillY, Screen.height - height - _skin.Px(18));
                _pill = new Rect(x, y, width, height);
                _pillReady = true;
            }
            else
            {
                _pill.width = width;
                _pill.height = height;
            }

            ClampPill();
        }

        private void ClampPill()
        {
            _pill.x = Mathf.Clamp(_pill.x, 0f, Mathf.Max(0f, Screen.width - _pill.width));
            _pill.y = Mathf.Clamp(_pill.y, 0f, Mathf.Max(0f, Screen.height - _pill.height));
        }

        private void SavePill()
        {
            PlayerPrefs.SetFloat(PrefPillX, _pill.x);
            PlayerPrefs.SetFloat(PrefPillY, _pill.y);
            PlayerPrefs.Save();
        }

        private void DrawWindow()
        {
            _skin.DrawSolid(_window, _skin.Window);
            _skin.DrawSolid(new Rect(_window.x, _window.y, _window.width, _titleH), _skin.Header);
            _skin.DrawBorder(_window);

            Rect title = new Rect(_window.x, _window.y, _window.width, _titleH);
            if (Event.current.type == EventType.Repaint)
            {
                _skin.Title.normal.textColor = _skin.Text;
                _skin.DrawText(title, _skin.Title, "GM  运行时管理器");
                _skin.MutedStyle.alignment = TextAnchor.MiddleRight;
                _skin.DrawText(new Rect(title.x, title.y, title.width - _skin.Px(70), title.height), _skin.MutedStyle, "F1");
                _skin.MutedStyle.alignment = TextAnchor.MiddleLeft;
            }

            Rect close = new Rect(_window.xMax - _skin.Px(42), _window.y + _skin.Px(8), _skin.Px(28), _skin.Px(28));
            if (_skin.Button(close, "X", false, _skin.ButtonFace))
                _hideRequested = true;

            HandleDrag(title, close);

            float tabGap = _skin.Px(2);
            float tabPad = _skin.Px(8);
            float tabY = _window.y + _titleH;
            float tabHeight = _tabH - _skin.Px(6);
            float tabX = _window.x + _pad;
            float consoleWidth = _skin.Measure(_skin.Tab, "Console").x + tabPad * 2f;
            float commandWidth = _skin.Measure(_skin.Tab, "GM").x + tabPad * 2f;
            float statsWidth = _skin.Measure(_skin.Tab, "性能").x + tabPad * 2f;
            Rect consoleTab = new Rect(tabX, tabY, consoleWidth, tabHeight);
            Rect commandTab = new Rect(consoleTab.xMax + tabGap, tabY, commandWidth, tabHeight);
            Rect statsTab = new Rect(commandTab.xMax + tabGap, tabY, statsWidth, tabHeight);
            if (_skin.Button(consoleTab, "Console", _tab == 0, _skin.EditorBlue) && _tab != 0)
                _tabRequest = 0;

            if (_skin.Button(commandTab, "GM", _tab == 1, _skin.EditorBlue) && _tab != 1)
                _tabRequest = 1;

            if (_skin.Button(statsTab, "性能", _tab == 2, _skin.EditorBlue) && _tab != 2)
                _tabRequest = 2;

            float contentTop = tabY + _tabH;
            _skin.DrawSolid(new Rect(_window.x + _pad, contentTop - _skin.Px(1), _window.width - _pad * 2f, 1f), _skin.Line);

            Rect content = new Rect(
                _window.x + _pad,
                contentTop + _skin.Px(8),
                _window.width - _pad * 2f,
                _window.height - (contentTop - _window.y) - _pad - _skin.Px(8));

            if (_tab == 0)
                _debug.Draw(content, _skin);
            else if (_tab == 1)
                _command.Draw(content, _skin);
            else
                _stats.Draw(content, _skin);

            if (_focusInput && _tab == 1)
            {
                _command.Focus();
                _focusInput = false;
            }

            Rect grip = new Rect(_window.xMax - _skin.Px(18), _window.yMax - _skin.Px(18), _skin.Px(14), _skin.Px(14));
            if (Event.current.type == EventType.Repaint)
                _skin.DrawSolid(grip, _skin.ButtonHover);
            HandleResize(grip);
        }

        private void HandleDrag(Rect title, Rect close)
        {
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && title.Contains(current.mousePosition) && !close.Contains(current.mousePosition))
            {
                _dragging = true;
                _dragOffset = current.mousePosition - _window.position;
                current.Use();
            }

            if (current.type == EventType.MouseUp)
                _dragging = false;
        }

        private void HandleResize(Rect grip)
        {
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && grip.Contains(current.mousePosition))
            {
                _resizing = true;
                _resizeSlack = _window.max - current.mousePosition;
                current.Use();
            }

            if (current.type == EventType.MouseUp && _resizing)
            {
                _resizing = false;
                SaveWindow();
            }
        }

        private void LoadWindow()
        {
            int savedTab = PlayerPrefs.GetInt(PrefTab, 0);
            _tab = savedTab >= 0 && savedTab <= 2 ? savedTab : 0;
            if (!PlayerPrefs.HasKey(PrefX))
            {
                ResetWindow();
                return;
            }

            _window = new Rect(
                PlayerPrefs.GetFloat(PrefX),
                PlayerPrefs.GetFloat(PrefY),
                PlayerPrefs.GetFloat(PrefW),
                PlayerPrefs.GetFloat(PrefH));
            ClampWindow();
        }

        private void ResetWindow()
        {
            float width = Mathf.Clamp(Screen.width * 0.62f, 720f, 1080f);
            float height = Mathf.Clamp(Screen.height * 0.68f, 460f, 760f);
            _window = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            ClampWindow();
        }

        private void ClampWindow()
        {
            float minWidth = Mathf.Min(680f, Screen.width);
            float minHeight = Mathf.Min(420f, Screen.height);
            _window.width = Mathf.Clamp(_window.width, minWidth, Screen.width);
            _window.height = Mathf.Clamp(_window.height, minHeight, Screen.height);
            _window.x = Mathf.Clamp(_window.x, 0f, Mathf.Max(0f, Screen.width - _window.width));
            _window.y = Mathf.Clamp(_window.y, 0f, Mathf.Max(0f, Screen.height - _titleH));
        }

        private void SaveWindow()
        {
            PlayerPrefs.SetFloat(PrefX, _window.x);
            PlayerPrefs.SetFloat(PrefY, _window.y);
            PlayerPrefs.SetFloat(PrefW, _window.width);
            PlayerPrefs.SetFloat(PrefH, _window.height);
            PlayerPrefs.SetInt(PrefTab, _tab);
            PlayerPrefs.Save();
        }
    }
}
