# Extension Practices

Source paths:
- `Books/LFramework框架解析教程/10-扩展模块实践建议.md`
- `Assets/LFramework/Runtime/Core/ILFrameworkModule.cs`
- `Assets/LFramework/Runtime/Core/LFrameworkEntry.cs`

## Add a Framework Module

Define an interface first:

```csharp
public interface IExampleManager
{
    void DoSomething();
}
```

Implement a Unity component:

```csharp
public sealed class ExampleComponent : MonoBehaviour, ILFrameworkModule, IExampleManager
{
    public int Priority => 0;

    private void Awake()
    {
        LFrameworkEntry.RegisterModule<IExampleManager>(this);
    }

    public void OnInit() {}
    public void OnUpdate(float elapseSeconds, float realElapseSeconds) {}
    public void Shutdown() {}
}
```

Rules:

- Register and retrieve by interface.
- Keep `OnInit()` for local state.
- Acquire other modules in `Start()` or later unless ordering is guaranteed.
- Set `Priority` only when there is a real update/shutdown order need.

## Extend GameLogic

GameLogic modules can expose project-friendly APIs over Runtime modules, like `AudioExtension`, `UIExtension`, `ResourceContainer`, and `EventContainer`.

Use this pattern when:

- a feature needs DataTable lookup before calling a runtime manager;
- a feature needs owner-scoped cleanup;
- business code would otherwise duplicate asset path or setting logic.

## Reference-Pooled Helpers

Use `IReference` for frequent temporary objects:

- `Create()` acquires from `ReferencePool`.
- `Clear()` resets all fields.
- owner releases with `ReferencePool.Release()`.
- release fields are nulled after release to avoid accidental reuse.

## Resource and Event Ownership

For owner-scoped objects, release in this order:

```text
Release/OnClose/OnRecycle
  -> release EventContainer
  -> release ResourceContainer
  -> remove timers
  -> clear own state
```

## UI Extensions

- Prefabs should include `UIForm` plus a concrete `UIFormLogic`/`UguiForm`.
- Do not bypass `UIComponent` pooling.
- Keep widget lifecycles inside `UguiForm` helpers.

## Object Pool Extensions

If custom objects enter ObjectPool, inherit `ObjectBase` and implement `Release(isShutdown)` carefully. Use `Locked` for in-use objects and `Priority` to influence release selection.
