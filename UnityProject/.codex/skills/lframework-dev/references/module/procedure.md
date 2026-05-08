# Procedure

## GameLogic 推荐用法

- GameLogic/业务代码中优先通过 `GameEntry.Procedure` 读取当前流程、查询流程或获取流程实例；仅在 `GameEntry` 尚未初始化的启动阶段，才直接使用 `LFrameworkEntry.GetModule<IProcedureManager>()`。
- 新增游戏流程时继承 `ProcedureBase`，并按需重写 `OnInit()`, `OnEnter()`, `OnUpdate()`, `OnLeave()`, `OnDestroy()`。
- 流程切换应在 `ProcedureBase` 内部调用 `ChangeState<TProcedure>(procedureOwner)` 或 `ChangeState(procedureOwner, procedureType)`，不要从外部直接操作 Procedure 内部 FSM。
- 跨流程传递临时数据时使用 `procedureOwner.SetData(name, variable)`, `GetData<TData>(name)`, `RemoveData(name)`；当前示例通过 `VarString` 传递目标场景名，通过 `VarInt32` 传递 Loading 界面编号。
- 热更侧 `GameEntry.Start()` 会先销毁启动器阶段已有的 `IProcedureManager` FSM，再用 GameLogic 流程集合重新 `Initialize`，随后初始化 `GameEntry` 组件缓存和 `EventHelper`，最后启动 `ProcedureGameLogicLaunch`。
- 普通业务入口优先放在流程生命周期中：进入流程时准备资源、订阅或打开界面，轮询中判断切换条件，离开流程时释放本流程持有的临时状态。

## 注意事项

- `ProcedureComponent` 注册为 `IProcedureManager`，底层通过 `IFsm<IProcedureManager>` 管理所有 `ProcedureBase` 流程状态。
- `ProcedureComponent.Start()` 会读取 Inspector 中的 `availableProcedureTypeNames` 和 `entranceProcedureTypeName`，创建流程实例并启动入口流程；热更侧 `GameEntry.Start()` 会重新初始化流程集合，因此修改启动链路前必须核对实际入口。
- `Initialize(IFsmManager, ProcedureBase[])` 只创建 Procedure FSM，不会自动启动流程；必须随后调用 `StartProcedure<T>()` 或 `StartProcedure(Type)`。
- 访问 `CurrentProcedure`, `CurrentProcedureTime`, `HasProcedure`, `GetProcedure` 或 `StartProcedure` 前，必须保证 `Initialize` 已完成，否则会抛出异常。
- `StartProcedure` 只能用于启动尚未运行的 Procedure FSM；运行后的流程切换使用 `ProcedureBase` 继承自 `FsmState<IProcedureManager>` 的 `ChangeState`。
- 在 `OnLeave()` 中停止异步加载、取消事件订阅、清理界面引用或临时字段；如果资源、事件或 UI 有独立生命周期所有者，应按对应模块规范释放。
- Procedure FSM 数据由 FSM 管理，同名 `SetData`、`RemoveData` 和 FSM 销毁都会释放旧 `Variable`；不要在外部继续持有已交给流程 FSM 的 `Variable` 对象。

## IProcedureManager API 速查

仅在流程系统封装、启动阶段或需要查询当前流程状态时优先考虑直接使用 `IProcedureManager`。

- 当前状态：`CurrentProcedure` 返回当前 `ProcedureBase`；`CurrentProcedureTime` 返回当前流程持续时间。
- 初始化：`Initialize(IFsmManager fsmManager, params ProcedureBase[] procedures)` 创建内部 Procedure FSM。
- 启动流程：`StartProcedure<T>()`, `StartProcedure(Type procedureType)`。
- 存在检查：`HasProcedure<T>()`, `HasProcedure(Type procedureType)`。
- 获取流程实例：`GetProcedure<T>()`, `GetProcedure(Type procedureType)`。
- 流程基类：`ProcedureBase` 继承 `FsmState<IProcedureManager>`，可重写 `OnInit`, `OnEnter`, `OnUpdate`, `OnLeave`, `OnDestroy`。
- 流程切换：在 `ProcedureBase` 内调用 `ChangeState<TProcedure>(procedureOwner)` 或 `ChangeState(procedureOwner, procedureType)`。
- 流程数据：通过生命周期方法参数 `IFsm<IProcedureManager> procedureOwner` 调用 `SetData`, `GetData`, `HasData`, `RemoveData`。

## 源码路径

- `Assets/LFramework/Runtime/Component/Procedure/IProcedureManager.cs`
- `Assets/LFramework/Runtime/Component/Procedure/ProcedureBase.cs`
- `Assets/LFramework/Runtime/Component/Procedure/ProcedureComponent.cs`
