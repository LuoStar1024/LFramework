# UnityWrapper 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `UnityWrapper` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 是否存在需要继承的基类；
- 模块核心生命周期。

---

## 2. 模块定位

`UnityWrapper` 模块是 LFramework 对 Unity 原生能力做的一层桥接包装。  
当前这个模块的实际核心职责主要只有两类：

- 对外提供统一的协程启动 / 停止接口；
- 在初始化阶段触达若干 Unity 类型，辅助裁剪保留。

因此它在项目中的定位，是“框架级 Unity 桥接模块”，当前重点是协程桥接。

---

## 3. 核心类型

## 3.1 UnityWrapperComponent

文件：

- `Assets/LFramework/Runtime/Component/UnityWrapper/UnityWrapperComponent.cs`
- `Assets/LFramework/Runtime/Component/UnityWrapper/UnityWrapperComponent.Coroutine.cs`

定义：

```csharp
public sealed partial class UnityWrapperComponent : MonoBehaviour, ILFrameworkModule, IUnityWrapperManager
```

职责：

- 作为 UnityWrapper 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 对外暴露 `IUnityWrapperManager`；
- 提供协程相关桥接能力；
- 在 `Awake()` 中触达若干 Unity 类型以降低裁剪风险。

说明：

- `sealed`，当前模块不通过继承 `UnityWrapperComponent` 扩展；
- 业务层通常通过 `LFrameworkEntry.GetModule<IUnityWrapperManager>()` 使用。

---

## 3.2 IUnityWrapperManager

文件：

- `Assets/LFramework/Runtime/Component/UnityWrapper/IUnityWrapperManager.cs`

职责：

- 定义 Unity 桥接模块的对外统一接口；
- 当前全部接口都围绕协程控制。

---

## 3.3 与流程模块的关系

当前明确调用点中，`ProcedureInitResources` 会通过该模块来启动协程：

```csharp
_unityWrapperComponent.StartCoroutineWrapper(InitResources(procedureOwner));
```

因此：

- UnityWrapper 模块的主要价值，是让非 `MonoBehaviour` 的流程代码也能借助统一入口运行协程。

---

## 4. 核心 API

## 4.1 启动协程 API

### 按方法名启动

```csharp
Coroutine StartCoroutineWrapper(string methodName)
Coroutine StartCoroutineWrapper(string methodName, object value)
```

说明：

- 这是对 `MonoBehaviour.StartCoroutine(string)` 和带参数重载的桥接；
- 依赖目标方法存在于该 `MonoBehaviour` 上。

### 按 IEnumerator 启动

```csharp
Coroutine StartCoroutineWrapper(IEnumerator routine)
```

说明：

- 这是当前实际更常用的形式；
- 适合流程模块传入自己的协程实例。

---

## 4.2 停止协程 API

### 按方法名停止

```csharp
void StopCoroutineWrapper(string methodName)
```

### 按 IEnumerator 停止

```csharp
void StopCoroutineWrapper(IEnumerator routine)
```

### 按 Coroutine 句柄停止

```csharp
void StopCoroutineWrapper(Coroutine routine)
```

### 停止全部协程

```csharp
void StopAllCoroutinesWrapper()
```

说明：

- 该接口会停止当前 `UnityWrapperComponent` 上挂着的全部协程。

---

## 5. 关键调用链

## 5.1 模块注册调用链

```text
UnityWrapperComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IUnityWrapperManager>(this)
    ↓
UnityWrapperComponent.OnInit()
```

---

## 5.2 协程启动调用链

以 `IEnumerator` 形式为例：

```text
业务层 / 流程层
    ↓
IUnityWrapperManager.StartCoroutineWrapper(routine)
    ↓
UnityWrapperComponent.StartCoroutineWrapper(routine)
    ↓
MonoBehaviour.StartCoroutine(routine)
```

---

## 5.3 协程停止调用链

以 `Coroutine` 句柄形式为例：

```text
业务层 / 流程层
    ↓
IUnityWrapperManager.StopCoroutineWrapper(coroutine)
    ↓
UnityWrapperComponent.StopCoroutineWrapper(coroutine)
    ↓
MonoBehaviour.StopCoroutine(coroutine)
```

---

## 5.4 停止全部协程调用链

```text
业务层 / 流程层
    ↓
IUnityWrapperManager.StopAllCoroutinesWrapper()
    ↓
UnityWrapperComponent.StopAllCoroutinesWrapper()
    ↓
MonoBehaviour.StopAllCoroutines()
```

---

## 6. 模块生命周期

## 6.1 UnityWrapperComponent 生命周期

### `Awake()`

作用：

- 将当前组件注册为 `IUnityWrapperManager` 模块；
- 调用多次 `RegisterType<T>()` 触达 Unity 类型。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IUnityWrapperManager>(this);
    RegisterType<Collider>();
    RegisterType<Rigidbody>();
    ...
}
```

### `OnInit()`

作用：

- 当前实现为空；
- 模块不依赖额外运行时容器初始化。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 当前实现为空；
- UnityWrapper 模块本身不依赖逐帧逻辑。

### `Shutdown()`

作用：

- 当前实现为空；
- 按模块语义它应代表 UnityWrapper 的关闭清理时机。

---

## 6.2 `RegisterType<T>()` 的含义

虽然它不是对外 API，但这是模块里比较关键的辅助逻辑。

作用：

- 在代码层显式触达某些 Unity 类型；
- 目的是降低 IL2CPP / 裁剪时这些类型被错误裁掉的概率。

当前已触达的类型包括：

- `Collider`
- `Collider2D`
- `Collision`
- `Collision2D`
- `CapsuleCollider2D`
- `Rigidbody`
- `Rigidbody2D`
- `Ray`
- `Ray2D`
- `Mesh`
- `MeshRenderer`
- `AnimationClip`
- `AnimationCurve`
- `AnimationEvent`
- `AnimationState`
- `Animator`
- `Animation`

说明：

- 这不是业务生命周期；
- 但它是该模块“Unity 桥接”职责的一部分。

---

## 7. 是否存在需要继承的基类

结论：当前 UnityWrapper 模块没有需要业务继承的基类。

原因：

- `UnityWrapperComponent` 是 `sealed`；
- `IUnityWrapperManager` 是纯接口调用模式；
- 调用方只需要把它当作“协程桥接器”使用。

因此：

- 不建议通过继承 UnityWrapper 模块扩展功能；
- 更适合通过增加接口能力或新增桥接方法扩展。

---

## 8. 典型使用方式

## 8.1 启动协程

```csharp
var unityWrapper = LFrameworkEntry.GetModule<IUnityWrapperManager>();
unityWrapper.StartCoroutineWrapper(SomeRoutine());
```

---

## 8.2 停止指定协程

```csharp
Coroutine coroutine = unityWrapper.StartCoroutineWrapper(SomeRoutine());
unityWrapper.StopCoroutineWrapper(coroutine);
```

---

## 8.3 停止全部协程

```csharp
unityWrapper.StopAllCoroutinesWrapper();
```

---

## 8.4 当前项目中的实际使用

在资源初始化流程中：

```csharp
_unityWrapperComponent.StartCoroutineWrapper(InitResources(procedureOwner));
```

这说明当前模块主要用于：

- 让流程层通过统一模块入口运行 Unity 协程。

---

## 9. 使用注意事项

### 9.1 UnityWrapper 是全局共享模块

因此：

- 它启动的协程都挂在同一个 `UnityWrapperComponent` 上；
- `StopAllCoroutinesWrapper()` 会影响该组件上的所有协程，而不只是当前调用方自己的协程。

---

### 9.2 当前模块不负责复杂任务调度

它不是：

- Timer 系统；
- Task 调度器；
- UniTask Runner；

它当前只是 Unity 原生协程的桥接层。

---

### 9.3 依赖组件存在

调用方默认：

- `UnityWrapperComponent` 已经在框架 Prefab / 场景中存在；
- 且已经完成 `Awake()` 注册。

否则通过 `LFrameworkEntry.GetModule<IUnityWrapperManager>()` 获取时会失败。

---

## 10. 总结

当前 `UnityWrapper` 模块可以概括为：

- 一个 `sealed` 的 Unity 桥接组件 `UnityWrapperComponent`；
- 一个统一接口 `IUnityWrapperManager`；
- 一组围绕协程启动与停止的桥接 API；
- 一段用于降低裁剪风险的类型触达逻辑。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `IUnityWrapperManager` 目前真正提供了哪些能力；
2. 协程都是挂在哪个组件上运行的；
3. `StopAllCoroutinesWrapper()` 在共享模块语义下的影响范围。
