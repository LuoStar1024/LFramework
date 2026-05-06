# UI 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `UI` 模块的：

- 核心类型；
- 对外 API；
- 在框架中的接入方式；
- UI 窗口、窗口组和 Widget 的核心生命周期；
- 资源、事件、对象池相关释放规则；
- 业务 UI 编写时需要理解的基类 / 接口。

---

## 2. 模块定位

`UI` 是 GameLogic 层的界面管理模块，路径为：

- `Assets/GameScripts/GameLogic/Component/UI`

它属于热更业务侧组件，不在 `Assets/LFramework/Runtime` 框架核心层中实现。模块通过 `IUIManager` 注册进 LFramework 模块系统，并最终由 `GameEntry.UI` 暴露给业务逻辑使用。

主要职责：

- 管理 UI 分组；
- 管理 UI 窗口打开、关闭、查找、激活；
- 通过资源系统加载 UI 预制体；
- 通过对象池复用 UI 实例；
- 转发 UI 生命周期到 `UIFormLogic` / `UguiForm`；
- 管理 `UIWidget` 子界面；
- 为 `UguiForm` 提供事件订阅和资源持有辅助。

如果把 `LFramework/Runtime/Component` 看作框架内置模块层，那么当前 `UI` 模块就是 GameLogic 侧对 UI 管理能力的一层业务模块实现。

---

## 3. 核心类型

## 3.1 UIComponent

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIComponent.cs`

定义：

```csharp
public sealed partial class UIComponent : MonoBehaviour, ILFrameworkModule, IUIManager, IUIRelease
```

职责：

- 注册 `IUIManager` 模块；
- 持有所有 `UIGroup`；
- 打开、关闭、查询、激活 UIForm；
- 管理加载中 UI、延迟关闭 UI；
- 创建并配置 UI 实例对象池；
- 通过 `IResourceManager` 加载 UI 资源；
- 在 `OnUpdate()` 中回收关闭后的 UI 实例；
- 在 `Shutdown()` 中关闭所有 UI 并释放对象池未使用实例。

核心接入点：

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IUIManager>(this);
}
```

说明：

- `RegisterModule<IUIManager>()` 的泛型参数是接口类型；
- 注册后会立即触发 `UIComponent.OnInit()`；
- 资源模块和对象池模块依赖在 Unity `Start()` 中获取。

---

## 3.2 IUIManager

文件：

- `Assets/GameScripts/GameLogic/Component/UI/IUIManager.cs`

定义：

```csharp
public interface IUIManager
```

职责：

- 暴露 UI 模块的主要对外 API；
- 业务侧通常通过 `GameEntry.UI` 访问；
- 管理 UIGroup、UIForm、加载状态、对象池参数。

主要能力：

- UIGroup 增加、查询、枚举；
- UIForm 查询；
- UIForm 打开、关闭、关闭全部；
- 正在加载 UI 的状态查询；
- UI 实例对象池锁定、优先级设置；
- 可等待打开 UI：`OpenUIFormAwait(...)`。

---

## 3.3 UIGroup / IUIGroup

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIGroup.cs`
- `Assets/GameScripts/GameLogic/Component/UI/IUIGroup.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIGroup.UIFormInfo.cs`

定义：

```csharp
public sealed partial class UIGroup : MonoBehaviour, IUIGroup
public interface IUIGroup
```

职责：

- 表示一个 UI 层级组；
- 维护组内 UIForm 链表；
- 管理组深度、暂停状态、当前窗口；
- 根据窗口覆盖关系触发 `OnPause()`、`OnResume()`、`OnCover()`、`OnReveal()`；
- 每帧只更新未暂停段内的窗口。

深度规则：

```csharp
public const int DepthFactor = 1000;
```

组深度会影响 Canvas 排序：

```csharp
_cachedCanvas.sortingOrder = DepthFactor * _depth;
```

---

## 3.4 UIForm / IUIForm

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIForm.cs`
- `Assets/GameScripts/GameLogic/Component/UI/IUIForm.cs`

定义：

```csharp
public sealed class UIForm : MonoBehaviour, IUIForm
public interface IUIForm
```

职责：

- 作为 UI 预制体实例上的运行时包装组件；
- 保存窗口序列号、资源名、所属 UIGroup、组内深度、暂停覆盖标记；
- 获取并持有真正业务逻辑组件 `UIFormLogic`；
- 将生命周期调用转发给 `UIFormLogic`。

重要行为：

- 新实例首次打开时才调用 `UIFormLogic.OnInit(userData)`；
- 对象池复用的 UIForm 再次打开时不会重复调用 `UIFormLogic.OnInit()`；
- 每次打开都会调用 `OnOpen(userData)`；
- 每次关闭都会调用 `OnClose(isShutdown, userData)`；
- 回收到对象池前调用 `OnRecycle()`。

---

## 3.5 UIFormLogic

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIFormLogic.cs`

定义：

```csharp
public abstract class UIFormLogic : MonoBehaviour
```

职责：

- UI 业务逻辑基类；
- 维护 UI 可用状态、可见状态、缓存 Transform、原始 Layer；
- 定义窗口生命周期虚函数；
- 默认通过 `gameObject.SetActive(visible)` 控制可见性。

核心生命周期方法：

```csharp
protected internal virtual void OnInit(object userData)
protected internal virtual void OnRecycle()
protected internal virtual void OnOpen(object userData)
protected internal virtual void OnClose(bool isShutdown, object userData)
protected internal virtual void OnPause()
protected internal virtual void OnResume()
protected internal virtual void OnCover()
protected internal virtual void OnReveal()
protected internal virtual void OnRefocus(object userData)
protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
```

---

## 3.6 UguiForm

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UguiForm.cs`

定义：

```csharp
public class UguiForm : UIFormLogic
```

职责：

- 项目业务 UI 的主要继承基类；
- 自动补齐 Canvas、GraphicRaycaster、RectTransform 铺满布局；
- 根据 UIGroup 深度和组内深度调整自身及子 Canvas / 粒子排序；
- 提供关闭 UI、播放 UI 音效、Widget 管理、事件订阅、资源加载辅助。

核心辅助能力：

- `Close()`
- `PlayUISound(int uiSoundId)`
- `AddUIWidget(...)`
- `OpenUIWidget(...)`
- `DynamicOpenUIWidget(...)`
- `CloseUIWidget(...)`
- `Subscribe(...)`
- `Unsubscribe(...)`
- `UnsubscribeAll()`
- `LoadAssetAsync<T>(string assetName)`
- `UnloadAsset(UnityEngine.Object asset)`
- `UnloadAllAssets()`

重要释放规则：

- `OnClose()` 会关闭 Widget；
- `OnClose()` 会调用 `UnsubscribeAll()`；
- `OnClose()` 会调用 `UnloadAllAssets()`；
- `isShutdown == true` 时，会进一步移除所有 Widget 并释放 `EventContainer`、`ResourceContainer`、`UIWidgetContainer`。

---

## 3.7 UIWidget / UIWidgetContainer

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIWidget.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIWidgetContainer.cs`

定义：

```csharp
public class UIWidget : MonoBehaviour
public class UIWidgetContainer : IReference
```

职责：

- `UIWidget` 是窗口内部可开关的子界面逻辑单元；
- `UIWidgetContainer` 归属于一个 `UguiForm`；
- 负责 Widget 添加、移除、打开、关闭和生命周期转发；
- `UIWidgetContainer` 通过 `ReferencePool` 获取和释放。

---

## 3.8 UIExtension

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIExtension.cs`

定义：

```csharp
public static partial class UIExtension
```

职责：

- 提供基于 Luban UIForm 配置表的便捷 API；
- 通过 `uiFormId` 查询 `GameEntry.DataTable.TbUIForm`；
- 根据配置里的 `AssetName`、`GroupName`、`AllowMultiInstance`、`PauseCoveredForm` 打开或查询 UI。

典型 API：

```csharp
public static bool HasUIForm(this IUIManager uiComponent, int uiFormId, string uiGroupName = null)
public static UIForm GetUIForm(this IUIManager uiComponent, int uiFormId, string uiGroupName = null)
public static int? OpenUIForm(this IUIManager uiComponent, int uiFormId, object userData = null)
```

依赖配置字段来自生成代码：

- `Assets/GameScripts/GameDataTable/DataTableCode/UIForm.cs`

字段：

- `AssetName`
- `GroupName`
- `AllowMultiInstance`
- `PauseCoveredForm`

---

## 3.9 OpenUIFormInfo

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIComponent.OpenUIFormInfo.cs`

定义：

```csharp
private sealed class OpenUIFormInfo : IReference
```

职责：

- 作为异步加载 UI 时的上下文对象；
- 保存序列号、目标 UIGroup、是否暂停被覆盖窗口、userData；
- 通过 `ReferencePool.Acquire<OpenUIFormInfo>()` 创建；
- 加载成功、失败或取消后通过 `ReferencePool.Release(...)` 回收。

---

## 3.10 UIFormInstanceObject

文件：

- `Assets/GameScripts/GameLogic/Component/UI/UIComponent.UIFormInstanceObject.cs`

定义：

```csharp
private sealed class UIFormInstanceObject : ObjectBase
```

职责：

- 包装 UI 实例对象池中的实例；
- 继承 `ObjectBase`，接入 `IObjectPoolManager`；
- 保存 UI 资源对象和释放接口；
- 对象池真正释放实例时调用 `IUIRelease.ReleaseUIForm(...)`。

释放行为：

```csharp
protected override void Release(bool isShutdown)
{
    _uiRelease.ReleaseUIForm(_uiFormAsset, Target);
}
```

最终由 `UIComponent.ReleaseUIForm(...)` 卸载资源并销毁实例。

---

## 4. 核心 API

以下列出当前 `UI` 模块最值得优先掌握的 API。

## 4.1 模块接入 API

### `void LFrameworkEntry.RegisterModule<IUIManager>(this)`

位置：

- `UIComponent.Awake()`

作用：

- 将 `UIComponent` 注册为 `IUIManager`；
- 注册后立刻触发 `UIComponent.OnInit()`；
- 后续由 `GameEntry.UI` 缓存访问。

---

### `void UIComponent.OnInit()`

作用：

- 初始化 UIGroup 字典；
- 初始化加载中 UI 记录；
- 初始化待加载后释放集合；
- 初始化回收队列；
- 创建资源加载回调；
- 清空资源管理器、对象池管理器、对象池引用；
- 重置序列号和关闭状态。

注意：

- 此时还没有设置 `IResourceManager` 和 `IObjectPoolManager`；
- 这两个依赖在 Unity `Start()` 中获取。

---

### `void UIComponent.OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 先处理 `_recycleQueue`；
- 对已关闭 UI 执行 `uiForm.OnRecycle()`；
- 将实例 `Unspawn` 回 UI 实例对象池；
- 再逐个驱动所有 UIGroup 的 `OnUpdate()`。

调用链：

```text
RootComponent.Update()
    -> LFrameworkEntry.OnUpdate(...)
    -> UIComponent.OnUpdate(...)
```

---

### `void UIComponent.Shutdown()`

作用：

- 标记 `_isShutdown = true`；
- 关闭所有已加载 UI；
- 立即清空回收队列，避免与资源对象池释放顺序冲突；
- `ReleaseAllUnused()` 释放 UI 实例对象池中未使用对象；
- 清理 UIGroup、加载状态和回收队列。

---

## 4.2 UIGroup API

### `bool AddUIGroup(string uiGroupName, int uiGroupDepth)`

作用：

- 创建一个新的 `UIGroup` GameObject；
- 挂到 `UIComponent` 节点下；
- 设置 Layer 为 `UI`；
- 设置组深度；
- 保存到 `_uiGroups`。

项目初始化调用点：

```csharp
for (int i = 0, len = Constant.Setting.UIGroupNames.Length; i < len; i++)
{
    GameEntry.UI.AddUIGroup(Constant.Setting.UIGroupNames[i], i);
}
```

当前项目默认 UIGroup：

- `Background`
- `Normal`
- `PopTip`
- `Guide`
- `Top`
- `Effect`
- `Debug`

---

### `IUIGroup GetUIGroup(string uiGroupName)`

作用：

- 按名称获取 UIGroup；
- 找不到时返回 `null`；
- `uiGroupName` 为空会抛 `LFrameworkException`。

---

### `bool HasUIGroup(string uiGroupName)`

作用：

- 判断 UIGroup 是否已存在。

---

### `IUIGroup[] GetAllUIGroups()`

作用：

- 返回当前所有 UIGroup。

---

## 4.3 UIForm 查询 API

### `bool HasUIForm(int serialId)`

作用：

- 按序列号判断已加载 UI 是否存在；
- 只查已加载 UI，不等同于“正在加载”。

---

### `bool HasUIForm(string uiFormAssetName)`

作用：

- 按资源名判断是否存在已加载 UI。

---

### `UIForm GetUIForm(int serialId)`

作用：

- 按序列号获取 UIForm；
- 找不到返回 `null`。

---

### `UIForm GetUIForm(string uiFormAssetName)`

作用：

- 按资源名获取第一个匹配 UIForm；
- 找不到返回 `null`。

---

### `UIForm[] GetUIForms(string uiFormAssetName)`

作用：

- 获取所有资源名匹配的已加载 UIForm。

---

### `void GetUIForms(string uiFormAssetName, List<UIForm> results)`

作用：

- 将所有资源名匹配的 UIForm 添加到传入列表。

注意：

- 当前 `UIComponent.GetUIForms(string, List<UIForm>)` 不会主动 `results.Clear()`；
- 当前 `UIComponent.GetAllLoadedUIForms(List<UIForm>)` 也不会主动 `results.Clear()`；
- 调用方如果复用列表，应先自行清空。

---

## 4.4 UIForm 加载状态 API

### `bool IsLoadingUIForm(int serialId)`

作用：

- 判断指定序列号 UI 是否仍在加载。

---

### `bool IsLoadingUIForm(string uiFormAssetName)`

作用：

- 判断指定资源名是否有 UI 正在加载。

---

### `int[] GetAllLoadingUIFormSerialIds()`

作用：

- 获取所有正在加载中的 UI 序列号。

---

### `void CloseAllLoadingUIForms()`

作用：

- 将所有正在加载 UI 的序列号加入 `_uiFormsToReleaseOnLoad`；
- 清空 `_uiFormsBeingLoaded`；
- 加载完成后，如果命中待释放集合，会卸载资源并不再打开 UI。

---

## 4.5 打开 UI API

### `int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)`

作用：

- 打开 UI 的核心入口；
- 返回本次打开的序列号；
- 如果对象池里已有实例，则直接复用；
- 如果对象池无实例，则通过 `IResourceManager.LoadAsset(...)` 异步加载资源。

核心流程：

```text
OpenUIForm(...)
    -> 校验 ResourceManager / assetName / groupName
    -> 获取 UIGroup
    -> ++serial
    -> _instancePool.Spawn(uiFormAssetName)
        -> 命中：InternalOpenUIForm(...)
        -> 未命中：_resourceManager.LoadAsset(...)
```

默认重载行为：

- 不传 priority 时默认使用 `100`；
- 不传 `pauseCoveredUIForm` 时默认 `false`；
- 不传 userData 时默认 `null`。

---

### `UniTask<int> OpenUIFormAwait(...)`

作用：

- 可等待版本；
- 内部通过 `await _resourceManager.LoadAsset<GameObject>(...)` 加载；
- 加载完成后仍复用同一套 `LoadAssetSuccessCallback(...)` / `LoadAssetFailureCallback(...)` 打开逻辑；
- 返回 UI 序列号。

注意：

- 这个方法返回的是 `serialId`；
- 它等待资源加载和打开流程完成后返回；
- 如果对象池命中，则不需要等待资源加载。

---

### `int? OpenUIForm(this IUIManager uiComponent, int uiFormId, object userData = null)`

来源：

- `UIExtension`

作用：

- 按 Luban 配置表 ID 打开 UI；
- 从 `GameEntry.DataTable.TbUIForm` 读取配置；
- 使用 `AssetUtility.GetUIFormAsset(drUIForm.AssetName)` 拼资源路径；
- 根据 `AllowMultiInstance` 防止重复打开或重复加载；
- 使用配置的 `GroupName`、`PauseCoveredForm`；
- priority 使用 `Constant.AssetPriority.UIFormAsset`。

调用链：

```text
GameEntry.UI.OpenUIForm(uiFormId)
    -> GameEntry.DataTable.TbUIForm.Get(uiFormId)
    -> AssetUtility.GetUIFormAsset(...)
    -> IUIManager.OpenUIForm(assetName, groupName, priority, pauseCoveredForm, userData)
```

前置条件：

- `GameEntry.DataTable` 已初始化；
- UIForm 配置表已加载；
- 对应 UIGroup 已创建；
- UI 资源路径存在。

---

## 4.6 关闭 UI API

### `void CloseUIForm(int serialId, object userData)`

作用：

- 按序列号关闭 UI；
- 如果 UI 还在加载，则加入待加载后释放集合；
- 如果已加载，则获取 UIForm 并进入关闭流程；
- 找不到已加载 UI 时抛异常。

加载中关闭流程：

```text
CloseUIForm(serialId)
    -> IsLoadingUIForm(serialId) == true
    -> _uiFormsToReleaseOnLoad.Add(serialId)
    -> _uiFormsBeingLoaded.Remove(serialId)
```

加载完成后：

```text
LoadAssetSuccessCallback(...)
    -> 发现 serialId 在 _uiFormsToReleaseOnLoad
    -> ReleaseUIForm(uiFormAsset, null)
    -> 不实例化、不打开 UI
```

---

### `void CloseUIForm(UIForm uiForm, object userData)`

作用：

- 从所属 UIGroup 移除 UIForm；
- 调用 `uiForm.OnClose(_isShutdown, userData)`；
- 刷新 UIGroup 覆盖 / 暂停关系；
- 将 UIForm 放入 `_recycleQueue`；
- 真正回收到对象池发生在下一次 `UIComponent.OnUpdate()`。

关闭链：

```text
CloseUIForm(uiForm)
    -> uiGroup.RemoveUIForm(uiForm)
    -> uiForm.OnClose(isShutdown, userData)
    -> uiGroup.Refresh()
    -> _recycleQueue.Enqueue(uiForm)
```

---

### `void CloseAllLoadedUIForms(object userData)`

作用：

- 获取所有已加载 UIForm；
- 逐个关闭；
- 关闭过程中会检查 UI 是否仍存在，避免重复关闭。

---

## 4.7 激活 UI API

### `void RefocusUIForm(UIForm uiForm, object userData)`

作用：

- 将指定 UIForm 移到所属 UIGroup 链表头部；
- 刷新 UIGroup；
- 调用 `uiForm.OnRefocus(userData)`。

调用链：

```text
RefocusUIForm(uiForm)
    -> uiGroup.RefocusUIForm(uiForm, userData)
    -> uiGroup.Refresh()
    -> uiForm.OnRefocus(userData)
```

---

## 4.8 对象池控制 API

### `float InstanceAutoReleaseInterval`

作用：

- 控制 UI 实例对象池自动释放可释放对象的间隔秒数。

---

### `int InstanceCapacity`

作用：

- 控制 UI 实例对象池容量。

---

### `float InstanceExpireTime`

作用：

- 控制 UI 实例对象池对象过期时间。

---

### `int InstancePriority`

作用：

- 控制 UI 实例对象池优先级。

---

### `void SetUIFormInstanceLocked(object uiFormInstance, bool locked)`

作用：

- 设置指定 UI 实例是否被对象池锁定。

---

### `void SetUIFormInstancePriority(object uiFormInstance, int priority)`

作用：

- 设置指定 UI 实例在对象池中的优先级。

---

## 4.9 UguiForm API

### `void Close()`

作用：

- 关闭当前 UIForm。

实现：

```csharp
GameEntry.UI.CloseUIForm(this.UIForm);
```

---

### `void PlayUISound(int uiSoundId)`

作用：

- 播放 UI 音效。

实现：

```csharp
GameEntry.Audio.PlayUISound(uiSoundId);
```

---

### Widget API

常见 API：

- `AddUIWidget(UIWidget widget, object userData = default)`
- `RemoveUIWidget(UIWidget widget)`
- `RemoveAllUIWidget()`
- `OpenUIWidget(UIWidget widget, object userData = default)`
- `DynamicOpenUIWidget(UIWidget widget, object userData = default)`
- `CloseUIWidget(UIWidget widget, object userData = default, bool isShutdown = false)`
- `CloseAllUIWidgets(object userData = default, bool isShutdown = false)`

语义：

- `AddUIWidget()` 会创建 `UIWidgetContainer` 并调用 Widget `OnInit()`；
- `OpenUIWidget()` 不刷新深度，适合在 UIForm `OnOpen()` 中打开静态 Widget；
- `DynamicOpenUIWidget()` 会额外调用 Widget `OnDepthChanged()`，适合运行时动态打开；
- `CloseUIWidget()` 只关闭，不移出容器；
- `RemoveUIWidget()` 只移出容器，不负责销毁 GameObject。

---

### 事件 API

常见 API：

- `Subscribe(...)`
- `Unsubscribe(...)`
- `UnsubscribeAll()`

作用：

- 内部懒创建 `EventContainer`；
- 订阅记录由 `EventContainer` 持有；
- `UguiForm.OnClose()` 会自动调用 `UnsubscribeAll()`。

推荐：

- UI 内事件订阅优先使用 `UguiForm.Subscribe(...)`；
- 不建议直接 `GameEntry.Event.Subscribe(...)`，除非手动保证取消订阅。

---

### 资源 API

常见 API：

- `UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object`
- `void UnloadAsset(UnityEngine.Object asset)`
- `void UnloadAllAssets()`

作用：

- 内部懒创建 `ResourceContainer`；
- UI 加载的附属资源由当前 `UguiForm` 持有；
- `UguiForm.OnClose()` 会自动调用 `UnloadAllAssets()`。

推荐：

```csharp
var sprite = await LoadAssetAsync<Sprite>(assetName);
```

不要直接在 UI 中裸调用 `GameEntry.Resource.LoadAsset<T>()`，除非能严格配对 `UnloadAsset()`。

---

## 5. UI 模块调用链

## 5.1 模块注册链

```text
UIComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IUIManager>(this)
    ↓
UIComponent.OnInit()
    ↓
UIComponent.Start()
    ↓
SetResourceManager(IResourceManager)
SetObjectPoolManager(IObjectPoolManager)
    ↓
GameEntry.Start()
    ↓
InitCustomComponents()
    ↓
GameEntry.UI = LFrameworkEntry.GetModule<IUIManager>()
```

说明：

- `RegisterModule<IUIManager>()` 的泛型是接口；
- `OnInit()` 早于 `Start()`；
- 资源模块和对象池模块依赖在 `Start()` 里绑定，不在 `OnInit()` 里绑定。

---

## 5.2 UIGroup 初始化链

```text
ProcedureGameLogicLaunch.OnEnter()
    ↓
InitUI()
    ↓
遍历 Constant.Setting.UIGroupNames
    ↓
GameEntry.UI.AddUIGroup(groupName, i)
    ↓
创建 UIGroup GameObject
    ↓
设置 Canvas sortingOrder = UIGroup.DepthFactor * depth
```

当前默认分组顺序：

```text
Background -> Normal -> PopTip -> Guide -> Top -> Effect -> Debug
```

组深度从数组下标得到。

---

## 5.3 打开 UI 链

对象池未命中时：

```text
GameEntry.UI.OpenUIForm(assetName, groupName, priority, pauseCovered, userData)
    ↓
UIComponent.OpenUIForm(...)
    ↓
_instancePool.Spawn(assetName)
    ↓
未命中
    ↓
_uiFormsBeingLoaded.Add(serialId, assetName)
    ↓
_resourceManager.LoadAsset(assetName, priority, callbacks, OpenUIFormInfo)
    ↓
LoadAssetSuccessCallback(...)
    ↓
InstantiateUIForm(uiFormAsset)
    ↓
UIFormInstanceObject.Create(...)
    ↓
_instancePool.Register(instanceObject, true)
    ↓
InternalOpenUIForm(...)
    ↓
CreateUIForm(...)
    ↓
UIForm.OnInit(...)
    ↓
UIGroup.AddUIForm(...)
    ↓
UIForm.OnOpen(...)
    ↓
UIGroup.Refresh()
```

对象池命中时：

```text
GameEntry.UI.OpenUIForm(...)
    ↓
_instancePool.Spawn(assetName)
    ↓
命中
    ↓
InternalOpenUIForm(..., isNewInstance: false)
    ↓
UIForm.OnInit(...)
        只刷新 serialId / assetName / group / pauseCovered
        不再调用 UIFormLogic.OnInit()
    ↓
UIGroup.AddUIForm(...)
    ↓
UIForm.OnOpen(...)
    ↓
UIGroup.Refresh()
```

业务含义：

- 一次性组件绑定、缓存节点适合放 `OnInit()`；
- 每次打开都要刷新的数据必须放 `OnOpen()`；
- 每次关闭都要释放的事件和资源必须放 `OnClose()`。

---

## 5.4 关闭 UI 链

```text
GameEntry.UI.CloseUIForm(uiForm)
    ↓
UIComponent.CloseUIForm(uiForm, userData)
    ↓
UIGroup.RemoveUIForm(uiForm)
    ↓
UIForm.OnClose(isShutdown, userData)
    ↓
UIFormLogic.OnClose(...)
    ↓
如果是 UguiForm:
        UIWidgetContainer.OnClose(...)
        UnsubscribeAll()
        UnloadAllAssets()
        CloseAllUIWidgets(...)
    ↓
UIGroup.Refresh()
    ↓
_recycleQueue.Enqueue(uiForm)
    ↓
下一帧 UIComponent.OnUpdate()
    ↓
uiForm.OnRecycle()
    ↓
UIFormLogic.OnRecycle()
    ↓
_instancePool.Unspawn(uiForm.Handle)
```

注意：

- 关闭不是立即销毁；
- 实例会先进入回收队列，再在 `OnUpdate()` 中回到对象池；
- 对象池最终释放时才调用 `ReleaseUIForm()` 卸载资源并销毁实例。

---

## 5.5 UIGroup 刷新链

```text
UIGroup.Refresh()
    ↓
从链表头部到尾部遍历 UIFormInfo
    ↓
调用 UIForm.OnDepthChanged(groupDepth, depthInGroup)
    ↓
根据 group.Pause 和 PauseCoveredUIForm 计算 pause / cover 状态
    ↓
触发 OnResume / OnPause / OnReveal / OnCover
```

关键语义：

- 链表头部是当前最前面的 UI；
- `PauseCoveredUIForm == true` 的 UI 会暂停其后方 UI；
- 被暂停 UI 不会继续收到 `OnUpdate()`；
- 被覆盖但未暂停的 UI 仍可能继续 `OnUpdate()`。

---

## 5.6 每帧更新链

```text
RootComponent.Update()
    ↓
LFrameworkEntry.OnUpdate(...)
    ↓
UIComponent.OnUpdate(...)
    ↓
处理回收队列
    ↓
foreach UIGroup
    ↓
UIGroup.OnUpdate(...)
    ↓
从链表头部开始遍历
    ↓
遇到 Paused UIFormInfo 后停止
    ↓
UIForm.OnUpdate(...)
    ↓
UIFormLogic.OnUpdate(...)
    ↓
UguiForm.OnUpdate(...)
    ↓
UIWidgetContainer.OnUpdate(...)
```

---

## 5.7 关闭模块链

```text
LFrameworkEntry.Shutdown()
    ↓
UIComponent.Shutdown()
    ↓
_isShutdown = true
    ↓
CloseAllLoadedUIForms()
    ↓
立即处理 _recycleQueue
    ↓
_instancePool.ReleaseAllUnused()
    ↓
清理 UIGroup / loading / release / recycle 状态
```

---

## 6. 生命周期

`UI` 模块里最关键的是 7 套生命周期。

## 6.1 UI 模块生命周期

适用对象：

- `UIComponent`

核心顺序：

1. Unity `Awake()`
2. `LFrameworkEntry.RegisterModule<IUIManager>(this)`
3. `UIComponent.OnInit()`
4. Unity `Start()`
5. `SetResourceManager(...)`
6. `SetObjectPoolManager(...)`
7. 每帧 `OnUpdate(...)`
8. 框架关闭时 `Shutdown()`

说明：

- `OnInit()` 只初始化自身容器状态；
- 跨模块依赖在 `Start()` 获取；
- 业务侧通过 `GameEntry.UI` 使用，不直接持有 `UIComponent`。

---

## 6.2 UIGroup 生命周期

适用对象：

- `UIGroup`

核心顺序：

1. `GameEntry.UI.AddUIGroup(...)`
2. 创建 `UIGroup` GameObject
3. `UIGroup.Awake()` 添加 Canvas / GraphicRaycaster
4. `UIGroup.Start()` 设置 RectTransform 铺满父节点
5. UI 打开时 `AddUIForm(...)`
6. UI 关闭时 `RemoveUIForm(...)`
7. 打开、关闭、激活、组暂停变化时 `Refresh()`
8. 每帧 `OnUpdate(...)`

说明：

- UIGroup 是运行时创建的；
- UIGroup 不负责加载资源；
- UIGroup 只管理组内 UIForm 的排序、暂停、覆盖和更新。

---

## 6.3 UIForm 生命周期

适用对象：

- `UIForm`
- `UIFormLogic`
- `UguiForm`

核心顺序：

```text
资源加载 / 对象池复用
    ↓
CreateUIForm()
    ↓
UIForm.OnInit(...)
    ↓
UIFormLogic.OnInit(...) 仅新实例调用
    ↓
UIGroup.AddUIForm()
    ↓
UIForm.OnOpen(...)
    ↓
UIFormLogic.OnOpen(...)
    ↓
运行中 OnUpdate / OnPause / OnResume / OnCover / OnReveal / OnRefocus / OnDepthChanged
    ↓
UIForm.OnClose(...)
    ↓
UIFormLogic.OnClose(...)
    ↓
进入回收队列
    ↓
UIForm.OnRecycle()
    ↓
UIFormLogic.OnRecycle()
    ↓
_instancePool.Unspawn(...)
```

最关键规则：

- 初始化组件引用放 `OnInit()`；
- 每次打开的数据绑定放 `OnOpen()`；
- 事件取消、附属资源释放放 `OnClose()`；
- 对象池复用前的状态清理放 `OnRecycle()`。

---

## 6.4 UguiForm 事件生命周期

适用对象：

- 继承 `UguiForm` 的业务窗口。

核心顺序：

```text
Subscribe(...)
    ↓
懒创建 EventContainer
    ↓
EventContainer.Subscribe(...)
    ↓
UguiForm.OnClose()
    ↓
UnsubscribeAll()
    ↓
EventContainer 保留到关闭管理器或 OnDestroy 时释放
```

说明：

- `OnClose()` 会取消全部订阅；
- `ClearUIForm()` 释放 `EventContainer` 发生在 `isShutdown == true` 或 `OnDestroy()`；
- 平时关闭后复用窗口时，容器对象可能仍保留，但其中订阅已清空。

---

## 6.5 UguiForm 资源生命周期

适用对象：

- 通过 `UguiForm.LoadAssetAsync<T>()` 加载的 UI 附属资源。

核心顺序：

```text
LoadAssetAsync<T>(assetName)
    ↓
懒创建 ResourceContainer
    ↓
ResourceContainer.LoadAsset<T>(...)
    ↓
UI 使用资源
    ↓
UguiForm.OnClose()
    ↓
UnloadAllAssets()
    ↓
关闭管理器或 OnDestroy 时 ReferencePool.Release(ResourceContainer)
```

说明：

- UI 自己持有的资源应优先走 `UguiForm.LoadAssetAsync<T>()`；
- 不推荐在 UI 内直接裸用 `GameEntry.Resource.LoadAsset<T>()`；
- 如果直接使用，必须明确配对 `GameEntry.Resource.UnloadAsset(...)`。

---

## 6.6 UIWidget 生命周期

适用对象：

- `UIWidget`
- `UIWidgetContainer`

核心顺序：

```text
UguiForm.AddUIWidget(widget)
    ↓
UIWidgetContainer.Create(owner)
    ↓
widget.OnInit(userData)
    ↓
UguiForm.OpenUIWidget(widget)
    ↓
widget.OnOpen(userData)
    ↓
跟随 UguiForm 接收 OnPause / OnResume / OnCover / OnReveal / OnUpdate / OnDepthChanged
    ↓
UguiForm.CloseUIWidget(widget)
    ↓
widget.OnClose(isShutdown, userData)
    ↓
UguiForm.OnRecycle()
    ↓
widget.OnRecycle()
```

说明：

- Widget 是窗口内部逻辑，不由 `UIComponent` 单独管理；
- Widget 生命周期由所属 `UguiForm` 转发；
- Widget 不进入 UI 实例对象池；
- Widget 容器本身进入 `ReferencePool`。

---

## 6.7 UI 实例对象池生命周期

适用对象：

- `UIFormInstanceObject`

核心顺序：

```text
LoadAssetSuccessCallback()
    ↓
InstantiateUIForm(uiFormAsset)
    ↓
UIFormInstanceObject.Create(assetName, uiFormAsset, instance, uiRelease)
    ↓
_instancePool.Register(instanceObject, true)
    ↓
Open 时 _instancePool.Spawn(assetName)
    ↓
Close 后 _recycleQueue.Enqueue(uiForm)
    ↓
OnUpdate 中 _instancePool.Unspawn(uiForm.Handle)
    ↓
对象池释放 unused 对象
    ↓
UIFormInstanceObject.Release(...)
    ↓
UIComponent.ReleaseUIForm(uiFormAsset, uiFormInstance)
    ↓
ResourceManager.UnloadAsset(uiFormAsset)
    ↓
Destroy(uiFormInstance)
```

说明：

- 关闭 UI 不代表资源立刻卸载；
- UI 实例进入对象池后可复用；
- 真正对象池释放时才卸载 UI 预制体资源并销毁实例。

---

## 7. 需要理解的基类 / 接口

用户在阅读或后续修复 UI 模块时，优先要理解以下 8 个抽象点。

## 7.1 `IUIManager`

UI 模块对外门面接口。

核心成员：

- `AddUIGroup(...)`
- `OpenUIForm(...)`
- `OpenUIFormAwait(...)`
- `CloseUIForm(...)`
- `GetUIForm(...)`
- `GetUIForms(...)`
- `IsLoadingUIForm(...)`

业务侧入口：

```csharp
GameEntry.UI
```

---

## 7.2 `UIComponent`

UI 模块实现类。

核心成员：

- `Awake()`
- `Start()`
- `OnInit()`
- `OnUpdate(...)`
- `Shutdown()`
- `OpenUIForm(...)`
- `CloseUIForm(...)`

核心意义：

- 接入 LFramework 模块生命周期；
- 持有 UIGroup；
- 串联资源加载、对象池复用和 UI 生命周期。

---

## 7.3 `IUIGroup` / `UIGroup`

UI 分组抽象和实现。

核心成员：

- `Depth`
- `Pause`
- `CurrentUIForm`
- `AddUIForm(...)`
- `RemoveUIForm(...)`
- `Refresh()`

核心意义：

- 控制 UI 排序；
- 控制覆盖关系；
- 控制暂停关系；
- 控制哪些 UI 能收到更新。

---

## 7.4 `IUIForm` / `UIForm`

UI 窗口运行时包装。

核心成员：

- `SerialId`
- `UIFormAssetName`
- `UIGroup`
- `DepthInUIGroup`
- `PauseCoveredUIForm`
- `Logic`
- `OnInit(...)`
- `OnOpen(...)`
- `OnClose(...)`
- `OnRecycle()`

核心意义：

- `UIForm` 不写业务逻辑；
- 它负责持有状态并转发生命周期；
- 业务逻辑应写在 `UIFormLogic` / `UguiForm` 子类中。

---

## 7.5 `UIFormLogic`

业务 UI 生命周期基类。

核心成员：

- `Available`
- `Visible`
- `CachedTransform`
- `OnInit(...)`
- `OnOpen(...)`
- `OnClose(...)`
- `OnRecycle()`

核心意义：

- 定义业务 UI 生命周期；
- 默认控制 GameObject Active；
- 是所有业务窗口逻辑的基础。

---

## 7.6 `UguiForm`

项目 UGUI 窗口基类。

核心成员：

- `Close()`
- `PlayUISound(...)`
- `AddUIWidget(...)`
- `Subscribe(...)`
- `UnsubscribeAll()`
- `LoadAssetAsync<T>(...)`
- `UnloadAllAssets()`

核心意义：

- 处理 Canvas 深度；
- 管理 Widget；
- 封装 `EventContainer`；
- 封装 `ResourceContainer`；
- 关闭时自动清理事件和资源。

业务 UI 通常应继承它，而不是直接继承 `UIFormLogic`。

---

## 7.7 `UIWidget`

窗口内部子 UI 基类。

核心成员：

- `IsOpen`
- `Visible`
- `OnInit(...)`
- `OnOpen(...)`
- `OnClose(...)`
- `OnUpdate(...)`
- `OnDepthChanged(...)`

核心意义：

- 适合一个窗口内部可开关、可复用的子区域；
- 生命周期由 `UguiForm` 管理，不由 `GameEntry.UI` 直接管理。

---

## 7.8 `IUIRelease`

UI 实例对象池释放回调接口。

核心成员：

```csharp
void ReleaseUIForm(object uiFormAsset, object uiFormInstance);
```

核心意义：

- 让 `UIFormInstanceObject` 在对象池释放时回调 `UIComponent`；
- 最终释放资源并销毁实例。

---

## 8. 使用注意事项

### 8.1 必须先创建 UIGroup，再打开 UI

`OpenUIForm(...)` 会按名称获取 UIGroup。找不到时直接抛异常：

```text
UI group '{0}' is not exist.
```

当前项目在 `ProcedureGameLogicLaunch.InitUI()` 中统一创建 UIGroup。

---

### 8.2 业务代码优先通过 `GameEntry.UI` 使用 UI 模块

推荐：

```csharp
GameEntry.UI.OpenUIForm(assetName, Constant.Setting.UIGroupNormal);
```

不推荐业务代码直接持有或查找 `UIComponent`。

---

### 8.3 配置表打开 UI 依赖 DataTable 已就绪

`UIExtension.OpenUIForm(int uiFormId)` 依赖：

```csharp
GameEntry.DataTable.TbUIForm.Get(uiFormId)
```

因此必须保证：

- `GameEntry.DataTable` 已初始化；
- Luban 表资源已加载；
- `TbUIForm` 中存在该 ID；
- 配置中的 UIGroup 已创建。

---

### 8.4 复用实例不会重复执行 `UIFormLogic.OnInit()`

当前 `UIForm.OnInit(...)` 中：

- `isNewInstance == true` 才获取 `UIFormLogic` 并调用 `OnInit()`；
- 对象池复用时只刷新 `UIForm` 的运行状态，不重复调用业务 `OnInit()`。

因此：

- 节点缓存、组件引用放 `OnInit()`；
- 每次打开的显示数据和 userData 处理放 `OnOpen()`。

---

### 8.5 关闭 UI 后不是立即销毁

`CloseUIForm()` 后 UI 会进入 `_recycleQueue`，下一次 `UIComponent.OnUpdate()` 才：

```text
uiForm.OnRecycle()
_instancePool.Unspawn(uiForm.Handle)
```

之后实例仍可能保留在对象池中。

---

### 8.6 不要绕过 UIComponent 直接 Destroy UI 实例

正确方式：

```csharp
GameEntry.UI.CloseUIForm(uiForm);
```

或在 `UguiForm` 内：

```csharp
Close();
```

原因：

- 直接 Destroy 会绕过 UIGroup 链表；
- 会绕过 `OnClose()` / `OnRecycle()`；
- 会破坏对象池记录；
- 可能导致事件和资源泄漏。

---

### 8.7 UI 内事件订阅优先使用 `UguiForm.Subscribe(...)`

推荐：

```csharp
Subscribe(eventId, OnEvent);
```

释放由 `UguiForm.OnClose()` 自动处理：

```csharp
UnsubscribeAll();
```

避免：

```csharp
GameEntry.Event.Subscribe(...);
```

除非手动在 `OnClose()` 中严格取消订阅。

---

### 8.8 UI 内资源优先使用 `UguiForm.LoadAssetAsync<T>()`

推荐：

```csharp
var sprite = await LoadAssetAsync<Sprite>(assetName);
```

关闭时 `UguiForm.OnClose()` 会调用：

```csharp
UnloadAllAssets();
```

如果直接使用 `GameEntry.Resource.LoadAsset<T>()`，必须自己配对卸载。

---

### 8.9 Widget 需要先 Add 再 Open

正确顺序：

```text
AddUIWidget(widget)
OpenUIWidget(widget)
```

否则 `UIWidgetContainer` 为空或不包含目标 Widget 时会抛异常。

---

### 8.10 `OpenUIWidget()` 与 `DynamicOpenUIWidget()` 语义不同

- `OpenUIWidget()`：不刷新深度，适合在 UIForm `OnOpen()` 中打开静态 Widget；
- `DynamicOpenUIWidget()`：打开后立即刷新深度，适合运行时动态打开 Widget。

---

## 9. 总结

当前 `UI` 模块最核心的理解方式可以概括为：

- 用 `UIComponent + IUIManager` 接入 LFramework 模块系统；
- 用 `UIGroup` 管 UI 分层、覆盖和暂停；
- 用 `UIForm` 包装 UI 实例并转发生命周期；
- 用 `UIFormLogic / UguiForm` 承载业务 UI 逻辑；
- 用 `IResourceManager` 加载 UI 预制体；
- 用 `IObjectPoolManager` 复用 UI 实例；
- 用 `EventContainer` 管 UI 事件订阅生命周期；
- 用 `ResourceContainer` 管 UI 附属资源生命周期；
- 用 `UIWidgetContainer` 管窗口内部 Widget 生命周期。

后续如果要开始修复 UI 模块问题，优先建议从以下几类基点入手：

1. UI 打开 / 关闭链路；
2. UIGroup 覆盖与暂停语义；
3. UguiForm 事件和资源释放；
4. UI 实例对象池复用语义；
5. 配置表 ID 打开 UI 的前置条件。
