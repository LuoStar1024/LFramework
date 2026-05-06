# Architecture

Source paths:
- `Assets/LFramework/Runtime/Component/RootComponent.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`
- `Assets/LFramework/Runtime/Core/ILFrameworkModule.cs`
- `Assets/GameScripts/GameLogic/Base/GameEntry*.cs`

LFramework is layered as:

```text
GameLogic
  project-facing entry points, UI, DataTable, Singleton, containers
Runtime/Component
  Unity-facing framework modules registered into LFrameworkEntry
Runtime/Core
  pure C# primitives: module registry, pools, events, variables, utility, log
```

Startup flow:

```text
RootComponent.Awake()
  -> listens for Application.lowMemory
Runtime/GameLogic component Awake()
  -> LFrameworkEntry.RegisterModule<I...>(this)
  -> module.OnInit()
RootComponent.Update()
  -> LFrameworkEntry.OnUpdate(deltaTime, unscaledDeltaTime)
GameEntry.Start()
  -> reinitializes Procedure FSM
  -> caches built-in and custom modules
```

`LFrameworkEntry` owns the module dictionary and priority-sorted module list. Modules are registered by interface type and retrieved by interface type. `GetModule<T>()` throws if `T` is not an interface or no module is registered.

`GameEntry` is the project-facing facade. It caches built-in modules such as `Resource`, `Scene`, `Audio`, `Event`, `Fsm`, `Procedure`, `Setting`, and custom modules such as `DataTable`, `UI`, `Singleton`. Business code should prefer `GameEntry.Xxx` after `GameEntry.Start()` has initialized these references.

Low-memory flow:

```text
Application.lowMemory
  -> RootComponent.OnLowMemory()
  -> ObjectPool.ReleaseAllUnused()
  -> Resource.ForceUnloadUnusedAssets(true)
```

Shutdown flow:

```text
RootComponent.Shutdown(type)
  -> Destroy(root gameObject)
  -> RootComponent.OnDestroy()
  -> LFrameworkEntry.Shutdown()
  -> modules shutdown in reverse priority order
  -> ReferencePool.ClearAll()
```

Cross-layer rule: Runtime framework code should not depend on GameLogic code. GameLogic may depend on Runtime via `GameEntry` or framework interfaces.
