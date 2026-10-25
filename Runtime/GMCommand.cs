using System.Collections.Generic;

namespace UniGM
{
    /// <summary>
    /// GM 命令基类。通过 <see cref="GM.Register(GMCommand)"/> 注册后，可在运行时命令行执行。
    /// </summary>
    public abstract class GMCommand
    {
        /// <summary>
        /// 命令名。取命令行的第一个词，忽略大小写。
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 用法说明。由 <c>help gm</c> 显示。
        /// </summary>
        public abstract string Hint { get; }

        /// <summary>
        /// 本次执行从命令行解析出的参数。在 <see cref="Execute"/> 之前写入。
        /// </summary>
        public List<object> Args { get; private set; } = new List<object>();

        /// <summary>
        /// 执行命令。<paramref name="args"/> 与 <see cref="Args"/> 是同一个列表。
        /// </summary>
        public abstract void Execute(IReadOnlyList<object> args);

        internal void SetArgs(List<object> args)
        {
            Args = args ?? new List<object>();
        }
    }
}
