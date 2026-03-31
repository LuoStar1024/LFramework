# Scene 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Scene` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 需要理解的基类或辅助类型；
- 模块核心生命周期。

---

## 2. 模块定位

`Scene` 模块是 LFramework 的场景状态管理层，用于：

- 查询场景加载状态；
- 统一发起场景加载与卸载；
- 跟踪当前已加载、加载中、卸载中的场景；
- 管理当前激活场景；
- 维护当前主摄像机引用。

它在项目中的定位，是“场景流程与场景状态的框架级管理模块”。

---

## 3. 核心类型

## 3.1 SceneComponent

文件：

- `Assets/LFramework/Runtime/Component/Scene/SceneComponent.cs`

定义：

```csharp
public sealed class SceneComponent : MonoBehaviour, ILFrameworkModule, ISceneManager
```

职责：

- 作为 Scene 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 对外暴露 `ISceneManager`；
- 管理场景状态集合；
- 调用 `IResourceManager` 完成底层场景资源加载与卸载；
- 决定哪个场景为激活场景；
- 提供当前主摄像机引用。

说明：

- `sealed`，当前模块不以继承 `SceneComponent` 扩展为主；
- 业务层通常通过 `GameEntry.Scene` 访问。

---

## 3.2 ISceneManager

文件：

- `Assets/LFramework/Runtime/Component/Scene/ISceneManager.cs`

职责：

- 定义场景模块对外统一接口；
- 覆盖以下几类能力：
  - 场景状态查询；
  - 场景加载；
  - 场景卸载。

---

## 3.3 LoadSceneInfo

文件：

- `Assets/LFramework/Runtime/Component/Scene/LoadSceneInfo.cs`

定义：

```csharp
internal sealed class LoadSceneInfo : IReference
```

职责：

- 保存一次场景加载请求的上下文；
- 持有：
  - 进度回调；
  - 成功回调；
  - 用户自定义数据；
- 通过 `ReferencePool` 进行复用。

说明：

- 它不是业务需要继承的类型；
- 但它是 Scene 模块里重要的辅助生命周期对象。

---

## 3.4 SceneComponentInspector

文件：

- `Assets/LFramework/Editor/Inspector/SceneComponentInspector.cs`

职责：

- 在运行时展示：
  - 已加载场景；
  - 正在加载场景；
  - 正在卸载场景；
  - 当前主摄像机。

说明：

- 这是调试支持，不属于运行时核心逻辑；
- 但阅读模块时值得一起理解。

---

## 3.5 与 Resource 模块的关系

Scene 模块本身不直接做底层资源场景加载，而是依赖：

```csharp
IResourceManager.LoadScene(...)
IResourceManager.UnloadScene(...)
```

因此：

- Scene 模块负责“场景状态与流程”；
- Resource 模块负责“资源层异步加载与卸载”；
- Scene 是上层，Resource 是底层支撑。

---

## 4. 核心 API

## 4.1 状态查询 API

### 已加载场景

```csharp
bool SceneIsLoaded(string sceneAssetName)
string[] GetLoadedSceneAssetNames()
void GetLoadedSceneAssetNames(List<string> results)
```

### 正在加载场景

```csharp
bool SceneIsLoading(string sceneAssetName)
string[] GetLoadingSceneAssetNames()
void GetLoadingSceneAssetNames(List<string> results)
```

### 正在卸载场景

```csharp
bool SceneIsUnloading(string sceneAssetName)
string[] GetUnloadingSceneAssetNames()
void GetUnloadingSceneAssetNames(List<string> results)
```

### 场景是否存在

```csharp
bool HasScene(string sceneAssetName)
```

说明：

- 该接口最终依赖 `IResourceManager.HasAsset(...)`。

---

## 4.2 场景加载 API

### 最基础形式

```csharp
void LoadScene(string sceneAssetName, Action<float> progressCallback = null, Action<bool> loadSuccessCallBack = null)
```

### 指定优先级

```csharp
void LoadScene(string sceneAssetName, int priority, Action<float> progressCallback = null, Action<bool> loadSuccessCallBack = null)
```

### 带用户数据

```csharp
void LoadScene(string sceneAssetName, object userData, Action<float> progressCallback = null, Action<bool> loadSuccessCallBack = null)
void LoadScene(string sceneAssetName, int priority, object userData, Action<float> progressCallback = null, Action<bool> loadSuccessCallBack = null)
```

说明：

- `progressCallback`：场景加载进度；
- `loadSuccessCallBack`：场景加载完成结果；
- `userData`：透传到内部 `LoadSceneInfo`。

---

## 4.3 场景卸载 API

```csharp
void UnloadScene(string sceneAssetName)
void UnloadScene(string sceneAssetName, object userData)
```

说明：

- 当前 Scene 模块对外只暴露资源名与自定义数据；
- 实际卸载由 Resource 模块完成。

---

## 4.4 SceneComponent 的扩展辅助 API

虽然不在 `ISceneManager` 接口里，但 `SceneComponent` 还额外提供：

```csharp
public static string GetSceneName(string sceneAssetName)
public void SetSceneOrder(string sceneAssetName, int sceneOrder)
public void RefreshMainCamera()
public Camera MainCamera { get; }
```

### `GetSceneName`

作用：

- 从场景资源路径中提取真正的场景名。

### `SetSceneOrder`

作用：

- 设置某个场景的优先级顺序；
- 最终影响当前激活场景。

### `RefreshMainCamera`

作用：

- 刷新当前 `MainCamera` 引用。

### `MainCamera`

作用：

- 获取当前场景主摄像机缓存。

---

## 5. 关键调用链

## 5.1 模块注册调用链

```text
SceneComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<ISceneManager>(this)
    ↓
SceneComponent.OnInit()
```

---

## 5.2 场景加载调用链

```text
GameEntry.Scene.LoadScene(...)
    ↓
SceneComponent.LoadScene(...)
    ↓
状态校验（未加载 / 未在加载 / 未在卸载）
    ↓
_loadingSceneAssetNames.Add(sceneAssetName)
    ↓
LoadSceneInfo.Create(...)
    ↓
IResourceManager.LoadScene(...)
    ↓
Resource 模块真正加载场景
    ↓
SceneComponent.LoadSceneSuccessCallback / LoadSceneFailureCallback
```

---

## 5.3 场景卸载调用链

```text
GameEntry.Scene.UnloadScene(...)
    ↓
SceneComponent.UnloadScene(...)
    ↓
状态校验（必须已加载，且不能正在加载/卸载）
    ↓
_unloadingSceneAssetNames.Add(sceneAssetName)
    ↓
IResourceManager.UnloadScene(...)
    ↓
Resource 模块真正卸载场景
    ↓
SceneComponent.UnloadSceneSuccessCallback / UnloadSceneFailureCallback
```

---

## 5.4 激活场景切换调用链

```text
LoadSceneSuccessCallback(...)
    ↓
_sceneOrder 更新
    ↓
RefreshSceneOrder()
    ↓
从已加载场景中找最高顺序场景
    ↓
SetActiveScene(...)
    ↓
RefreshMainCamera()
```

---

## 6. 模块生命周期

## 6.1 SceneComponent 生命周期

### `Awake()`

作用：

- 把当前组件注册为 `ISceneManager` 模块；
- 记录框架场景 `_frameworkScene`。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<ISceneManager>(this);
    _frameworkScene = SceneManager.GetSceneAt(0);
}
```

### `OnInit()`

作用：

- 初始化：
  - 场景状态列表；
  - 加载/卸载回调函数集；
  - `_resourceManager` 占位。

### `Start()`

作用：

- 通过 `LFrameworkEntry.GetModule<IResourceManager>()` 获取资源模块引用。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 当前实现为空；
- Scene 模块主要通过回调驱动，而不是轮询驱动。

### `Shutdown()`

作用：

- 遍历当前已加载场景并发起卸载；
- 清理内部状态列表。

---

## 6.2 LoadSceneInfo 生命周期

这是当前 Scene 模块里最需要理解的辅助生命周期对象。

### 创建

```csharp
LoadSceneInfo.Create(userData, progressCallback, loadSuccessCallBack)
```

### 使用

在以下回调中被读取：

- `LoadSceneSuccessCallback(...)`
- `LoadSceneFailureCallback(...)`
- `LoadSceneUpdateCallback(...)`

### 清理

通过 `Clear()` 重置：

- `_progressCallback`
- `_loadSuccessCallBack`
- `_userData`

说明：

- 该对象实现了 `IReference`，理论上应在回调链结束后归还到 `ReferencePool`。

---

## 7. 是否存在需要继承的基类

结论：当前 Scene 模块没有面向业务层的“需要继承的基类”。

原因：

- `SceneComponent` 是 `sealed`；
- `ISceneManager` 是接口调用型模块；
- 业务层通常通过 `GameEntry.Scene` 直接使用，而不是派生子类。

因此：

- 不建议通过继承 Scene 模块扩展功能；
- 更适合通过：
  - 流程层封装；
  - 资源层配合；
  - 调用约束；
  - 工具方法
 进行扩展。

---

## 8. 典型使用方式

## 8.1 判断场景是否存在

```csharp
bool hasScene = GameEntry.Scene.HasScene("Assets/Scenes/Menu.unity");
```

---

## 8.2 加载场景

```csharp
GameEntry.Scene.LoadScene(
    "Assets/Scenes/Menu.unity",
    progress => { },
    success => { });
```

---

## 8.3 指定优先级加载场景

```csharp
GameEntry.Scene.LoadScene(
    "Assets/Scenes/Game.unity",
    10,
    progress => { },
    success => { });
```

---

## 8.4 卸载场景

```csharp
GameEntry.Scene.UnloadScene("Assets/Scenes/Menu.unity");
```

---

## 8.5 查询场景状态

```csharp
var loaded = GameEntry.Scene.GetLoadedSceneAssetNames();
var loading = GameEntry.Scene.GetLoadingSceneAssetNames();
var unloading = GameEntry.Scene.GetUnloadingSceneAssetNames();
```

---

## 9. 使用注意事项

### 9.1 Scene 模块依赖 Resource 模块

当前 Scene 模块不是独立工作的，底层加载依赖 `IResourceManager`。

因此：

- Resource 模块必须先可用；
- Scene 的成功/失败语义也受 Resource 模块影响。

---

### 9.2 `LoadScene` 会先做状态保护

同一个场景如果：

- 正在加载；
- 正在卸载；
- 已经加载；

再次调用时会直接抛异常。

---

### 9.3 `UnloadScene` 要求场景已经处于已加载状态

如果目标场景还没真正进入 `_loadedSceneAssetNames`，卸载会直接报错。

---

### 9.4 `SetSceneOrder` 只对已加载或正在加载的场景有效

如果场景既不在加载，也不在已加载列表中，会输出错误日志。

---

### 9.5 `MainCamera` 是缓存值

它依赖：

- `RefreshMainCamera()`
- 激活场景变更后的刷新逻辑

因此不是每时每刻自动实时查询的直接包装。

---

## 10. 总结

当前 `Scene` 模块可以概括为：

- 一个 `sealed` 的场景管理组件 `SceneComponent`；
- 一个统一管理接口 `ISceneManager`；
- 一个请求上下文对象 `LoadSceneInfo`；
- 一套围绕“加载状态列表 + 场景顺序 + 主摄像机刷新”的场景管理机制。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `SceneComponent` 如何维护 loaded/loading/unloading 三类状态；
2. `SceneComponent` 如何通过 `IResourceManager` 驱动底层场景加载；
3. `LoadSceneSuccess / Failure / Update` 回调如何改变场景状态和激活场景。
