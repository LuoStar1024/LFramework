# ObjectPool

Source paths:
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolComponent.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectPoolComponent.ObjectPool.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/ObjectBase.cs`
- `Assets/LFramework/Runtime/Component/ObjectPool/IObjectPool*.cs`

`ObjectPoolComponent` registers `IObjectPoolManager` and manages pools of `ObjectBase` wrappers.

Responsibility:

- Create, query, and destroy named object pools.
- Spawn and unspawn objects.
- Release unused objects by capacity, expiration, priority, and lock state.
- Update each pool for automatic release.

Lifecycle:

- `Awake()` registers `IObjectPoolManager`.
- `OnUpdate()` updates all object pools.
- `Shutdown()` shuts down pools and releases wrappers.
- Low-memory flow calls `ReleaseAllUnused()`.

Object requirements:

- Poolable objects inherit `ObjectBase`.
- `ObjectBase.Target` is the actual object.
- `Name`, `Locked`, and `Priority` affect spawn/release behavior.
- `Release(bool isShutdown)` must distinguish ordinary recycle from shutdown.

Dependencies:

- `ResourceComponent` uses ObjectPool for asset objects.
- `UIComponent` uses ObjectPool for UI form instances.

Usage guidance:

- Do not manually destroy pooled instances behind the pool.
- Unlock objects when they become releasable.
- Choose object names consistently; named spawn/release depends on them.
- Use ReferencePool for short-lived metadata, ObjectPool for reusable target objects.
