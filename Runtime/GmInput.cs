namespace UniGM
{
    /// <summary>
    /// 把外部传入的 <see cref="IGmInput"/> 交给窗口使用。
    /// </summary>
    internal static class GmInput
    {
        private static IGmInput _input;

        public static void Bind(IGmInput input)
        {
            _input = input;
        }

        public static bool GetOpenOrCloseGMShortcutKeyTriggered(out string shortcutKeyName)
        {
            if (_input == null)
            {
                shortcutKeyName = string.Empty;
                return false;
            }

            return _input.GetOpenOrCloseGMShortcutKeyTriggered(out shortcutKeyName);
        }

        public static bool GetDragOrResizeGMPointerTriggered(out string shortcutKeyName)
        {
            if (_input == null)
            {
                shortcutKeyName = string.Empty;
                return false;
            }

            return _input.GetDragOrResizeGMPointerTriggered(out shortcutKeyName);
        }

        public static bool GetSubmitGMCommandShortcutKeyTriggered(out string shortcutKeyName)
        {
            if (_input == null)
            {
                shortcutKeyName = string.Empty;
                return false;
            }

            return _input.GetSubmitGMCommandShortcutKeyTriggered(out shortcutKeyName);
        }

        public static bool GetSubmitGMCommandKeypadShortcutKeyTriggered(out string shortcutKeyName)
        {
            if (_input == null)
            {
                shortcutKeyName = string.Empty;
                return false;
            }

            return _input.GetSubmitGMCommandKeypadShortcutKeyTriggered(out shortcutKeyName);
        }
    }
}
