# FSM 模块分析与优化建议

## 1. 文档目的

本文针对当前 `FSM` 模块进行静态分析，目标是：

- 梳理模块结构与职责；
- 识别当前实现中已经存在或高概率会暴露的问题；
- 为后续正式修复提供优先级和改动方向。

本次分析主要覆盖以下文件：

- `Assets/LFramework/Runtime/Component/Fsm/FsmComponent.cs`
- `Assets/LFramework/Runtime/Component/Fsm/IFsmManager.cs`
- `Assets/LFramework/Runtime/Component/Fsm/IFsm.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmBase.cs`
- `Assets/LFramework/Runtime/Component/Fsm/Fsm.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmState.cs`
- `Assets/LFramework/Runtime/Component/Procedure/ProcedureBase.cs`
- `Assets/LFramework/Runtime/Component/Procedure/ProcedureComponent.cs`

---

## 2. 当前模块定位

`FSM` 模块是 LFramework 的通用有限状态机系统，主要负责：

- 创建和销毁状态机实例；
- 管理状态集合；
- 驱动当前状态的 `OnUpdate`；
- 处理状态切换；
- 为状态机提供临时数据存取能力；
- 作为 `Procedure` 模块的底层运行机制。

从结构上看，当前设计是：

```text
FsmComponent（模块入口 / 管理器）
    ├── IFsmManager（对外接口）
    └── Fsm<T>（具体状态机实例）
            ├── FsmBase（非泛型基类）
            ├── IFsm<T>（对外运行时接口）
            └── FsmState<T>（状态基类）
```

---

## 3. 当前模块结构

### 3.1 FsmComponent

职责：

- 作为框架模块接入 `LFrameworkEntry`；
- 维护所有已创建的 FSM；
- 在 `OnUpdate()` 中统一驱动所有 FSM；
- 提供 `CreateFsm / GetFsm / DestroyFsm` 等管理能力。

### 3.2 IFsmManager

职责：

- 对外定义 FSM 管理器接口；
- 统一暴露创建、查询、销毁 FSM 的能力；
- 允许通过 `ownerType + name` 组合区分不同 FSM。

### 3.3 FsmBase

职责：

- 作为所有 FSM 的非泛型抽象基类；
- 提供通用信息：
  - `Name`
  - `FullName`
  - `OwnerType`
  - `CurrentStateName`
  - `CurrentStateTime`
  - `IsRunning`
  - `IsDestroyed`
- 定义内部生命周期：
  - `OnUpdate(...)`
  - `Shutdown()`

### 3.4 IFsm<T>

职责：

- 作为业务代码和状态类可见的运行时接口；
- 对外暴露：
  - 启动状态机；
  - 查询状态；
  - 获取全部状态；
  - FSM 数据读写；
  - 当前状态/持有者读取。

### 3.5 Fsm<T>

职责：

- FSM 的真正实现；
- 持有状态字典、当前状态、临时数据字典；
- 执行状态初始化、进入、更新、离开、销毁；
- 通过 `ReferencePool` 复用自身实例。

### 3.6 FsmState<T>

职责：

- 作为所有状态的继承基类；
- 定义状态生命周期：
  - `OnInit`
  - `OnEnter`
  - `OnUpdate`
  - `OnLeave`
  - `OnDestroy`
- 为子类提供受保护的 `ChangeState(...)` 切换入口。

### 3.7 Procedure 模块与 FSM 的关系

`ProcedureBase` 直接继承 `FsmState<IProcedureManager>`，`ProcedureComponent` 则通过 `IFsmManager.CreateFsm(...)` 创建流程状态机。

这说明：

- `Procedure` 模块本质上是 `FSM` 模块的一层业务封装；
- `FSM` 的稳定性会直接影响流程系统。

---

## 4. 当前模块优点

### 4.1 抽象层次清晰

当前把：

- 管理器层；
- 状态机实例层；
- 状态层；
- 非泛型公共基类层

拆分得比较明确，整体结构易理解。

### 4.2 泛型约束合理

`Fsm<T>` 与 `FsmState<T>` 通过统一的 `T` 约束，把“状态机持有者类型”显式化，业务代码使用时比较清晰。

### 4.3 Procedure 已成功复用该模块

说明当前 FSM 模块具备通用性，不只是某个单点功能，而是框架级基础设施。

### 4.4 管理器更新逻辑考虑了遍历期销毁问题

`FsmComponent.OnUpdate()` 先复制到 `_tempFsmList` 再遍历，已经避免了更新期间直接修改 `_fsmDict` 的典型问题。

---

## 5. 当前主要问题与修复建议

以下按照优先级排序。

## 5.1 高优先级问题

### 5.1.1 `Fsm<T>.Create(...)` 在构建失败时会泄漏池化实例

位置：

- `Fsm.cs`
- `Create(string name, T owner, params FsmState<T>[] states)`
- `Create(string name, T owner, List<FsmState<T>> states)`

现状：

- 代码先通过 `ReferencePool.Acquire<Fsm<T>>()` 取出一个 FSM 实例；
- 然后再逐个检查 `state == null`、重复状态、执行 `state.OnInit(fsm)`；
- 一旦中途抛异常，当前 `fsm` 不会被清理，也不会归还对象池。

影响：

- 可能把部分初始化过的状态残留在池对象中；
- 会造成对象池污染或状态泄漏；
- 失败路径越复杂，后续问题越难定位。

建议：

- 在 `Create(...)` 中对构建过程做异常保护；
- 一旦任一状态校验或 `OnInit` 失败，应：
  1. 清理已加入的状态；
  2. 重置内部字段；
  3. 归还 `fsm` 到 `ReferencePool`；
  4. 再重新抛出异常。

---

### 5.1.2 状态切换在 `OnEnter` 抛异常时不是原子操作

位置：

- `Fsm.cs`
- `ChangeState(Type stateType)`

现状：

- 当前流程是：
  
  1. 旧状态 `OnLeave(this, false)`；
  2. `_currentStateTime = 0f`；
  3. `_currentState = state`；
  4. 新状态 `OnEnter(this)`。

- 如果新状态 `OnEnter` 抛异常，FSM 已经：
  
  - 离开旧状态；
  - 切换了当前状态引用；
  - 但新状态没有完成进入。

影响：

- FSM 会落入“半切换”状态；
- 当前状态看起来已经变成新状态，但初始化并不完整；
- 后续 `OnUpdate()` 可能在错误状态上继续执行。

建议：

- 切换时保留旧状态引用；
- 对 `OnEnter` 加异常保护；
- 失败后至少应二选一：
  - 回滚到旧状态；
  - 或明确将 FSM 标记为不可运行并抛出框架异常。

---

### 5.1.3 状态基类中的 `ChangeState(...)` 存在错误类型异常泄漏

位置：

- `FsmState.cs`
- `ChangeState<TState>(IFsm<T> fsm)`
- `ChangeState(IFsm<T> fsm, Type stateType)`

现状：

- 当前实现直接使用 `(Fsm<T>)fsm` 强制转换；
- 如果调用方传入的只是 `IFsm<T>`，但不是 `Fsm<T>` 的具体实现，会直接抛出 `InvalidCastException`。

影响：

- 异常类型不统一；
- 与框架内部一贯使用 `LFrameworkException` 的约定不一致；
- 错误信息也不够可控。

建议：

- 使用 `as Fsm<T>` 或模式匹配进行安全转换；
- 当转换失败时，主动抛出清晰的 `LFrameworkException`。

---

## 5.2 中优先级问题

### 5.2.1 `Clear()` / `Shutdown()` 异常安全不足

位置：

- `Fsm.cs`
- `Clear()`
- `Shutdown()`

现状：

- `Clear()` 会依次调用：
  
  - 当前状态的 `OnLeave(this, true)`；
  - 所有状态的 `OnDestroy(this)`；
  - 释放数据字典中的 `Variable`；
  - 重置内部字段。

- 但如果任何一个状态回调抛异常，后续清理将直接中断。

影响：

- 当前 FSM 可能只清理了一半；
- `_isDestroyed` 可能没有被正确置回；
- 状态字典、数据字典、Owner 等残留，后续再次复用时有风险。

建议：

- 至少保证内部字段重置放到 `finally`；
- 状态回调异常可以记录后继续清理；
- 不要让单个状态的异常破坏整个 FSM 销毁流程。

---

### 5.2.2 FSM 数据对象的所有权约束不清晰

位置：

- `Fsm.cs`
- `SetData(...)`
- `RemoveData(...)`
- `Clear()`

现状：

- FSM 在替换、移除、清理数据时会自动 `ReferencePool.Release(oldData)`；
- 但 `SetData` 的签名允许传入任意 `Variable` 对象，并没有在接口层明确“必须来自对象池”。

影响：

- 如果调用方传入的是手动 `new` 出来的 `Variable`，则存在错误回收风险；
- 模块行为依赖隐式约定，不够稳健。

建议：

- 二选一：
  - 明确约束 FSM 只接受来自 `ReferencePool` 的 `Variable`；
  - 或取消 FSM 对外部传入数据的自动释放。

说明：

- 当前项目里的 `VarInt32 / VarString / VarBoolean` 等类型普遍通过隐式转换从对象池获取实例，因此短期内不一定立刻爆炸；
- 但 API 语义仍然偏模糊，建议在修复阶段一并明确。

---

### 5.2.3 构造默认名 FSM 的查询/销毁 API 易误用

位置：

- `IFsmManager.cs`
- `FsmComponent.cs`

现状：

- 管理器支持通过 `ownerType + name` 创建多个 FSM；
- 但同时又提供 `GetFsm<T>()`、`DestroyFsm<T>()` 这类“未传 name”的 API。

影响：

- 一旦同一 `ownerType` 下并存多个 FSM，未传 name 的接口只会操作空名字的 FSM；
- 新接手代码的人容易误以为“按类型唯一”。

建议：

- 若框架设计允许同一类型多个 FSM，建议文档明确“生产代码应优先使用带 name 的重载”；
- 或在后续收敛接口语义，减少歧义。

---

## 5.3 低优先级问题 / 结构观察

### 5.3.1 `Fsm<T>` 的池化策略对失败路径要求很高

现状：

- 当前 FSM 实例不是普通 `new`，而是 `ReferencePool.Acquire<Fsm<T>>()`；
- 这本身没有问题，但意味着所有异常路径都必须非常谨慎。

观察：

- 当前正常路径的 `Clear()` 逻辑是完整的；
- 但失败路径保护不足，导致池化收益被风险抵消。

建议：

- 后续修复时优先补齐“构建失败 / 切换失败 / 销毁失败”的兜底逻辑；
- 不建议在未补齐异常安全前继续扩展 FSM 功能。

---

## 6. 建议的修复顺序

建议分两阶段推进。

### 第一阶段：保证状态机正确性

优先建议：

1. 修复 `Create(...)` 的失败清理问题；
2. 修复 `ChangeState(...)` 的半切换问题；
3. 统一 `FsmState.ChangeState(...)` 的异常行为；
4. 提升 `Clear()` / `Shutdown()` 的异常安全。

目标：

- 保证 FSM 在异常路径下仍然可控；
- 降低对象池污染和状态错乱风险。

### 第二阶段：明确 API 约束

建议处理：

1. 明确 `SetData(...)` 的数据所有权；
2. 明确 name-less 管理器接口的使用边界；
3. 补充文档或断言，减少误用空间。

目标：

- 提升可维护性；
- 降低后续业务接入成本。

---

## 7. 推荐修改清单

### 必改建议

- 为 `Fsm<T>.Create(...)` 增加失败回滚；
- 为 `Fsm<T>.ChangeState(...)` 增加异常回滚或失败兜底；
- 为 `FsmState<T>.ChangeState(...)` 增加安全类型校验；
- 为 `Fsm<T>.Clear()` 增加异常安全处理。

### 建议改

- 明确 FSM 数据对象的所有权；
- 强化 `IFsmManager` 未命名接口的使用限制。

### 可延后

- 增加更完善的调试输出；
- 为 FSM 模块补充运行时状态可视化信息。

---

## 8. 总结

当前 `FSM` 模块的总体设计是成立的，结构也比较清晰。  
它当前最需要修复的不是“功能缺失”，而是异常路径和对象池路径上的稳定性问题，尤其是：

- 创建失败后的清理；
- 状态切换失败后的回滚；
- 销毁流程的异常安全；
- 状态数据的所有权约束。

在你阅读并确认后，后续修复建议优先围绕“正确性优先、API 约束次之”的顺序展开。
