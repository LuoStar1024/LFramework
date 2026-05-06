# DataNode

Source paths:
- `Assets/LFramework/Runtime/Component/DataNode/DataNodeComponent.cs`
- `Assets/LFramework/Runtime/Component/DataNode/DataNodeComponent.DataNode.cs`
- `Assets/LFramework/Runtime/Component/DataNode/IDataNode.cs`
- `Assets/LFramework/Runtime/Component/DataNode/IDataNodeManager.cs`
- `Assets/LFramework/Runtime/Core/Variable/*.cs`

`DataNodeComponent` is a tree-shaped runtime data store. It registers `IDataNodeManager` and stores values as `Variable` objects under named nodes.

Responsibility:

- Create, query, and remove hierarchical data nodes.
- Store typed values through the framework variable system.
- Reuse internal `DataNode` objects through `ReferencePool`.

Lifecycle:

- `Awake()` registers `IDataNodeManager`.
- `OnInit()` initializes the root node.
- `Shutdown()` releases the root and clears child/value state.
- Internal `DataNode` implements `IDataNode` and `IReference`; `Clear()` must release or null retained data.

Dependencies:

- Depends on Core `ReferencePool` and `Variable` types.
- Can be used by procedures/FSM or gameplay systems needing shared transient state.

Usage:

- Use `GameEntry.DataNode` after GameEntry initialization.
- Use strongly named paths and keep ownership clear; DataNode is global mutable state.
- When replacing node data, ensure the previous `Variable` is released by the component path.

Extension guidance:

- Prefer explicit FSM data for procedure-local values. Use DataNode for shared runtime data that is truly cross-system.
- Avoid storing Unity object references without a clear release owner.
