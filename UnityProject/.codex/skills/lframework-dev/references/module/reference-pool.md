# ReferencePool

## GameLogic 推荐用法

- GameLogic/业务代码中只有需要复用短生命周期托管对象时才直接使用 `ReferencePool`，常见对象包括临时数据、事件参数、UI 打开信息、资源/事件/Widget 容器和对象池包装对象。
- 自定义可回收对象必须实现 `IReference`，并提供统一的 `Create(...)` 工厂方法，在内部调用 `ReferencePool.Acquire<T>()`。
- `ReferencePool.Acquire<T>()`：从对应类型的引用池取对象；没有可复用对象时创建新实例。
- `ReferencePool.Release(reference)`：归还对象到引用池；归还时会自动调用对象的 `Clear()`。
- 释放后立即把调用方持有的字段置为 `null`，避免继续访问已回收实例。

```csharp
public sealed class ExampleInfo : IReference
{
    public object UserData { get; private set; }

    public static ExampleInfo Create(object userData)
    {
        ExampleInfo info = ReferencePool.Acquire<ExampleInfo>();
        info.UserData = userData;
        return info;
    }

    public void Clear()
    {
        UserData = null;
    }
}
```

## 注意事项

- `ReferencePool` 是 Core 静态池，不是 `GameEntry.ReferencePool` 风格的业务模块，也没有 `IReferencePoolManager` 接口。
- `ReferencePool.Acquire<T>()` 要求 `T : class, IReference, new()`；通过 `Acquire(Type)` 获取时，严格检查开启后会校验类型是否为非抽象类并实现 `IReference`。
- `Clear()` 必须清空所有保留字段，包括引用字段、集合内容、状态枚举、计数器和回调委托；否则下次复用会读到脏数据。
- `Release()` 必须只调用一次；严格检查开启后，重复释放同一实例会抛出 `LFrameworkException`。
- 不要在释放后继续访问对象，也不要把已释放对象传给异步回调、事件或协程。
- 不要把 Unity `GameObject`、`Component` 或需要 Unity 生命周期管理的对象直接放入 `ReferencePool`；引用池只适合普通托管对象。
- `ReferencePoolComponent` 只负责在运行时设置 `ReferencePool.EnableStrictCheck`，严格检查会影响性能，发布配置需按项目策略选择。
- `LFrameworkEntry.Shutdown()` 会调用 `ReferencePool.ClearAll()` 清空所有引用池；业务所有者仍应在自身生命周期内主动释放持有对象。
- `ReferencePool.GetAllReferencePoolInfos()` 只用于调试和诊断引用池数量、正在使用数量、获取/释放统计，不应作为业务逻辑依赖。

## ReferencePool API 速查

`ReferencePool` 是静态工具类，直接通过 `ReferencePool.Xxx` 调用。

- 配置与统计：`EnableStrictCheck`, `Count`, `GetAllReferencePoolInfos()`。
- 获取引用：`Acquire<T>()`, `Acquire(referenceType)`。
- 归还引用：`Release(reference)`；内部会先调用 `reference.Clear()`，再放回对应类型队列。
- 预热引用：`Add<T>(count)`, `Add(referenceType, count)`。
- 移除引用：`Remove<T>(count)`, `Remove(referenceType, count)`, `RemoveAll<T>()`, `RemoveAll(referenceType)`。
- 全量清理：`ClearAll()`。
- 引用对象契约：`IReference.Clear()`。
- 诊断信息：`ReferencePoolInfo.Type`, `UnusedReferenceCount`, `UsingReferenceCount`, `AcquireReferenceCount`, `ReleaseReferenceCount`, `AddReferenceCount`, `RemoveReferenceCount`。
- 严格检查配置：`ReferenceStrictCheckType.AlwaysEnable`, `OnlyEnableWhenDevelopment`, `OnlyEnableInEditor`, `AlwaysDisable`。

## 源码路径

- `Assets/LFramework/Runtime/Core/ReferencePool/IReference.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePool.ReferenceCollection.cs`
- `Assets/LFramework/Runtime/Core/ReferencePool/ReferencePoolInfo.cs`
