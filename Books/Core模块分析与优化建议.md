# Core 模块分析与优化建议

## 1. 文档目的

本文针对当前 `Core` 模块进行静态分析，目标是：

- 梳理模块职责与边界；
- 识别当前实现中的缺陷、风险和后续优化点；
- 为后续正式修复提供优先级参考。

本次分析主要覆盖目录：

- `Assets/LFramework/Runtime/Core/`

并结合以下接入点辅助判断生命周期与实际运行行为：

- `Assets/LFramework/Runtime/Component/RootComponent.cs`
- `Assets/LFramework/Runtime/Component/*/*Component.cs` 中对 `LFrameworkEntry.RegisterModule<T>()` 的调用

---

## 2. 当前模块定位

`Core` 是 LFramework 的基础设施层，负责提供整个框架共用的底层能力，主要包括：

- 模块注册、轮询、关闭；
- 事件派发；
- 引用池；
- 任务池；
- 日志；
- 通用工具类；
- 通用数据结构；
- 变量包装；
- 扩展方法。

从架构上看，它不是某一个业务功能模块，而是所有上层模块的“运行基础层”。

---

## 3. 当前模块结构

当前 `Core` 大致可拆为 8 个子域。

### 3.1 模块入口

核心文件：

- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`
- `Assets/LFramework/Runtime/Core/ILFrameworkModule.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkException.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkEventArgs.cs`

职责：

- 定义框架模块接入规范；
- 管理模块注册顺序、更新顺序和关闭顺序；
- 统一框架异常与事件参数基类。

### 3.2 EventPool

核心文件：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventPool.Event.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventRuntimeId.cs`
- `Assets/LFramework/Runtime/Core/EventPool/EventPoolMode.cs`

职责：

- 提供事件订阅、取消订阅、延迟派发、立即派发；
- 支持运行时事件 Id；
- 支持事件组。

### 3.3 ReferencePool

核心文件：

- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.ReferenceCollection.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/IReference.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePoolInfo.cs`

职责：

- 为事件、任务、变量等对象提供池化复用；
- 统一对象获取、归还、统计与批量清理。

### 3.4 TaskPool

核心文件：

- `Assets/LFramework/Runtime/Core/TaskPool/TaskPool.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/TaskBase.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/ITaskAgent.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/TaskInfo.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/TaskStatus.cs`
- `Assets/LFramework/Runtime/Core/TaskPool/StartTaskStatus.cs`

职责：

- 提供任务排队、优先级调度、任务代理分发与回收。

### 3.5 Log

核心文件：

- `Assets/LFramework/Runtime/Core/Log/LFrameworkLog.cs`
- `Assets/LFramework/Runtime/Core/Log/Log.cs`
- `Assets/LFramework/Runtime/Core/Log/DefaultLogHelper.cs`
- `Assets/LFramework/Runtime/Core/Log/LFrameworkLog.ILogHelper.cs`

职责：

- 提供统一日志入口；
- 支持通过 helper 切换具体日志实现。

### 3.6 Utility

核心文件：

- `Assets/LFramework/Runtime/Core/Utility/*.cs`
- `Assets/LFramework/Runtime/Core/Utility/DefaultHelper/*.cs`

职责：

- 提供文本、JSON、路径、文件、Marshal、程序集、随机数、转换、校验、加密等基础能力。

### 3.7 DataStruct

核心文件：

- `Assets/LFramework/Runtime/Core/DataStruct/LFrameworkLinkedList.cs`
- `Assets/LFramework/Runtime/Core/DataStruct/LFrameworkLinkedListRange.cs`
- `Assets/LFramework/Runtime/Core/DataStruct/LFrameworkMultiDictionary.cs`
- `Assets/LFramework/Runtime/Core/DataStruct/TypeNamePair.cs`

职责：

- 为事件池等底层容器提供轻量数据结构支持。

### 3.8 Variable / Extension

核心文件：

- `Assets/LFramework/Runtime/Core/Variable/*.cs`
- `Assets/LFramework/Runtime/Core/Extension/*.cs`

职责：

- 提供池化变量包装类型；
- 提供字符串与 Unity 相关扩展方法。

---

## 4. 当前模块优点

### 4.1 基础设施边界相对清晰

`Core` 的大方向划分是合理的，模块入口、事件、对象池、任务池、工具类基本都在预期位置，后续查找成本较低。

### 4.2 模块接入方式统一

当前框架模块基本都遵循同一接入方式：

- `MonoBehaviour + ILFrameworkModule + IxxxManager`
- `Awake()` 中注册；
- `OnInit()` 初始化；
- `RootComponent.Update()` 统一驱动。

这让整个框架的启动链路比较一致。

### 4.3 复用能力较强

`ReferencePool`、`EventPool`、`TaskPool`、`Variable` 之间配合紧密，说明 Core 已具备一套可复用的底层运行模型。

### 4.4 对上层模块侵入较低

上层模块大多数只依赖接口和静态入口，不直接关心底层容器实现细节，这一点对后续优化比较有利。

---

## 5. 当前主要问题与优化点

以下问题按优先级分组。  
其中“确定性缺陷”表示代码行为已经存在明确错误；“高风险点”表示当前实现虽未必立即出错，但在真实项目中极易造成隐患。

## 5.1 高优先级问题

### 5.1.1 `LFrameworkEntry.Shutdown()` 未清空 `ModuleDict`

位置：

- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`

现状：

- 关闭时清空了 `ModuleLinkedList`、`UpdateModuleExecuteList`、`ReferencePool`；
- 但没有清理内部模块字典 `ModuleDict`。

影响：

- 如果 Root 被销毁后重新初始化框架，旧模块接口映射仍残留；
- 再次注册同接口模块时会直接报 “already exist”。

建议：

- 在 `Shutdown()` 中补充 `ModuleDict.Clear()`；
- 同时重置 `_isExecuteListDirty`，避免脏状态残留。

---

### 5.1.2 `RootComponent.OnLowMemory()` 对 `GetModule<T>()` 的空判断无效

位置：

- `Assets/LFramework/Runtime/Component/RootComponent.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`

现状：

- `GetModule<T>()` 找不到模块时会直接抛异常；
- 但 `OnLowMemory()` 仍写成：
  - 先 `GetModule<IObjectPoolManager>()`
  - 再 `if (objectPoolManager != null)`

影响：

- 如果低内存回调早于对象池/资源模块注册，将直接抛异常；
- 当前空判断无法提供保护。

建议：

- 提供 `TryGetModule<T>(out T module)`；
- 或者让 `OnLowMemory()` 改成安全查询逻辑。

---

### 5.1.3 `EventPool.Clear()` 直接清队列，事件结点未归还引用池

位置：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`

现状：

- `Clear()` 只做 `_events.Clear()`；
- 没有把队列中的事件对象 `ReferencePool.Release(...)`。

影响：

- 队列中的事件节点不会回池；
- 对象池统计失真；
- 长时间运行下会造成逻辑层面的池化泄漏。

建议：

- 在清理时逐个 `Dequeue` 并归还引用池。

---

### 5.1.4 `Utility.Encryption.GetSelfXorBytes` 循环边界错误

位置：

- `Assets/LFramework/Runtime/Core/Utility/Utility.Encryption.cs`

现状：

- 当前循环为：

```csharp
for (int i = startIndex; i < length; i++)
```

- 正确边界应当是：

```csharp
for (int i = startIndex; i < startIndex + length; i++)
```

影响：

- `startIndex > 0` 时会处理错误区间；
- 某些情况下甚至根本不会执行任何异或操作。

建议：

- 修正循环上界；
- 补充带偏移量的单元测试。

---

### 5.1.5 `Utility.File.CreateFile(string)` 存在文件句柄未释放问题

位置：

- `Assets/LFramework/Runtime/Core/Utility/Utility.File.cs`

现状：

- 使用 `System.IO.File.Create(filePath);` 后未关闭返回的 `FileStream`。

影响：

- Windows 平台下文件可能被占用；
- 后续写入、覆盖、删除可能失败。

建议：

- 改为 `using (System.IO.File.Create(filePath)) { }`。

---

### 5.1.6 `Utility.File.BinToUtf8` 未做长度保护

位置：

- `Assets/LFramework/Runtime/Core/Utility/Utility.File.cs`

现状：

- 直接访问 `total[0]`、`total[1]`、`total[2]`；
- 没有判空和长度检查。

影响：

- 空数组或长度不足 3 时会越界异常。

建议：

- 增加 `null` / 长度判断后再识别 BOM。

---

### 5.1.7 `DefaultLogHelper` 对空消息不安全

位置：

- `Assets/LFramework/Runtime/Core/Log/DefaultLogHelper.cs`

现状：

- `Info/Warning/Error` 分支大量直接调用 `message.ToString()`。

影响：

- 如果外部传入 `null`，记录日志本身就会触发 `NullReferenceException`。

建议：

- 统一转成 `message?.ToString() ?? "<Null>"`。

---

## 5.2 中优先级问题

### 5.2.1 `EventRuntimeId` 非线程安全

位置：

- `Assets/LFramework/Runtime/Core/EventPool/EventRuntimeId.cs`

现状：

- 运行时事件 Id 的两个字典和 `_currentRuntimeId` 都没有任何并发保护。

影响：

- 多线程首次生成事件 Id 时，可能出现：
  - 自增竞争；
  - 字典写入竞争；
  - 重复 Id 或异常。

建议：

- 用锁保护；
- 或改为 `ConcurrentDictionary + Interlocked.Increment`。

---

### 5.2.2 `EventPool.OnUpdate()` 在锁内执行事件回调

位置：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`

现状：

- `lock (_events)` 覆盖了整个事件出队和 `HandleEvent()` 回调过程。

影响：

- 一旦回调耗时较长，其它线程的 `Fire()` 会被阻塞；
- 会放大锁竞争；
- 不利于高频跨线程投递事件。

建议：

- 仅在锁内取出当前批次事件；
- 在锁外执行事件回调。

---

### 5.2.3 池化事件参数 `Clear()` 未统一清空委托字段

位置：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.Event.cs`

现状：

- 非泛型 `EventArgs.Clear()` 会置空 `_handleEvent`；
- 泛型 `EventArgs<T...>.Clear()` 只清参数，不清 `_handleEvent`。

影响：

- 回池对象会保留旧委托引用；
- 增加无意义引用链；
- 长期来看会加大排查难度。

建议：

- 所有泛型 `Clear()` 都补 `_handleEvent = null`。

---

### 5.2.4 `EventPool.RegisterGroup<T>()` 重复注册时抛原生异常

位置：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`

现状：

- 使用 `_eventGroupDict.Add(groupName, group)`；
- 重复注册时会直接抛 `ArgumentException`。

影响：

- 异常风格与框架其它部分不一致；
- 不利于统一诊断。

建议：

- 先判重；
- 统一抛 `LFrameworkException` 或支持覆盖注册。

---

### 5.2.5 `ReferencePool.ReferenceCollection` 统计字段不是线程安全的

位置：

- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.ReferenceCollection.cs`

现状：

- `_usingReferenceCount`、`_acquireReferenceCount`、`_releaseReferenceCount` 等都在锁外修改。

影响：

- 多线程获取/归还时统计不准确；
- 严重时 `UsingReferenceCount` 可能异常。

建议：

- 计数与队列操作统一纳入锁；
- 或改用原子操作。

---

### 5.2.6 `TaskPool.ProcessRunningTasks()` 迭代当前节点时不够稳妥

位置：

- `Assets/LFramework/Runtime/Core/TaskPool/TaskPool.cs`

现状：

- 当任务未完成时：
  - 先调用 `current.Value.OnUpdate(...)`
  - 再直接 `current = current.Next`

影响：

- 如果 `OnUpdate()` 内部触发当前任务被重置、移除或链表变动，`current.Next` 的语义会变得不稳定。

建议：

- 在调用 `OnUpdate()` 前先缓存 `next = current.Next`，再继续遍历。

---

### 5.2.7 `TaskPool.ProcessWaitingTasks()` 对等待态任务会重复尝试

位置：

- `Assets/LFramework/Runtime/Core/TaskPool/TaskPool.cs`

现状：

- `HasToWait` 状态下会把 agent 放回空闲栈，但任务仍留在 waiting 队列中。

影响：

- 下一帧会继续重复尝试同一个任务；
- 在依赖等待较多的情况下会制造无效调度开销。

建议：

- 区分“立即重试”和“等待依赖”的任务状态；
- 增加更明确的唤醒策略。

---

### 5.2.8 `Utility.Assembly` 只缓存静态构造时的程序集快照

位置：

- `Assets/LFramework/Runtime/Core/Utility/Utility.Assembly.cs`

现状：

- `_assemblies = AppDomain.CurrentDomain.GetAssemblies()` 只在静态构造里执行一次。

影响：

- 后续运行时动态加载的程序集不会进入扫描范围；
- 对热更、动态插件场景不友好。

建议：

- 按需实时读取程序集列表；
- 或提供刷新缓存接口。

---

## 5.3 低优先级问题 / 设计风险

### 5.3.1 `RootComponent.OnDestroy()` 与模块真实销毁顺序存在耦合风险

位置：

- `Assets/LFramework/Runtime/Component/RootComponent.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`

现状：

- `RootComponent.OnDestroy()` 内调用 `LFrameworkEntry.Shutdown()`；
- 但 Unity 场景中各组件销毁顺序并不总是可控。

影响：

- 某些模块可能已经被 Unity 销毁，但框架仍会继续调用其 `Shutdown()`；
- 若 `Shutdown()` 中访问 Unity 对象，可能出现 `MissingReferenceException` 或空引用。

建议：

- 后续梳理统一的关闭时机；
- 或在各模块 `Shutdown()` 中增加对象有效性保护。

---

### 5.3.2 `ReferencePool.EnableStrictCheck` 默认关闭，容易掩盖重复归还

位置：

- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.cs`

现状：

- 强检查默认关闭。

影响：

- 重复 `Release` 时更难在开发阶段发现问题；
- 可能导致相同对象被重复入池。

建议：

- 编辑器/开发环境默认开启；
- 发行环境按需关闭。

---

### 5.3.3 `GetModule<T>()` 只有抛异常式访问，缺少更温和的查询 API

位置：

- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`

现状：

- 当前只有 `GetModule<T>()`；
- 无 `HasModule<T>()` 或 `TryGetModule<T>()`。

影响：

- 对于可选模块、启动检查、低内存清理这类场景不够友好。

建议：

- 增加：
  - `bool HasModule<T>()`
  - `bool TryGetModule<T>(out T module)`

---

## 6. 建议的优化顺序

建议分三批进行。

### 第一批：确定性缺陷修复

优先建议：

1. 修复 `Shutdown()` 未清字典；
2. 修复 `GetModule<T>()` 使用方式导致的低内存异常风险；
3. 修复 `EventPool.Clear()` 不归还对象；
4. 修复 `Utility.Encryption.GetSelfXorBytes` 边界错误；
5. 修复 `Utility.File.CreateFile` 句柄泄漏；
6. 修复 `Utility.File.BinToUtf8` 越界；
7. 修复 `DefaultLogHelper` 空消息异常。

目标：

- 先解决当前最明确、最容易形成线上故障的问题。

### 第二批：并发与调度稳定性

建议处理：

1. `EventRuntimeId` 加并发保护；
2. `EventPool.OnUpdate()` 缩小锁范围；
3. `ReferencePool` 统计与线程安全梳理；
4. `TaskPool` 迭代与等待态调度优化。

目标：

- 提高 Core 在复杂运行时场景下的稳定性。

### 第三批：架构可维护性

建议处理：

1. 增加 `TryGetModule` / `HasModule`；
2. 统一关闭流程；
3. 补齐更系统的开发期严格检查和测试。

目标：

- 提升可观测性、可诊断性和后续扩展能力。

---

## 7. 推荐修改清单

### 必改建议

- `LFrameworkEntry.Shutdown()` 清理残留模块状态；
- 增加安全模块获取接口；
- 修复文件、加密、日志中的确定性缺陷；
- 修复 `EventPool.Clear()` 的对象归还问题。

### 建议改

- 为 `EventRuntimeId`、`ReferencePool`、`EventPool` 增加并发保护；
- 优化 `TaskPool` 的迭代和等待态调度；
- 统一框架异常风格。

### 可延后

- 动态程序集刷新；
- 更严格的开发环境检查开关；
- 更完善的生命周期关闭设计。

---

## 8. 总结

当前 `Core` 模块整体架构方向是对的，已经具备一套较完整的框架底层能力。  
它当前最需要处理的不是大规模重构，而是先修复若干确定性缺陷，并补齐并发安全与生命周期边界。

建议你先阅读本文和配套的 API / 生命周期文档；  
确认后，再进入正式 bug 修复阶段会更稳妥。
