using System;
using System.Text.RegularExpressions;

namespace Game.GM
{
    /// <summary>
    /// 已注册参数解析器的内部视图，供调度和 help 使用。
    /// </summary>
    internal interface IArgParser
    {
        Type ArgType { get; }

        Regex Template { get; }

        string Hint { get; }

        bool TryParse(string arg, out object argObj);
    }

    /// <summary>
    /// 参数类型解析器。一种类型对应一个解析器，通过 <see cref="GM.Register{T}"/> 注册；再次注册同一类型会覆盖旧解析器。
    /// <see cref="Template"/> 用来判断整段参数文本是否属于该类型。
    /// </summary>
    public abstract class ArgParser<T> : IArgParser
    {
        Type IArgParser.ArgType => typeof(T);

        /// <summary>
        /// 整段参数的正则，需要能从开头匹配到结尾。
        /// </summary>
        public abstract Regex Template { get; }

        /// <summary>
        /// 这种类型在命令行里怎么写。由 <c>help arg</c> 显示。
        /// </summary>
        public abstract string Hint { get; }

        /// <summary>
        /// 解析单段参数。成功时 <paramref name="argObj"/> 为装箱后的 <typeparamref name="T"/>。
        /// </summary>
        public abstract bool TryParse(string arg, out object argObj);
    }
}
