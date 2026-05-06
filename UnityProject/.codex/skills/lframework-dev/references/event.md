# Event

Source paths:
- `Assets/LFramework/Runtime/Component/Event/EventComponent.cs`
- `Assets/LFramework/Runtime/Component/Event/IEventManager.cs`
- `Assets/LFramework/Runtime/Core/EventPool/*.cs`
- `Assets/GameScripts/GameLogic/Component/Event/EventContainer.cs`

`EventComponent` wraps Core `EventPool` and registers `IEventManager`. `EventContainer` is the GameLogic lifecycle helper for subscriptions.

Responsibility:

- Subscribe and unsubscribe delegates by integer event id.
- Fire events through EventPool.
- Update queued event dispatch in `OnUpdate()`.
- Help owner objects release all subscriptions via `EventContainer`.

Lifecycle:

- `EventComponent.Awake()` registers `IEventManager`.
- `OnInit()` creates/configures the event pool.
- `OnUpdate()` calls `_eventPool.OnUpdate()`.
- `Shutdown()` shuts down the event pool.
- `EventContainer.Clear()` calls `UnsubscribeAll()`, clears handlers, and nulls `Owner`.

Usage:

```csharp
_eventContainer = EventContainer.Create(this);
_eventContainer.Subscribe(eventId, handler);

ReferencePool.Release(_eventContainer);
_eventContainer = null;
```

Direct subscription is valid but must be paired with `Unsubscribe()`. Prefer `EventContainer` for UI, managers, and objects with clear lifetimes.

Dependencies:

- `EventContainer` depends on `GameEntry.Event`.
- Core `EventPool` uses reference-pooled event nodes.

Cleanup rules:

- Do not leave delegates subscribed after owner release.
- Do not call `EventContainer.Unsubscribe()` for a handler it did not record; it throws.
- Release the container before `GameEntry.Event` is shut down.
