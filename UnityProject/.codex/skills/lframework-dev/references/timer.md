# Timer

Source paths:
- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.cs`
- `Assets/LFramework/Runtime/Component/Timer/TimerComponent.Timer.cs`
- `Assets/LFramework/Runtime/Component/Timer/TimerInfo.cs`
- `Assets/LFramework/Runtime/Component/Timer/ITimerManager.cs`

`TimerComponent` registers `ITimerManager` and manages delayed or repeated callbacks.

Responsibility:

- Add timers with scaled or unscaled time.
- Stop, resume, reset, remove, and query timers by id.
- Update timers each frame.
- Reuse internal timer objects through `ReferencePool`.

Lifecycle:

- `Awake()` registers `ITimerManager`.
- `OnUpdate()` advances scaled and unscaled timers.
- `Shutdown()` removes timers and releases internal objects.

Usage:

```csharp
int timerId = GameEntry.Timer.AddTimer(1.0f, OnTimeout, repeatCount: 1);
GameEntry.Timer.RemoveTimer(timerId);
```

Use unscaled timers for UI or pause-independent behavior. Store timer ids when the owner may need cancellation.

Cleanup rules:

- Remove owner-created timers when the owner closes or releases.
- Avoid callbacks capturing disposed UI, released containers, or destroyed GameObjects.
- `RemoveAllTimer()` is broad; prefer targeted removal unless shutting down a whole subsystem.

Diagnostics:

- `GetTimersInfo()` and `GetUnscaledTimersInfo()` expose timer data for debugging.
