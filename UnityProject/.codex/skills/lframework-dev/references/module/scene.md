# Scene

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Scene` 管理场景加载状态，不要直接调用 YooAsset 或 Unity `SceneManager` 加载/卸载业务场景。
- 按项目现有约定，场景资源名通过 `AssetUtility.GetSceneAsset(sceneName)` 构建，例如 `Assets/GameResRaw/Scene/Menu`。
- `GameEntry.Scene.LoadScene(sceneAssetName, progressCallback = null, loadSuccessCallBack = null)`：按默认优先级异步加载场景；进度通过 `Action<float>` 回调，完成结果通过 `Action<bool>` 回调。
- `GameEntry.Scene.LoadScene(sceneAssetName, priority, ...)`：需要指定资源加载优先级时使用。
- `GameEntry.Scene.LoadScene(sceneAssetName, userData, ...)` 或完整重载：需要把自定义数据传入底层加载流程时使用。
- `GameEntry.Scene.UnloadScene(sceneAssetName)`：卸载已由 Scene 模块加载并跟踪的场景。
- `SceneIsLoaded(sceneAssetName)`、`SceneIsLoading(sceneAssetName)`、`SceneIsUnloading(sceneAssetName)`：发起加载或卸载前检查状态，避免重复加载、重复卸载或在加载中卸载。
- 当前 GameLogic 场景切换流程通过 `ProcedureChangeScene` 统一卸载已加载场景、加载目标场景，并使用 FSM 数据传递目标场景名。

## 注意事项

- `SceneComponent` 只维护由自身加载、卸载的场景状态列表：已加载、正在加载、正在卸载。
- `LoadScene` 会拒绝空场景名、正在卸载、正在加载或已加载的同名场景；这些情况会抛出 `LFrameworkException`。
- `UnloadScene` 会拒绝空场景名、正在卸载、正在加载或尚未加载的场景；这些情况会抛出 `LFrameworkException`。
- `HasScene(sceneAssetName)` 通过 `IResourceManager.HasAsset(sceneAssetName)` 判断资源是否存在；资源管理器为空时会抛出异常。
- 底层 `ResourceComponent.LoadScene` 使用 YooAsset `LoadSceneAsync` 并以 `LoadSceneMode.Additive` 加载子场景。
- 加载成功后，Scene 模块会移除 loading 状态、记录 loaded 状态、刷新场景顺序和主摄像机，并释放内部 `LoadSceneInfo` 引用对象。
- 加载失败后，Scene 模块会移除 loading 状态、记录错误、调用完成回调 `false`，并释放内部 `LoadSceneInfo` 引用对象。
- 卸载成功后，Scene 模块会移除 unloading 和 loaded 状态，清理场景顺序并刷新激活场景。
- `Shutdown()` 会尝试卸载当前已加载且未处于卸载中的场景，然后清空三类状态列表。
- `SetSceneOrder(sceneAssetName, sceneOrder)`、`RefreshMainCamera()` 和 `MainCamera` 当前是 `SceneComponent` 的 public 成员，不在 `ISceneManager` 接口中；通过 `GameEntry.Scene` 不能直接访问。

## ISceneManager API 速查

仅在框架集成代码或 `GameEntry.Scene` facade 内部优先考虑直接使用 `ISceneManager`。

- 已加载状态：`SceneIsLoaded(sceneAssetName)`、`GetLoadedSceneAssetNames()`、`GetLoadedSceneAssetNames(List<string> results)`。
- 加载中状态：`SceneIsLoading(sceneAssetName)`、`GetLoadingSceneAssetNames()`、`GetLoadingSceneAssetNames(List<string> results)`。
- 卸载中状态：`SceneIsUnloading(sceneAssetName)`、`GetUnloadingSceneAssetNames()`、`GetUnloadingSceneAssetNames(List<string> results)`。
- 资源存在性：`HasScene(sceneAssetName)`。
- 加载：`LoadScene(sceneAssetName, progressCallback = null, loadSuccessCallBack = null)`。
- 指定优先级加载：`LoadScene(sceneAssetName, priority, progressCallback = null, loadSuccessCallBack = null)`。
- 携带用户数据加载：`LoadScene(sceneAssetName, userData, progressCallback = null, loadSuccessCallBack = null)`。
- 完整加载重载：`LoadScene(sceneAssetName, priority, userData, progressCallback = null, loadSuccessCallBack = null)`。
- 卸载：`UnloadScene(sceneAssetName)`、`UnloadScene(sceneAssetName, userData)`。

## 源码路径

- `Assets/LFramework/Runtime/Component/Scene/ISceneManager.cs`
- `Assets/LFramework/Runtime/Component/Scene/SceneComponent.cs`
- `Assets/LFramework/Runtime/Component/Scene/LoadSceneInfo.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`
