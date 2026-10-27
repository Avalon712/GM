namespace UniGM
{
    /// <summary>
    /// GM 用到的快捷键是否正按住。由游戏在 <see cref="GM.Initialize"/> 时传入。
    /// </summary>
    public interface IGmInput
    {
        /// <summary>
        /// 打开或关闭 GM 窗口的快捷键是否正按住。<paramref name="shortcutKeyName"/> 为界面上显示的键名。
        /// </summary>
        bool GetOpenOrCloseGMShortcutKeyTriggered(out string shortcutKeyName);

        /// <summary>
        /// 拖动窗口、缩放窗口或拖动外部按钮的指针键是否正按住。<paramref name="shortcutKeyName"/> 为界面上显示的键名。
        /// </summary>
        bool GetDragOrResizeGMPointerTriggered(out string shortcutKeyName);

        /// <summary>
        /// 提交当前 GM 命令的回车是否正按住。<paramref name="shortcutKeyName"/> 为界面上显示的键名。
        /// </summary>
        bool GetSubmitGMCommandShortcutKeyTriggered(out string shortcutKeyName);

        /// <summary>
        /// 提交当前 GM 命令的小键盘回车是否正按住。<paramref name="shortcutKeyName"/> 为界面上显示的键名。
        /// </summary>
        bool GetSubmitGMCommandKeypadShortcutKeyTriggered(out string shortcutKeyName);

        /// <summary>
        /// 显示上一条历史命令的快捷键是否正按住。<paramref name="shortcutKeyName"/> 为界面上显示的键名。
        /// </summary>
        bool GetShowPreviousGMCommandShortcutKeyTriggered(out string shortcutKeyName);

        /// <summary>
        /// 显示下一条历史命令的快捷键是否正按住。<paramref name="shortcutKeyName"/> 为界面上显示的键名。
        /// </summary>
        bool GetShowNextGMCommandShortcutKeyTriggered(out string shortcutKeyName);
    }
}
