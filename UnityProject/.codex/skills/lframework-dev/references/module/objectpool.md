# ObjectPool

## GameLogic 推荐用法

- GameLogic/业务代码中优先通过 `GameEntry.ObjectPool` 创建、获取和销毁对象池；仅在框架集成代码或 `GameEntry` 尚未初始化时，才直接使用 `LFrameworkEntry.GetModule<IObjectPoolManager>()`。
- 可复用对象需要定义 `ObjectBase` 子类包装真实对象，通过静态工厂使用 `ReferencePool.Acquire<T>()` 创建包装对象，并调用 `Initialize(name, target)` 记录对象名和真实 `Target`。
- 普通 GameObject 复用可 `OnSpawn()` 激活对象，`OnUnspawn()` 隐藏对象，`Release(isShutdown)` 销毁真实 GameObject。
- 创建对象池时根据需求选择 `CreateSingleSpawnObjectPool<T>()` 或 `CreateMultiSpawnObjectPool<T>()`；单次获取池中的同一对象未回收前不会再次被 `Spawn`。
- 获取对象时先调用 `objectPool.Spawn(name)`，返回 `null` 时再创建真实对象，并通过 `objectPool.Register(ObjectBase, spawned: true)` 注册进池。
- 对象用完后调用 `objectPool.Unspawn(obj)` 或 `Unspawn(target)` 回收，不要绕过对象池直接销毁已注册对象。
- 生命周期结束时调用 `objectPool.ReleaseAllUnused()` 释放未使用对象，并通过 `GameEntry.ObjectPool.DestroyObjectPool(objectPool)` 销毁对象池。

## 注意事项

- `ObjectPoolComponent` 注册为 `IObjectPoolManager`，按对象包装类型和对象池名称管理多个对象池，并在模块轮询中触发对象池自动释放。
- `ObjectBase.Target` 是真实对象；`Name` 用于按名称 `Spawn(name)`，`Locked`、`Priority`、`CustomCanReleaseFlag`、`LastUseTime` 会参与释放筛选。
- `ObjectBase` 实现 `IReference`，对象池释放包装对象时会调用 `ReferencePool.Release()`；`ObjectBase.Clear()` 必须清空自定义保留字段。
- `Register(obj, spawned)` 的 `obj.Target` 不能重复注册；`spawned: true` 会立即触发 `OnSpawn()` 并将获取计数设为 1。
- `Unspawn` 会降低获取计数并触发 `OnUnspawn()`；如果目标对象不属于当前对象池，或获取计数已小于 1，会抛出异常。
- `ReleaseObject`、`Release` 和 `ReleaseAllUnused` 只释放未使用、未加锁且 `CustomCanReleaseFlag` 为 `true` 的对象；正在使用的对象不会被普通释放流程释放。
- 调整 `Capacity` 或 `ExpireTime` 会立即触发一次释放检查；对象池优先级用于管理器按优先级排序释放。
- `ResourceComponent` 和 `UIComponent` 也会使用对象池管理资源对象或 UI 实例，业务代码不要直接销毁这些模块已经纳入管理的对象。

## IObjectPoolManager API 速查

仅在业务对象池封装、资源/UI 等框架集成代码或调试对象池状态时优先考虑直接使用 `IObjectPoolManager`。

- 数量：`Count` 返回当前对象池数量。
- 存在检查：`HasObjectPool<T>()`, `HasObjectPool(Type objectType)`, `HasObjectPool<T>(name)`, `HasObjectPool(Type objectType, name)`, `HasObjectPool(condition)`。
- 获取对象池：`GetObjectPool<T>()`, `GetObjectPool(Type objectType)`, `GetObjectPool<T>(name)`, `GetObjectPool(Type objectType, name)`, `GetObjectPool(condition)`。
- 获取多个对象池：`GetObjectPools(condition)`, `GetObjectPools(condition, results)`, `GetAllObjectPools()`, `GetAllObjectPools(results)`, `GetAllObjectPools(sort)`, `GetAllObjectPools(sort, results)`。
- 创建单次获取池：`CreateSingleSpawnObjectPool<T>(...)` 或 `CreateSingleSpawnObjectPool(Type, ...)`；常用参数组合包括 `name`, `capacity`, `expireTime`, `priority`, `autoReleaseInterval`。
- 创建多次获取池：`CreateMultiSpawnObjectPool<T>(...)` 或 `CreateMultiSpawnObjectPool(Type, ...)`；参数组合与单次获取池一致。
- 销毁对象池：`DestroyObjectPool<T>()`, `DestroyObjectPool(Type)`, `DestroyObjectPool<T>(name)`, `DestroyObjectPool(Type, name)`, `DestroyObjectPool<T>(objectPool)`, `DestroyObjectPool(ObjectPoolBase)`。
- 全局释放：`Release()` 释放所有对象池中的可释放对象；`ReleaseAllUnused()` 释放所有对象池中的未使用对象。
- `IObjectPool<T>` 常用操作：`Register`, `CanSpawn`, `Spawn`, `Unspawn`, `SetLocked`, `SetPriority`, `ReleaseObject`, `Release`, `ReleaseAllUnused`。
- `IObjectPool<T>` 状态属性：`Name`, `FullName`, `ObjectType`, `Count`, `CanReleaseCount`, `AllowMultiSpawn`, `AutoReleaseInterval`, `Capacity`, `ExpireTime`, `Priority`。

## 源码路径

- `Assets/LFramework/Runtime/Component/ObjectPool/IObjectPoolManager.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/IObjectPool.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectBase.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectInfo.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolBase.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolComponent.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolComponent.Object.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolComponent.ObjectPool.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ReleaseObjectFilterCallback.cs`
