# Base

Source paths:
- `Assets/LFramework/Runtime/Component/Base/BaseComponent.cs`
- `Assets/LFramework/Runtime/Component/Base/IBaseManager.cs`

`BaseComponent` is the runtime holder for application-level framework settings and implements `ILFrameworkModule, IBaseManager`. It registers as `IBaseManager` in `Awake()`.

Responsibility:

- Expose runtime flags and application metadata needed by other modules.
- Provide values used by debugging and environment display.
- Participate in module lifecycle without owning heavy resources.

Lifecycle:

- `Awake()` registers `IBaseManager`.
- `OnInit()` performs local initialization only.
- `OnUpdate()` has no heavy per-frame ownership in the current implementation.
- `Shutdown()` releases Base-owned state if added later.

Dependencies:

- Other modules may read Base data through `IBaseManager`.
- Debugger environment windows read Base values for runtime display.

Usage:

- Business code normally accesses Base through `GameEntry.Base`.
- Framework code can use `LFrameworkEntry.GetModule<IBaseManager>()`.

Extension guidance:

- Keep Base as configuration/state exposure, not a place for gameplay services.
- If new values are serialized on the component, preserve Unity inspector compatibility.
- Avoid adding cross-module initialization in `OnInit()` unless the dependency order is guaranteed.
