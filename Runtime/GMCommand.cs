using System;
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
        /// 用法说明。由 <c>help -gm</c> 显示。
        /// </summary>
        public abstract string Hint { get; }

        /// <summary>
        /// 为 true 时，GM 控制台还没有任何输出时显示 <see cref="Hint"/>。help 总会显示，并且排在最前。
        /// </summary>
        public virtual bool DisplayUsage => false;

        /// <summary>
        /// 本次执行从命令行解析出的只读参数字典。在 <see cref="Execute"/> 之前写入。
        /// 命令名对应按输入顺序排列的全部参数。选项名没有参数时为 <c>null</c>，只有一个参数时为该对象，有多个参数时为 <c>List&lt;object&gt;</c>。
        /// </summary>
        public IReadOnlyDictionary<string, object> Args { get; private set; } = EmptyArgs;

        /// <summary>
        /// 执行命令。<paramref name="args"/> 与 <see cref="Args"/> 是同一个只读字典。
        /// </summary>
        public abstract void Execute(IReadOnlyDictionary<string, object> args);

        internal void SetArgs(IReadOnlyDictionary<string, object> args)
        {
            Args = args ?? EmptyArgs;
        }

        private static readonly IReadOnlyDictionary<string, object> EmptyArgs =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
}
