# Event 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Event` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 生命周期；
- 需要理解的接口/基类关系。

---

## 2. 模块定位

`Event` 模块是 LFramework 的全局事件系统，用于：

- 注册与移除事件处理函数；
- 线程安全地延迟分发事件；
- 立即分发事件；
- 提供事件组封装能力。

它在项目中的定位，是“全局消息总线”。

---

## 3. 核心类型

## 3.1 EventComponent

文件：

- `Assets/LFramework/Runtime/Component/Event/EventComponent.cs`

定义：

```csharp
public sealed class EventComponent : MonoBehaviour, ILFrameworkModule, IEventManager
```

职责：

- 作为 Event 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 在 `OnInit()` 中初始化 `EventPool`；
- 在 `OnUpdate()` 中驱动延迟事件队列处理；
- 对外暴露 `IEventManager` API。

说明：

- `sealed`，当前模块不以继承扩展为主；
- 业务层一般通过 `GameEntry.Event` 访问。

---

## 3.2 IEventManager

文件：

- `Assets/LFramework/Runtime/Component/Event/IEventManager.cs`

职责：

- 定义事件系统对外公开的统一接口。

主要分为四类能力：

1. 查询
2. 订阅 / 取消订阅
3. 触发
4. 事件组

---

## 3.3 EventPool

文件：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.cs`

职责：

- 作为真正的事件引擎；
- 保存 handler；
- 保存待分发事件队列；
- 负责实际分发逻辑；
- 管理默认处理函数和事件组。

说明：

- `EventComponent` 本身主要是模块包装，真正核心逻辑基本都在 `EventPool`。

---

## 3.4 EventPool.Event / EventArgs<T...>

文件：

- `Assets/LFramework/Runtime/Core/EventPool/EventPool.Event.cs`

职责：

- 封装延迟分发事件的数据；
- 保存事件参数；
- 在 `OnUpdate()` 时被取出并执行。

说明：

- 这套类型通过 `ReferencePool` 复用；
- 是 `Fire(...)` 延迟触发机制的基础。

---

## 3.5 EventRuntimeId

文件：

- `Assets/LFramework/Runtime/Core/EventPool/EventRuntimeId.cs`

职责：

- 把字符串事件名映射为运行时 `int` Id；
- 提供反查字符串。

常见用法：

```csharp
public static readonly int ReturnMenuId = EventRuntimeId.ToRuntimeId("EventGroupUI.ReturnMenuId");
```

---

## 3.6 EventGroup 封装类

文件示例：

- `Assets/GameScripts/GameLogic/Event/EventGroupLogic.cs`
- `Assets/GameScripts/GameLogic/Event/EventGroupUI.cs`

职责：

- 以业务语义封装事件调用；
- 在构造时注册到事件系统；
- 把“业务动作”转成 `Event.Fire(...)`。

说明：

- 这不是框架强制基类，而是一种当前项目中已经在使用的组织方式。

---

## 3.7 EventContainer

文件：

- `Assets/GameScripts/GameLogic/Component/Event/EventContainer.cs`

职责：

- 用于某个 owner 管理自己的订阅；
- 便于统一 `UnsubscribeAll()`；
- 降低 UI/对象生命周期结束后事件泄漏的风险。

---

## 4. 核心 API

## 4.1 查询类 API

| API                                    | 说明                |
| -------------------------------------- | ----------------- |
| `int EventHandlerCount`                | 当前事件处理函数总数量       |
| `int EventCount`                       | 当前待处理事件队列数量       |
| `int Count(int id)`                    | 获取指定事件 Id 的处理函数数量 |
| `bool Check(int id, Delegate handler)` | 检查某个处理函数是否已订阅     |

---

## 4.2 订阅 / 取消订阅 API

当前模块为 `0~8` 个参数都提供了重载，核心模式如下：

### 订阅

```csharp
Subscribe(int id, Action handler)
Subscribe<TArg1>(int id, Action<TArg1> handler)
...
Subscribe<TArg1, ..., TArg8>(int id, Action<...> handler)
Subscribe(int id, Delegate handler)
```

### 取消订阅

```csharp
Unsubscribe(int id, Action handler)
Unsubscribe<TArg1>(int id, Action<TArg1> handler)
...
Unsubscribe<TArg1, ..., TArg8>(int id, Action<...> handler)
Unsubscribe(int id, Delegate handler)
```

说明：

- 泛型重载是常用强类型调用方式；
- `Delegate` 版本更灵活，但也更容易误用。

---

## 4.3 触发 API

当前模块有两套触发方式。

### `Fire(...)`

特点：

- 线程安全；
- 不立刻执行；
- 进入队列后，在下一帧 `OnUpdate()` 中统一处理。

核心形式：

```csharp
Fire(int id)
Fire<TArg1>(int id, TArg1 arg1)
...
Fire<TArg1, ..., TArg8>(int id, ...)
```

适用场景：

- 跨线程触发；
- 希望统一在主线程处理；
- 希望逻辑顺序落到下一帧。

### `FireNow(...)`

特点：

- 非线程安全；
- 立即执行；
- 直接在当前调用栈分发。

核心形式：

```csharp
FireNow(int id)
FireNow<TArg1>(int id, TArg1 arg1)
...
FireNow<TArg1, ..., TArg8>(int id, ...)
```

适用场景：

- 当前已经确认在主线程；
- 需要立刻执行回调逻辑。

---

## 4.4 事件组 API

| API                              | 说明      |
| -------------------------------- | ------- |
| `void RegisterGroup<T>(T group)` | 注册事件组实例 |
| `T FireGroup<T>()`               | 获取事件组实例 |

说明：

- 尽管名字叫 `FireGroup<T>()`，但它的行为实际上是“获取事件组对象”。

---

## 4.5 默认处理函数

| API                                           | 说明                    |
| --------------------------------------------- | --------------------- |
| `void SetDefaultHandler(Action<int> handler)` | 当某个事件没有处理函数时，使用默认处理函数 |

说明：

- 仅在没有对应 handler 时生效；
- 如果也没有默认处理函数，并且模式不允许空 handler，会抛异常。

---

## 5. 关键调用链

## 5.1 模块注册调用链

```text
EventComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IEventManager>(this)
    ↓
LFrameworkEntry 保存模块
    ↓
立即调用 OnInit()
```

---

## 5.2 延迟事件调用链

```text
GameEntry.Event.Fire(...)
    ↓
EventComponent.Fire(...)
    ↓
EventPool.Fire(...)
    ↓
创建 EventArgs<T...> 队列节点
    ↓
加入 _events
    ↓
下一帧 EventComponent.OnUpdate(...)
    ↓
EventPool.OnUpdate(...)
    ↓
取出事件并 HandleEvent(...)
```

---

## 5.3 立即事件调用链

```text
GameEntry.Event.FireNow(...)
    ↓
EventComponent.FireNow(...)
    ↓
EventPool.FireNow(...)
    ↓
直接调用 HandleEvent(...)
    ↓
遍历当前事件 Id 的全部 handler
```

---

## 5.4 事件组调用链

以 UI 事件组为例：

```text
EventGroupUI 构造
    ↓
GameEntry.Event.RegisterGroup(this)
```

后续使用：

```text
GameEntry.Event.FireGroup<EventGroupUI>()
    ↓
拿到 EventGroupUI 实例
    ↓
调用 ReturnMenu()
    ↓
内部再执行 GameEntry.Event.Fire(ReturnMenuId)
```

---

## 6. 生命周期

## 6.1 EventComponent 生命周期

### `Awake()`

作用：

- 把当前组件注册为 `IEventManager` 模块。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IEventManager>(this);
}
```

---

### `Priority`

作用：

- 声明模块优先级。

当前实现：

```csharp
public int Priority
{
    get { return 7; }
}
```

说明：

- 当前优先级较高，说明事件系统希望较早参与模块轮询。

---

### `OnInit()`

作用：

- 初始化事件池。

当前实现：

```csharp
public void OnInit()
{
    _eventPool = new EventPool(EventPoolMode.AllowNoHandler | EventPoolMode.AllowMultiHandler);
}
```

说明：

- 当前配置允许：
  - 无处理函数事件；
  - 同一事件多个处理函数。

---

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 驱动延迟事件队列分发。

当前实现：

```csharp
_eventPool.OnUpdate(elapseSeconds, realElapseSeconds);
```

说明：

- 这是 `Fire(...)` 延迟分发的关键生命周期节点。

---

### `Shutdown()`

作用：

- 清理事件池。

当前实现：

```csharp
public void Shutdown()
{
    _eventPool.Shutdown();
}
```

说明：

- 会清理：
  - handler 表；
  - 队列；
  - group；
  - 默认处理函数。

---

## 6.2 EventPool.Event 生命周期

### 创建

由 `Fire(...)` 调用时通过 `EventArgs<T...>.Create(...)` 创建。

### 入队

进入 `_events` 队列等待下一帧处理。

### 处理

在 `EventPool.OnUpdate()` 中调用 `eventNode.HandleEvent()`。

### 回收

处理完成后通过 `ReferencePool.Release(eventNode)` 回收到对象池。

---

## 6.3 事件组生命周期

当前项目中的事件组没有统一基类，生命周期由业务层自己控制。

以 `EventGroupLogic` / `EventGroupUI` 为例：

### 创建

- 在 `EventHelper.OnInit()` 中通过 `new` 创建。

### 注册

- 在构造函数里调用 `GameEntry.Event.RegisterGroup(this)`。

### 销毁

- `EventHelper.OnDestroy()` 中仅把字段置空；
- 当前没有显式 `UnregisterGroup`。

说明：

- 这意味着当前事件组更偏“全局单例式长生命周期对象”。

---

## 6.4 EventContainer 生命周期

### 创建

```csharp
EventContainer.Create(owner)
```

### 使用

- 通过 `Subscribe(...)` 记录本地 handler；
- 同时调用全局 `GameEntry.Event.Subscribe(...)`。

### 清理

- 通常先调用 `UnsubscribeAll()`；
- 再由引用池回收。

说明：

- 当前安全用法依赖调用方先显式 `UnsubscribeAll()`。

---

## 7. 是否存在需要继承的基类

## 7.1 Event 模块自身

结论：当前 Event 模块本身不以继承扩展为主。

原因：

- `EventComponent` 是 `sealed`；
- 核心逻辑封装在 `EventPool`；
- 业务层通常通过接口或事件组封装来使用，而不是继承 `EventComponent`。

因此：

- 不建议通过继承 `EventComponent` 扩展功能；
- 更适合通过封装事件组、辅助类、容器类来组织使用方式。

---

## 7.2 需要重点理解的接口/基类

虽然 Event 模块不靠继承扩展，但有两个核心体系必须理解。

### `ILFrameworkModule`

作用：

- 让 `EventComponent` 接入框架模块系统。

核心生命周期：

| 生命周期         | 作用     |
| ------------ | ------ |
| `Priority`   | 模块优先级  |
| `OnInit()`   | 模块初始化  |
| `OnUpdate()` | 模块轮询   |
| `Shutdown()` | 模块关闭清理 |

### `Delegate / Action<T...>`

作用：

- 当前事件系统的 handler 基础。

说明：

- 当前不是基于自定义事件参数基类的统一分发；
- 而是依赖 `Action / Action<T...>` 的委托签名。

### `LFrameworkEventArgs`

作用：

- 框架中存在的通用事件参数基类。

当前状态：

- 当前 `EventPool` 并未直接使用它。

说明：

- 如果后续项目要统一事件参数体系，需要关注它；
- 但在当前模块主路径中，它不是核心必经类型。

---

## 8. 典型使用方式

## 8.1 直接订阅事件

```csharp
GameEntry.Event.Subscribe(SomeEventId, OnSomeEvent);
```

## 8.2 直接触发事件

```csharp
GameEntry.Event.Fire(SomeEventId);
GameEntry.Event.Fire(SomeEventId, someValue);
```

## 8.3 立即触发

```csharp
GameEntry.Event.FireNow(SomeEventId);
```

## 8.4 事件组使用

```csharp
GameEntry.Event.FireGroup<EventGroupUI>().ReturnMenu();
```

## 8.5 生命周期绑定容器

```csharp
eventContainer.Subscribe(SomeEventId, handler);
eventContainer.UnsubscribeAll();
```

---

## 9. 使用注意事项

### 9.1 `Fire(...)` 与 `FireNow(...)` 语义不同

- `Fire(...)`：线程安全、下一帧分发
- `FireNow(...)`：非线程安全、立即分发

不要混淆。

### 9.2 事件 Id 与 handler 签名应保持一致

虽然当前系统允许错误签名静默跳过，但业务上应避免同一事件 Id 混用不同 handler 类型。

### 9.3 事件组当前没有反注册

这意味着事件组更适合做长生命周期对象，而不是频繁创建销毁的对象。

### 9.4 使用 EventContainer 时要先 `UnsubscribeAll()`

否则只清本地容器，不一定会解除全局事件系统中的订阅关系。

---

## 10. 总结

当前 `Event` 模块可以概括为：

- 一个 `sealed` 的事件组件入口；
- 通过 `IEventManager` 对外提供完整事件 API；
- 通过 `EventPool` 实现真正的分发、队列和组管理；
- 通过 `ILFrameworkModule.OnUpdate()` 驱动延迟事件；
- 通过事件组和事件容器组织业务使用方式。

如果后续要改造它，最重要的不是继承模块本身，而是理解：

- `Fire` / `FireNow` 的行为差异；
- `EventPool` 的内部分发方式；
- 事件组的生命周期；
- `ILFrameworkModule` 对事件模块的更新驱动机制。
