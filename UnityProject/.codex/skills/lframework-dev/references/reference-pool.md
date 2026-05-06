# ReferencePool

Source paths:
- `Assets/LFramework/Runtime/Core/ReferencePool/*.cs`
- `Assets/LFramework/Runtime/Component/ReferencePool/ReferencePoolComponent.cs`
- `Assets/LFramework/Runtime/Component/ReferencePool/ReferenceStrictCheckType.cs`

`ReferencePool` is the Core static pool for short-lived managed objects. `ReferencePoolComponent` configures strict checking from Unity.

Responsibility:

- Reuse objects implementing `IReference`.
- Reduce allocations for event args, FSM data, timers, UI metadata, object wrappers, and containers.
- Provide pool diagnostics through `ReferencePoolInfo`.

Required pattern:

```csharp
public sealed class ExampleInfo : IReference
{
    public object UserData { get; private set; }

    public static ExampleInfo Create(object userData)
    {
        var info = ReferencePool.Acquire<ExampleInfo>();
        info.UserData = userData;
        return info;
    }

    public void Clear()
    {
        UserData = null;
    }
}
```

Rules:

- `Clear()` must reset all retained references and primitive state.
- Release with `ReferencePool.Release(instance)` exactly once.
- Do not use released instances.
- Enable strict checks during development to catch duplicate release and invalid reference types.

Dependencies:

- EventPool, FSM, Timer, ObjectPool wrappers, `ResourceContainer`, `EventContainer`, UI metadata, and audio params use this pattern.

Troubleshooting:

- If stale data appears, inspect `Clear()`.
- If duplicate release occurs, check ownership and set fields to null after release.
- `LFrameworkEntry.Shutdown()` calls `ReferencePool.ClearAll()`, so owner cleanup should happen before shutdown.
