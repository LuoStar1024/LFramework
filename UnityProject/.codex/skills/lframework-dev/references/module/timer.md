# Timer

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Timer`，不要直接获取或依赖 `TimerComponent`。
- `GameEntry.Timer.AddTimer(time, callback, isUnscaled = false, repeatCount = 1)`：添加定时器，返回定时器 ID；默认使用缩放时间，默认只回调一次。
- `GameEntry.Timer.AddTimer(time, callbackArgs, isUnscaled = false, repeatCount = 1, params object[] args)`：添加带参数回调的定时器，可用于避免闭包捕获。
- `GameEntry.Timer.RemoveTimer(timerId)`：移除指定定时器。拥有生命周期的对象创建定时器后，应保存 ID 并在释放、关闭或销毁时移除。
- `GameEntry.Timer.StopTimer(timerId)`：暂停指定定时器，不会移除它。
- `GameEntry.Timer.ResumeTimer(timerId)`：恢复已暂停的定时器。
- `GameEntry.Timer.ResetTimer(timerId)`：把定时器恢复到初始剩余时间，并设置为运行状态。
- `GameEntry.Timer.IsRunningTimer(timerId)`：查询定时器是否存在、未标记移除且正在运行。
- UI、暂停菜单或不受 `Time.timeScale` 影响的逻辑使用 `isUnscaled: true`；普通玩法逻辑使用默认缩放时间。

## 注意事项

- `TimerComponent` 在 `Awake()` 中注册 `ITimerManager`，在 `OnUpdate()` 中分别推进缩放时间定时器和非缩放时间定时器，`Shutdown()` 时移除全部定时器。
- `time` 必须大于 `0`，`callback` 不能为 `null`；否则会抛出 `LFrameworkException`。
- `repeatCount` 表示回调次数；`repeatCount <= 0` 表示无限重复，必须由拥有者主动 `RemoveTimer`。
- `StopTimer` 只是暂停计时，`RemoveTimer` 才是移除并等待框架释放内部定时器对象。
- 定时器内部对象通过 `ReferencePool` 复用；业务侧不要持有内部 `Timer` 对象，只保存 `AddTimer` 返回的 ID。
- 回调中避免捕获已经关闭的 UI、已释放的资源容器或已销毁的 `GameObject`；生命周期结束时优先按 ID 移除自身创建的定时器。
- `RemoveAllTimer()` 会移除所有缩放和非缩放定时器，除非正在关闭整个子系统，否则优先使用 `RemoveTimer(timerId)`。
- 新增定时器会先进入缓存列表，并在下一次 `OnUpdate()` 插入对应计时列表；不要依赖添加后立刻出现在调试列表中。
- `GetTimersInfo()` 和 `GetUnscaledTimersInfo()` 是 `TimerComponent` 的公开调试方法，主要供 `TimerComponentInspector` 展示运行时状态，不属于 `ITimerManager` 接口。

## ITimerManager API 速查

仅在框架集成代码或 `GameEntry.Timer` 已初始化后的业务代码中使用 `ITimerManager`。

- 计数：`TimerCount`, `UnscaledTimerCount`。
- 添加：`AddTimer(float time, Action callback, bool isUnscaled = false, int repeatCount = 1)`。
- 添加带参数回调：`AddTimer(float time, Action<object[]> callback, bool isUnscaled = false, int repeatCount = 1, params object[] args)`。
- 暂停/恢复：`StopTimer(timerId)`, `ResumeTimer(timerId)`。
- 查询：`IsRunningTimer(timerId)`。
- 重置：`ResetTimer(timerId)`。
- 移除：`RemoveTimer(timerId)`, `RemoveAllTimer()`。

## 源码路径

- `Assets/LFramework/Runtime/Component/Timer/ITimerManager.cs`
- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.cs`
- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.Timer.cs`
- `Assets/LFramework/Runtime/Component/Timer/TimerInfo.cs`
