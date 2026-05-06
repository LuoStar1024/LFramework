# Module Lifecycle

Source paths:
- `Assets/LFramework/Runtime/Core/ILFrameworkModule.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`
- `Assets/LFramework/Runtime/Component/*/*Component.cs`
- `Assets/GameScripts/GameLogic/Component/*/*Component.cs`

Every framework module implements `ILFrameworkModule`:

```csharp
int Priority { get; }
void OnInit();
void OnUpdate(float elapseSeconds, float realElapseSeconds);
void Shutdown();
```

Typical registration pattern:

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IExampleManager>(this);
}
```

Registration rules:

- The generic type must be an interface.
- Duplicate registrations for the same interface throw `LFrameworkException`.
- `RegisterModule<T>()` immediately inserts by `Priority` and calls `OnInit()`.
- Higher `Priority` modules update earlier.
- Shutdown walks the module list backward, so higher `Priority` modules shut down later.

Initialization timing matters. Because Unity `Awake()` order across objects can vary, `OnInit()` should initialize the module's own collections, flags, and cached local data. Cross-module dependencies are safer in `Start()` or later. Existing examples: `ResourceComponent`, `AudioComponent`, `SceneComponent`, and `LocalizationComponent` acquire dependencies after registration.

Access rules:

- Use `LFrameworkEntry.GetModule<IResourceManager>()` in framework-level code.
- Use `GameEntry.Resource`, `GameEntry.UI`, `GameEntry.Event`, and similar facades in GameLogic/business code after `GameEntry.Start()`.
- Do not retrieve modules by concrete component class.

Priority guidance:

- Event dispatch, object pool, and resource systems usually need stable early updates.
- Business flow modules can use lower priorities unless there is a concrete ordering requirement.
- No explicit ordering need: use `0`.

Shutdown guidance:

- Release owner-scoped containers before their backing modules shut down.
- UI should close/recycle through `UIComponent`, not direct `Destroy`.
- Reference-pooled temporary objects must be released before `ReferencePool.ClearAll()`.
