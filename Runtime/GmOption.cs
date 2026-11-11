namespace UniGM
{
    /// <summary>
    /// 命令行选项，例如 <c>-gm</c>、<c>--file</c>。由内置解析器从参数文本解析得到。
    /// </summary>
    public readonly struct GmOption
    {
        public GmOption(string token)
        {
            Token = token ?? string.Empty;
        }

        /// <summary>
        /// 命令行上的原文，包含前导横线。
        /// </summary>
        public string Token { get; }

        public override string ToString()
        {
            return Token;
        }
    }
}
