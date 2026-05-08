# 资源

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Resource` 和带所有者生命周期的 `ResourceContainer`，不要直接绕过框架访问 YooAsset。
- 带生命周期的对象应通过 `ResourceContainer.Create(owner)` 创建容器，并在对象关闭、回收或销毁时调用 `ReferencePool.Release(resourceContainer)`。
- `_resourceContainer.LoadAsset<T>(assetName, priority = 0, packageName = "")`：异步加载资源，内部调用 `GameEntry.Resource.LoadAsset<T>()`，并记录已加载资源，便于统一释放。
- `_resourceContainer.UnloadAsset(asset)`：释放单个已由容器跟踪的资源，并从容器记录中移除。
- `_resourceContainer.UnloadAllAssets()` 或 `ReferencePool.Release(_resourceContainer)`：释放容器跟踪的全部资源，并取消未完成的异步加载。
- UI 窗口内部优先使用 `UguiForm.LoadAssetAsync<T>()`, `UnloadAsset(asset)`, `UnloadAllAssets()`，让 UI 生命周期负责资源释放。
- 已确认资源已经加载进资源池时，才使用 `GameEntry.Resource.LoadExistAsset<T>(assetName, packageName)`；典型场景是 DataTable 在初始化后读取已加载的 `TextAsset`。

```csharp
_resourceContainer = ResourceContainer.Create(this);
var prefab = await _resourceContainer.LoadAsset<GameObject>(assetName);

ReferencePool.Release(_resourceContainer);
_resourceContainer = null;
```

## 注意事项

- `ResourceContainer` 实现 `IReference`，必须通过 `ReferencePool.Acquire` 间接创建，并通过 `ReferencePool.Release()` 回收；不要手动 `new ResourceContainer()`。
- `ResourceContainer.Clear()` 会调用 `UnloadAllAssets()`，释放已记录资源、取消并释放内部 `CancellationTokenSource`，然后清空 `Owner`。
- 直接调用 `GameEntry.Resource.LoadAsset<T>()` 时，调用方必须明确资源所有权，并在不再使用时配对调用 `GameEntry.Resource.UnloadAsset(asset)`。
- `LoadExistAsset<T>()` 只从资源对象池中取已存在资源；资源尚未加载时返回 `null`，不会触发新的异步加载。
- `LoadAsset<T>()` 在资源定位无效或取消/失败时可能返回 `null`，调用方需要处理空值。
- 资源定位地址和资源包名称会参与缓存 Key；非默认包资源要传入正确 `packageName`。
- `ResourceComponent.Start()` 会读取 `IConfigManager` 的资源下载地址并初始化 YooAsset；启动或热更流程中调整资源初始化前，必须先核对 Launcher 和资源包流程。
- `ResourceComponent.OnUpdate()` 会按间隔触发 `Resources.UnloadUnusedAssets()` 和资源对象池未使用资源释放；不要把它等同于具体业务资源的所有权释放。
- 场景加载接口在 `IResourceManager` 中存在，但普通业务流程优先通过 `GameEntry.Scene` 管理场景生命周期。
- WebGL 下 `ForceUnloadAllAssets()` 不执行实际强制卸载，只输出 warning。
- 资源 API 重载较多，编辑调用点前先核对 `IResourceManager.cs`、`ResourceComponent.Asset.cs` 和对应调用场景。

## IResourceManager API 速查

仅在框架集成代码、模块内部或确实需要底层资源能力时优先考虑直接使用 `IResourceManager`。

- 基础状态：`ResourceMode`, `UpdatableWhilePlaying`, `LoadResourceWayWebGL`, `EncryptionType`, `DefaultPackageName`, `Milliseconds`。
- 更新地址和版本：`UpdatePrefixUrl`, `FallbackUpdatePrefixUrl`, `ApplicableGameVersion`, `InternalResourceVersion`, `PackageVersion`, `GetPackageVersion(customPackageName = "")`。
- 对象池参数：`AssetAutoReleaseInterval`, `AssetCapacity`, `AssetExpireTime`, `AssetPriority`, `MinUnloadUnusedAssetsInterval`, `MaxUnloadUnusedAssetsInterval`。
- 初始化：`SetObjectPoolManager(objectPoolManager)`, `Initialize()`, `InitPackage(packageName)`。
- 资源包更新：`RequestPackageVersionAsync(appendTimeTicks = false, timeout = 60, customPackageName = "")`, `UpdatePackageManifestAsync(packageVersion, timeout = 60, customPackageName = "")`。
- 下载与缓存：`Downloader`, `CreateResourceDownloader(customPackageName = "")`, `ClearCacheFilesAsync(clearMode, customPackageName = "")`。
- 资源检查：`HasAsset(assetName, packageName = "")`, `CheckAssetValid(assetName, packageName = "")`。
- 资源加载：`LoadAsset(assetName, priority, loadAssetCallbacks, userData, packageName = "")`, `LoadAsset(assetName, assetType, priority, loadAssetCallbacks, userData, packageName = "")`。
- 泛型加载：`LoadAsset<T>(assetName, callback, packageName = "")`, `LoadAsset<T>(assetName, priority, cancellationToken = default, packageName = "")`。
- 已加载资源获取：`LoadExistAsset<T>(assetName, packageName = null)`。
- 资源卸载：`UnloadAsset(asset)`, `UnloadUnusedAssets()`, `ForceUnloadAllAssets()`, `ForceUnloadUnusedAssets(performGCCollect)`。
- 场景加载：`LoadScene(sceneAssetName, loadSceneCallbacks, packageName = "")`, `LoadScene(sceneAssetName, priority, loadSceneCallbacks, packageName = "")`, `LoadScene(sceneAssetName, loadSceneCallbacks, userData, packageName = "")`, `LoadScene(sceneAssetName, priority, loadSceneCallbacks, userData, packageName = "")`。
- 场景卸载：`UnloadScene(sceneAssetName, unloadSceneCallbacks)`, `UnloadScene(sceneAssetName, unloadSceneCallbacks, userData)`。

## 源码路径

- `Assets/GameScripts/GameLogic/Component/Resource/ResourceContainer.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UguiForm.cs`
- `Assets/LFramework/Runtime/Component/Resource/IResourceManager.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Asset.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Pool.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`
