# Core 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Core` 模块的：

- 核心类型；
- 对外 API；
- 在框架中的接入方式；
- 核心生命周期；
- 需要理解的基类 / 接口。

---

## 2. 模块定位

`Core` 是 LFramework 的底层运行基础层，负责为上层所有模块提供统一的：

- 模块管理；
- 事件系统；
- 对象池；
- 任务调度；
- 日志；
- 通用工具；
- 通用变量与数据结构。

如果把 `Component/*` 看作“业务功能模块层”，那么 `Core` 就是这些模块共同依赖的“基础运行时层”。

---

## 3. 核心类型

## 3.1 LFrameworkEntry

文件：

- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`

定义：

```csharp
public static class LFrameworkEntry
```

职责：

- 统一管理框架模块注册、排序、轮询与关闭；
- 保存接口类型到模块实例的映射；
- 作为整个框架模块系统的总入口。

---

## 3.2 ILFrameworkModule

文件：

- `Assets/LFramework/Runtime/Core/ILFrameworkModule.cs`

定义：

```csharp
public interface ILFrameworkModule
```

职责：

- 约束所有框架模块的统一生命周期；
- 约定优先级、初始化、更新、关闭行为。

说明：

- 当前 `Core` 没有为模块提供统一抽象基类；
- 模块接入依赖的是 `ILFrameworkModule` 接口，而不是某个基类。

---

## 3.3 LFrameworkEventArgs

文件：

- `Assets/LFramework/Runtime/Core/LFrameworkEventArgs.cs`

定义：

```csharp
public abstract class LFrameworkEventArgs : EventArgs, IReference
```

职责：

- 作为框架事件参数对象的统一基类；
- 接入 `ReferencePool`，支持事件参数池化复用。

适用场景：

- 自定义事件参数类型；
- 需要进入引用池管理的事件数据对象。

---

## 3.4 ReferencePool / IReference

文件：

- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/IReference.cs`

定义：

```csharp
public static partial class ReferencePool
public interface IReference
```

职责：

- 为引用类型提供统一的获取、归还和清理机制；
- `IReference.Clear()` 约定对象回池前必须重置状态。

---

## 3.5 TaskBase / ITaskAgent<T> / TaskPool<T>

文件：

- `Assets/LFramework/Runtime/Core/TaskPool/TaskBase.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/ITaskAgent.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/TaskPool.cs`

定义：

```csharp
internal abstract class TaskBase : IReference
internal interface ITaskAgent<T> where T : TaskBase
internal sealed class TaskPool<T> where T : TaskBase
```

职责：

- `TaskBase`：定义任务的公共数据与回池语义；
- `ITaskAgent<T>`：定义任务执行代理；
- `TaskPool<T>`：负责任务排队、分发、更新、回收。

---

## 3.6 Variable / Variable<T>

文件：

- `Assets/LFramework/Runtime/Core/Variable/Variable.cs`
- `Assets/LFramework/Runtime/Core/Variable/GenericVariable.cs`

定义：

```csharp
public abstract class Variable : IReference
public abstract class Variable<T> : Variable
```

职责：

- 将不同类型的值统一抽象为可池化的变量对象；
- 为 DataNode / 配置 / 黑板式数据访问提供基础包装。

---

## 3.7 EventPool

文件：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventPool.Event.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventRuntimeId.cs`

定义：

```csharp
internal sealed partial class EventPool
public static class EventRuntimeId
```

职责：

- 提供事件订阅、取消订阅、下一帧派发、立即派发；
- 通过运行时字符串映射生成事件 Id。

---

## 3.8 LFrameworkLog / Log

文件：

- `Assets/LFramework/Runtime/Core/Log/LFrameworkLog.cs`
- `Assets/LFramework/Runtime/Core/Log/Log.cs`

职责：

- 提供统一日志门面；
- 上层通常使用 `Log.Info(...)`、`Log.Warning(...)` 等接口；
- 底层真正实现由 `ILogHelper` 决定，默认是 `DefaultLogHelper`。

---

## 3.9 Utility

文件：

- `Assets/LFramework/Runtime/Core/Utility/*.cs`

职责：

- 提供跨模块共用的基础静态方法；
- 当前最常用的子域包括：
  - `Utility.Text`
  - `Utility.Json`
  - `Utility.Path`
  - `Utility.File`
  - `Utility.Assembly`
  - `Utility.Encryption`
  - `Utility.Marshal`
  - `Utility.Verifier`

---

## 4. 核心 API

以下列出当前 `Core` 模块最值得优先掌握的 API。

## 4.1 模块系统 API

来自 `LFrameworkEntry` 与 `ILFrameworkModule`。

### `void LFrameworkEntry.RegisterModule<T>(ILFrameworkModule module)`

作用：

- 按接口注册框架模块；
- 按 `Priority` 插入执行链；
- 注册完成后立刻调用 `module.OnInit()`。

使用示例：

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<ISettingManager>(this);
}
```

注意：

- `T` 必须是接口类型；
- 同一接口重复注册会抛异常。

---

### `T LFrameworkEntry.GetModule<T>()`

作用：

- 按接口类型获取已注册模块。

注意：

- 当前不存在“取不到返回 null”的语义；
- 找不到时会直接抛 `LFrameworkException`。

---

### `void LFrameworkEntry.OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 驱动所有已注册模块执行 `OnUpdate(...)`。

上层入口：

```text
RootComponent.Update()
    -> LFrameworkEntry.OnUpdate(...)
```

---

### `void LFrameworkEntry.Shutdown()`

作用：

- 按逆序关闭所有已注册模块；
- 清理更新列表、引用池、Marshal 缓存、日志 helper。

---

### `int ILFrameworkModule.Priority`

作用：

- 决定模块更新顺序和关闭顺序。

语义：

- 优先级越高，越先更新；
- 关闭时越后执行。

---

### `void ILFrameworkModule.OnInit()`

作用：

- 模块完成注册后立即调用；
- 是当前框架模块最关键的初始化入口。

---

### `void ILFrameworkModule.OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 每帧更新入口。

---

### `void ILFrameworkModule.Shutdown()`

作用：

- 框架关闭时的释放入口。

---

## 4.2 事件系统 API

来自 `EventPool` 与 `EventRuntimeId`。

### 订阅 / 取消订阅

常见 API：

- `Subscribe(int id, Action handler)`
- `Subscribe<TArg1>(int id, Action<TArg1> handler)`
- `SubscribeDelegate(int id, Delegate handler)`
- `Unsubscribe(...)`
- `UnsubscribeDelegate(...)`

作用：

- 绑定和解绑指定事件 Id 的处理函数。

---

### 延迟派发

常见 API：

- `Fire(int id)`
- `Fire<TArg1>(int id, TArg1 arg1)`
- `Fire<TArg1, TArg2>(...)`

作用：

- 线程安全；
- 事件会进入队列，在下一帧 `OnUpdate()` 中分发。

---

### 立即派发

常见 API：

- `FireNow(int id)`
- `FireNow<TArg1>(...)`

作用：

- 立即执行事件处理；
- 当前实现不是线程安全的。

---

### 默认处理器与事件组

常见 API：

- `SetDefaultHandler(Action<int> handler)`
- `RegisterGroup<T>(T group)`
- `T FireGroup<T>()`

作用：

- 未命中事件处理器时可以走默认兜底；
- 支持按接口注册/获取事件组实例。

---

### 运行时事件 Id

常见 API：

- `int EventRuntimeId.ToRuntimeId(string value)`
- `string EventRuntimeId.ToString(int runtimeId)`

作用：

- 将字符串映射为运行时唯一整数 Id；
- 便于统一事件常量定义。

常见写法：

```csharp
public static readonly int OpenFormEventId =
    EventRuntimeId.ToRuntimeId("GameLogic.UI.OpenForm");
```

---

## 4.3 引用池 API

来自 `ReferencePool`。

### 获取与归还

常见 API：

- `T Acquire<T>() where T : class, IReference, new()`
- `IReference Acquire(Type referenceType)`
- `void Release(IReference reference)`

作用：

- 从对象池获取实例；
- 使用完成后回池。

前提：

- 目标类型必须实现 `IReference`；
- 回池前会调用 `Clear()`。

---

### 维护与统计

常见 API：

- `void Add<T>(int count)`
- `void Remove<T>(int count)`
- `void RemoveAll<T>()`
- `void ClearAll()`
- `ReferencePoolInfo[] GetAllReferencePoolInfos()`

作用：

- 预热对象池；
- 批量移除；
- 获取池统计信息。

---

### 检查开关

常见 API：

- `bool EnableStrictCheck { get; set; }`

作用：

- 开启后会做更严格的引用类型与重复归还检查。

---

## 4.4 任务池 API

来自 `TaskBase`、`ITaskAgent<T>`、`TaskPool<T>`。

### TaskBase 关键成员

常见 API：

- `int SerialId`
- `string Tag`
- `int Priority`
- `object UserData`
- `bool Done`
- `virtual string Description`
- `void Initialize(int serialId, string tag, int priority, object userData)`
- `virtual void Clear()`

作用：

- 描述任务元数据；
- 标识任务是否完成；
- 进入 `ReferencePool` 后可复用。

---

### ITaskAgent<T> 关键成员

常见 API：

- `T Task { get; }`
- `void Initialize()`
- `StartTaskStatus Start(T task)`
- `void OnUpdate(float elapseSeconds, float realElapseSeconds)`
- `void Reset()`
- `void Shutdown()`

作用：

- 定义具体任务执行器的生命周期。

---

### TaskPool<T> 关键成员

常见 API：

- `void AddAgent(ITaskAgent<T> agent)`
- `void AddTask(T task)`
- `bool RemoveTask(int serialId)`
- `int RemoveTasks(string tag)`
- `int RemoveAllTasks()`
- `TaskInfo GetTaskInfo(int serialId)`
- `TaskInfo[] GetTaskInfos(string tag)`
- `TaskInfo[] GetAllTaskInfos()`
- `void OnUpdate(float elapseSeconds, float realElapseSeconds)`
- `void Shutdown()`

作用：

- 管理任务队列、代理队列、运行中的任务和已完成任务回收。

---

### StartTaskStatus

枚举值：

- `Done`
- `CanResume`
- `HasToWait`
- `UnknownError`

含义：

- 用来描述任务代理对新任务的启动结果。

---

## 4.5 变量系统 API

来自 `Variable` 与 `Variable<T>`。

### `Type Type`

作用：

- 获取变量真实类型。

### `object GetValue()`

作用：

- 以非泛型方式读取值。

### `void SetValue(object value)`

作用：

- 以非泛型方式写入值。

### `T Value`

作用：

- 以泛型方式直接读取或写入值。

### `void Clear()`

作用：

- 回池前重置变量。

---

## 4.6 日志 API

来自 `LFrameworkLog` / `Log`。

常见 API：

- `Log.Debug(...)`
- `Log.Info(...)`
- `Log.Warning(...)`
- `Log.Error(...)`
- `Log.Fatal(...)`
- `LFrameworkLog.SetLogHelper(...)`

作用：

- 统一输出不同等级日志；
- 通过 helper 切换日志后端实现。

---

## 4.7 常用 Utility API

这里只列出最常用的一组。

### Text

- `Utility.Text.Format(...)`
- `Utility.Text.SetTextHelper(...)`

作用：

- 提供统一字符串格式化入口。

### Json

- `Utility.Json.ToJson(object obj)`
- `Utility.Json.ToObject<T>(string json)`
- `Utility.Json.ToObject(Type objectType, string json)`
- `Utility.Json.SetJsonHelper(...)`

作用：

- 提供统一 JSON 序列化 / 反序列化入口。

### Path

- `Utility.Path.GetRegularPath(string path)`
- `Utility.Path.GetRemotePath(string path)`
- `Utility.Path.RemoveEmptyDirectory(string directoryName)`

作用：

- 规范化路径、转远程路径、删除空目录。

### File

- `Utility.File.CreateFile(...)`
- `Utility.File.GetPersistentDataPlatformPath(...)`
- `Utility.File.Md5ByPathName(...)`
- `Utility.File.BinToUtf8(...)`
- `Utility.File.GetFileSize(...)`

作用：

- 提供常用文件操作辅助。

### Assembly

- `Utility.Assembly.GetAssemblies()`
- `Utility.Assembly.GetTypes()`
- `Utility.Assembly.GetType(string typeName)`

作用：

- 提供程序集和类型反射能力。

### Encryption / Verifier

- `Utility.Encryption.GetXorBytes(...)`
- `Utility.Encryption.GetSelfXorBytes(...)`
- `Utility.Verifier.GetCrc32(...)`

作用：

- 提供简单异或加密和 CRC 校验能力。

---

## 5. Core 模块调用链

## 5.1 模块注册链

```text
某个 XxxComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IxxxManager>(this)
    ↓
按 Priority 插入模块链表
    ↓
module.OnInit()
```

当前项目中已存在的典型注册点包括：

- `SettingComponent`
- `TimerComponent`
- `UnityWrapperComponent`
- `SceneComponent`
- `ResourceComponent`
- `EventComponent`
- `ObjectPoolComponent`
- `BaseComponent`

---

## 5.2 逐帧更新链

```text
RootComponent.Update()
    ↓
LFrameworkEntry.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime)
    ↓
按 Priority 顺序执行所有模块 OnUpdate(...)
```

---

## 5.3 关闭链

```text
RootComponent.OnDestroy()
    ↓
LFrameworkEntry.Shutdown()
    ↓
按模块链表逆序执行 Shutdown()
    ↓
清理引用池 / Marshal 缓存 / 日志 helper
```

---

## 6. 生命周期

`Core` 里最关键的是 4 套生命周期。

## 6.1 模块生命周期（ILFrameworkModule）

适用对象：

- 所有挂到框架中的功能模块。

核心顺序：

1. Unity `Awake()`
2. `LFrameworkEntry.RegisterModule<T>(this)`
3. `OnInit()`
4. 每帧 `OnUpdate(...)`
5. 框架关闭时 `Shutdown()`

说明：

- 这是整个框架最重要的一层生命周期；
- `Core` 本身就是所有业务模块的生命周期基座。

---

## 6.2 引用池生命周期（IReference）

适用对象：

- `LFrameworkEventArgs`
- `TaskBase`
- `Variable`
- `EventPool` 内部事件结点
- 其它实现 `IReference` 的对象

核心顺序：

1. `ReferencePool.Acquire<T>()`
2. 对象被使用
3. `ReferencePool.Release(obj)`
4. 自动调用 `obj.Clear()`
5. 等待下次复用

说明：

- `Clear()` 是最关键的回收钩子；
- 如果 `Clear()` 没有完全清理对象状态，复用时就可能出现脏数据问题。

---

## 6.3 任务生命周期（TaskBase + ITaskAgent<T>）

适用对象：

- 基于 `TaskPool<T>` 的任务系统。

核心顺序：

1. 通过 `ReferencePool` 获取任务对象；
2. 调用 `TaskBase.Initialize(...)`；
3. `TaskPool.AddTask(task)` 放入等待队列；
4. 空闲 agent 调用 `Start(task)`；
5. 若任务进入工作态，后续每帧执行 `agent.OnUpdate(...)`；
6. 任务完成后 `task.Done = true`；
7. agent `Reset()`；
8. 任务 `ReferencePool.Release(task)` 回池。

---

## 6.4 事件生命周期（EventPool）

适用对象：

- 所有通过 `EventPool` 发出的事件。

### 延迟事件

顺序：

1. `Fire(...)`
2. 创建池化事件结点
3. 入 `_events` 队列
4. 下一帧 `EventPool.OnUpdate()`
5. 执行 `HandleEvent(...)`
6. 释放事件结点到 `ReferencePool`

### 立即事件

顺序：

1. `FireNow(...)`
2. 直接执行 `HandleEvent(...)`

说明：

- 延迟事件是线程安全入队；
- 立即事件是同步调用。

---

## 7. 需要理解的基类 / 接口

用户在阅读或后续修复 Core 时，优先要理解以下 5 个抽象点。

## 7.1 `ILFrameworkModule`

这是所有框架模块的统一生命周期接口。

核心成员：

- `Priority`
- `OnInit()`
- `OnUpdate(...)`
- `Shutdown()`

---

## 7.2 `IReference`

这是所有池化对象的统一回收接口。

核心成员：

- `Clear()`

---

## 7.3 `LFrameworkEventArgs`

这是框架事件参数对象的统一基类。

核心意义：

- 同时具备 `EventArgs` 语义和 `IReference` 语义。

---

## 7.4 `TaskBase`

这是任务对象的统一基类。

核心成员：

- `SerialId`
- `Tag`
- `Priority`
- `UserData`
- `Done`
- `Description`
- `Initialize(...)`
- `Clear()`

---

## 7.5 `Variable<T>`

这是强类型变量包装的统一泛型基类。

核心成员：

- `Type`
- `Value`
- `GetValue()`
- `SetValue(object value)`
- `Clear()`

---

## 8. 使用注意事项

### 8.1 `RegisterModule<T>()` 后会立刻触发 `OnInit()`

这意味着：

- 注册不是单纯入表；
- 会立即执行模块初始化逻辑；
- 如果模块依赖其它模块，需注意注册时机。

---

### 8.2 `GetModule<T>()` 是强制获取，不是安全查询

这意味着：

- 取不到就会抛异常；
- 不适合直接用于“可选模块”或“试探式访问”。

---

### 8.3 所有池化对象都必须保证 `Clear()` 完整清理状态

否则：

- 下次 `Acquire()` 时可能读到上次使用留下的旧数据。

---

### 8.4 事件的 `Fire()` 与 `FireNow()` 语义不同

- `Fire()`：下一帧分发，线程安全入队；
- `FireNow()`：立刻分发，不保证线程安全。

---

## 9. 总结

当前 `Core` 模块最核心的理解方式可以概括为：

- 用 `LFrameworkEntry + ILFrameworkModule` 管模块生命周期；
- 用 `ReferencePool + IReference` 管对象复用；
- 用 `EventPool` 管事件；
- 用 `TaskPool<T>` 管任务；
- 用 `Utility`、`Log`、`Variable` 提供基础支撑。

后续如果要开始修复 bug，优先建议从以下几类基点入手：

1. 模块生命周期；
2. 引用池回收语义；
3. 事件派发与线程安全；
4. 文件与工具类中的确定性缺陷。
