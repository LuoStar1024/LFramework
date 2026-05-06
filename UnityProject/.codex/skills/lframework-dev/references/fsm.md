# FSM

Source paths:
- `Assets/LFramework/Runtime/Component/Fsm/FsmComponent.cs`
- `Assets/LFramework/Runtime/Component/Fsm/Fsm.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmBase.cs`
- `Assets/LFramework/Runtime/Component/Fsm/FsmState.cs`
- `Assets/LFramework/Runtime/Component/Fsm/IFsm*.cs`

`FsmComponent` registers `IFsmManager` and owns named finite-state machines. `Fsm<T>` is internal and reference-pooled; user code interacts through `IFsm<T>` and `FsmState<T>`.

Responsibility:

- Create and destroy FSMs by owner type/name.
- Update all FSMs each frame.
- Store FSM data using `Variable` values.
- Manage state lifecycle.

State lifecycle:

```text
OnInit()
OnEnter()
OnUpdate()
OnLeave()
OnDestroy()
```

Usage:

- Define states by inheriting `FsmState<TOwner>`.
- Use `ChangeState<TState>(fsm)` from inside a state to transition.
- Use `fsm.SetData()` / `fsm.GetData()` for state data; release/replace variable values carefully.
- Destroy FSMs when replacing flows or owners.

Dependencies:

- Uses `ReferencePool` for `Fsm<T>`.
- `ProcedureComponent` builds on FSM with owner type `IProcedureManager`.

Cleanup rules:

- Stop async work and unsubscribe events in `OnLeave()` or `OnDestroy()`.
- Avoid retaining owner references outside FSM state unless ownership is explicit.
- When reinitializing procedures, destroy the existing procedure FSM first, as `GameEntry.Start()` does.
