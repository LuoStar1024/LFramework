# Timer 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Timer` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 需要理解的内部基类或辅助类型；
- 模块核心生命周期。

---

## 2. 模块定位

`Timer` 模块是 LFramework 的轻量定时器系统，用于：

- 延时执行回调；
- 重复执行回调；
- 区分受时间缩放和不受时间缩放两类计时；
- 提供运行时定时器调试信息。

它在项目中的定位，是“框架级轻量时间调度模块”。

---

## 3. 核心类型

## 3.1 TimerComponent

文件：

- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.cs`
- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.Timer.cs`

定义：

```csharp
public sealed partial class TimerComponent : MonoBehaviour, ILFrameworkModule, ITimerManager
```

职责：

- 作为 Timer 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 维护普通定时器与非缩放定时器；
- 在 `OnUpdate()` 中推进时间并触发回调；
- 提供调试信息查询接口。

说明：

- `sealed`，当前模块不通过继承 `TimerComponent` 扩展；
- 业务层通常通过 `GameEntry.Timer` 访问。

---

## 3.2 ITimerManager

文件：

- `Assets/LFramework/Runtime/Component/Timer/ITimerManager.cs`

职责：

- 定义定时器模块的对外统一接口；
- 覆盖：
  - 添加定时器；
  - 暂停/恢复；
  - 查询运行中状态；
  - 重置；
  - 单个移除；
  - 全部移除。

---

## 3.3 Timer

文件：

- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.Timer.cs`

定义：

```csharp
private sealed class Timer : IReference
```

职责：

- 作为 Timer 模块内部的真实运行对象；
- 保存：
  - 定时器 Id
  - 时间间隔
  - 当前剩余时间
  - 重复次数
  - 回调
  - 参数
  - 运行状态
  - 是否待移除
  - 是否不受时间缩放影响

说明：

- 这是内部实现对象，不是业务侧需要继承的基类；
- 通过 `ReferencePool` 进行复用。

---

## 3.4 TimerInfo

文件：

- `Assets/LFramework/Runtime/Component/Timer/TimerInfo.cs`

定义：

```csharp
public struct TimerInfo
```

职责：

- 为 Inspector 提供只读调试快照；
- 包含：
  - Id
  - 类名
  - 方法名
  - 时间间隔
  - 重复次数
  - 当前剩余时间

---

## 3.5 TimerComponentInspector

文件：

- `Assets/LFramework/Editor/Inspector/TimerComponentInspector.cs`

职责：

- 运行时展示：
  - 普通 Timer 数量；
  - 非缩放 Timer 数量；
  - 各定时器的回调来源和当前剩余时间。

---

## 4. 核心 API

## 4.1 基础信息 API

```csharp
int TimerCount { get; }
int UnscaledTimerCount { get; }
```

说明：

- `TimerCount`：当前普通定时器数量；
- `UnscaledTimerCount`：当前非缩放定时器数量。

---

## 4.2 添加定时器 API

### 无参回调

```csharp
int AddTimer(float time, Action callback, bool isUnscaled = false, int repeatCount = 1)
```

### 带参数回调

```csharp
int AddTimer(float time, Action<object[]> callback, bool isUnscaled = false, int repeatCount = 1, params object[] args)
```

### 参数含义

| 参数            | 说明              |
| ------------- | --------------- |
| `time`        | 触发间隔            |
| `callback`    | 回调函数            |
| `isUnscaled`  | 是否使用非缩放时间       |
| `repeatCount` | 调用次数，小于等于 0 为无限 |
| `args`        | 传递给回调的参数数组      |

### 返回值

- 返回一个整型 `timerId`，后续可用于暂停、恢复、重置、移除。

---

## 4.3 控制定时器 API

```csharp
void StopTimer(int timerId)
void ResumeTimer(int timerId)
bool IsRunningTimer(int timerId)
void ResetTimer(int timerId)
void RemoveTimer(int timerId)
void RemoveAllTimer()
```

### API 含义

| API              | 说明            |
| ---------------- | ------------- |
| `StopTimer`      | 暂停指定定时器       |
| `ResumeTimer`    | 恢复指定定时器       |
| `IsRunningTimer` | 判断指定定时器是否正在运行 |
| `ResetTimer`     | 重置指定定时器为初始状态  |
| `RemoveTimer`    | 标记并移除指定定时器    |
| `RemoveAllTimer` | 移除全部定时器       |

---

## 4.4 调试信息 API

虽然不在 `ITimerManager` 接口里，但 `TimerComponent` 还额外提供：

```csharp
public TimerInfo[] GetTimersInfo()
public TimerInfo[] GetUnscaledTimersInfo()
```

作用：

- 返回当前普通 / 非缩放定时器的调试快照；
- 主要供 Inspector 使用。

---

## 5. 关键调用链

## 5.1 模块注册调用链

```text
TimerComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<ITimerManager>(this)
    ↓
TimerComponent.OnInit()
```

---

## 5.2 添加定时器调用链

```text
GameEntry.Timer.AddTimer(...)
    ↓
Timer.Create(...)
    ↓
加入 _cacheAddTimerList
    ↓
下一次 OnUpdate(...)
    ↓
InsertTimer(...)
    ↓
进入 _timerList 或 _unscaledTimerList
```

说明：

- 当前新增定时器不是立刻进入正式列表；
- 而是先进入待插入缓存。

---

## 5.3 普通定时器更新调用链

```text
TimerComponent.OnUpdate(elapseSeconds, realElapseSeconds)
    ↓
UpdateTimer(elapseSeconds)
    ↓
对 _timerList 中运行中的定时器减去 CurTime
    ↓
CurTime <= 0 时触发回调
    ↓
根据 RepeatCount 决定：
    ├── 继续保留
    └── 移除并回收到 ReferencePool
```

---

## 5.4 非缩放定时器更新调用链

```text
TimerComponent.OnUpdate(elapseSeconds, realElapseSeconds)
    ↓
UpdateUnscaledTimer(realElapseSeconds)
    ↓
对 _unscaledTimerList 中运行中的定时器减去 CurTime
    ↓
CurTime <= 0 时触发回调
```

说明：

- 与普通 Timer 逻辑基本一致；
- 区别在于使用 `realElapseSeconds`。

---

## 5.5 坏帧补偿调用链

```text
UpdateTimer / UpdateUnscaledTimer
    ↓
如果某次触发后 CurTime 仍 <= 0
    ↓
LoopCallInBadFrame / LoopCallUnscaledInBadFrame
    ↓
在同一帧继续补触发
```

说明：

- 这是为了处理大帧导致“应触发多次”的情况。

---

## 6. 模块生命周期

## 6.1 TimerComponent 生命周期

### `Awake()`

作用：

- 把当前组件注册为 `ITimerManager` 模块。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<ITimerManager>(this);
}
```

### `OnInit()`

作用：

- 当前实现为空；
- 定时器容器在字段初始化时已经创建。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 先把 `_cacheAddTimerList` 中的新增 Timer 插入正式列表；
- 然后分别更新：
  - 普通 Timer
  - 非缩放 Timer

### `Shutdown()`

作用：

- 清理缓存列表；
- 调用 `RemoveAllTimer()` 清空定时器。

---

## 6.2 Timer 内部生命周期

这是内部运行对象 `Timer` 的生命周期。

### 创建

通过以下两个工厂方法之一创建：

```csharp
Timer.Create(int id, float time, Action callback, bool isUnscaled, int repeatCount)
Timer.Create(int id, float time, Action<object[]> callback, bool isUnscaled, int repeatCount, params object[] args)
```

### 活动阶段

创建后进入：

- `_cacheAddTimerList`
- 然后插入 `_timerList` 或 `_unscaledTimerList`

在活动阶段主要被以下字段驱动：

- `CurTime`
- `RepeatCount`
- `IsRunning`
- `IsNeedRemove`

### 回调触发阶段

当 `CurTime <= 0` 时：

- 执行 `Callback` 或 `CallbackArgs`；
- 然后更新 `RepeatCount` 和下一轮 `CurTime`。

### 移除阶段

当满足移除条件时：

- 从正式列表移除；
- 调用 `ReferencePool.Release(timer)`。

### 清理阶段

`ReferencePool` 回收时会调用：

```csharp
public void Clear()
```

清理内容包括：

- `ID`
- `Time`
- `Callback`
- `CallbackArgs`
- `Args`
- `RepeatCount`
- `CurTime`
- `IsRunning`
- `IsNeedRemove`
- `IsUnscaled`

---

## 7. 是否存在需要继承的基类

结论：当前 Timer 模块没有需要业务继承的基类。

原因：

- `TimerComponent` 是 `sealed`；
- `Timer` 是私有内部类；
- 业务层通过 `ITimerManager` 直接使用即可。

因此：

- 不建议通过继承 Timer 模块扩展功能；
- 更适合通过组合式调用来使用它。

---

## 8. 典型使用方式

## 8.1 添加一次性定时器

```csharp
int timerId = GameEntry.Timer.AddTimer(0.5f, OnDelayComplete);
```

---

## 8.2 添加循环定时器

```csharp
int timerId = GameEntry.Timer.AddTimer(1f, OnTick, false, 5);
```

说明：

- 这里会每 1 秒触发一次，共 5 次。

---

## 8.3 添加无限循环定时器

```csharp
int timerId = GameEntry.Timer.AddTimer(1f, OnTick, false, 0);
```

说明：

- `repeatCount <= 0` 视为无限次。

---

## 8.4 添加带参数定时器

```csharp
GameEntry.Timer.AddTimer(1f, OnDelayWithArgs, false, 1, "A", 123);
```

回调示例：

```csharp
private void OnDelayWithArgs(object[] args)
{
}
```

---

## 8.5 暂停 / 恢复 / 重置 / 移除

```csharp
GameEntry.Timer.StopTimer(timerId);
GameEntry.Timer.ResumeTimer(timerId);
GameEntry.Timer.ResetTimer(timerId);
GameEntry.Timer.RemoveTimer(timerId);
```

---

## 8.6 游戏中的典型使用

当前项目中 `Player` 的死亡延迟结束使用了：

```csharp
GameEntry.Timer.AddTimer(0.5f, OnGameOverDelayComplete);
```

这是当前 Timer 模块的典型落地方式。

---

## 9. 使用注意事项

### 9.1 普通 Timer 和 Unscaled Timer 不同

- 普通 Timer 使用 `elapseSeconds`
- Unscaled Timer 使用 `realElapseSeconds`

也就是说：

- 前者受游戏时间缩放影响；
- 后者不受影响。

---

### 9.2 新增 Timer 当前先进入缓存

所以从实现角度看，它不是“立刻插入正式列表”，而是“下一次 `OnUpdate()` 时正式生效”。

---

### 9.3 `repeatCount <= 0` 表示无限循环

这不是错误输入，而是当前模块定义的语义。

---

### 9.4 调试信息主要来自正式列表

Inspector 当前展示的是运行中定时器的快照，不一定完全等价于所有内部缓存状态。

---

## 10. 总结

当前 `Timer` 模块可以概括为：

- 一个 `sealed` 的定时器组件 `TimerComponent`；
- 一个统一接口 `ITimerManager`；
- 一个内部池化运行对象 `Timer`；
- 一套围绕“添加 -> 缓存插入 -> 每帧更新 -> 触发回调 -> 回收”的轻量定时器机制。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `TimerComponent` 如何区分普通与非缩放 Timer；
2. 新增 Timer 为什么先进入 `_cacheAddTimerList`；
3. 回调触发、重复次数递减和引用池回收是如何串起来的。
