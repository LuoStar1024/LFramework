# UI

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.UI` 和 `UIExtension`，不要直接实例化、销毁或移动已纳入 UI 管理的窗口实例。
- `GameEntry.UI.OpenUIForm(uiFormId, userData = null)`：读取 `TbUIForm`，通过 `AssetUtility` 构建 UI 资源路径，检查 `AllowMultiInstance`，并按配置表的组名、暂停策略和 UI 资源优先级打开窗口。
- `GameEntry.UI.OpenUIForm(uiFormAssetName, uiGroupName, userData)`：已明确资源路径和 UI 组时使用，返回 UI 序列编号。
- `await GameEntry.UI.OpenUIFormAwait(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData)`：需要等待 UI 资源加载和打开流程进入队列时使用。
- `GameEntry.UI.CloseUIForm(serialId)`, `CloseUIForm(uiForm)`：关闭已加载窗口；`UguiForm` 内部优先调用 `Close()`。
- 业务窗口优先继承 `UguiForm`，在 `OnOpen(object userData)` 绑定数据和订阅事件，在 `OnClose(bool isShutdown, object userData)` 或 `OnRecycle()` 释放临时状态。
- UI 自有资源优先使用 `UguiForm.LoadAssetAsync<T>()`, `UnloadAsset(asset)`, `UnloadAllAssets()`，让窗口生命周期统一释放资源。
- UI 事件订阅优先使用 `UguiForm.Subscribe(...)`，关闭时由 `UnsubscribeAll()` 清理。
- UIWidget 通过所属 `UguiForm` 的 `AddUIWidget()`, `OpenUIWidget()`, `DynamicOpenUIWidget()`, `CloseUIWidget()` 管理，保持 `UIWidgetContainer` 状态一致。

## 注意事项

- `UIComponent` 在 `Awake()` 注册 `IUIManager`，在 `Start()` 获取 `IResourceManager` 和 `IObjectPoolManager`；打开 UI 前必须已经添加对应 UI 组。
- UI 组通常在启动流程中通过 `GameEntry.UI.AddUIGroup(groupName, depth)` 创建，组名应使用项目常量或 `TbUIForm` 配置，不要写散落的字符串。
- `UIExtension.OpenUIForm(int uiFormId, object userData = null)` 找不到配置、重复打开非多实例 UI、或同资源正在加载时返回 `null`；调用方需要处理空结果。
- `IUIManager.OpenUIForm(...)` 返回序列编号，不代表 UI 逻辑已经完成业务初始化；需要等待时使用 `OpenUIFormAwait(...)`。
- `CloseUIForm(serialId)` 遇到仍在加载中的 UI 会标记加载完成后释放；不要自行销毁加载中的 prefab 或实例。
- `UguiForm.OnClose()` 会关闭 Widget、取消事件订阅并卸载窗口自有资源；覆写时应调用 `base.OnClose(isShutdown, userData)`，并保持资源和订阅释放路径完整。
- `UguiForm.OnDestroy()` 会清理 Widget、EventContainer 和 ResourceContainer；不要绕过 `GameEntry.UI.CloseUIForm()` 直接 `Destroy` 由 UI 管理器创建的窗口。
- Widget 必须先 `AddUIWidget()` 再 `OpenUIWidget()`；`OpenUIWidget()` 不刷新深度，运行时动态打开并需要刷新深度时使用 `DynamicOpenUIWidget()`。
- UI 深度由 `UIGroup.DepthFactor`、`UguiForm.DepthFactor` 和窗口在组内的顺序共同计算，手动修改 Canvas sorting order 可能破坏 UI 层级。
- UI 资源加载、实例复用和释放依赖 `IResourceManager` 与 UI 实例对象池；调整打开、关闭或回收逻辑前先核对资源所有权和对象池释放顺序。

## IUIManager API 速查

仅在框架集成代码、UI 基础设施或需要底层 UI 管理能力时优先考虑直接使用 `IUIManager`。

- UI 组：`UIGroupCount`, `HasUIGroup(uiGroupName)`, `GetUIGroup(uiGroupName)`, `GetAllUIGroups()`, `GetAllUIGroups(results)`, `AddUIGroup(uiGroupName[, uiGroupDepth])`。
- UI 查询：`HasUIForm(serialId)`, `HasUIForm(uiFormAssetName)`, `GetUIForm(serialId)`, `GetUIForm(uiFormAssetName)`, `GetUIForms(uiFormAssetName[, results])`, `GetAllLoadedUIForms([results])`。
- 加载状态：`GetAllLoadingUIFormSerialIds([results])`, `IsLoadingUIForm(serialId)`, `IsLoadingUIForm(uiFormAssetName)`, `IsValidUIForm(uiForm)`。
- 打开 UI：`OpenUIForm(uiFormAssetName, uiGroupName, ...)` 返回序列编号。根据加载优先级、是否暂停被覆盖窗口和 `userData` 需求选择匹配重载。
- 等待打开：`OpenUIFormAwait(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData)` 返回 `UniTask<int>`。
- 关闭 UI：`CloseUIForm(serialId[, userData])`, `CloseUIForm(uiForm[, userData])`, `CloseAllLoadedUIForms([userData])`, `CloseAllLoadingUIForms()`。
- 焦点控制：`RefocusUIForm(uiForm[, userData])`。
- 实例池设置：`InstanceAutoReleaseInterval`, `InstanceCapacity`, `InstanceExpireTime`, `InstancePriority`, `SetUIFormInstanceLocked(uiFormInstance, locked)`, `SetUIFormInstancePriority(uiFormInstance, priority)`。
- 模块依赖注入：`SetObjectPoolManager(objectPoolManager)`, `SetResourceManager(resourceManager)`。

## 源码路径

- `Assets/GameScripts/GameLogic/Component/UI/IUIManager.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIComponent.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIExtension.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIGroup.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIForm.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIFormLogic.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UguiForm.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIWidget.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIWidgetContainer.cs`
