# UnityWrapper

Source paths:
- `Assets/LFramework/Runtime/Component/UnityWrapper/UnityWrapperComponent.cs`
- `Assets/LFramework/Runtime/Component/UnityWrapper/UnityWrapperComponent.Coroutine.cs`
- `Assets/LFramework/Runtime/Component/UnityWrapper/IUnityWrapperManager.cs`

`UnityWrapperComponent` registers `IUnityWrapperManager` and exposes selected MonoBehaviour coroutine APIs through the framework module system.

Responsibility:

- Provide `StartCoroutineWrapper` overloads.
- Provide `StopCoroutineWrapper` overloads.
- Provide `StopAllCoroutinesWrapper()`.
- Act as a Unity lifecycle bridge for systems that should access coroutine capability through a module.

Lifecycle:

- `Awake()` registers `IUnityWrapperManager`.
- `OnInit()` and `OnUpdate()` are light in current implementation.
- `Shutdown()` stops module-owned coroutine behavior where needed.

Usage:

- Prefer UniTask for new async IO/resource work when existing code follows that pattern.
- Use UnityWrapper when an API explicitly needs Unity coroutine semantics.
- Access through `GameEntry.Unity` in GameLogic after initialization.

Cleanup rules:

- Stop owner-created coroutines on owner close/release.
- Avoid string-based coroutine overloads unless matching MonoBehaviour method names are stable.
- Do not use UnityWrapper as a general global MonoBehaviour dumping ground.
