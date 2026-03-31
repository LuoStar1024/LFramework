# Base 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Base` 模块的：

- 核心类型；
- 对外 API；
- 在框架中的接入方式；
- 核心生命周期；
- 是否存在需要继承理解的基类。

---

## 2. 模块定位

`Base` 模块是 LFramework 中的基础运行控制模块，主要负责全局运行参数与时间流速控制，包括：

- 帧率；
- 游戏速度；
- 暂停与恢复；
- 后台运行；
- 屏幕休眠；
- 屏幕 DPI 初始化。

从架构定位上，它更接近“全局运行环境控制器”。

---

## 3. 核心类型

## 3.1 BaseComponent

文件：

- `Assets/LFramework/Runtime/Component/Base/BaseComponent.cs`

定义：

```csharp
public sealed class BaseComponent : MonoBehaviour, ILFrameworkModule, IBaseManager
```

职责：

- 作为 `Base` 模块的运行时实现；
- 通过 `Awake()` 注册到 `LFrameworkEntry`；
- 在 `OnInit()` 中应用基础运行参数；
- 对外提供暂停、恢复、重置正常速度等基础能力。

说明：

- `sealed`，当前设计不是给业务层继承扩展用的；
- 业务通常通过接口 `IBaseManager` 或 `GameEntry.Base` 使用。

---

## 3.2 IBaseManager

文件：

- `Assets/LFramework/Runtime/Component/Base/IBaseManager.cs`

职责：

- 定义 `Base` 模块对外暴露的公共接口；
- 隔离业务层对具体 `MonoBehaviour` 实现的直接依赖。

---

## 3.3 BaseComponentInspector

文件：

- `Assets/LFramework/Editor/Inspector/BaseComponentInspector.cs`

职责：

- 在 Unity Inspector 中编辑 `BaseComponent` 的参数；
- 在运行时允许直接调整基础控制项。

---

## 3.4 GameEntry.Base

文件：

- `Assets/GameScripts/GameLogic/Base/GameEntry.Builtin.cs`

职责：

- 作为游戏侧访问 `Base` 模块的统一入口；
- 在 `InitBuiltinComponents()` 中通过 `LFrameworkEntry.GetModule<IBaseManager>()` 获取并缓存。

常见调用方式：

```csharp
GameEntry.Base.ResetNormalGameSpeed();
```

---

## 4. 核心 API

`Base` 模块的核心 API 来自 `IBaseManager`。

## 4.1 属性

### `int FrameRate`

作用：

- 获取或设置游戏目标帧率。

实现行为：

- 设置时会同步写入 `Application.targetFrameRate`。

---

### `float GameSpeed`

作用：

- 获取或设置游戏速度。

实现行为：

- 设置时会同步写入 `Time.timeScale`；
- 当前实现对负值做保护，负数会被改为 `0f`。

---

### `bool IsGamePaused`

作用：

- 获取当前游戏是否被视为暂停。

当前语义：

- `gameSpeed <= 0f` 时返回 `true`。

---

### `bool IsNormalGameSpeed`

作用：

- 判断当前是否为正常游戏速度。

当前语义：

- `gameSpeed == 1f` 时返回 `true`。

---

### `bool RunInBackground`

作用：

- 获取或设置游戏是否允许后台运行。

实现行为：

- 设置时同步写入 `Application.runInBackground`。

---

### `bool NeverSleep`

作用：

- 获取或设置设备是否禁止休眠。

实现行为：

- `true` 时写入 `SleepTimeout.NeverSleep`；
- `false` 时写入 `SleepTimeout.SystemSetting`。

---

## 4.2 方法

### `void PauseGame()`

作用：

- 暂停游戏。

实现行为：

1. 如果当前已经暂停，则直接返回；
2. 记录暂停前速度到 `_gameSpeedBeforePause`；
3. 设置 `GameSpeed = 0f`。

---

### `void ResumeGame()`

作用：

- 恢复游戏。

实现行为：

1. 如果当前不是暂停状态，则直接返回；
2. 恢复 `GameSpeed = _gameSpeedBeforePause`。

---

### `void ResetNormalGameSpeed()`

作用：

- 把游戏速度恢复为正常值 `1f`。

实现行为：

- 如果当前已是正常速度，则不做处理；
- 否则设置 `GameSpeed = 1f`。

---

## 5. 当前模块调用链

## 5.1 注册与获取

```text
BaseComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IBaseManager>(this)
    ↓
LFrameworkEntry 内部保存模块
    ↓
GameEntry.Builtin.InitBuiltinComponents()
    ↓
Base = LFrameworkEntry.GetModule<IBaseManager>()
```

---

## 5.2 运行驱动

```text
RootComponent.Update()
    ↓
LFrameworkEntry.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime)
    ↓
各模块 OnUpdate(...)
```

说明：

- `BaseComponent` 虽然参与模块轮询体系，但当前 `OnUpdate()` 为空。

---

## 5.3 业务使用

当前明确业务使用点之一：

文件：

- `Assets/GameScripts/GameLogic/Procedure/ProcedureChangeScene.cs`

示例：

```csharp
GameEntry.Base.ResetNormalGameSpeed();
```

该调用用于切场景时恢复标准游戏速度，避免上一流程遗留慢速/暂停状态。

---

## 6. 生命周期

`BaseComponent` 同时具有 Unity 生命周期和框架模块生命周期两层含义。

## 6.1 Unity 生命周期

### `Awake()`

作用：

- 注册自身到 `LFrameworkEntry`：

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IBaseManager>(this);
}
```

说明：

- 这是模块进入框架体系的入口；
- 注册完成后，`LFrameworkEntry` 会立即调用模块的 `OnInit()`。

---

## 6.2 ILFrameworkModule 生命周期

### `Priority`

作用：

- 声明模块执行优先级。

当前实现：

```csharp
public int Priority
{
    get { return 0; }
}
```

说明：

- 优先级越高，越先轮询；
- 关闭时越后执行；
- 当前 Base 使用默认优先级 `0`。

---

### `OnInit()`

作用：

- 初始化基础运行参数。

当前做的事情：

1. 初始化 `Utility.Converter.ScreenDpi`；
2. 如果 `Screen.dpi <= 0`，则使用默认值 `96`；
3. 设置 `Application.targetFrameRate = frameRate`；
4. 设置 `Time.timeScale = gameSpeed`；
5. 设置 `Application.runInBackground = runInBackground`；
6. 设置 `Screen.sleepTimeout`。

说明：

- `OnInit()` 是 Base 模块当前最核心的生命周期节点；
- 模块绝大部分有效逻辑都在这里。

---

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 参与框架模块轮询。

当前实现：

- 空实现。

说明：

- 当前 Base 模块没有逐帧逻辑。

---

### `Shutdown()`

作用：

- 模块关闭和清理。

当前实现：

- 空实现。

说明：

- 当前 Base 模块不持有额外资源或事件订阅，因此暂未做清理；
- 如果后续扩展功能，`Shutdown()` 会成为重要生命周期点。

---

## 7. 是否存在需要继承的基类

## 7.1 Base 模块自身是否需要继承

结论：当前 `Base` 模块本身不以继承扩展为主。

原因：

- `BaseComponent` 是 `sealed`；
- 当前设计偏向：
  - `IBaseManager` 接口访问；
  - `GameEntry.Base` 统一入口；
  - 不鼓励业务层直接继承运行时组件。

因此：

- 业务接入应优先通过 `GameEntry.Base`；
- 不建议以继承 `BaseComponent` 的方式扩展。

---

## 7.2 如果要理解接入方式，需要关注哪些基类/接口

虽然 `BaseComponent` 不用于继承，但要理解它在框架中的角色，需要关注两个核心基类/接口语义。

### `MonoBehaviour`

含义：

- 使 `BaseComponent` 成为 Unity 场景组件；
- 可以参与 `Awake()` 等 Unity 生命周期。

当前最关键的 Unity 生命周期：

- `Awake()`：完成模块注册。

### `ILFrameworkModule`

含义：

- 使 `BaseComponent` 接入框架模块系统。

核心生命周期：

| 生命周期         | 作用      |
| ------------ | ------- |
| `Priority`   | 声明模块优先级 |
| `OnInit()`   | 模块初始化   |
| `OnUpdate()` | 模块轮询    |
| `Shutdown()` | 模块关闭清理  |

这就是当前 Base 模块真正需要理解的“继承/实现体系”。

---

## 8. Base 模块使用注意事项

### 8.1 前提是模块必须已注册

调用 `GameEntry.Base` 前，必须保证场景中已存在并初始化 `BaseComponent`。

否则：

- `LFrameworkEntry.GetModule<IBaseManager>()` 会直接抛异常。

---

### 8.2 `PauseGame()` 与 `GameSpeed = 0` 当前语义一致

这意味着：

- 手动把 `GameSpeed` 设为 `0`，也会被视为暂停；
- 恢复逻辑会依赖 `_gameSpeedBeforePause`。

使用时应避免业务层在不同地方混乱地直接写 `GameSpeed` 和 `PauseGame()`。

---

### 8.3 Inspector 约束不等于运行时约束

当前 Inspector 对参数范围做了可视化限制，但运行时属性层的约束还不完全一致。

因此：

- 外部代码仍应谨慎设置极端值。

---

## 9. 总结

当前 Base 模块的核心理解可以概括为：

- 一个 `sealed` 的基础运行控制组件；
- 通过 `IBaseManager` 对外暴露能力；
- 通过 `Awake() + ILFrameworkModule.OnInit()` 接入框架；
- 主要负责全局运行参数与时间流速控制。

它没有复杂继承体系，真正需要掌握的是：

- `IBaseManager` 的核心 API；
- `ILFrameworkModule` 的生命周期；
- `GameEntry.Base` 的访问方式；
- `Pause / Resume / GameSpeed` 的当前语义关系。
