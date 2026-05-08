# 事件

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Event`，不要直接访问 `EventComponent` 或 Core `EventPool`。
- 有明确生命周期的对象优先使用 `EventContainer.Create(owner)` 管理订阅；`EventContainer` 提供无参到 8 个参数的 `Subscribe(...)`、`Unsubscribe(...)` 重载，结束时通过 `ReferencePool.Release(eventContainer)` 释放，触发 `Clear()` 和 `UnsubscribeAll()`。
- `UguiForm` 已内置事件容器封装，UI 窗口中优先使用 `Subscribe(...)`、`Unsubscribe(...)`、`UnsubscribeAll()`，关闭或回收时会随窗口生命周期清理。
- Procedure、Manager 等非 UI 对象如果直接调用 `GameEntry.Event.Subscribe(...)`，必须在对应的离开、关闭或销毁生命周期中配对调用 `Unsubscribe(...)`。
- 事件组用于封装业务事件入口：在事件组构造函数中调用 `GameEntry.Event.RegisterGroup(this)`，业务侧通过 `GameEntry.Event.FireGroup<TGroup>()` 获取事件组并触发语义化方法。
- 事件编号使用 `EventRuntimeId.ToRuntimeId(string)` 生成，建议放在事件组或事件定义类中集中声明，避免散落硬编码整数。
- 常规事件派发使用 `GameEntry.Event.Fire(id, ...)`；需要立即同步回调时才使用 `FireNow(id, ...)`。

## 注意事项

- `EventComponent.Awake()` 通过 `LFrameworkEntry.RegisterModule<IEventManager>(this)` 注册模块，业务代码不要用具体组件类型获取事件模块。
- `EventComponent.OnInit()` 创建的事件池模式为 `AllowNoHandler | AllowMultiHandler`：允许事件没有处理函数，也允许同一事件存在多个处理函数，但不允许重复注册同一个处理函数。
- `Fire(...)` 会把事件节点放入队列，事件在后续 `OnUpdate()` 中分发；该路径加锁入队，可用于非主线程触发后回到主线程回调。
- `FireNow(...)` 会立即分发事件，不经过队列，也不是线程安全操作；只在确定当前调用栈需要同步响应时使用。
- 订阅和取消订阅的事件 id、委托实例、参数类型必须一致；`Unsubscribe(...)` 找不到指定处理函数时会抛出异常。
- `EventContainer.Unsubscribe(id, handler)` 只能取消该容器记录过的订阅；取消未记录的处理函数会抛出异常。
- `EventContainer.Clear()` 会执行 `UnsubscribeAll()`、清空内部处理函数表并重置 `Owner`，不要在释放后继续复用旧引用。
- 释放事件容器应早于 `GameEntry.Event` 关闭；框架关闭阶段不要再依赖容器批量取消订阅。
- 如需处理未订阅事件，可使用 `SetDefaultHandler(Action<int>)` 设置默认处理函数；当前事件池允许无处理函数，默认情况下不会因为无订阅者抛错。

## IEventManager API 速查

仅在框架集成代码、`GameEntry.Event` facade 或需要精确控制事件行为的 GameLogic 代码中直接使用 `IEventManager`。

- 状态查询：`EventHandlerCount` 获取已注册处理函数总数，`EventCount` 获取待分发事件数量，`Count(id)` 获取指定事件处理函数数量，`Check(id, handler)` 检查指定处理函数是否已订阅。
- 订阅：`Subscribe(id, Action...)` 支持无参到 8 个参数的泛型 `Action` 重载，也支持 `Subscribe(id, Delegate)`。
- 取消订阅：`Unsubscribe(id, Action...)` 支持无参到 8 个参数的泛型 `Action` 重载，也支持 `Unsubscribe(id, Delegate)`。
- 延迟派发：`Fire(id, ...)` 支持无参到 8 个参数的重载，事件进入队列并在 `EventComponent.OnUpdate()` 中分发。
- 立即派发：`FireNow(id, ...)` 支持无参到 8 个参数的重载，事件会在当前调用中立刻分发。
- 默认处理：`SetDefaultHandler(handler)` 设置未命中事件处理函数时的默认回调。
- 事件组：`RegisterGroup<T>(group)` 注册事件组实例，`FireGroup<T>()` 获取已注册事件组；未注册时会抛出异常。

## 源码路径

- `Assets/GameScripts/GameLogic/Component/Event/EventContainer.cs`
- `Assets/GameScripts/GameLogic/Event/EventGroupLogic.cs`
- `Assets/GameScripts/GameLogic/Event/EventGroupUI.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UguiForm.cs`
- `Assets/LFramework/Runtime/Component/Event/IEventManager.cs`
- `Assets/LFramework/Runtime/Component/Event/EventComponent.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventPool.Event.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventPoolMode.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventRuntimeId.cs`
