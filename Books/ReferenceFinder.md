# ReferenceFinder 资产引用查找工具

## 概述

ReferenceFinder 是一个 Unity 编辑器工具，用于查找和分析项目中的资产引用关系。它可以帮助开发者：

1. **查找资产引用**：查找选中资产被哪些其他资产引用
2. **查找资产依赖**：查看选中资产依赖了哪些其他资产
3. **可视化展示**：以树形结构展示引用关系
4. **缓存机制**：使用多线程和缓存提高查询性能
5. **拖拽支持**：支持从 Project 窗口拖拽资产到工具窗口

## 核心功能

- **引用模式**：查看资产被哪些地方引用
- **依赖模式**：查看资产依赖了哪些资源
- **树形展示**：层次化展示引用/依赖关系
- **状态提示**：显示缓存状态（正常、变更、丢失、无效）
- **引用计数**：统计资产被引用的次数
- **智能排序**：支持按名称和路径升序/降序排序
- **多线程处理**：使用多线程加速资产信息收集

## 文件结构

```
Assets/Editor/ReferenceFinder/
├── ResourceReferenceInfo.cs    # 主窗口类（EditorWindow）
├── ReferenceFinderData.cs      # 数据管理类，负责资产信息的收集和缓存
├── AssetTreeView.cs           # 树形视图类（TreeView）
├── AssetViewItem.cs           # 树形视图项（TreeViewItem）
├── DragAreaGetObject.cs       # 拖拽支持工具类
├── SortHelper.cs              # 排序帮助类
├── SortConfig.cs              # 排序配置类
├── SortType.cs                # 排序类型枚举
├── ClickColumn.cs             # 列头点击处理类
└── ListInfo.cs                # 列表信息数据结构
```

## 主要组件详解

### ResourceReferenceInfo

**类型**：`EditorWindow`

**功能**：主窗口类，提供用户界面和交互逻辑。

**核心功能**：
- 菜单入口：`LFramework/查找资产引用`（快捷键 F10）
- 支持从 Project 窗口选择资产或拖拽资产到窗口
- 切换引用/依赖模式
- 展开/折叠树形节点
- 显示资产的预制体和材质引用统计

**关键字段**：
- `selectedAssetGuid`：当前选中的资产 GUID 列表
- `mAssetTreeView`：资产树形视图实例
- `_isDepend`：是否为依赖模式（true=依赖模式，false=引用模式）

### ReferenceFinderData

**类型**：`sealed class`

**功能**：数据管理核心类，负责收集、缓存和管理资产依赖关系。

**核心功能**：
- **多线程收集**：使用多线程并行处理资产信息
- **缓存机制**：序列化缓存到 `Library/ReferenceFinderCache`
- **依赖分析**：解析 `.meta` 文件和资产文件中的 GUID 引用
- **状态跟踪**：跟踪资产的修改状态（Normal、Changed、Missing、Invalid）

**关键数据结构**：
```csharp
internal sealed class AssetDescription
{
    public string name;                          // 资产名称
    public string path;                          // 资产路径
    public string assetDependencyHashString;    // 修改时间哈希
    public List<string> dependencies;            // 依赖的 GUID 列表
    public List<string> references;              // 引用该资产的 GUID 列表
    public AssetState state;                     // 资产状态
}
```

**支持的资产类型**：
- `.prefab` - 预制体
- `.unity` - 场景
- `.mat` - 材质
- `.asset` - 资产文件
- `.anim` - 动画
- `.controller` - 动画控制器

### AssetTreeView

**类型**：`TreeView`

**功能**：树形视图组件，用于展示资产的引用/依赖关系。

**功能特性**：
- 多列表头（名称、路径、状态、引用计数）
- 双击资产可在 Project 窗口中定位
- 展开/折叠节点时自动排序
- 显示资产图标

**列定义**：
| 列名 | 说明 | 可排序 |
|------|------|--------|
| 名称 | 资产文件名 | 是 |
| 路径 | 资产在项目中的路径 | 是 |
| 状态 | 缓存状态（正常/变更/丢失/无效） | 否 |
| 引用数量 | 资产被引用的次数 | 否 |

### SortHelper

**类型**：`sealed class`

**功能**：排序帮助类，提供多种排序策略。

**排序类型**：
- `None` - 无排序
- `AscByName` - 按名称升序
- `DescByName` - 按名称降序
- `AscByPath` - 按路径升序
- `DescByPath` - 按路径降序

**排序策略**：
- **普通排序**：使用比较函数进行排序
- **快速排序**：仅反转列表（当切换升降序时使用）

### DragAreaGetObject

**类型**：`sealed class`

**功能**：提供拖拽支持，允许从 Project 窗口拖拽资产到工具窗口。

## 使用流程

### 1. 打开工具

通过菜单 `LFramework/查找资产引用` 或快捷键 `F10` 打开工具窗口。

### 2. 选择资产

**方式一**：在 Project 窗口选中资产，然后点击工具窗口
**方式二**：直接将资产从 Project 窗口拖拽到工具窗口

支持选择：
- 单个资产
- 多个资产
- 文件夹（会自动包含文件夹内的所有资产）

### 3. 查看引用关系

**引用模式**（默认）：
- 查看选中资产被哪些地方引用
- 树形结构展示引用层级

**依赖模式**：
- 查看选中资产依赖了哪些资源
- 树形结构展示依赖层级

### 4. 操作功能

**工具栏按钮**：
- `点击更新本地缓存`：重新扫描项目并更新缓存
- `依赖模式/引用模式`：切换查看模式
- `展开`：展开所有树形节点
- `折叠`：折叠所有树形节点

**列头点击**：
- 点击"名称"列：按名称排序（升序/降序切换）
- 点击"路径"列：按路径排序（升序/降序切换）

**双击操作**：
- 双击资产行：在 Project 窗口中定位并选中该资产

### 5. 查看统计信息

在 Console 窗口会输出统计信息：
- 预制体总数及名称列表
- 材质总数及名称列表
- 各资产的类型和引用计数

## 缓存机制

### 缓存位置

`Library/ReferenceFinderCache`

### 缓存格式

使用 BinaryFormatter 序列化：
1. GUID 列表
2. 修改时间哈希列表
3. 依赖索引列表

### 缓存策略

1. **首次使用**：收集所有资产信息并写入缓存
2. **后续使用**：读取缓存，只更新修改过的资产
3. **手动更新**：点击"点击更新本地缓存"按钮强制刷新

### 资产状态

- **Normal（正常）**：缓存与文件系统一致
- **Changed（变更）**：文件被修改，缓存需要更新
- **Missing（丢失）**：文件不存在
- **Invalid（无效）**：GUID 无法解析为有效路径

## 性能优化

### 多线程处理

- 使用 `Environment.ProcessorCount` 确定线程数（最少 8 个）
- 每个线程处理部分资产
- 使用线程本地字典减少锁竞争

### 快速排序

- 切换升降序时，仅反转列表而不重新排序
- 使用哈希集合跟踪已排序的资产

### 延迟加载

- 只在需要时更新资产树
- 使用标志位控制更新时机

## 注意事项

1. **GUID 唯一性**：资产 GUID 必须在项目中唯一
2. **文件编码**：支持 UTF-8 编码的资产文件
3. **循环引用**：工具会检测循环引用并给出警告
4. **大项目优化**：对于大型项目，首次缓存可能需要较长时间
5. **缓存失效**：资产结构大幅变更后，建议手动更新缓存

## 扩展开发

### 添加新的资产类型支持

在 `ReferenceFinderData.FileExtension` 集合中添加新的扩展名：

```csharp
private static readonly HashSet<string> FileExtension = new HashSet<string>
{
    ".prefab",
    ".unity",
    ".mat",
    ".asset",
    ".anim",
    ".controller",
    ".yourExtension"  // 添加新类型
};
```

### 自定义排序方式

在 `SortHelper.CompareFunction` 中添加新的比较函数：

```csharp
public static readonly Dictionary<SortType, SortCompare> CompareFunction = new Dictionary<SortType, SortCompare>
{
    { SortType.AscByPath, CompareWithPath },
    { SortType.DescByPath, CompareWithPathDesc },
    { SortType.AscByName, CompareWithName },
    { SortType.DescByName, CompareWithNameDesc },
    { SortType.AscBySize, CompareWithSize }  // 添加新排序
};
```

## 更新日志

### v1.0
- 初始版本发布
- 支持引用和依赖两种模式
- 多线程资产信息收集
- 缓存机制优化性能
- 树形视图展示
- 拖拽支持

---

**文件位置**：`Assets/Editor/ReferenceFinder/`

**菜单路径**：`LFramework/查找资产引用`

**快捷键**：`F10`
