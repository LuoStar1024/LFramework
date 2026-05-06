# Procedure

Source paths:
- `Assets/LFramework/Runtime/Component/Procedure/ProcedureComponent.cs`
- `Assets/LFramework/Runtime/Component/Procedure/ProcedureBase.cs`
- `Assets/LFramework/Runtime/Component/Procedure/IProcedureManager.cs`
- `Assets/GameScripts/GameLogic/Procedure/*.cs`
- `Assets/GameScripts/GameLogic/Base/GameEntry.cs`

`ProcedureComponent` registers `IProcedureManager` and implements the game flow system on top of FSM.

Responsibility:

- Own the current procedure.
- Initialize a procedure FSM with `ProcedureBase` states.
- Start and transition game flow procedures.
- Expose current procedure information through `IProcedureManager`.

Lifecycle:

- `Awake()` registers `IProcedureManager`.
- `Initialize(IFsmManager, ProcedureBase[])` creates the FSM.
- `StartProcedure<T>()` enters the initial procedure.
- `Shutdown()` destroys procedure state.

GameLogic startup:

```text
GameEntry.Start()
  -> Fsm.DestroyFsm<IProcedureManager>()
  -> Procedure.Initialize(Fsm, GameLogic procedures)
  -> InitComponents()
  -> Procedure.StartProcedure<ProcedureGameLogicLaunch>()
```

Usage:

- Inherit custom flows from `ProcedureBase`.
- Use FSM data for cross-procedure values, such as scene names.
- Do transition work in procedure lifecycle methods, not arbitrary MonoBehaviour updates.

Cleanup rules:

- Stop async loads and unsubscribe events in `OnLeave()`.
- Do not keep stale FSM data across flow resets unless intended.
- Procedure logic can use `GameEntry` after components are initialized.
