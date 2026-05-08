# FSM

## GameLogic 推荐用法

- GameLogic/业务代码中优先通过 `GameEntry.Fsm` 使用有限状态机管理器；仅在框架集成代码或 `GameEntry` 尚未初始化时，才直接使用 `LFrameworkEntry.GetModule<IFsmManager>()`。
- 定义状态时继承 `FsmState<TOwner>`，并按需重写 `OnInit()`, `OnEnter()`, `OnUpdate()`, `OnLeave()`, `OnDestroy()`。
- 通过 `GameEntry.Fsm.CreateFsm(owner, states)` 或 `CreateFsm(name, owner, states)` 创建 FSM；创建后调用 `fsm.Start<TState>()` 或 `fsm.Start(stateType)` 启动入口状态。
- 状态切换应在 `FsmState<TOwner>` 内部调用 `ChangeState<TState>(fsm)` 或 `ChangeState(fsm, stateType)`，不要在外部绕过状态生命周期切换当前状态。
- 跨状态共享数据时使用 `fsm.SetData(name, variable)`, `fsm.GetData<TData>(name)`, `fsm.RemoveData(name)`；数据类型必须继承 `Variable`。
- FSM 不再使用时调用 `GameEntry.Fsm.DestroyFsm(...)` 销毁，确保当前状态收到 `OnLeave(..., true)`，所有状态收到 `OnDestroy()`，并释放 FSM 数据。

## 注意事项

- `FsmComponent` 注册为 `IFsmManager`，负责按持有者类型和名称管理 FSM，并在模块轮询中更新所有未销毁的 FSM。
- `Fsm<T>` 为内部实现类并通过 `ReferencePool` 复用；业务代码只应持有 `IFsm<T>` 和 `FsmState<T>`。
- `CreateFsm` 要求 `owner` 非空、状态集合非空，且同一个 FSM 内不能存在重复状态类型；同一持有者类型和名称下重复创建会抛出异常。
- `Start` 只能调用一次；FSM 已运行时再次启动会抛出异常。运行后切换状态使用状态内部的 `ChangeState`。
- `SetData` 会释放同名旧 `Variable`，`RemoveData` 和 FSM 销毁也会释放已保存的 `Variable`；不要在外部继续持有已交给 FSM 管理的数据对象。
- 在 `OnLeave()` 或 `OnDestroy()` 中停止异步任务、取消事件订阅并释放状态持有资源；`OnLeave(fsm, isShutdown)` 的 `isShutdown` 可用于区分普通切换和 FSM 销毁。
- `ProcedureComponent` 基于 FSM 实现流程状态机，持有者类型为 `IProcedureManager`；普通游戏流程优先使用 `GameEntry.Procedure`，不要直接操作 Procedure 内部 FSM。

## IFsmManager API 速查

仅在框架集成代码、通用 FSM 业务或封装上层流程时优先考虑直接使用 `IFsmManager`。

- 数量：`Count` 返回当前已注册 FSM 数量。
- 存在检查：`HasFsm<T>()`, `HasFsm(Type ownerType)`, `HasFsm<T>(string name)`, `HasFsm(Type ownerType, string name)`。
- 获取：`GetFsm<T>()`, `GetFsm(Type ownerType)`, `GetFsm<T>(string name)`, `GetFsm(Type ownerType, string name)`；未找到时返回 `null`。
- 获取全部：`GetAllFsms()` 返回数组；`GetAllFsms(results)` 会先清空传入列表再写入结果。
- 创建：`CreateFsm<T>(owner, params FsmState<T>[] states)`, `CreateFsm<T>(name, owner, params FsmState<T>[] states)`, `CreateFsm<T>(owner, List<FsmState<T>> states)`, `CreateFsm<T>(name, owner, List<FsmState<T>> states)`。
- 销毁：`DestroyFsm<T>()`, `DestroyFsm(Type ownerType)`, `DestroyFsm<T>(string name)`, `DestroyFsm(Type ownerType, string name)`, `DestroyFsm<T>(IFsm<T> fsm)`, `DestroyFsm(FsmBase fsm)`。
- `IFsm<T>` 状态控制：`Start<TState>()`, `Start(Type stateType)`, `HasState<TState>()`, `HasState(Type stateType)`, `GetState<TState>()`, `GetState(Type stateType)`, `GetAllStates()`, `GetAllStates(results)`。
- `IFsm<T>` 运行信息：`Name`, `FullName`, `Owner`, `FsmStateCount`, `IsRunning`, `IsDestroyed`, `CurrentState`, `CurrentStateTime`。
- `IFsm<T>` 数据：`HasData(name)`, `GetData<TData>(name)`, `GetData(name)`, `SetData<TData>(name, data)`, `SetData(name, data)`, `RemoveData(name)`。

## 源码路径

- `Assets/LFramework/Runtime/Component/Fsm/IFsmManager.cs`
- `Assets/LFramework/Runtime/Component/Fsm/IFsm.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmState.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmComponent.cs`
- `Assets/LFramework/Runtime/Component/Fsm/Fsm.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmBase.cs`
