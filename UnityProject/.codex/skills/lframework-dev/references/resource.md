# Resource

Source paths:
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Asset.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Pool.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`
- `Assets/LFramework/Runtime/Component/Resource/IResourceManager.cs`
- `Assets/GameScripts/GameLogic/Component/Resource/ResourceContainer.cs`

`ResourceComponent` registers `IResourceManager` and wraps YooAsset packages, asset loading, asset pooling, downloader operations, scene loading, and unload policies. `ResourceContainer` is the GameLogic owner-scoped asset lifecycle helper.

Responsibility:

- Initialize YooAsset and resource packages.
- Load assets by callback or `UniTask<T>`.
- Reuse loaded assets through the framework object pool.
- Unload individual assets and unused assets.
- Create package version/manifest/downloader operations.

Lifecycle:

- `Awake()` registers `IResourceManager`.
- `Start()` reads `IConfigManager`, initializes YooAsset, and sets the object pool manager.
- `OnUpdate()` performs interval-based unused asset cleanup.
- `Shutdown()` unloads assets and clears resource state.

Preferred GameLogic usage:

```csharp
_resourceContainer = ResourceContainer.Create(this);
var prefab = await _resourceContainer.LoadAsset<GameObject>(assetName);

ReferencePool.Release(_resourceContainer);
_resourceContainer = null;
```

Direct usage:

- `GameEntry.Resource.LoadAsset<T>(assetName, priority, token, packageName)`
- pair direct loads with `GameEntry.Resource.UnloadAsset(asset)`
- use `LoadExistAsset<T>()` only when the asset is already loaded into the asset pool

Cleanup rules:

- `ResourceContainer.Clear()` unloads all tracked assets and cancels pending async loads.
- Release containers before Resource and ObjectPool shutdown.
- For UI assets, prefer `UguiForm.LoadAssetAsync()` or owner container patterns.

Conflict check:

- Resource APIs have many overloads. Verify `IResourceManager.cs` and `ResourceComponent.Asset.cs` before editing call sites.
