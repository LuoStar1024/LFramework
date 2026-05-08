# Singleton

## GameLogic 推荐用法

- GameLogic/业务代码中优先通过具体管理器的 `Instance` 使用单例，不要手动 `new` 单例对象，也不要绕过 `SingletonComponent` 管理生命周期。
- 纯 C# 管理器优先继承 `Singleton<T>`。首次访问 `T.Instance` 时会创建实例、调用 `OnInit()`，并通过 `GameEntry.Singleton.RegisterSingleton(instance)` 注册到单例模块。
- 需要 `Transform`、`GameObject`、Unity 组件能力或 Unity 消息时，继承 `SingletonBehaviour<T>`。首次访问 `T.Instance` 时会按类型名查找已托管对象、场景根节点对象或创建新 `GameObject`，再挂载组件并注册到单例模块。
- 初始化逻辑写在 `OnInit()`，释放逻辑写在 `OnRelease()`。释放事件订阅、定时器、资源容器、对象池等持有内容时，优先集中放在 `OnRelease()`。
- 需要被框架模块驱动更新的单例实现 `ISingletonUpdate`，`SingletonComponent.OnUpdate()` 会转发 `OnUpdate(elapseSeconds, realElapseSeconds)`。

## 注意事项

- 业务代码通常通过 `GameEntry.Singleton` 访问。
- `Singleton<T>` 带 `new()` 约束，编辑器下如果绕过 `Instance` 构造对象会输出错误日志；不要在业务代码中直接调用构造函数。
- `SingletonBehaviour<T>` 以类型名作为默认 `GameObject` 名称，并通过 `GameEntry.Singleton.GetGameObject(typeName)`、`GameObject.Find($"/{typeName}")` 或新建对象获取载体；同名单例对象需要保持唯一，避免注册和查找混乱。
- 不要直接 `Destroy` 已纳入单例模块托管的 `SingletonBehaviour<T>` 对象。需要主动释放时，使用 `GameEntry.Singleton.ReleaseSingleton(singleton, gameObject)`，或让组件销毁流程触发 `OnDestroy()` 中的释放通知。
- 单例不适合保存短生命周期的场景状态或 UI 临时状态；这类数据应优先放在对应场景、流程、UIForm 或明确生命周期的对象中。
- 如果单例实现了 `ISingletonUpdate`，必须通过 `GameEntry.Singleton.ReleaseSingleton(...)` 或模块 `Shutdown()` 释放，确保它从更新列表移除。

## ISingletonManager API 速查

仅在框架集成代码、单例基类内部或确实需要手动管理单例生命周期时直接使用 `ISingletonManager`。

- 纯 C# 单例注册：`RegisterSingleton(ISingleton singleton)`，注册后如果对象实现 `ISingletonUpdate`，会加入模块更新列表。
- 纯 C# 单例释放：`ReleaseSingleton(ISingleton singleton)`，会先移除生命周期更新，再调用 `singleton.Release()`，最后从托管列表移除。
- Behaviour 单例注册：`RegisterSingleton(ISingleton singleton, GameObject go)`，以 `go.name` 记录托管对象，并注册 `ISingletonUpdate` 生命周期。
- Behaviour 单例释放：`ReleaseSingleton(ISingleton singleton, GameObject go)`，会移除生命周期更新、调用 `singleton.Release()`、移除托管记录并销毁 `go`。
- Behaviour 对象查找：`GetGameObject(string goName)`，按名称返回已由 `SingletonComponent` 托管的单例 `GameObject`，未找到时返回 `null`。
- 模块关闭：`SingletonComponent.Shutdown()` 会释放已托管的 Behaviour 单例、销毁对应 `GameObject`，再逆序释放纯 C# 单例并清空更新列表。

## 源码路径

- `Assets/GameScripts/GameLogic/Component/Singleton/ISingleton.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/ISingletonManager.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/Singleton.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/SingletonBehaviour.cs`
- `Assets/GameScripts/GameLogic/Component/Singleton/SingletonComponent.cs`
