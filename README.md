# GM

运行时 GM 管理器。在游戏里用 Unity 遗产 GUI（IMGUI）查看 Debug 输出，并执行已注册的 GM 命令。Debug 日志和命令输出分开展示。

- **Unity**：2022.3+
- **命名空间**：`UniGM`

---

## 安装

### 一、方式一

通过 Package Manager 的 **Add package from git URL** 添加，填写下面的 URL：

```text
https://github.com/Avalon712/GM.git
```

### 二、方式二

把仓库下载到本地，然后通过 Package Manager 的 **Add package from disk**，选中该目录下的 `package.json`。

### 三、方式三

在项目的 `Packages` 目录下创建一个子目录，名称取 **org.avalon712.gm**，把仓库内容放进这个目录。Unity 会识别其中的 `package.json`，并把包加入项目。

### 四、安装需要注意的一些事项

本包依赖 `com.unity.inputsystem` 1.7.0。`GM.Runtime` 引用了 `Unity.InputSystem`，Package Manager 会随本包一起装上。键盘和鼠标与 FinalIK 的 `CameraController` 相同：定义了 `ENABLE_LEGACY_INPUT_MANAGER` 时用旧的 `UnityEngine.Input`，Player Settings 选 Both 时也走这条路径。只有关掉旧输入、单独启用 Input System 时，才使用 `UnityEngine.InputSystem`。

---

## 打开界面

游戏启动时调用一次：

```csharp
GM.Initialize();
```

这一步会创建窗口并开始接收 `Debug` 日志。包内不会自己用 `RuntimeInitializeOnLoadMethod` 启动。

然后：

- 按 **F1** 打开或关闭
- 或点击屏幕上的 **GM** 按钮。这个按钮可以拖到屏幕里的任意位置，松手后记住；点一下（没有拖动）才会打开窗口

窗口可以拖动标题栏，右下角可以改大小。位置、按钮位置和当前页签会记在 `PlayerPrefs` 里，下次 Play 仍恢复；窗口默认是关着的。界面只在播放中绘制，退出 Play 后不会留在 Game 视图里。

三个页签的数据和输入互不相通：

| 页签 | 内容 |
| --- | --- |
| **Console** | `Debug.Log` / `LogWarning` / `LogError`。可按 Info、Warning、Error 过滤，正则搜索、折叠、清空。点击一行后，下方单独的面板显示完整内容和堆栈 |
| **GM** | 命令行风格，和 Console 分开。命令回显、`GM.Print`、解析错误和异常都在这里。底部输入命令，Enter 执行，↑↓ 翻历史。还没有任何输出时，会显示 `help` 的用法 |
| **性能** | 最近 1 秒的平均帧率、帧时间，以及最近 1000 帧的 1% Low、0.1% Low。同时显示总分配、总保留、Mono、图形驱动和托管内存 |

命令想显示在 GM 页，调用 `GM.Print`。`Debug.Log` 只会进 Console 页。

命令框获得焦点时 `GM.IsTyping` 为 `true`，游戏逻辑可以据此忽略自己的快捷键。按 Esc：正在输入时先取消焦点，否则关闭窗口。

---

## 注册命令

命令继承 `GMCommand`，再用 `GM.Register` 注册。同名命令第二次注册会失败。

```csharp
using System.Collections.Generic;
using UniGM;
using UnityEngine;

public sealed class CubeCommand : GMCommand
{
    public override string Name => "cube";

    public override string Hint => "cube (x,y,z) (x,y,z)，位置与欧拉角，例如 cube (1,9,0) (0,89,0)";

    public override void Execute(IReadOnlyList<object> args)
    {
        if (args.Count != 2 || !(args[0] is Vector3) || !(args[1] is Vector3))
        {
            GM.Print(Hint);
            return;
        }

        GM.Print("位置 " + (Vector3)args[0] + "    欧拉角 " + (Vector3)args[1]);
    }
}

GM.Register(new CubeCommand());
```

执行时，调度器会先把参数解析好，写进 `Args`，再调用 `Execute`。`Execute` 收到的列表和 `Args` 是同一个。

```csharp
GM.Unregister("cube");
```

名称比较忽略大小写。命令名不能包含空白。

---

## 命令格式

第一个词是命令名，后面是用空格分开的参数：

```text
cube (1,9,0) (0,89,0)
```

上面会得到两个 `Vector3`。括号里面和英文双引号里面的空格不会把参数切开，所以 `(1, 9, 0)` 和 `"hello world"` 都是一段参数。

字符串里的转义：`\\`、`\"`、`\n`、`\t`。

某个参数没有解析器能匹配，或同时匹配多种类型时，命令不会执行，GM 页会写出原因。

---

## 参数类型

数字使用不变区域（小数点是 `.`）。没有后缀、又互相长得一样的整数只内置了 `int`。`byte` / `sbyte` / `short` / `ushort` 和 `int` 无法从字面量区分，需要的话自己注册解析器，或覆盖 `int`。

| 类型 | 写法示例 |
| --- | --- |
| `int` | `42`、`-3`、`+8` |
| `uint` | `42u` |
| `long` | `42L` |
| `ulong` | `42ul`、`42lu` |
| `float` | `1.5`、`.5`、`1e-3`、`1.5f`、`1f` |
| `double` | `1.5d`、`1d` |
| `decimal` | `1.5m`、`1m` |
| `bool` | `true`、`false` |
| `char` | `'a'`、`'\n'`、`'\t'` |
| `string` | `"hello"`、`"say \"hi\""` |
| `Vector2` | `(1,2)`、`(1.5, -2)` |
| `Vector3` | `(1,9,0)` |
| `Vector4` | `(1,0,0,1)` |
| `Vector2Int` | `i(3,4)`、`i(-1, 2)` |
| `Vector3Int` | `i(1,2,3)` |
| `Quaternion` | `q(0,0,0,1)` |
| `Color` | `c(1,0,0)`、`c(1,0,0,1)`，分量按 0~1 |
| `Rect` | `rect(0,0,100,50)` |

向量、颜色、四元数、Rect 里的数字沿用同一套数字字面量（可带 `f` / `d` / `m`）。`Vector2Int` / `Vector3Int` 只接受整数。前缀 `i`、`q`、`c`、`rect` 忽略大小写。

每种类型的 `Template` 是匹配整段参数的正则。内置模板不会同时命中同一段文本。

---

## help

只内置一条命令，名字是 `help`。`arg` 和 `gm` 是它的参数，用来指名要查类型还是命令，不走上面的类型解析器。

| 输入 | 输出 |
| --- | --- |
| `help` | 同时列出全部参数写法和全部命令 |
| `help arg` | 每个已注册解析器一行：类型名和 Hint |
| `help arg TypeName` | 只输出这个类型的 Hint |
| `help gm` | 每个已注册命令一行：名字和 Hint |
| `help gm CommandName` | 只输出这条命令的 Hint |

`TypeName` 忽略大小写，可以写 C# 关键字、短名或完整名，例如 `int`、`Int32`、`System.Int32`、`Vector3`、`UnityEngine.Vector3`。

找不到类型或命令时，GM 页会提示。其它写法会打印上面的用法。

---

## 扩展或覆盖解析器

一个类型对应一个解析器。再次 `GM.Register` 同一类型会覆盖旧的，包括内置解析器。

```csharp
using System;
using System.Text.RegularExpressions;
using UniGM;

public enum GmSampleTeam
{
    Red,
    Blue
}

public sealed class GmSampleTeamParser : ArgParser<GmSampleTeam>
{
    private static readonly Regex _template = new Regex(
        @"^(?:red|blue)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public override Regex Template => _template;

    public override string Hint => "red 或 blue，例如 red";

    public override bool TryParse(string arg, out object argObj)
    {
        if (arg.Equals("red", StringComparison.OrdinalIgnoreCase))
        {
            argObj = GmSampleTeam.Red;
            return true;
        }

        if (arg.Equals("blue", StringComparison.OrdinalIgnoreCase))
        {
            argObj = GmSampleTeam.Blue;
            return true;
        }

        argObj = null;
        return false;
    }
}

GM.Register(new GmSampleTeamParser());
GM.Unregister<GmSampleTeam>();
```

`Template` 用来判断这段文本是不是你的类型；`TryParse` 成功时把装箱后的值放到 `argObj`。`Hint` 会在 `help arg` 里显示。

覆盖内置类型时，新模板如果和别的类型重叠，调度器会报告歧义，不会猜一个。

---

## 本仓库里的用例

`Assets/Scripts/Runtime/GM` 提供这些示例命令。本仓库在 `TestLuncher.Awake` 里调用 `GM.Initialize()` 和 `GMSampleRegister.Register()`。

| 命令 | 示例 |
| --- | --- |
| `cube` | `cube (1,9,0) (0,89,0)` |
| `add` | `add 2 40` |
| `echo` | `echo "hello world"` |
| `team` | `team red` |

`team` 使用上面的自定义 `GmSampleTeam` 解析器，用来演示扩展类型，不覆盖内置解析器。

---

## 常见问题

**Q: Play 之后没有 GM 按钮？**  
A: 确认启动流程调用了 `GM.Initialize()`。

**Q: 参数一直解析失败？**  
A: 输入 `help arg` 看写法。字符串必须是英文双引号；整数向量是 `i(1,2)` 这种形式，不是 `(1,2)`。

**Q: `add 1.5 2` 不计算？**  
A: `1.5` 会解析成 `float`，而示例命令 `add` 只要两个 `int`。无后缀整数才是 `int`，小数请带类型自己约定，或在命令里接受 `float`。

**Q: 命令执行了但 GM 页是空的？**  
A: 输出请用 `GM.Print`。`Debug.Log` 在 Console 页。

**Q: 怎么让发布包里不出现 GM 窗口？**  
A: 发布用的启动流程不要调用 `GM.Initialize()`。不调用就不会创建窗口，也不会订阅 `Debug` 日志。

---

## 许可证

MIT。
