# Scene

Source paths:
- `Assets/LFramework/Runtime/Component/Scene/SceneComponent.cs`
- `Assets/LFramework/Runtime/Component/Scene/ISceneManager.cs`
- `Assets/LFramework/Runtime/Component/Scene/LoadSceneInfo.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`

`SceneComponent` registers `ISceneManager` and manages high-level scene state while delegating actual YooAsset scene operations to `ResourceComponent`.

Responsibility:

- Track loaded, loading, and unloading scene asset names.
- Validate duplicate load/unload state.
- Load and unload scenes through `IResourceManager`.
- Refresh scene order and main camera.

Lifecycle:

- `Awake()` registers `ISceneManager`.
- `Start()` obtains `IResourceManager`.
- `OnUpdate()` is light in current implementation.
- `Shutdown()` clears scene tracking state.

Typical flow:

```text
GameEntry.Scene.LoadScene(sceneAssetName)
  -> SceneComponent records loading
  -> ResourceComponent.LoadScene(...)
  -> YooAssets.LoadSceneAsync(...)
  -> success callback records loaded and refreshes order/camera
```

Usage:

- Use `AssetUtility.GetSceneAsset(sceneName)` in GameLogic when following existing project patterns.
- Query `SceneIsLoaded`, `SceneIsLoading`, and `SceneIsUnloading` before issuing conflicting operations.
- Use `SetSceneOrder()` when additive scene order matters.

Cleanup rules:

- Unload through `GameEntry.Scene.UnloadScene()` so tracking dictionaries stay consistent.
- Do not call YooAsset scene unload directly from business code.
- Procedure scene transitions should store scene names in FSM data and let the scene procedure execute the change.
