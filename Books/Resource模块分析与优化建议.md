# Resource 模块分析与优化建议

## 1. 文档目的

本文针对当前 `Resource` 模块进行静态分析，目标是：

- 梳理模块结构与职责；
- 识别当前实现中存在或高概率会暴露的问题；
- 为后续正式修复提供优先级和改动方向。

本次分析主要覆盖以下文件：

- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Asset.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Scene.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Pool.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.AssetObject.cs`
- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.Services.cs`
- `Assets/LFramework/Runtime/Component/Resource/IResourceManager.cs`
- `Assets/LFramework/Runtime/Component/Resource/HasAssetResult.cs`
- `Assets/LFramework/Runtime/Component/Scene/SceneComponent.cs`

---

## 2. 当前模块定位

`Resource` 模块是 LFramework 对 YooAsset 的框架层封装，主要负责：

- 初始化资源系统与资源包；
- 请求版本、更新清单、创建下载器；
- 加载和卸载资源；
- 加载和卸载场景；
- 管理资源对象池；
- 定时清理未使用资源；
- 处理加密、解密与远端地址服务。

从结构上看，当前设计是：

```text
ResourceComponent（模块入口）
    ├── 包初始化 / 版本更新 / 下载
    ├── 资源加载（Asset）
    ├── 场景加载（Scene）
    ├── 资源对象池（Pool + AssetObject）
    └── 远端与加解密服务（Services）
```

---

## 3. 当前模块结构

### 3.1 ResourceComponent

职责：

- 作为框架模块接入 `LFrameworkEntry`；
- 初始化 YooAsset；
- 管理资源包、缓存的 `AssetInfo`、正在加载的资源键；
- 提供版本、清单、下载、缓存清理等对外能力；
- 在 `OnUpdate()` 中驱动未使用资源回收。

### 3.2 ResourceComponent.Asset

职责：

- 提供多组 `LoadAsset(...)` 异步加载接口；
- 通过 `_assetPool` 缓存已加载资源；
- 使用 `_assetLoadingHashSet` 协调相同资源的并发加载；
- 提供 `UnloadAsset / UnloadUnusedAssets / ForceUnloadAllAssets`。

### 3.3 ResourceComponent.Scene

职责：

- 封装场景异步加载与卸载；
- 管理 `_subScenes` 中的场景句柄；
- 向上层 `SceneComponent` 提供资源层场景操作。

### 3.4 ResourceComponent.Pool / AssetObject

职责：

- 通过 `IObjectPool<AssetObject>` 缓存资源对象；
- `AssetObject` 负责保存 `AssetHandle`；
- 当对象真正从池中释放时，调用 `AssetHandle.Dispose()`。

### 3.5 ResourceComponent.Services

职责：

- 提供远端地址解析服务 `RemoteServices`；
- 提供：
  - `FileStreamEncryption / FileStreamDecryption`
  - `FileOffsetEncryption / FileOffsetDecryption`
  - `FileOffsetWebDecryption / FileStreamWebDecryption`

---

## 4. 当前模块优点

### 4.1 对 YooAsset 做了较完整的框架封装

当前不仅封装了加载资源，还包含：

- 包初始化；
- 版本请求；
- Manifest 更新；
- 下载器创建；
- 缓存清理；
- 场景加载；
- 对象池缓存。

整体上已经具备完整资源模块雏形。

### 4.2 已考虑重复加载协调

通过 `_assetLoadingHashSet` + `TryWaitingLoading(...)`，当前实现已经意识到“同一资源并发加载”的问题。

### 4.3 已接入对象池

资源对象通过 `_assetPool` 缓存，避免同一资源重复加载后立刻销毁，方向是对的。

### 4.4 支持多种运行模式

已支持：

- EditorSimulate
- Offline
- Host
- WebPlay

这对实际项目发布是有价值的。

---

## 5. 当前主要问题与修复建议

以下按照优先级排序。

## 5.1 高优先级问题

### 5.1.1 场景加载接口忽略了 `packageName`

位置：

- `ResourceComponent.Scene.cs`
- `LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData, string packageName = "")`

现状：

- 接口签名接收了 `packageName`；
- 但内部固定调用的是：

```csharp
YooAssets.LoadSceneAsync(...)
```

- 没有使用指定包去加载场景。

影响：

- 多包场景会从默认包加载；
- 调用方以为自己在加载指定包场景，实际行为却不一致；
- 场景资源组织一旦复杂，问题会非常隐蔽。

建议：

- 与资源加载接口保持一致；
- 传了 `packageName` 时应从指定 `ResourcePackage` 发起加载。

---

### 5.1.2 场景卸载重复调用了两次 `UnloadAsync()`

位置：

- `ResourceComponent.Scene.cs`
- `UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)`

现状：

- 当前代码先执行一次：

```csharp
subScene.UnloadAsync();
```

- 然后又立即执行一次：

```csharp
subScene.UnloadAsync().Completed += ...
```

影响：

- 同一个场景句柄会发起两次卸载请求；
- 可能造成重复卸载、句柄状态异常、回调重复或竞态问题。

建议：

- 只调用一次 `UnloadAsync()`；
- 保存返回操作句柄，再对这一次操作绑定完成回调。

---

### 5.1.3 场景加载失败时不会走失败回调

位置：

- `ResourceComponent.Scene.cs`
- `LoadScene(...)`

现状：

- 当前 `subScene.Completed += handle => { loadSceneCallbacks.LoadSceneSuccessCallback(...); }`
- 无论实际成功还是失败，都会直接走成功回调；
- `LoadSceneFailureCallback` 在资源模块中实际上没有被调用。

影响：

- 上层 `SceneComponent` 会把失败场景当成成功场景处理；
- `_loadingSceneAssetNames / _loadedSceneAssetNames` 的状态同步可能出错；
- 用户侧很难正确感知加载失败。

建议：

- 在完成回调中检查场景句柄状态；
- 成功走 `LoadSceneSuccessCallback`；
- 失败走 `LoadSceneFailureCallback`，并传入明确错误信息。

---

### 5.1.4 `LoadAsset<T>(..., CancellationToken, ...)` 缺少失败状态检查

位置：

- `ResourceComponent.Asset.cs`
- `LoadAsset<T>(string assetName, int priority, CancellationToken cancellationToken = default, string packageName = "")`

现状：

- 该方法等待 `handle.ToUniTask(...)` 后，没有检查：
  
  - `handle.Status`
  - `handle.AssetObject == null`

- 而是直接：

```csharp
assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
```

影响：

- 若资源加载失败，`handle.AssetObject` 可能为 `null`；
- 会在 `AssetObject.Create -> Initialize(name, target)` 中触发异常；
- 同时 `_assetLoadingHashSet` 的清理也可能被异常打断。

建议：

- 与另外两组 `LoadAsset(...)` 实现保持一致；
- 在注册对象前显式检查加载结果。

---

### 5.1.5 `_assetLoadingHashSet` 在异常路径下可能残留脏键

位置：

- `ResourceComponent.Asset.cs`
- 各 `LoadAsset(...)` 方法

现状：

- 当前方法在开始真实加载前执行：

```csharp
_assetLoadingHashSet.Add(assetObjectKey);
```

- 但后续很多路径并没有统一 `finally` 清理；
- 一旦中途抛异常，键可能永久残留。

影响：

- 同资源后续加载会在 `TryWaitingLoading(...)` 中持续等待；
- 编辑器下会等到超时；
- 非编辑器下甚至可能无限等待。

建议：

- 对真实加载主流程统一加 `try/finally`；
- 无论成功、失败、取消还是异常，都要确保 `Remove(assetObjectKey)`。

---

### 5.1.6 `HasAsset(...)` 存在双重逻辑错误

位置：

- `ResourceComponent.cs`
- `HasAsset(string assetName, string packageName = "")`
- `HasAssetResult.cs`

现状一：

- 当前代码写的是：

```csharp
if (!CheckAssetValid(assetName))
{
    return HasAssetResult.Valid;
}
```

- 这里忽略了 `packageName` 参数，检查的是默认包。

现状二：

- `HasAssetResult.Valid` 的注释含义是“资源定位地址无效”；
- 但名称和返回语义本身非常容易误判。

影响：

- 指定包资源的存在性判断可能错误；
- 上层如 `SceneComponent.HasScene()` 使用：

```csharp
return _resourceManager.HasAsset(sceneAssetName) != HasAssetResult.NotExist;
```

- 这会把“定位地址无效”也当成“场景存在”。

建议：

- 修正 `CheckAssetValid(assetName, packageName)` 的调用；
- 同时统一 `HasAssetResult.Valid` 的命名或返回策略，避免语义反向。

---

### 5.1.7 `BundleStream.Read(...)` 解密逻辑处理范围错误

位置：

- `ResourceComponent.Services.cs`
- `BundleStream.Read(byte[] array, int offset, int count)`

现状：

- 当前实际读取长度是 `index`；
- 但解密时遍历的是整个 `array.Length`；
- 同时也没有从 `offset` 开始处理。

影响：

- 会错误修改未读到的缓冲区内容；
- 也会误修改 `offset` 之前的数据；
- 在流式加载资源包时会产生高风险数据损坏。

建议：

- 只处理本次真实读到的数据范围：
  - 从 `offset`
  - 到 `offset + index`

---

## 5.2 中优先级问题

### 5.2.1 资源加载接口中的 `priority` 参数目前没有生效

位置：

- `ResourceComponent.Asset.cs`
- 多组 `LoadAsset(...)`

现状：

- API 对外接收 `priority`；
- 但内部 `GetAssetHandle(...)` 和 `LoadAssetAsync(...)` 没有使用它。

影响：

- 调用方以为自己传了加载优先级；
- 实际资源加载行为并不会变化；
- 接口语义与实际行为不一致。

建议：

- 若 YooAsset 对资产加载支持优先级，应把该参数透传；
- 若不支持，应移除该参数或明确说明它当前无效。

---

### 5.2.2 `ClearCacheFilesAsync(...)` 忽略了传入的 `clearMode`

位置：

- `ResourceComponent.cs`
- `ClearCacheFilesAsync(...)`

现状：

- 无论传入什么，最终都固定执行：

```csharp
package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
```

影响：

- API 表面支持多种清理模式；
- 实际调用方完全无法生效。

建议：

- 直接把传入的 `clearMode` 透传下去。

---

### 5.2.3 `RequestPackageVersionAsync / UpdatePackageManifestAsync / CreateResourceDownloader` 缺少包空值保护

位置：

- `ResourceComponent.cs`

现状：

- 这些方法直接从 `YooAssets.GetPackage(...)` 取包后马上调用；
- 没有判断包是否初始化成功。

影响：

- 如果上层调用顺序不对，会直接触发 `NullReferenceException`；
- 错误信息不够友好，也不符合框架统一异常风格。

建议：

- 在进入具体调用前做包存在性校验；
- 抛出明确的 `LFrameworkException`。

---

### 5.2.4 `useSystemUnloadUnusedAssets` 标记当前并未真正控制系统回收调用

位置：

- `ResourceComponent.cs`
- `OnUpdate(...)`

现状：

- 当前每次都会先执行：

```csharp
_asyncOperation = Resources.UnloadUnusedAssets();
```

- 然后当 `useSystemUnloadUnusedAssets == true` 时，再额外调用模块自己的 `UnloadUnusedAssets()`。

影响：

- 即使关闭该开关，Unity 的 `Resources.UnloadUnusedAssets()` 仍然会执行；
- 当前布尔字段的语义与实际行为不一致。

建议：

- 明确这个字段到底控制什么；
- 然后把系统回收和模块回收拆开按条件执行。

---

### 5.2.5 `AssetHandle.Dispose()` 在对象池关闭路径下被跳过

位置：

- `ResourceComponent.AssetObject.cs`
- `Release(bool isShutdown)`

现状：

- 当前仅在 `!isShutdown` 时执行 `handle.Dispose()`；
- 如果资源对象池整体关闭，则不会走释放句柄逻辑。

影响：

- 若关闭路径没有其它地方兜底释放句柄，可能残留句柄生命周期问题；
- 风险依赖于上层对象池关闭时序。

建议：

- 重新确认对象池整体关闭路径的句柄释放职责；
- 若无其它明确兜底，建议统一释放。

---

## 5.3 低优先级问题 / 结构观察

### 5.3.1 `LoadAsset<T>(string assetName, Action<T> callback, ...)` 中存在重复参数校验

位置：

- `ResourceComponent.Asset.cs`

现状：

- 该方法里对 `string.IsNullOrEmpty(assetName)` 连续检查了两次。

影响：

- 不影响功能；
- 但会增加一点噪音，反映该分支可能是复制修改后未清理干净。

建议：

- 后续顺手收敛即可，不属于优先修复项。

---

## 6. 建议的修复顺序

建议分两阶段推进。

### 第一阶段：保证正确性与状态一致性

优先建议：

1. 修复场景加载忽略 `packageName`；
2. 修复场景卸载重复调用；
3. 修复场景失败回调缺失；
4. 修复 `LoadAsset<T>(..., CancellationToken, ...)` 的失败检查；
5. 为 `_assetLoadingHashSet` 增加统一 `finally` 清理；
6. 修复 `BundleStream.Read(...)` 的解密范围。

目标：

- 避免资源/场景加载状态错乱；
- 避免等待死锁和数据解密错误。

### 第二阶段：修正 API 语义与边界

建议处理：

1. 修复 `HasAsset(...)` 的返回语义；
2. 让 `clearMode` 真正生效；
3. 明确 `priority` 是否有效；
4. 补足包空值保护；
5. 明确关闭路径的句柄释放职责。

目标：

- 提升模块可维护性；
- 降低调用方误用风险。

---

## 7. 推荐修改清单

### 必改建议

- 修复场景加载/卸载逻辑；
- 修复资源加载失败检查与加载中集合清理；
- 修复 `BundleStream.Read(...)` 的解密错误；
- 修复 `HasAsset(...)` 的包参数与返回语义问题。

### 建议改

- 让 `clearMode` 真正生效；
- 明确并处理 `priority` 参数；
- 为资源包操作增加空值保护；
- 明确关闭路径的 `AssetHandle.Dispose()` 策略。

### 可延后

- 收敛重复校验与局部重复代码；
- 再考虑更统一的资源加载入口收敛。

---

## 8. 总结

当前 `Resource` 模块整体框架是成立的，而且已经具备较完整的资源系统封装能力。  
它当前最需要修复的不是“功能缺失”，而是：

- 场景加载/卸载链路存在明确逻辑错误；
- 资源加载异常路径缺少一致性保障；
- 部分 API 的对外语义和实际行为不一致；
- 解密读取逻辑存在高风险实现错误。

在你阅读并确认后，后续修复建议优先围绕“加载状态正确性优先、API 语义一致性其次”的顺序展开。
