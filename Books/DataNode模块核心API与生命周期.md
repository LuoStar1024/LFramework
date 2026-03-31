# DataNode 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `DataNode` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 生命周期；
- 需要理解的接口/基类关系。

---

## 2. 模块定位

`DataNode` 模块是 LFramework 中的树状数据管理模块，用于：

- 通过路径访问节点；
- 在节点上保存 `Variable` 数据；
- 组织运行时层级状态；
- 作为框架共享数据树由全局访问。

它适合承载“层级化、可递归组织”的运行时数据，而不是简单平铺字典。

---

## 3. 核心类型

## 3.1 DataNodeComponent

文件：

- `Assets/LFramework/Runtime/Component/DataNode/DataNodeComponent.cs`

定义：

```csharp
public sealed partial class DataNodeComponent : MonoBehaviour, ILFrameworkModule, IDataNodeManager
```

职责：

- 作为 `DataNode` 模块入口；
- 注册 `IDataNodeManager`；
- 创建和维护根节点；
- 负责路径切分与整棵树访问；
- 对外提供节点和数据的统一管理 API。

说明：

- `sealed`，当前不以继承扩展为主；
- 业务层通常通过 `IDataNodeManager` 或 `GameEntry.DataNode` 使用。

---

## 3.2 DataNode

文件：

- `Assets/LFramework/Runtime/Component/DataNode/DataNodeComponent.DataNode.cs`

定义：

```csharp
private sealed class DataNode : IDataNode, IReference
```

职责：

- 作为模块内部节点实现；
- 保存节点名称、父子关系和节点数据；
- 提供子节点增删查与节点清理；
- 接入 `ReferencePool`，支持对象池复用。

说明：

- 这是 `DataNodeComponent` 的内部类，外部通过 `IDataNode` 接口访问它；
- 节点本身不是 `MonoBehaviour`。

---

## 3.3 IDataNode

文件：

- `Assets/LFramework/Runtime/Component/DataNode/IDataNode.cs`

职责：

- 定义单个数据节点的公共能力。

包含三类核心能力：

1. 节点信息
2. 数据访问
3. 子节点管理

---

## 3.4 IDataNodeManager

文件：

- `Assets/LFramework/Runtime/Component/DataNode/IDataNodeManager.cs`

职责：

- 定义整棵数据树的管理能力；
- 对外暴露基于路径的访问接口。

---

## 3.5 Variable

文件：

- `Assets/LFramework/Runtime/Core/Variable/Variable.cs`

定义：

```csharp
public abstract class Variable : IReference
```

职责：

- 作为节点数据的统一抽象；
- 实际值通过具体派生类承载。

说明：

- `DataNode` 存储的不是任意对象，而是 `Variable`；
- 这意味着如果要存业务数据，需要使用 `Variable` 派生类型。

---

## 3.6 DataNodeComponentInspector

文件：

- `Assets/LFramework/Editor/Inspector/DataNodeComponentInspector.cs`

职责：

- 在运行时递归展示节点树；
- 用于调试当前 DataNode 数据结构。

---

## 4. 核心 API

## 4.1 IDataNodeManager

### 根节点

| API              | 说明      |
| ---------------- | ------- |
| `IDataNode Root` | 获取根数据节点 |

### 数据读取

| API                                             | 说明                 |
| ----------------------------------------------- | ------------------ |
| `T GetData<T>(string path)`                     | 从根节点按路径读取指定类型数据    |
| `Variable GetData(string path)`                 | 从根节点按路径读取原始数据      |
| `T GetData<T>(string path, IDataNode node)`     | 从指定起始节点按路径读取指定类型数据 |
| `Variable GetData(string path, IDataNode node)` | 从指定起始节点按路径读取原始数据   |

说明：

- 如果指定路径不存在，当前实现会抛异常；
- 泛型版本会直接把节点数据强转为 `T`。

### 数据写入

| API                                                        | 说明              |
| ---------------------------------------------------------- | --------------- |
| `void SetData<T>(string path, T data)`                     | 从根节点按路径设置指定类型数据 |
| `void SetData(string path, Variable data)`                 | 从根节点按路径设置原始数据   |
| `void SetData<T>(string path, T data, IDataNode node)`     | 从指定起始节点按路径设置数据  |
| `void SetData(string path, Variable data, IDataNode node)` | 从指定起始节点按路径设置数据  |

说明：

- 如果路径中间节点不存在，会自动创建。

### 节点访问

| API                                                   | 说明                 |
| ----------------------------------------------------- | ------------------ |
| `IDataNode GetNode(string path)`                      | 从根节点按路径获取节点，不存在返回空 |
| `IDataNode GetNode(string path, IDataNode node)`      | 从指定节点按路径获取节点       |
| `IDataNode GetOrAddNode(string path)`                 | 从根节点获取或创建节点        |
| `IDataNode GetOrAddNode(string path, IDataNode node)` | 从指定节点获取或创建节点       |
| `void RemoveNode(string path)`                        | 从根节点按路径移除节点        |
| `void RemoveNode(string path, IDataNode node)`        | 从指定节点按路径移除节点       |
| `void Clear()`                                        | 清空整棵树的数据和子节点       |

---

## 4.2 IDataNode

### 节点信息

| API                | 说明      |
| ------------------ | ------- |
| `string Name`      | 节点名称    |
| `string FullName`  | 节点完整路径名 |
| `IDataNode Parent` | 父节点     |
| `int ChildCount`   | 子节点数量   |

### 数据访问

| API                           | 说明           |
| ----------------------------- | ------------ |
| `T GetData<T>()`              | 获取节点上的指定类型数据 |
| `Variable GetData()`          | 获取节点上的原始数据   |
| `void SetData<T>(T data)`     | 设置节点数据       |
| `void SetData(Variable data)` | 设置节点原始数据     |

### 子节点管理

| API                                         | 说明             |
| ------------------------------------------- | -------------- |
| `bool HasChild(int index)`                  | 按索引检查子节点       |
| `bool HasChild(string name)`                | 按名称检查子节点       |
| `IDataNode GetChild(int index)`             | 按索引获取子节点       |
| `IDataNode GetChild(string name)`           | 按名称获取子节点       |
| `IDataNode GetOrAddChild(string name)`      | 获取或创建指定名称子节点   |
| `IDataNode[] GetAllChild()`                 | 获取全部子节点        |
| `void GetAllChild(List<IDataNode> results)` | 将全部子节点写入外部列表   |
| `void RemoveChild(int index)`               | 按索引删除子节点       |
| `void RemoveChild(string name)`             | 按名称删除子节点       |
| `void Clear()`                              | 清除当前节点数据和所有子节点 |
| `string ToDataString()`                     | 获取节点数据的文本表示    |

---

## 5. 路径规则

`DataNodeComponent` 当前支持以下路径分隔符：

- `.`
- `/`
- `\`

例如以下形式都可以：

```text
Player.Level
Player/Level
Player\Level
```

注意：

- 节点名本身不能包含这些分隔符；
- 空路径当前会被解析为“当前节点”。

---

## 6. 核心调用链

## 6.1 模块注册调用链

```text
DataNodeComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<IDataNodeManager>(this)
    ↓
LFrameworkEntry 内部保存模块
    ↓
立即调用 OnInit()
```

---

## 6.2 节点读取调用链

```text
GameEntry.DataNode.GetData<T>(path)
    ↓
DataNodeComponent.GetData<T>(path)
    ↓
GetNode(path, node)
    ↓
逐级调用 IDataNode.GetChild(name)
    ↓
找到目标节点后执行 IDataNode.GetData<T>()
```

---

## 6.3 节点写入调用链

```text
GameEntry.DataNode.SetData(path, data)
    ↓
DataNodeComponent.SetData(path, data)
    ↓
GetOrAddNode(path, node)
    ↓
逐级调用 IDataNode.GetOrAddChild(name)
    ↓
定位目标节点后执行 IDataNode.SetData(data)
```

---

## 6.4 节点移除调用链

```text
GameEntry.DataNode.RemoveNode(path)
    ↓
DataNodeComponent.RemoveNode(path, node)
    ↓
逐级找到目标节点
    ↓
parent.RemoveChild(current.Name)
    ↓
ReferencePool.Release(node)
```

---

## 7. 生命周期

## 7.1 DataNodeComponent 生命周期

### `Awake()`

作用：

- 把当前模块注册到 `LFrameworkEntry`。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IDataNodeManager>(this);
}
```

说明：

- 注册后框架会立即调用 `OnInit()`。

---

### `Priority`

作用：

- 声明模块优先级。

当前实现：

```csharp
public int Priority
{
    get { return 0; }
}
```

说明：

- 当前与许多其他模块一样使用默认优先级 `0`。

---

### `OnInit()`

作用：

- 创建根节点。

当前实现：

```csharp
public void OnInit()
{
    _root = DataNode.Create(RootName, null);
}
```

说明：

- 这是模块最核心的初始化节点；
- 根节点名称固定为 `"<Root>"`。

---

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 参与框架模块轮询。

当前实现：

- 空实现。

说明：

- 当前 DataNode 模块没有逐帧逻辑。

---

### `Shutdown()`

作用：

- 关闭模块并释放根节点。

当前实现：

```csharp
public void Shutdown()
{
    ReferencePool.Release(_root);
    _root = null;
}
```

说明：

- 根节点释放后，其子节点和节点数据会通过清理链路一起回收。

---

## 7.2 DataNode 生命周期

### `Create(string name, DataNode parent)`

作用：

- 创建节点实例；
- 校验节点名称合法性；
- 从 `ReferencePool` 获取节点对象。

---

### `SetData(Variable data)`

作用：

- 替换当前节点数据；
- 若已有旧数据，先释放旧数据；
- 保存新数据引用。

说明：

- 当前实现默认由节点接管传入数据的生命周期。

---

### `GetOrAddChild(string name)`

作用：

- 查找指定名称子节点；
- 若不存在则创建并加入子节点列表。

---

### `Clear()`

作用：

- 释放当前节点数据；
- 释放所有子节点；
- 清空子节点列表。

说明：

- 这是节点最重要的清理入口。

---

### `IReference.Clear()`

作用：

- 在引用池回收时重置节点状态。

当前实现：

- 清空 `_name`、`_parent`；
- 调用 `Clear()` 释放数据与子节点。

---

## 8. 是否存在需要继承的基类

## 8.1 DataNode 模块自身

结论：当前模块不以继承扩展为主。

原因：

- `DataNodeComponent` 是 `sealed`；
- `DataNode` 是内部私有类；
- 外部设计明显偏向“通过接口访问”，而不是“通过继承改写”。

因此：

- 业务侧应通过 `IDataNodeManager` / `IDataNode` 使用；
- 不建议通过继承 `DataNodeComponent` 扩展模块。

---

## 8.2 真正需要理解的接口/基类

虽然模块本身不建议继承，但有两个核心体系必须理解。

### `ILFrameworkModule`

作用：

- 让 `DataNodeComponent` 接入框架模块系统。

核心生命周期：

| 生命周期         | 作用     |
| ------------ | ------ |
| `Priority`   | 模块优先级  |
| `OnInit()`   | 模块初始化  |
| `OnUpdate()` | 模块轮询   |
| `Shutdown()` | 模块关闭清理 |

### `Variable`

作用：

- 作为节点值的统一抽象基类。

核心生命周期：

| 生命周期                     | 作用        |
| ------------------------ | --------- |
| `GetValue()`             | 获取值       |
| `SetValue(object value)` | 设置值       |
| `Clear()`                | 引用池回收时清理值 |

如果后续要向 DataNode 中存储新类型数据，真正需要“继承”的通常是 `Variable`，而不是 `DataNodeComponent`。

---

## 9. 典型使用方式

## 9.1 获取或创建节点

```csharp
IDataNode playerNode = GameEntry.DataNode.GetOrAddNode("Player");
```

## 9.2 设置节点数据

```csharp
GameEntry.DataNode.SetData("Player.Level", someVarInt32);
```

## 9.3 读取节点数据

```csharp
VarInt32 level = GameEntry.DataNode.GetData<VarInt32>("Player.Level");
```

## 9.4 删除节点

```csharp
GameEntry.DataNode.RemoveNode("Player.Level");
```

## 9.5 清空整棵树

```csharp
GameEntry.DataNode.Clear();
```

---

## 10. 使用注意事项

### 10.1 节点名不能包含路径分隔符

当前禁止：

- `.`
- `/`
- `\`

否则创建节点会抛异常。

### 10.2 空路径当前表示“当前节点”

这意味着：

- `GetNode("")` 默认返回根节点；
- 基于指定 `node` 的调用会返回该起始节点本身。

### 10.3 传入的 `Variable` 默认由节点管理生命周期

设置到节点后，不建议外部继续把同一个实例当作长期持有对象使用。

### 10.4 当前更适合中低频树状状态

由于：

- 路径切分有分配；
- 子节点名称查找是线性扫描；

因此当前实现更适合作为普通运行时状态树，而不是极高频热点数据结构。

---

## 11. 总结

当前 `DataNode` 模块可以概括为：

- 一个 `sealed` 的树状数据管理模块；
- 通过 `IDataNodeManager` / `IDataNode` 暴露访问能力；
- 通过 `Variable` 抽象承载节点值；
- 通过 `ILFrameworkModule` 生命周期接入框架；
- 通过 `ReferencePool` 管理节点与变量对象复用。

如果后续要扩展它，最重要的不是继承模块本身，而是：

- 理解 `IDataNodeManager` 与 `IDataNode` 的访问方式；
- 理解 `Variable` 的继承与回收语义；
- 理解节点树的初始化、清理和路径访问规则。
