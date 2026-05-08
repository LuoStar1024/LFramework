# DataNode

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.DataNode`，不要直接获取或依赖 `DataNodeComponent`。
- `GameEntry.DataNode.SetData(path, data)`：按路径获取或创建数据结点，并把 `Variable` 数据写入该结点。
- `GameEntry.DataNode.GetData<T>(path)`：按路径读取指定 `Variable` 派生类型的数据；路径不存在或类型不匹配时会抛出 `LFrameworkException`。
- `GameEntry.DataNode.GetNode(path)`：只查询已有结点；路径不存在时返回 `null`。
- `GameEntry.DataNode.GetOrAddNode(path)`：查询结点，不存在时按路径逐级创建。
- `GameEntry.DataNode.RemoveNode(path)`：移除指定结点及其子结点，底层会通过 `ReferencePool` 释放结点和结点数据。
- `GameEntry.DataNode.Clear()`：清空根结点下的数据和所有子结点，适合在明确拥有全局状态清理时使用。
- 路径支持使用 `.`, `/`, `\` 作为分隔符，例如 `Player.Profile.Name` 或 `Player/Profile/Name`。
- 写入数据必须使用 `Variable` 派生类型，例如 `VarString`, `VarInt32`, `VarBoolean` 等；这些类型通常支持从对应基础类型隐式转换。

## 注意事项

- DataNode 是全局可变运行时状态。优先用于确实需要跨系统共享的临时数据；流程或 FSM 内部私有数据优先放在对应流程/FSM 数据中。
- `GetData` 要求目标结点存在；如果只是判断路径是否存在，先用 `GetNode` 判空。
- `SetData` 会自动创建路径结点，并在替换数据时释放旧的 `Variable`；不要在写入后继续手动释放同一个 `Variable`。
- `Variable` 及内部 `DataNode` 都通过 `ReferencePool` 管理；自定义变量类型必须正确实现 `IReference.Clear()`，清空所有保留字段。
- 结点名称不能为 `null`、空字符串，也不能包含 `.`, `/`, `\` 分隔符；只有路径字符串可以包含这些分隔符。
- 避免在 DataNode 中长期保存 Unity 对象引用，除非有明确的资源或对象生命周期所有者。

## IDataNodeManager API 速查

仅在框架集成代码或 `GameEntry.DataNode` 已初始化后的业务代码中使用 `IDataNodeManager`。

- 根结点：`Root` 返回当前数据树根结点。
- 读取数据：`GetData<T>(path)`, `GetData(path)`, `GetData<T>(path, node)`, `GetData(path, node)`。
- 写入数据：`SetData<T>(path, data)`, `SetData(path, data)`, `SetData<T>(path, data, node)`, `SetData(path, data, node)`。
- 查询结点：`GetNode(path)`, `GetNode(path, node)`；路径不存在时返回 `null`。
- 查询或创建结点：`GetOrAddNode(path)`, `GetOrAddNode(path, node)`。
- 移除结点：`RemoveNode(path)`, `RemoveNode(path, node)`。
- 清空数据树：`Clear()`。

`IDataNode` 可用于局部树操作：

- 元信息：`Name`, `FullName`, `Parent`, `ChildCount`。
- 当前结点数据：`GetData<T>()`, `GetData()`, `SetData<T>(data)`, `SetData(data)`。
- 子结点查询：`HasChild(index)`, `HasChild(name)`, `GetChild(index)`, `GetChild(name)`, `GetOrAddChild(name)`。
- 子结点遍历：`GetAllChild()`, `GetAllChild(results)`。
- 子结点移除：`RemoveChild(index)`, `RemoveChild(name)`。
- 清理与调试：`Clear()`, `ToString()`, `ToDataString()`。

## 源码路径

- `Assets/LFramework/Runtime/Component/DataNode/IDataNodeManager.cs`
- `Assets/LFramework/Runtime/Component/DataNode/IDataNode.cs`
- `Assets/LFramework/Runtime/Component/DataNode/DataNodeComponent.cs`
- `Assets/LFramework/Runtime/Component/DataNode/DataNodeComponent.DataNode.cs`
- `Assets/LFramework/Runtime/Core/Variable/Variable.cs`
- `Assets/LFramework/Runtime/Core/Variable/GenericVariable.cs`
- `Assets/LFramework/Runtime/Core/Variable/Var*.cs`
