# Resource 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Resource` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 模块生命周期；
- 是否存在需要继承或实现的扩展点。

---

## 2. 模块定位

`Resource` 模块是 LFramework 对 YooAsset 的统一封装，用于：

- 初始化资源系统和资源包；
- 请求包版本与更新清单；
- 创建资源下载器；
- 异步加载与卸载资源；
- 异步加载与卸载场景；
- 通过对象池缓存已加载资源；
- 定期回收未使用资源。

它在项目中的定位，是“框架级资源加载基础设施”。

---

## 3. 核心类型

## 3.1 ResourceComponent

文件：

- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Asset.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Pool.cs`

定义：

```csharp
public sealed partial class ResourceComponent : MonoBehaviour, ILFrameworkModule, IResourceManager
```

职责：

- 作为 Resource 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 初始化 YooAsset；
- 管理资源包、资源缓存、正在加载资源集合；
- 对外暴露 `IResourceManager`；
- 在 `OnUpdate()` 中驱动资源清理。

说明：

- `sealed`，当前模块不通过继承 `ResourceComponent` 扩展；
- 业务层通常通过 `GameEntry.Resource` 或模块接口访问。

---

## 3.2 IResourceManager

文件：

- `Assets/LFramework/Runtime/Component/Resource/IResourceManager.cs`

职责：

- 定义资源模块的统一对外接口；
- 覆盖以下几类能力：
  - 模块配置查询；
  - 包初始化；
  - 版本与下载；
  - 资源加载；
  - 场景加载；
  - 资源卸载；
  - 回收控制。

---

## 3.3 AssetObject

文件：

- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.AssetObject.cs`

定义：

```csharp
private sealed class AssetObject : ObjectBase
```

职责：

- 作为资源缓存对象池中的池对象；
- 保存资源实例对应的 `AssetHandle`；
- 当对象真正被池释放时，处理句柄释放。

说明：

- 该类型是 Resource 模块内部实现；
- 业务层不会直接使用它。

---

## 3.4 回调函数集类型

文件：

- `Callback/LoadAssetCallbacks.cs`
- `Callback/LoadSceneCallbacks.cs`
- `Callback/UnloadSceneCallbacks.cs`

职责：

- 把成功、失败、进度回调打包成统一参数对象；
- 作为资源和场景异步加载的对外通知方式。

---

## 3.5 资源状态与模式枚举

文件：

- `ResourceMode.cs`
- `HasAssetResult.cs`
- `LoadResourceStatus.cs`

职责：

- 表达模块运行模式；
- 表达资源是否存在；
- 表达加载成功或失败状态。

---

## 3.6 远端与加解密扩展点

文件：

- `ResourceComponent.Services.cs`

当前模块虽然没有“需要继承的基类”，但存在扩展接口实现点：

- `IRemoteServices`
- `IEncryptionServices`
- `IDecryptionServices`
- `IWebDecryptionServices`

说明：

- 如果后续要扩展远端地址策略或加解密方式，通常是实现这些接口，而不是继承 `ResourceComponent`。

---

## 4. 核心 API

## 4.1 模块配置与状态 API

| API                                         | 说明              |
| ------------------------------------------- | --------------- |
| `ResourceMode ResourceMode`                 | 当前资源模式          |
| `bool UpdatableWhilePlaying`                | 是否边玩边下          |
| `LoadResourceWayWebGL LoadResourceWayWebGL` | WebGL 本地/远端加载策略 |
| `EncryptionType EncryptionType`             | 当前加密方式          |
| `string UpdatePrefixUrl`                    | 更新地址            |
| `string FallbackUpdatePrefixUrl`            | 备用更新地址          |
| `string DefaultPackageName`                 | 默认资源包名          |
| `long Milliseconds`                         | YooAsset 每帧时间切片 |
| `float AssetAutoReleaseInterval`            | 资源对象池自动释放间隔     |
| `int AssetCapacity`                         | 资源对象池容量         |
| `float AssetExpireTime`                     | 资源对象池过期时间       |
| `int AssetPriority`                         | 资源对象池优先级        |
| `float MinUnloadUnusedAssetsInterval`       | 最小回收间隔          |
| `float MaxUnloadUnusedAssetsInterval`       | 最大回收间隔          |
| `string PackageVersion`                     | 当前记录的包版本        |
| `ResourceDownloaderOperation Downloader`    | 当前下载器           |

---

## 4.2 初始化与更新相关 API

### 初始化

```csharp
void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
void Initialize()
UniTask<InitializationOperation> InitPackage(string packageName)
```

说明：

- `Initialize()` 负责初始化 YooAsset 与默认包；
- `InitPackage(...)` 负责对某个资源包执行实际初始化。

### 版本与清单

```csharp
string GetPackageVersion(string customPackageName = "")
RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = false, int timeout = 60, string customPackageName = "")
UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60, string customPackageName = "")
```

### 下载与缓存清理

```csharp
ResourceDownloaderOperation CreateResourceDownloader(string customPackageName = "")
ClearCacheFilesOperation ClearCacheFilesAsync(EFileClearMode clearMode = EFileClearMode.ClearUnusedBundleFiles, string customPackageName = "")
```

---

## 4.3 资源查询与加载 API

### 资源查询

```csharp
HasAssetResult HasAsset(string assetName, string packageName = "")
bool CheckAssetValid(string assetName, string packageName = "")
```

说明：

- `HasAsset(...)` 用于判断资源是否存在、在本地还是远端；
- `CheckAssetValid(...)` 用于判断资源定位地址是否合法。

### 回调式加载

```csharp
void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "")
void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "")
```

### 泛型异步加载

```csharp
UniTaskVoid LoadAsset<T>(string assetName, Action<T> callback, string packageName = "") where T : UnityEngine.Object
UniTask<T> LoadAsset<T>(string assetName, int priority, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
```

### 已缓存资源获取

```csharp
T LoadExistAsset<T>(string assetName, string packageName = null) where T : UnityEngine.Object
```

说明：

- 该接口不会发起真实加载；
- 只会尝试从资源对象池里取已有资源。

---

## 4.4 资源卸载与回收 API

```csharp
void UnloadAsset(object asset)
void UnloadUnusedAssets()
void ForceUnloadAllAssets()
void ForceUnloadUnusedAssets(bool performGCCollect)
```

语义说明：

- `UnloadAsset(...)`：归还单个资源引用；
- `UnloadUnusedAssets()`：回收引用计数为 0 的资源；
- `ForceUnloadAllAssets()`：强制让包卸载全部资源；
- `ForceUnloadUnusedAssets(...)`：请求下一次更新周期执行资源清理。

---

## 4.5 场景加载相关 API

```csharp
void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, string packageName = "")
void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, string packageName = "")
void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData, string packageName = "")
void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData, string packageName = "")

void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks)
void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
```

说明：

- 当前资源模块的场景接口是资源层能力；
- 上层 `SceneComponent` 会再对其进行一层封装。

---

## 5. 关键调用链

## 5.1 模块启动调用链

```text
ResourceComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IResourceManager>(this)
    ↓
ResourceComponent.OnInit()
    ↓
ResourceComponent.Start()
    ↓
Initialize()
```

---

## 5.2 资源包初始化调用链

```text
业务层调用 InitPackage(packageName)
    ↓
根据 ResourceMode 选择 EPlayMode
    ↓
创建对应参数（Editor / Offline / Host / Web）
    ↓
package.InitializeAsync(...)
    ↓
await 初始化完成
```

---

## 5.3 资源加载调用链

```text
LoadAsset(...)
    ↓
CheckAssetValid(...)
    ↓
TryWaitingLoading(assetObjectKey)
    ↓
先尝试从 _assetPool.Spawn(assetObjectKey) 获取缓存资源
    ↓
若无缓存，则获取 AssetHandle
    ↓
等待 handle 完成
    ↓
包装为 AssetObject
    ↓
_assetPool.Register(assetObject, true)
```

---

## 5.4 资源卸载调用链

```text
UnloadAsset(asset)
    ↓
_assetPool.Unspawn(asset)
```

说明：

- 这里只是归还到资源对象池；
- 不一定立刻销毁资源。

---

## 5.5 未使用资源回收调用链

```text
OnUpdate(...)
    ↓
达到回收条件
    ↓
Resources.UnloadUnusedAssets()
    ↓
UnloadUnusedAssets()
    ↓
_assetPool.ReleaseAllUnused()
    ↓
每个 package.UnloadUnusedAssetsAsync()
```

---

## 5.6 场景加载调用链

```text
LoadScene(...)
    ↓
YooAssets.LoadSceneAsync(...)
    ↓
绑定 Completed
    ↓
成功后触发 LoadSceneCallbacks
    ↓
缓存到 _subScenes
```

---

## 5.7 场景卸载调用链

```text
UnloadScene(...)
    ↓
从 _subScenes 找到 SceneHandle
    ↓
执行 UnloadAsync()
    ↓
完成后移除 _subScenes 记录
    ↓
触发 UnloadSceneCallbacks
```

---

## 6. 模块生命周期

## 6.1 ResourceComponent 生命周期

### `Awake()`

作用：

- 将当前组件注册为 `IResourceManager` 模块。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IResourceManager>(this);
}
```

### `OnInit()`

作用：

- 初始化模块内部缓存容器：
  - `_packageDict`
  - `_assetInfoDict`
  - `_assetLoadingHashSet`
  - `_subScenes`

### `Start()`

作用：

- 从配置模块读取更新地址；
- 调用 `Initialize()` 启动资源系统。

### `Initialize()`

作用：

- 初始化 YooAsset；
- 创建或设置默认资源包；
- 绑定对象池管理器。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 定时触发未使用资源清理；
- 跟踪 `Resources.UnloadUnusedAssets()` 的完成；
- 按需执行 `GC.Collect()`。

### `Shutdown()`

作用：

- 当前实现为空；
- 说明模块关闭逻辑还没有完全封装在此处。

---

## 6.2 AssetObject 生命周期

这是 Resource 模块内部资源缓存对象的生命周期。

### 创建

```csharp
AssetObject assetObject = ReferencePool.Acquire<AssetObject>();
assetObject.Initialize(name, target);
assetObject._assetHandle = handle;
```

### 被资源池取出

- 走 `ObjectBase.OnSpawn()`；
- 当前 `AssetObject` 没有覆写，因此没有额外逻辑。

### 被资源池归还

- 走 `ObjectBase.OnUnspawn()`；
- 当前 `AssetObject` 也没有覆写。

### 被资源池真正释放

- 调用：

```csharp
protected internal override void Release(bool isShutdown)
```

- 当前逻辑在非关闭路径下释放 `AssetHandle`。

### 回收到引用池

- 调用 `Clear()`；
- 清空 `_assetHandle` 与基础字段。

---

## 7. 是否存在需要继承的基类

## 7.1 Resource 模块自身

结论：当前 `Resource` 模块本身没有面向业务的“必须继承的基类”。

原因：

- `ResourceComponent` 是 `sealed`；
- 业务层通常通过 `IResourceManager` 调用接口；
- 加载行为通过回调或 `UniTask` 获取结果。

因此：

- 不建议通过继承 `ResourceComponent` 扩展资源模块；
- 应优先通过接口、回调、配置和服务实现扩展。

---

## 7.2 可扩展接口

如果需要扩展资源系统行为，当前更合理的方式是实现以下接口：

### `IDecryptionServices`

用途：

- 自定义本地包解密逻辑。

当前 Resource 模块中对应实现示例：

- `FileStreamDecryption`
- `FileOffsetDecryption`

### `IWebDecryptionServices`

用途：

- 自定义 Web 平台资源包解密逻辑。

当前实现示例：

- `FileOffsetWebDecryption`
- `FileStreamWebDecryption`

### `IEncryptionServices`

用途：

- 自定义资源打包时的加密逻辑。

当前实现示例：

- `FileStreamEncryption`
- `FileOffsetEncryption`

### `IRemoteServices`

用途：

- 自定义远端资源地址生成逻辑。

当前实现示例：

- `RemoteServices`

说明：

- 这些是接口实现点，不是带生命周期的业务基类；
- 因此没有类似 `OnInit / OnEnter / OnUpdate` 这种继承生命周期。

---

## 8. 典型使用方式

## 8.1 初始化资源包

```csharp
await GameEntry.Resource.InitPackage("DefaultPackage");
```

---

## 8.2 请求包版本并更新清单

```csharp
var request = GameEntry.Resource.RequestPackageVersionAsync();
await request.ToUniTask();

var update = GameEntry.Resource.UpdatePackageManifestAsync(request.PackageVersion);
await update.ToUniTask();
```

---

## 8.3 加载资源

```csharp
GameEntry.Resource.LoadAsset("Assets/Some.prefab", 0,
    new LoadAssetCallbacks(OnLoadSuccess, OnLoadFailure, OnLoadUpdate),
    null);
```

---

## 8.4 泛型加载资源

```csharp
var prefab = await GameEntry.Resource.LoadAsset<GameObject>("Assets/Some.prefab", 0);
```

---

## 8.5 卸载资源

```csharp
GameEntry.Resource.UnloadAsset(prefab);
```

---

## 8.6 加载场景

```csharp
GameEntry.Resource.LoadScene("Assets/Scenes/Main.unity",
    new LoadSceneCallbacks(OnSceneSuccess, OnSceneFailure, OnSceneUpdate));
```

---

## 8.7 卸载场景

```csharp
GameEntry.Resource.UnloadScene("Assets/Scenes/Main.unity",
    new UnloadSceneCallbacks(OnSceneUnloadSuccess, OnSceneUnloadFailure));
```

---

## 9. 使用注意事项

### 9.1 `UnloadAsset` 不等于立即销毁资源

当前语义是：

- 先把资源归还到对象池；
- 真正释放取决于对象池回收和包卸载时机。

---

### 9.2 `LoadExistAsset<T>` 只查缓存

如果对象池里没有该资源，它不会主动加载。

---

### 9.3 资源模块依赖对象池模块

`Initialize()` 中会调用：

```csharp
SetObjectPoolManager(LFrameworkEntry.GetModule<IObjectPoolManager>())
```

因此：

- ObjectPool 模块必须先正常注册并可获取。

---

### 9.4 场景加载本质上也是资源系统的一部分

当前场景加载不是由 `SceneComponent` 直接处理底层资源，而是由 `ResourceComponent` 提供资源层能力，再由 `SceneComponent` 上层组织状态。

---

## 10. 总结

当前 `Resource` 模块可以概括为：

- 一个 `sealed` 的资源管理组件 `ResourceComponent`；
- 一个统一管理接口 `IResourceManager`；
- 一组资源、场景、下载、清理相关 API；
- 一个内部资源池对象 `AssetObject`；
- 一组可扩展的远端与加解密服务接口。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `ResourceComponent` 如何初始化 YooAsset 与资源包；
2. `LoadAsset / UnloadAsset` 如何与 `_assetPool` 协同工作；
3. `LoadScene / UnloadScene` 如何与上层 `SceneComponent` 对接。
