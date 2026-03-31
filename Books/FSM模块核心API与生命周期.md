# FSM 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `FSM` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 状态生命周期；
- 需要继承的基类与继承方式。

---

## 2. 模块定位

`FSM` 模块是 LFramework 的通用有限状态机系统，用于：

- 管理状态集合；
- 启动和驱动状态机；
- 在运行时切换状态；
- 为状态提供共享数据存取；
- 为 `Procedure` 模块等上层功能提供底层支撑。

它在项目中的定位，是“框架级状态流转基础设施”。

---

## 3. 核心类型

## 3.1 FsmComponent

文件：

- `Assets/LFramework/Runtime/Component/Fsm/FsmComponent.cs`

定义：

```csharp
public sealed class FsmComponent : MonoBehaviour, ILFrameworkModule, IFsmManager
```

职责：

- 作为 FSM 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 对外暴露 `IFsmManager`；
- 维护所有已创建的状态机；
- 在 `OnUpdate()` 中驱动所有 FSM 的运行。

说明：

- `sealed`，当前模块不以继承 `FsmComponent` 扩展为主；
- 业务层通常通过 `GameEntry` 或模块注册系统间接访问它。

---

## 3.2 IFsmManager

文件：

- `Assets/LFramework/Runtime/Component/Fsm/IFsmManager.cs`

职责：

- 定义 FSM 管理器对外公开的统一接口；
- 负责：
  - 查询 FSM；
  - 创建 FSM；
  - 销毁 FSM；
  - 获取所有 FSM。

说明：

- 管理器以 `ownerType + name` 作为 FSM 的唯一键。

---

## 3.3 FsmBase

文件：

- `Assets/LFramework/Runtime/Component/Fsm/FsmBase.cs`

定义：

```csharp
public abstract class FsmBase
```

职责：

- 提供 FSM 的非泛型公共基类；
- 统一暴露：
  - `Name`
  - `FullName`
  - `OwnerType`
  - `FsmStateCount`
  - `IsRunning`
  - `IsDestroyed`
  - `CurrentStateName`
  - `CurrentStateTime`

说明：

- 这个类型主要用于“管理器层”和“统一枚举所有 FSM”；
- 业务状态类不会继承它。

---

## 3.4 IFsm<T>

文件：

- `Assets/LFramework/Runtime/Component/Fsm/IFsm.cs`

职责：

- 作为泛型状态机对外运行时接口；
- 给状态类和业务逻辑提供：
  - 启动状态机；
  - 查询状态；
  - 获取当前状态；
  - 读写 FSM 数据。

说明：

- `T` 表示状态机持有者类型；
- 例如流程系统里，`T` 就是 `IProcedureManager`。

---

## 3.5 Fsm<T>

文件：

- `Assets/LFramework/Runtime/Component/Fsm/Fsm.cs`

定义：

```csharp
internal sealed class Fsm<T> : FsmBase, IReference, IFsm<T> where T : class
```

职责：

- FSM 的真正实现；
- 保存当前状态和所有状态；
- 管理共享数据字典；
- 执行状态生命周期调用；
- 通过 `ReferencePool` 进行对象复用。

说明：

- 该类型是内部实现，业务层通常不会直接持有 `Fsm<T>`，而是通过 `IFsm<T>` 访问。

---

## 3.6 FsmState<T>

文件：

- `Assets/LFramework/Runtime/Component/Fsm/FsmState.cs`

定义：

```csharp
public abstract class FsmState<T> where T : class
```

职责：

- 作为所有业务状态的继承基类；
- 定义状态生命周期；
- 为子类提供 `ChangeState(...)` 能力。

说明：

- 这是当前 FSM 模块中最核心、最需要继承的基类。

---

## 3.7 ProcedureBase（FSM 的典型业务派生）

文件：

- `Assets/LFramework/Runtime/Component/Procedure/ProcedureBase.cs`

定义：

```csharp
public abstract class ProcedureBase : FsmState<IProcedureManager>
```

说明：

- 它不是 FSM 模块本体的一部分，但它展示了 FSM 在项目中的典型接入方式；
- `ProcedureBase` 本质上就是 `FsmState<T>` 的业务子类。

---

## 4. 核心 API

## 4.1 IFsmManager 核心 API

### 查询类 API

| API                                      | 说明             |
| ---------------------------------------- | -------------- |
| `int Count`                              | 当前 FSM 总数      |
| `bool HasFsm<T>()`                       | 是否存在默认名 FSM    |
| `bool HasFsm<T>(string name)`            | 是否存在指定名称 FSM   |
| `IFsm<T> GetFsm<T>()`                    | 获取默认名 FSM      |
| `IFsm<T> GetFsm<T>(string name)`         | 获取指定名称 FSM     |
| `FsmBase[] GetAllFsms()`                 | 获取全部 FSM       |
| `void GetAllFsms(List<FsmBase> results)` | 填充全部 FSM 到外部列表 |

### 创建类 API

```csharp
IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states)
IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states)
IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states)
IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states)
```

说明：

- `owner` 不能为空；
- `states` 至少要有一个；
- 同一 `ownerType + name` 组合不能重复创建。

### 销毁类 API

```csharp
bool DestroyFsm<T>()
bool DestroyFsm<T>(string name)
bool DestroyFsm<T>(IFsm<T> fsm)
bool DestroyFsm(Type ownerType)
bool DestroyFsm(Type ownerType, string name)
bool DestroyFsm(FsmBase fsm)
```

说明：

- 管理器销毁 FSM 时，会先调用 `fsm.Shutdown()`，再从字典中移除。

---

## 4.2 IFsm<T> 核心 API

### 基础信息

| API                        | 说明                              |
| -------------------------- | ------------------------------- |
| `string Name`              | FSM 名称                          |
| `string FullName`          | FSM 完整名称，格式为 `OwnerType + Name` |
| `T Owner`                  | FSM 持有者                         |
| `int FsmStateCount`        | 状态数量                            |
| `bool IsRunning`           | 是否已启动                           |
| `bool IsDestroyed`         | 是否已销毁                           |
| `FsmState<T> CurrentState` | 当前状态                            |
| `float CurrentStateTime`   | 当前状态已持续时间                       |

### 启动类 API

```csharp
void Start<TState>() where TState : FsmState<T>
void Start(Type stateType)
```

说明：

- 一个 FSM 只能 `Start` 一次；
- 如果已经运行，再次启动会抛异常；
- 启动后会立即执行目标状态的 `OnEnter(...)`。

### 状态查询类 API

```csharp
bool HasState<TState>() where TState : FsmState<T>
bool HasState(Type stateType)
TState GetState<TState>() where TState : FsmState<T>
FsmState<T> GetState(Type stateType)
FsmState<T>[] GetAllStates()
void GetAllStates(List<FsmState<T>> results)
```

### 数据存取类 API

```csharp
bool HasData(string name)
TData GetData<TData>(string name) where TData : Variable
Variable GetData(string name)
void SetData<TData>(string name, TData data) where TData : Variable
void SetData(string name, Variable data)
bool RemoveData(string name)
```

说明：

- 数据存取适合保存 FSM 运行期的上下文；
- 当前实现中的 `Variable` 通常与 `ReferencePool` 配合使用。

---

## 4.3 FsmState<T> 核心生命周期 API

这是业务侧最重要的基类。

### 需要继承的基类

```csharp
public abstract class MyState : FsmState<MyOwner>
{
}
```

### 生命周期方法

```csharp
protected internal virtual void OnInit(IFsm<T> fsm)
protected internal virtual void OnEnter(IFsm<T> fsm)
protected internal virtual void OnUpdate(IFsm<T> fsm, float elapseSeconds, float realElapseSeconds)
protected internal virtual void OnLeave(IFsm<T> fsm, bool isShutdown)
protected internal virtual void OnDestroy(IFsm<T> fsm)
```

### 各生命周期含义

| 生命周期        | 触发时机                | 常见用途                |
| ----------- | ------------------- | ------------------- |
| `OnInit`    | FSM 创建时，每个状态初始化一次   | 缓存引用、初始化状态级资源       |
| `OnEnter`   | 状态成为当前状态时           | 重置状态、播放进入逻辑         |
| `OnUpdate`  | FSM 每帧更新，且该状态是当前状态时 | 驱动状态逻辑、判断切换条件       |
| `OnLeave`   | 离开当前状态时             | 停止状态逻辑、清理进入时创建的临时效果 |
| `OnDestroy` | FSM 销毁时，每个状态销毁一次    | 最终清理、释放状态级长期资源      |

### 状态切换 API

`FsmState<T>` 提供两个受保护方法：

```csharp
protected void ChangeState<TState>(IFsm<T> fsm) where TState : FsmState<T>
protected void ChangeState(IFsm<T> fsm, Type stateType)
```

说明：

- 通常在 `OnUpdate(...)` 或 `OnEnter(...)` 中根据条件切换；
- 不需要业务代码直接拿到 `Fsm<T>` 实现类。

---

## 5. 核心调用链

## 5.1 FSM 创建调用链

```text
业务代码 / ProcedureComponent.Initialize(...)
    ↓
IFsmManager.CreateFsm(...)
    ↓
FsmComponent.CreateFsm(...)
    ↓
Fsm<T>.Create(...)
    ↓
为每个状态执行 OnInit(...)
    ↓
把 FSM 放入 FsmComponent._fsmDict
```

---

## 5.2 FSM 启动调用链

```text
fsm.Start<TState>()
    ↓
Fsm<T>.Start(...)
    ↓
设置 _currentState
    ↓
执行当前状态 OnEnter(...)
```

---

## 5.3 FSM 每帧更新调用链

```text
LFramework 模块系统驱动 OnUpdate(...)
    ↓
FsmComponent.OnUpdate(...)
    ↓
遍历全部 FSM
    ↓
Fsm<T>.OnUpdate(...)
    ↓
CurrentState.OnUpdate(...)
```

说明：

- 只有当前状态会收到 `OnUpdate(...)`；
- 每帧更新前会累加 `CurrentStateTime`。

---

## 5.4 状态切换调用链

```text
状态内部调用 ChangeState<TState>(fsm)
    ↓
FsmState<T>.ChangeState(...)
    ↓
Fsm<T>.ChangeState(...)
    ↓
旧状态 OnLeave(...)
    ↓
切换 _currentState
    ↓
新状态 OnEnter(...)
```

---

## 5.5 FSM 销毁调用链

```text
IFsmManager.DestroyFsm(...)
    ↓
FsmComponent.InternalDestroyFsm(...)
    ↓
Fsm<T>.Shutdown()
    ↓
ReferencePool.Release(this)
    ↓
Fsm<T>.Clear()
    ↓
当前状态 OnLeave(..., true)
    ↓
全部状态 OnDestroy(...)
```

说明：

- `Fsm<T>` 实现了 `IReference`，因此真正的重置逻辑在 `Clear()` 中完成。

---

## 6. 模块生命周期

## 6.1 FsmComponent 生命周期

### `Awake()`

作用：

- 把当前组件注册为 `IFsmManager` 模块。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IFsmManager>(this);
}
```

### `Priority`

作用：

- 声明模块优先级。

当前实现：

```csharp
public int Priority
{
    get { return 1; }
}
```

### `OnInit()`

作用：

- 当前实现为空；
- FSM 模块主要采用“按需创建状态机”的模式，而不是启动时预初始化。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 驱动所有未销毁 FSM 的当前状态更新。

### `Shutdown()`

作用：

- 关闭并清理所有已注册的 FSM。

---

## 6.2 FsmState<T> 生命周期

这是业务状态最需要关注的生命周期。

### `OnInit(IFsm<T> fsm)`

作用：

- 状态被加入 FSM 时调用一次；
- 适合做只需初始化一次的工作。

### `OnEnter(IFsm<T> fsm)`

作用：

- 当状态正式成为当前状态时调用；
- 适合做进入状态时的重置和启动逻辑。

### `OnUpdate(IFsm<T> fsm, float elapseSeconds, float realElapseSeconds)`

作用：

- 当前状态每帧调用；
- 是大多数状态判断和切换的主逻辑入口。

### `OnLeave(IFsm<T> fsm, bool isShutdown)`

作用：

- 状态离开时调用；
- `isShutdown == true` 表示这是 FSM 销毁路径，不是普通切换。

### `OnDestroy(IFsm<T> fsm)`

作用：

- FSM 被销毁时调用一次；
- 适合做状态级最终清理。

---

## 6.3 ProcedureBase 生命周期

`ProcedureBase` 本质上没有新增生命周期，只是把 `FsmState<IProcedureManager>` 的几个方法原样继承下来。

也就是说，流程系统的核心生命周期仍然是：

```text
OnInit
OnEnter
OnUpdate
OnLeave
OnDestroy
```

因此：

- 写 `Procedure` 时，其实就是在写一个特定 Owner 类型的 FSM 状态。

---

## 7. 典型使用方式

## 7.1 创建并启动 FSM

```csharp
var fsm = fsmManager.CreateFsm(owner,
    new StateA(),
    new StateB());

fsm.Start<StateA>();
```

---

## 7.2 在状态内切换

```csharp
protected internal override void OnUpdate(IFsm<MyOwner> fsm, float elapseSeconds, float realElapseSeconds)
{
    if (ShouldChange())
    {
        ChangeState<StateB>(fsm);
    }
}
```

---

## 7.3 使用 FSM 数据

```csharp
fsm.SetData("RetryCount", (VarInt32)1);

VarInt32 retryCount = fsm.GetData<VarInt32>("RetryCount");
int value = retryCount;
```

说明：

- 当前项目里的 `VarInt32` 等变量类型支持隐式转换；
- 这也是 FSM 数据字典的典型用法。

---

## 7.4 Procedure 模块中的使用方式

`ProcedureComponent.Initialize(...)` 中的核心逻辑：

```csharp
_procedureFsm = _fsmManager.CreateFsm(this, procedures);
```

随后：

```csharp
_procedureFsm.Start(procedureType);
```

这就是 FSM 在项目中的标准落地方式。

---

## 8. 使用注意事项

### 8.1 状态类应该继承 `FsmState<T>`

FSM 模块中真正需要业务继承的核心基类就是：

```csharp
FsmState<T>
```

而不是：

- `FsmBase`
- `Fsm<T>`
- `FsmComponent`

---

### 8.2 `OnInit` 与 `OnEnter` 的职责不要混淆

- `OnInit`：只初始化一次
- `OnEnter`：每次进入都会执行

如果把“每次进入重置”的逻辑写到 `OnInit`，会导致状态复用时行为不符合预期。

---

### 8.3 `OnLeave` 可能来自普通切换，也可能来自销毁

要根据 `isShutdown` 区分：

- 普通状态切换；
- FSM 关闭清理。

---

### 8.4 `GetState(Type)` / `Start(Type)` 需要传具体状态类型

当前实现是按状态真实类型作为字典键保存的，因此传入的类型应当与注册时的状态类型一致。

---

### 8.5 当前 FSM 数据基于 `Variable`

不要直接把普通对象塞进 FSM 数据字典，应该使用框架的 `Variable` 体系。

---

## 9. 总结

当前 `FSM` 模块可以概括为：

- 一个 `sealed` 的 FSM 管理组件 `FsmComponent`；
- 一个统一管理接口 `IFsmManager`；
- 一个泛型运行时接口 `IFsm<T>`；
- 一个真正需要业务继承的状态基类 `FsmState<T>`；
- 一套由 `OnInit -> OnEnter -> OnUpdate -> OnLeave -> OnDestroy` 构成的完整状态生命周期。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `FsmComponent` 如何创建和驱动 FSM；
2. `Fsm<T>` 如何切换状态和管理数据；
3. `FsmState<T>` 的生命周期和 `ChangeState(...)` 使用方式。
