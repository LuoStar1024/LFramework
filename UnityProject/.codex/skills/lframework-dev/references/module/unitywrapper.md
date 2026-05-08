# UnityWrapper

## GameLogic 推荐用法

- GameLogic/业务代码优先通过 `GameEntry.Unity` 访问协程桥接能力，不要在业务层直接查找或持有 `UnityWrapperComponent`。
- 新增异步 IO、资源加载或流程编排时，优先沿用项目中已有的 UniTask/YooAsset 异步模式；只有调用方明确需要 Unity 协程语义时，再使用 UnityWrapper。
- `GameEntry.Unity.StartCoroutineWrapper(IEnumerator routine)`：启动一个由 UnityWrapper 托管的协程；如果后续需要单独停止，应保存返回的 `Coroutine`。
- `GameEntry.Unity.StopCoroutineWrapper(Coroutine routine)`：停止通过 UnityWrapper 启动并已保存句柄的协程。
- `GameEntry.Unity.StopCoroutineWrapper(IEnumerator routine)`：停止同一个 `IEnumerator` 实例对应的协程。
- `GameEntry.Unity.StopAllCoroutinesWrapper()`：停止 UnityWrapper 组件上所有协程，仅适合模块级清理或明确拥有全部协程的场景。
- Launcher、框架集成代码或 `GameEntry` 尚未初始化的阶段，可通过 `LFrameworkEntry.GetModule<IUnityWrapperManager>()` 获取接口。

## 注意事项

- `UnityWrapperComponent` 是 Unity 生命周期桥接模块，`Awake()` 中注册 `IUnityWrapperManager`，并显式引用部分 UnityEngine 类型用于 AOT/裁剪保留。
- 该模块当前只包装 MonoBehaviour 协程 API；`OnInit()`、`OnUpdate()`、`Shutdown()` 没有额外业务逻辑。
- `StartCoroutineWrapper` 传入空字符串或 `null` routine 时返回 `null`；`StopCoroutineWrapper` 传入空字符串、`null` routine 或 `null` `Coroutine` 时直接返回。
- 尽量使用 `IEnumerator` 或 `Coroutine` 重载。字符串重载依赖 MonoBehaviour 方法名，重命名和裁剪风险更高，只在确有稳定方法名时使用。
- 调用方必须明确协程所有权，在窗口关闭、流程退出、模块释放等生命周期节点停止自己创建的协程。
- 不要把 UnityWrapper 当作全局 MonoBehaviour 容器挂载任意逻辑；它只负责框架模块化场景下的 Unity 协程与 AOT 保留桥接。

## IUnityWrapperManager API 速查

仅在框架集成代码、Launcher 流程或 `GameEntry.Unity` facade 内部优先考虑直接使用 `IUnityWrapperManager`。

- 启动协程：`StartCoroutineWrapper(IEnumerator routine)` 返回 `Coroutine`。
- 字符串启动：`StartCoroutineWrapper(string methodName)`、`StartCoroutineWrapper(string methodName, object value)` 返回 `Coroutine`。
- 停止协程：`StopCoroutineWrapper(Coroutine routine)`、`StopCoroutineWrapper(IEnumerator routine)`、`StopCoroutineWrapper(string methodName)`。
- 停止全部：`StopAllCoroutinesWrapper()` 停止 UnityWrapper 组件上的全部协程。

## 源码路径

- `Assets/LFramework/Runtime/Component/UnityWrapper/IUnityWrapperManager.cs`
- `Assets/LFramework/Runtime/Component/UnityWrapper/UnityWrapperComponent.cs`
- `Assets/LFramework/Runtime/Component/UnityWrapper/UnityWrapperComponent.Coroutine.cs`
