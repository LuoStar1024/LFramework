# 架构

LFramework 按运行时职责分为三层：

```text
Launcher
  AOT 启动流程：YooAsset 初始化/更新、HybridCLR 元数据和 DLL 加载，
  然后实例化 GameEntry 预制体。
GameLogic
  热更新业务层：面向项目的 GameEntry facade、游戏流程、
  UI、DataTable、Singleton、玩法服务，以及 ResourceContainer/EventContainer 使用。
LFramework.Runtime
  框架层：RootComponent、模块注册表、内置管理器、
  池、事件、资源、场景、音频、设置、工具和日志。
```

程序集边界：

- `LFramework.Runtime` 提供框架接口和运行时组件。
- `Launcher` 在进入热更新逻辑前依赖运行时服务。
- `GameLogic` 依赖 `LFramework.Runtime` 和 `GameDataTable`。
- `GameDataTable` 是 Luban 生成的配置表代码，不应依赖 GameLogic。
- Runtime 框架代码不能依赖 GameLogic。GameLogic 可以通过 `GameEntry` 或框架接口依赖 Runtime。

## 模块注册表

每个框架模块都实现 `ILFrameworkModule`：

```csharp
int Priority { get; }
void OnInit();
void OnUpdate(float elapseSeconds, float realElapseSeconds);
void Shutdown();
```

`LFrameworkEntry` 持有模块字典和按优先级排序的模块链表：

- 模块通过 `LFrameworkEntry.RegisterModule<I...>(this)` 按接口类型注册。
- 模块通过 `LFrameworkEntry.GetModule<I...>()` 按接口类型获取。
- 泛型类型必须是接口。传入具体 Component 类型是非法用法，会抛出 `LFrameworkException`。
- 同一接口重复注册会抛出 `LFrameworkException`。
- 注册时会按 `Priority` 从高到低插入链表，标记更新执行列表为脏，并立即调用 `module.OnInit()`。
- `Priority` 更高的模块更早更新，并且更晚关闭。

内置运行时模块是 `Assets/LFramework/Runtime/Component` 下的 MonoBehaviour 组件。当前自定义 GameLogic 模块包括：

- `DataTableComponent` 注册为 `IDataTableManager`
- `UIComponent` 注册为 `IUIManager`
- `SingletonComponent` 注册为 `ISingletonManager`

由于不同 Unity 对象之间的 `Awake()` 顺序不能作为稳定依赖边界，`OnInit()` 只应初始化模块自身状态。跨模块依赖应在 `Start()` 或更晚阶段获取，或在实际使用点按需获取。

## 启动流程

高层启动链路：

```text
Unity scene loads
  -> RootComponent.Awake()
     -> 缓存 RootComponent.Instance
     -> 订阅 Application.lowMemory
  -> Runtime 和 GameLogic 组件 Awake()
     -> LFrameworkEntry.RegisterModule<I...>(this)
     -> module.OnInit()
  -> Launcher procedure flow
     -> 初始化资源和更新检查
     -> ProcedureLoadAssembly 在启用时加载 HybridCLR 元数据/DLL 资源
     -> ProcedureStartGame 加载并实例化 Assets/GameResRaw/GameEntry/GameEntry
  -> RootComponent.Update()
     -> LFrameworkEntry.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime)
  -> GameEntry.Start()
     -> 销毁 IProcedureManager 旧 Procedure FSM
     -> 初始化游戏流程
     -> 等待一帧
     -> 缓存内置和自定义模块
     -> EventHelper.OnInit()
     -> 启动 ProcedureGameLogicLaunch
```

`RootComponent.Update()` 是中心更新驱动。它调用 `LFrameworkEntry.OnUpdate()`；当模块注册发生变化时，`LFrameworkEntry` 会重建缓存的执行列表，然后按优先级更新模块。

`GameEntry` 是 `Start()` 协程初始化静态引用后的项目侧 facade。业务代码应优先使用 `GameEntry.Resource`、`GameEntry.UI`、`GameEntry.Event`、`GameEntry.DataTable` 等属性，而不是直接调用 `LFrameworkEntry.GetModule<I...>()`。

只有框架集成代码、Launcher 代码、`GameEntry` 尚未就绪前的模块初始化代码，或明确运行在 GameEntry facade 生命周期之外的代码，才应直接使用 `LFrameworkEntry.GetModule<I...>()`。

## GameEntry Facade

`GameEntry.InitBuiltinComponents()` 缓存：

```text
Base, Config, DataNode, Debugger, Event, Fsm, Localization,
ObjectPool, Procedure, Resource, Scene, Setting, Audio, Timer, Unity
```

`GameEntry.InitCustomComponents()` 缓存：

```text
DataTable, UI, Singleton
```

`GameEntry.DataTable` 会解析 `LFrameworkEntry.GetModule<IDataTableManager>().Tables`。`DataTableComponent` 会延迟创建 Luban `Tables`，并从资源系统已持有的资源中加载配置表 `TextAsset`。访问配置表行数据前，必须确保配置表资源已经加载。

## 资源和热更新边界

Launcher 代码运行在 GameLogic 完全激活之前。它直接使用运行时接口：

```text
LFrameworkEntry.GetModule<IResourceManager>()
LFrameworkEntry.GetModule<IConfigManager>()
LFrameworkEntry.GetModule<ISettingManager>()
```

`ProcedureLoadAssembly` 通过 `IResourceManager` 加载 AOT 元数据和热更新 DLL 文本资源。当 HybridCLR/更新模式关闭，或处于编辑器模拟模式时，它会改为从当前 AppDomain 中解析程序集。程序集加载完成后，`ProcedureStartGame` 加载 GameEntry 预制体，并将其实例化到 `RootComponent.Instance.transform` 下。

不要把 GameLogic 专属依赖移动到 Launcher 或 Runtime 中。热更新业务入口应保持在 GameEntry 预制体和 GameLogic 程序集边界之后。

## 低内存

```text
Application.lowMemory
  -> RootComponent.OnLowMemory()
  -> IObjectPoolManager.ReleaseAllUnused()
  -> IResourceManager.ForceUnloadUnusedAssets(true)
```

低内存处理是兜底清理。拥有资源的代码仍然必须通过 `ResourceContainer`、`UnloadAsset()`、UI close/recycle 或其他所有者清理逻辑显式释放资源。

## 关闭流程

```text
RootComponent.Shutdown(type)
  -> Destroy(root gameObject)
  -> RootComponent.OnDestroy()
  -> LFrameworkEntry.Shutdown()
  -> 模块按优先级反向关闭
  -> 清空 ModuleLinkedList 和更新执行列表
  -> ReferencePool.ClearAll()
  -> Utility.Marshal.FreeCachedHGlobal()
  -> LFrameworkLog.SetLogHelper(null)
```

`ShutdownType.Restart` 会在销毁 root 对象后重新加载场景 `0`。`ShutdownType.Quit` 会调用 `Application.Quit()`，并在 Unity Editor 中停止播放模式。

所有者范围内的清理必须在底层模块消失前完成：

- UI 窗口应通过 `UIComponent` 关闭/回收，不要直接销毁。
- 事件订阅应由 `EventContainer` 持有，或手动取消订阅。
- 资源加载必须有清晰所有权；直接 `LoadAsset<T>()` 时必须匹配 `UnloadAsset()`。
- 通过 `ReferencePool.Acquire<T>()` 创建的引用池对象必须在 `ReferencePool.ClearAll()` 前释放。
