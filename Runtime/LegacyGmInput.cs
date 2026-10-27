#if ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine;

namespace UniGM
{
    /// <summary>
    /// 旧输入系统上的默认快捷键：F1、鼠标左键、回车、小键盘回车、上方向键、下方向键。
    /// </summary>
    public sealed class LegacyGmInput : IGmInput
    {
        /// <summary>
        /// F1 是否正按住。
        /// </summary>
        public bool GetOpenOrCloseGMShortcutKeyTriggered(out string shortcutKeyName)
        {
            shortcutKeyName = "F1";
            return Input.GetKey(KeyCode.F1);
        }

        /// <summary>
        /// 鼠标左键是否正按住。
        /// </summary>
        public bool GetDragOrResizeGMPointerTriggered(out string shortcutKeyName)
        {
            shortcutKeyName = "Left Mouse";
            return Input.GetMouseButton(0);
        }

        /// <summary>
        /// 回车是否正按住。
        /// </summary>
        public bool GetSubmitGMCommandShortcutKeyTriggered(out string shortcutKeyName)
        {
            shortcutKeyName = "Enter";
            return Input.GetKey(KeyCode.Return);
        }

        /// <summary>
        /// 小键盘回车是否正按住。
        /// </summary>
        public bool GetSubmitGMCommandKeypadShortcutKeyTriggered(out string shortcutKeyName)
        {
            shortcutKeyName = "KeypadEnter";
            return Input.GetKey(KeyCode.KeypadEnter);
        }

        /// <summary>
        /// 上方向键是否正按住。
        /// </summary>
        public bool GetShowPreviousGMCommandShortcutKeyTriggered(out string shortcutKeyName)
        {
            shortcutKeyName = "↑";
            return Input.GetKey(KeyCode.UpArrow);
        }

        /// <summary>
        /// 下方向键是否正按住。
        /// </summary>
        public bool GetShowNextGMCommandShortcutKeyTriggered(out string shortcutKeyName)
        {
            shortcutKeyName = "↓";
            return Input.GetKey(KeyCode.DownArrow);
        }
    }
}
#endif
