# Scene 模块分析与优化建议

## 1. 文档目的

本文针对当前 `Scene` 模块进行静态分析，目标是：

- 梳理模块结构与职责；
- 识别当前实现中存在或高概率会暴露的问题；
- 为后续正式修复提供优先级和改动方向。

本次分析主要覆盖以下文件：

- `Assets/LFramework/Runtime/Component/Scene/SceneComponent.cs`
- `Assets/LFramework/Runtime/Component/Scene/ISceneManager.cs`
- `Assets/LFramework/Runtime/Component/Scene/LoadSceneInfo.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`
- `Assets/GameScripts/GameLogic/Procedure/ProcedureChangeScene.cs`

---

## 2. 当前模块定位

`Scene` 模块是 LFramework 对“场景状态管理”的框架层封装，主要负责：

- 记录场景加载、已加载、卸载中的状态；
- 对外提供统一的场景加载与卸载接口；
- 通过 `IResourceManager` 驱动底层场景资源加载；
- 管理当前激活场景顺序；
- 维护主摄像机引用。

从结构上看，当前设计是：

```text
SceneComponent（模块入口）
    ├── ISceneManager（对外接口）
    ├── LoadSceneInfo（加载请求上下文）
    └── IResourceManager（底层场景资源加载依赖）
```

---

## 3. 当前模块结构

### 3.1 SceneComponent

职责：

- 作为框架模块接入 `LFrameworkEntry`；
- 维护：
  - `_loadedSceneAssetNames`
  - `_loadingSceneAssetNames`
  - `_unloadingSceneAssetNames`
  - `_sceneOrder`
- 对外提供：
  - 场景查询；
  - 场景加载；
  - 场景卸载；
  - 场景顺序控制；
  - 主摄像机刷新。

### 3.2 ISceneManager

职责：

- 定义场景模块对外的统一接口；
- 对业务层暴露场景状态查询与加载/卸载能力。

### 3.3 LoadSceneInfo

职责：

- 作为一次场景加载请求的上下文载体；
- 保存：
  - 进度回调；
  - 成功回调；
  - 用户自定义数据；
- 通过 `ReferencePool` 复用。

### 3.4 与 Resource 模块的关系

`SceneComponent` 本身不直接调用 `SceneManager.LoadSceneAsync` 之类的 Unity 原生 API，  
而是依赖：

```csharp
IResourceManager.LoadScene(...)
IResourceManager.UnloadScene(...)
```

这意味着：

- Scene 模块本质上是“场景状态层”；
- 底层异步场景加载能力由 Resource 模块提供；
- 两者的边界正确性会直接互相影响。

---

## 4. 当前模块优点

### 4.1 状态分层明确

当前把场景状态拆分为：

- 已加载；
- 正在加载；
- 正在卸载；

整体思路清晰，便于上层流程控制。

### 4.2 激活场景切换有统一入口

通过 `_sceneOrder + RefreshSceneOrder()`，当前模块对“哪个场景应该处于激活状态”有统一管理。

### 4.3 与主摄像机关联明确

每次切换激活场景后，都会 `RefreshMainCamera()`，这一点对游戏逻辑使用比较友好。

### 4.4 已提供 Inspector 支持

运行时可以看到：

- Loaded 场景；
- Loading 场景；
- Unloading 场景；
- MainCamera；

便于排查基础状态。

---

## 5. 当前主要问题与修复建议

以下按照优先级排序。

## 5.1 高优先级问题

### 5.1.1 `LoadSceneInfo` 创建后没有归还引用池

位置：

- `LoadSceneInfo.cs`
- `SceneComponent.cs`
- `LoadSceneSuccessCallback(...)`
- `LoadSceneFailureCallback(...)`

现状：

- 场景加载时会调用：

```csharp
LoadSceneInfo.Create(userData, progressCallback, loadSuccessCallBack)
```

- 但无论成功还是失败，当前模块都没有：

```csharp
ReferencePool.Release(loadSceneInfo)
```

影响：

- 每次场景切换都会泄漏一个 `LoadSceneInfo` 引用；
- 长时间运行或频繁切场景时，会积累无用引用对象和回调引用；
- 还可能延长用户数据的生命周期。

建议：

- 在成功与失败回调中统一释放 `LoadSceneInfo`；
- 最好放在 `finally` 中，保证回调异常时也能归还。

---

### 5.1.2 场景加载成功/失败完全依赖 Resource 层，但当前 Scene 层没有兜底校验

位置：

- `SceneComponent.cs`
- `LoadSceneSuccessCallback(...)`
- `LoadSceneFailureCallback(...)`

现状：

- `SceneComponent` 默认信任底层 `IResourceManager` 回调是正确的；
- 但当前 Resource 模块里，场景加载存在：
  - 忽略 `packageName`
  - 成功回调与失败回调分流不完整

影响：

- 一旦底层错误触发成功回调，Scene 模块就会直接：
  - 从 loading 移到 loaded；
  - 更新 `_sceneOrder`；
  - 触发业务成功回调。

- Scene 层缺乏二次校验，会把底层错误直接放大到上层流程。

建议：

- 在修复 Resource 模块的同时，Scene 模块也应对关键回调路径保持更强约束；
- 至少保证失败路径能正确回滚 loading 状态，不误入 loaded 状态。

说明：

- 这是跨模块边界问题，但对 Scene 模块的稳定性影响是高优先级的。

---

## 5.2 中优先级问题

### 5.2.1 `_resourceManager` 初始化时机偏晚，存在调用窗口风险

位置：

- `SceneComponent.cs`
- `OnInit()`
- `Start()`

现状：

- `OnInit()` 中明确把 `_resourceManager` 置为 `null`；
- 真正赋值发生在 `Start()`：

```csharp
_resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
```

影响：

- 如果有其他模块或流程在 `SceneComponent.Start()` 之前就调用 `GameEntry.Scene.LoadScene(...)`；
- 会直接抛出：

```csharp
You must set resource manager first.
```

建议：

- 更稳妥的做法是：
  - 在 `OnInit()` 就尝试获取；
  - 或在 `LoadScene / UnloadScene / HasScene` 中懒加载获取。

---

### 5.2.2 默认框架场景依赖 `SceneManager.GetSceneAt(0)`，假设过强

位置：

- `SceneComponent.cs`
- `Awake()`

现状：

- 当前把：

```csharp
_frameworkScene = SceneManager.GetSceneAt(0);
```

- 视为“框架场景”。

影响：

- 该假设依赖运行时场景布局；
- 如果启动方式、编辑器 Play 配置或场景组织方式变化，索引 0 不一定就是期望的基础场景；
- 后续 `RefreshSceneOrder()` 在没有业务场景时，会把激活场景切回可能错误的场景。

建议：

- 比起固定索引，更稳妥的是使用：
  - 当前组件所在场景；
  - 或显式约定的框架场景标识。

---

### 5.2.3 `HasScene(...)` 依赖 `HasAsset(...)`，当前语义会被底层错误放大

位置：

- `SceneComponent.cs`
- `HasScene(string sceneAssetName)`

现状：

- 当前实现是：

```csharp
return _resourceManager.HasAsset(sceneAssetName) != HasAssetResult.NotExist;
```

影响：

- 如果 Resource 模块把“无效定位地址”也返回为非 `NotExist`，那么：
  - Scene 层会错误认定场景存在；
  - 上层流程会进入错误加载路径。

建议：

- 在 Resource 模块修复 `HasAsset(...)` 语义后同步复核这里；
- 场景模块最好只把“明确存在”视为可加载，而不是“不是 NotExist 就算存在”。

---

### 5.2.4 `Shutdown()` 直接清空列表，但未等待异步卸载完成

位置：

- `SceneComponent.cs`
- `Shutdown()`

现状：

- 当前会先对每个已加载场景调用 `UnloadScene(...)`；
- 然后立刻清空：
  - `_loadedSceneAssetNames`
  - `_loadingSceneAssetNames`
  - `_unloadingSceneAssetNames`

影响：

- 异步卸载还没完成时，管理器本地状态已经被清空；
- 后续卸载完成回调再回来时，状态可能再次被改写，或者与真实运行时不一致。

建议：

- 若关机路径允许异步，应该明确“等待回调后再清空”；
- 若关机路径不等待，则应至少保证回调回来时不会访问已无效状态。

---

### 5.2.5 `ProcedureChangeScene` 未等待旧场景卸载完成就开始加载新场景

位置：

- `ProcedureChangeScene.cs`

现状：

- 进入切场景流程时，先遍历已加载场景并调用：

```csharp
GameEntry.Scene.UnloadScene(...)
```

- 紧接着马上：

```csharp
GameEntry.Scene.LoadScene(...)
```

影响：

- 卸载是异步的；
- 新旧场景会有一段时间并存；
- 激活场景顺序、主摄像机刷新和场景切换时机可能出现竞态。

建议：

- 这不是 Scene 模块内部代码，但它揭示了 Scene 模块当前“缺少等待卸载完成再继续”的标准流程约束；
- 后续可考虑补充更清晰的场景切换使用模式或辅助 API。

---

## 5.3 低优先级问题 / 结构观察

### 5.3.1 `GetSceneName(...)` 对路径格式有隐式依赖

位置：

- `SceneComponent.cs`
- `GetSceneName(string sceneAssetName)`

现状：

- 该方法依赖资源名中存在 `/`，且可能带 `.unity` 后缀。

影响：

- 当前项目里问题不大；
- 但如果以后传的是别名路径或非标准资源名，行为会比较脆弱。

建议：

- 当前先不作为优先修复项；
- 只需在后续文档中明确入参约束。

---

## 6. 建议的修复顺序

建议分两阶段推进。

### 第一阶段：保证状态一致性

优先建议：

1. 修复 `LoadSceneInfo` 未释放问题；
2. 修复 `Shutdown()` 异步状态清理不一致问题；
3. 强化 `LoadSceneSuccess / Failure` 对状态迁移的正确性保障。

目标：

- 避免场景状态集合失真；
- 避免场景切换请求泄漏引用对象。

### 第二阶段：修正依赖边界与初始化时机

建议处理：

1. 优化 `_resourceManager` 的获取时机；
2. 修正框架场景识别方式；
3. 在 Scene/Resource 边界上统一“场景存在”和“加载成功”的语义。

目标：

- 降低跨模块耦合误差；
- 提升场景模块的可维护性。

---

## 7. 推荐修改清单

### 必改建议

- 为 `LoadSceneInfo` 增加统一释放；
- 修复 `Shutdown()` 的异步状态清理策略；
- 校准 Scene 层与 Resource 层之间的成功/失败语义边界。

### 建议改

- 让 `_resourceManager` 更早可用；
- 改进 `_frameworkScene` 的确定方式；
- 收紧 `HasScene(...)` 的判断条件。

### 可延后

- 收敛 `GetSceneName(...)` 的路径假设；
- 增加更明确的场景切换辅助流程。

---

## 8. 总结

当前 `Scene` 模块的整体结构是成立的，职责也比较清晰。  
它当前最需要修复的不是功能缺失，而是：

- 场景加载请求上下文对象没有正确回收；
- 异步卸载与本地状态清理之间存在不一致；
- 对 Resource 模块回调语义过度信任，导致错误容易被放大。

在你阅读并确认后，后续修复建议优先围绕“状态一致性优先、跨模块边界约束其次”的顺序展开。
