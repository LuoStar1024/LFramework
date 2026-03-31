# ObjectPool 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `ObjectPool` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 需要继承的基类；
- 基类的核心生命周期。

---

## 2. 模块定位

`ObjectPool` 模块是 LFramework 的通用对象池系统，用于：

- 创建和管理对象池；
- 注册可复用对象；
- 获取与回收对象；
- 自动释放未使用对象；
- 为资源缓存、UI 实例缓存、游戏对象缓存等系统提供基础能力。

它在项目中的定位，是“框架级对象复用基础设施”。

---

## 3. 核心类型

## 3.1 ObjectPoolComponent

文件：

- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolComponent.cs`

定义：

```csharp
public sealed partial class ObjectPoolComponent : MonoBehaviour, ILFrameworkModule, IObjectPoolManager
```

职责：

- 作为 ObjectPool 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 维护所有对象池实例；
- 在 `OnUpdate()` 中驱动对象池自动释放；
- 对外暴露 `IObjectPoolManager` 能力。

说明：

- 当前模块不以继承 `ObjectPoolComponent` 扩展为主；
- 业务层通常通过 `GameEntry.ObjectPool` 或模块注册系统访问。

---

## 3.2 IObjectPoolManager

文件：

- `Assets/LFramework/Runtime/Component/ObjectPool/IObjectPoolManager.cs`

职责：

- 定义对象池管理器的对外统一接口；
- 负责：
  - 查询对象池；
  - 创建对象池；
  - 销毁对象池；
  - 获取全部对象池；
  - 统一释放所有池里的可回收对象。

说明：

- 管理器以 `objectType + name` 作为对象池唯一键。

---

## 3.3 ObjectPoolBase

文件：

- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolBase.cs`

定义：

```csharp
public abstract class ObjectPoolBase
```

职责：

- 提供对象池非泛型公共基类；
- 统一暴露：
  - `Name`
  - `FullName`
  - `ObjectType`
  - `Count`
  - `CanReleaseCount`
  - `AllowMultiSpawn`
  - `AutoReleaseInterval`
  - `Capacity`
  - `ExpireTime`
  - `Priority`

说明：

- 该类型主要用于管理器层、Inspector 层和“统一枚举所有对象池”场景；
- 业务对象不会继承它。

---

## 3.4 IObjectPool<T>

文件：

- `Assets/LFramework/Runtime/Component/ObjectPool/IObjectPool.cs`

职责：

- 定义泛型对象池对外接口；
- 给业务系统提供：
  - 注册对象；
  - 获取对象；
  - 回收对象；
  - 修改锁定与优先级；
  - 释放对象。

说明：

- `T` 必须继承 `ObjectBase`。

---

## 3.5 ObjectBase

文件：

- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectBase.cs`

定义：

```csharp
public abstract class ObjectBase : IReference
```

职责：

- 作为所有池对象的继承基类；
- 保存对象基础信息；
- 定义对象池对象的生命周期回调；
- 供上层业务对象扩展具体释放逻辑。

说明：

- 这是当前 ObjectPool 模块里最需要业务继承的核心基类。

---

## 3.6 ObjectInfo

文件：

- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectInfo.cs`

职责：

- 提供对象池运行时调试信息快照；
- 用于 Inspector 展示和导出 CSV。

包含信息：

- 对象名称；
- 是否加锁；
- 是否允许释放；
- 优先级；
- 最后使用时间；
- 是否在使用中；
- 获取计数。

---

## 3.7 当前项目中的典型派生对象

文件示例：

- `Assets/LFramework/Runtime/Component/Resource/ResourceComponent.AssetObject.cs`
- `Assets/GameScripts/GameLogic/Game/GoPoolObject.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIComponent.UIFormInstanceObject.cs`

说明：

- 这些类型都直接继承 `ObjectBase`；
- 它们展示了当前项目对 ObjectPool 的典型接入方式。

---

## 4. 核心 API

## 4.1 IObjectPoolManager 核心 API

### 查询类 API

| API                                                    | 说明              |
| ------------------------------------------------------ | --------------- |
| `int Count`                                            | 当前对象池数量         |
| `bool HasObjectPool<T>()`                              | 是否存在默认名对象池      |
| `bool HasObjectPool<T>(string name)`                   | 是否存在指定名称对象池     |
| `IObjectPool<T> GetObjectPool<T>()`                    | 获取默认名对象池        |
| `IObjectPool<T> GetObjectPool<T>(string name)`         | 获取指定名称对象池       |
| `ObjectPoolBase[] GetAllObjectPools()`                 | 获取全部对象池         |
| `ObjectPoolBase[] GetAllObjectPools(bool sort)`        | 获取全部对象池，可按优先级排序 |
| `void GetAllObjectPools(List<ObjectPoolBase> results)` | 填充全部对象池到外部列表    |

### 创建类 API

管理器提供两大类创建入口：

```csharp
CreateSingleSpawnObjectPool(...)
CreateMultiSpawnObjectPool(...)
```

语义区别：

- `SingleSpawn`：同一对象同一时刻只允许被一个使用方持有；
- `MultiSpawn`：同一对象允许被多次获取，内部通过 `SpawnCount` 计数。

最常用的核心形式：

```csharp
IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
```

说明：

- 其余大量重载，本质上都是对这组核心参数的简化包装。

核心配置含义：

| 参数                    | 说明     |
| --------------------- | ------ |
| `name`                | 对象池名称  |
| `autoReleaseInterval` | 自动释放间隔 |
| `capacity`            | 池容量    |
| `expireTime`          | 过期时间   |
| `priority`            | 池优先级   |

### 销毁类 API

```csharp
bool DestroyObjectPool<T>()
bool DestroyObjectPool<T>(string name)
bool DestroyObjectPool<T>(IObjectPool<T> objectPool)
bool DestroyObjectPool(Type objectType)
bool DestroyObjectPool(Type objectType, string name)
bool DestroyObjectPool(ObjectPoolBase objectPool)
```

### 批量释放类 API

```csharp
void Release()
void ReleaseAllUnused()
```

说明：

- `Release()`：让每个池按自身释放策略尝试释放；
- `ReleaseAllUnused()`：让每个池释放所有未使用对象。

---

## 4.2 IObjectPool<T> 核心 API

### 基础信息

| API                         | 说明       |
| --------------------------- | -------- |
| `string Name`               | 对象池名称    |
| `string FullName`           | 对象池完整名称  |
| `Type ObjectType`           | 池对象类型    |
| `int Count`                 | 池内对象总数   |
| `int CanReleaseCount`       | 当前可释放对象数 |
| `bool AllowMultiSpawn`      | 是否允许多次获取 |
| `float AutoReleaseInterval` | 自动释放间隔   |
| `int Capacity`              | 池容量      |
| `float ExpireTime`          | 对象过期秒数   |
| `int Priority`              | 池优先级     |

### 注册 / 获取 / 回收 API

```csharp
void Register(T obj, bool spawned)
bool CanSpawn()
bool CanSpawn(string name)
T Spawn()
T Spawn(string name)
void Unspawn(T obj)
void Unspawn(object target)
```

语义说明：

- `Register(obj, false)`：把对象放进池里，但此时不算“已借出”；
- `Register(obj, true)`：注册后立即视为正在使用；
- `Spawn(...)`：从池中取一个符合条件的对象；
- `Unspawn(...)`：把对象归还给池。

### 状态控制 API

```csharp
void SetLocked(T obj, bool locked)
void SetLocked(object target, bool locked)
void SetPriority(T obj, int priority)
void SetPriority(object target, int priority)
```

说明：

- `Locked == true` 的对象不会被释放；
- `Priority` 会影响默认释放策略。

### 释放 API

```csharp
bool ReleaseObject(T obj)
bool ReleaseObject(object target)
void Release()
void Release(int toReleaseCount)
void Release(ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
void Release(int toReleaseCount, ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
void ReleaseAllUnused()
```

说明：

- `ReleaseObject(...)`：释放某个具体对象；
- `Release(...)`：按策略释放若干对象；
- `ReleaseAllUnused()`：释放池里所有未使用对象。

---

## 4.3 ObjectBase 核心 API

这是业务侧最需要继承的基类。

### 需要继承的基类

```csharp
public sealed class MyPoolObject : ObjectBase
{
}
```

### 基础属性

| API                         | 说明                  |
| --------------------------- | ------------------- |
| `string Name`               | 对象名称                |
| `object Target`             | 实际被缓存的目标对象          |
| `bool Locked`               | 是否加锁                |
| `int Priority`              | 对象优先级               |
| `DateTime LastUseTime`      | 上次使用时间              |
| `bool CustomCanReleaseFlag` | 自定义是否允许释放，默认 `true` |

### 初始化 API

`ObjectBase` 提供多个受保护 `Initialize(...)` 重载：

```csharp
protected void Initialize(object target)
protected void Initialize(string name, object target)
protected void Initialize(string name, object target, bool locked)
protected void Initialize(string name, object target, int priority)
protected void Initialize(string name, object target, bool locked, int priority)
```

说明：

- 业务对象通常在静态 `Create(...)` 工厂里调用它。

---

## 5. ObjectBase 核心生命周期

这是当前模块中最重要的“继承后要理解的生命周期”。

### 生命周期方法

```csharp
protected internal virtual void OnSpawn()
protected internal virtual void OnUnspawn()
protected internal abstract void Release(bool isShutdown)
public virtual void Clear()
```

### 各生命周期含义

| 生命周期                       | 触发时机                       | 常见用途                     |
| -------------------------- | -------------------------- | ------------------------ |
| `OnSpawn`                  | 对象被池取出时                    | 激活 GameObject、恢复状态、增加可见性 |
| `OnUnspawn`                | 对象归还到池时                    | 隐藏对象、停止表现、撤销激活状态         |
| `Release(bool isShutdown)` | 对象被真正移出对象池时                | 销毁目标对象、释放句柄、释放外部资源       |
| `Clear`                    | 对象自身被归还到 `ReferencePool` 时 | 清空字段，恢复默认值               |

### `isShutdown` 的含义

`Release(bool isShutdown)` 中：

- `false`：普通释放路径；
- `true`：对象池整体关闭时触发。

业务对象可以根据这个标记区分：

- 平时回收时的处理；
- 模块关闭时的最终收尾。

---

## 6. 核心调用链

## 6.1 对象池创建调用链

```text
业务代码 / 模块初始化
    ↓
IObjectPoolManager.CreateSingleSpawnObjectPool(...) 或 CreateMultiSpawnObjectPool(...)
    ↓
ObjectPoolComponent.InternalCreateObjectPool(...)
    ↓
new ObjectPool<T>(...)
    ↓
加入 ObjectPoolComponent._objectPools
```

---

## 6.2 对象注册调用链

```text
业务代码创建业务池对象
    ↓
pool.Register(obj, spawned)
    ↓
Object<T>.Create(obj, spawned)
    ↓
加入 _objects 和 _objectMap
```

如果 `spawned == true`，当前实现会立即视为该对象已被使用。

---

## 6.3 对象获取调用链

```text
pool.Spawn(name)
    ↓
ObjectPool<T>.Spawn(name)
    ↓
按名称找到可用对象
    ↓
Object<T>.Spawn()
    ↓
更新 SpawnCount / LastUseTime
    ↓
执行 ObjectBase.OnSpawn()
```

---

## 6.4 对象回收调用链

```text
pool.Unspawn(target)
    ↓
ObjectPool<T>.Unspawn(target)
    ↓
通过 _objectMap 找到内部对象
    ↓
Object<T>.Unspawn()
    ↓
执行 ObjectBase.OnUnspawn()
```

---

## 6.5 对象释放调用链

```text
pool.ReleaseObject(target)
    ↓
ObjectPool<T>.ReleaseObject(target)
    ↓
从 _objects / _objectMap 移除
    ↓
Object<T>.Release(false)
    ↓
ObjectBase.Release(false)
    ↓
ReferencePool.Release(业务池对象)
    ↓
ReferencePool.Release(内部包装对象)
```

---

## 6.6 自动释放调用链

```text
LFramework 模块系统驱动 OnUpdate(...)
    ↓
ObjectPoolComponent.OnUpdate(...)
    ↓
每个 ObjectPool.OnUpdate(...)
    ↓
累计 AutoReleaseInterval
    ↓
到时间后执行 Release()
```

---

## 6.7 对象池关闭调用链

```text
IObjectPoolManager.DestroyObjectPool(...)
    ↓
ObjectPoolComponent.InternalDestroyObjectPool(...)
    ↓
ObjectPool<T>.Shutdown()
    ↓
逐个对象执行 Release(true)
    ↓
归还业务池对象与内部包装对象到 ReferencePool
    ↓
清空内部容器
```

---

## 7. 模块生命周期

## 7.1 ObjectPoolComponent 生命周期

### `Awake()`

作用：

- 把当前组件注册为 `IObjectPoolManager` 模块。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IObjectPoolManager>(this);
}
```

### `Priority`

作用：

- 声明模块优先级。

当前实现：

```csharp
public int Priority
{
    get { return 6; }
}
```

### `OnInit()`

作用：

- 初始化对象池字典、缓存列表和排序器。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 驱动所有对象池执行自动释放计时。

### `Shutdown()`

作用：

- 关闭并清理所有对象池。

---

## 7.2 ObjectBase 生命周期

这是业务对象最需要关注的生命周期。

### 创建阶段

通常写法：

```csharp
public static MyPoolObject Create(string name, object target)
{
    MyPoolObject obj = ReferencePool.Acquire<MyPoolObject>();
    obj.Initialize(name, target);
    return obj;
}
```

### 使用阶段

#### `OnSpawn()`

作用：

- 对象被取出时调用；
- 常用于启用对象、显示对象、恢复运行状态。

#### `OnUnspawn()`

作用：

- 对象归还池时调用；
- 常用于隐藏对象、停止逻辑、进入待机状态。

### 释放阶段

#### `Release(bool isShutdown)`

作用：

- 对象真正被从池中移除时调用；
- 常用于销毁底层 `GameObject`、释放资源句柄、释放 UI 实例等。

### 回收到引用池阶段

#### `Clear()`

作用：

- 清理对象自身字段；
- 让对象恢复到可再次复用的默认状态。

---

## 8. 典型使用方式

## 8.1 创建对象池

```csharp
_goObjectPool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<GoPoolObject>("GoPool");
```

---

## 8.2 定义池对象

```csharp
public sealed class MyPoolObject : ObjectBase
{
    public static MyPoolObject Create(string name, object target)
    {
        MyPoolObject obj = ReferencePool.Acquire<MyPoolObject>();
        obj.Initialize(name, target);
        return obj;
    }

    protected internal override void Release(bool isShutdown)
    {
    }
}
```

---

## 8.3 注册对象

```csharp
pool.Register(MyPoolObject.Create("Enemy", go), true);
```

说明：

- `true` 表示注册后立即视为已使用。

---

## 8.4 获取对象

```csharp
var obj = pool.Spawn("Enemy");
if (obj != null)
{
    var go = (GameObject)obj.Target;
}
```

---

## 8.5 回收对象

```csharp
pool.Unspawn(go);
```

---

## 8.6 修改锁和优先级

```csharp
pool.SetLocked(go, true);
pool.SetPriority(go, 10);
```

---

## 8.7 释放未使用对象

```csharp
pool.Release();
pool.ReleaseAllUnused();
```

---

## 9. 使用注意事项

### 9.1 业务对象应继承 `ObjectBase`

当前模块里真正需要业务继承的核心基类是：

```csharp
ObjectBase
```

而不是：

- `ObjectPoolBase`
- `ObjectPoolComponent`

---

### 9.2 `OnUnspawn` 不等于真正销毁

- `OnUnspawn`：只是归还到池
- `Release`：才是真正移出对象池并释放资源

不要把“最终释放资源”的逻辑放到 `OnUnspawn`。

---

### 9.3 `Clear()` 与 `Release()` 的职责不同

- `Release()`：处理目标对象或外部资源
- `Clear()`：清理池对象包装类自身字段

这两个职责不要混淆。

---

### 9.4 `Target` 不能为空

`ObjectBase.Initialize(...)` 内部会校验 `target`，所以池对象必须包装一个真实目标对象。

---

### 9.5 当前项目默认与 `ReferencePool` 配合使用

当前项目中的池对象创建方式，基本都采用：

```csharp
ReferencePool.Acquire<T>()
```

因此在接入新业务对象时，建议保持同样模式。

---

## 10. 总结

当前 `ObjectPool` 模块可以概括为：

- 一个 `sealed` 的对象池管理组件 `ObjectPoolComponent`；
- 一个统一管理接口 `IObjectPoolManager`；
- 一个泛型池接口 `IObjectPool<T>`；
- 一个真正需要业务继承的池对象基类 `ObjectBase`；
- 一套由 `OnSpawn -> OnUnspawn -> Release -> Clear` 组成的对象生命周期。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `ObjectPoolComponent` 如何创建和驱动对象池；
2. `ObjectPool<T>` 如何注册、获取、回收和释放对象；
3. `ObjectBase` 的生命周期边界，尤其是 `OnUnspawn`、`Release`、`Clear` 的职责区别。
