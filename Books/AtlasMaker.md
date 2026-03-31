# AtlasMaker 图集生成工具

## 概述

AtlasMaker 是一个 Unity 编辑器工具，用于自动将散落的精灵图片资源打包成图集（Sprite Atlas）。通过配置目录规则，工具可以在资源导入、删除、移动时自动更新对应的图集，大大提高图集管理的效率。

## 核心功能

1. **自动生成图集**：根据配置的目录规则，自动将图片资源打包成图集
2. **多目录模式支持**：支持多种图集生成模式（完整路径、子级目录、单张图片）
3. **平台格式配置**：可为不同平台配置不同的纹理压缩格式
4. **自动资源检查**：自动检查文件名空格、同名资源冲突等问题
5. **智能增量更新**：只重新生成有变动的图集，提高构建效率
6. **V2 图集格式支持**：支持 Unity 2020.1+ 的 V2 图集格式

## 文件结构

```
Assets/Editor/AtlasMakerEditor/
├── AtlasConfiguration.cs      # 图集配置类（EditorScriptableSingleton）
├── AtlasEditorWindow.cs       # 配置窗口（EditorWindow）
├── EditorSpriteSaveInfo.cs    # 精灵信息管理核心类
└── SpritePostprocessor.cs     # 资源后处理器（AssetPostprocessor）
```

## 配置说明

### 目录设置

#### outputAtlasDir
- **说明**：生成的图集输出目录
- **默认**：`Assets/GameResArt/Atlas`
- **用途**：所有自动生成的 SpriteAtlas 文件将保存到此目录

#### sourceAtlasRootDir
- **说明**：需要生成图集的 UI 根目录数组
- **默认**：`[ "Assets/GameResRaw/Sprite" ]`
- **用途**：系统会扫描这些目录下的所有图片资源，并按目录结构自动生成对应的图集

#### rootChildAtlasDir
- **说明**：以当前目录的子级生成子级图集的目录数组
- **默认**：`[]`
- **用途**：这些目录下的每个子文件夹会生成一个独立的图集，而不是按完整路径生成

#### singleAtlasDir
- **说明**：每张图都单独生成图集的目录数组
- **默认**：`[]`
- **用途**：这些目录下的每张图片都会生成一个独立的图集文件，适用于大图或需要单独管理的资源

#### excludeFolder
- **说明**：不需要生成图集的 UI 目录数组
- **默认**：`[]`
- **用途**：这些目录下的图片资源将被排除，不会被打入任何图集

### 平台格式设置

#### androidFormat
- **说明**：Android 平台的纹理压缩格式
- **默认**：`ASTC_6x6`
- **建议**：在质量和压缩率之间取得平衡

#### iosFormat
- **说明**：iOS 平台的纹理压缩格式
- **默认**：`ASTC_5x5`
- **建议**：iOS 设备对 ASTC 格式有良好支持

#### webglFormat
- **说明**：WebGL 平台的纹理压缩格式
- **默认**：`ASTC_6x6`

### 打包设置

#### padding
- **说明**：图集中精灵之间的间距（像素）
- **默认**：`2`
- **用途**：用于防止精灵边缘出现渗色问题

#### enableRotation
- **说明**：是否允许旋转精灵以获得更好的打包效率
- **默认**：`true`
- **用途**：启用后可能会提高图集空间利用率

#### tightPacking
- **说明**：是否启用紧密打包（剔除透明区域）
- **默认**：`true`
- **用途**：启用后会根据精灵的实际像素边界进行打包，而不是矩形边界

### Sprite 导入设置

#### checkMipmaps
- **说明**：是否检查 Mipmap 导入设置
- **默认**：`true`
- **用途**：启用后会在导入精灵时检查并修正 Mipmap 设置

#### enableMipmaps
- **说明**：是否为精灵启用 Mipmap
- **默认**：`false`
- **用途**：UI 精灵通常不需要 Mipmap，禁用可以减少内存占用

### 高级设置

#### autoGenerate
- **说明**：是否自动生成图集
- **默认**：`true`
- **用途**：启用后，当图片资源发生变化时会自动更新对应的图集

#### enableLogging
- **说明**：是否启用日志输出
- **默认**：`true`
- **用途**：启用后会在控制台输出图集生成的详细信息

#### enableV2
- **说明**：是否启用 V2 版本的 SpriteAtlas 格式（.spriteatlasv2）
- **默认**：`true`
- **用途**：V2 格式在 Unity 2020.1+ 中提供更好的性能和功能

#### excludeKeywords
- **说明**：排除关键词数组
- **默认**：`[ "_Delete", "_Temp" ]`
- **用途**：文件路径中包含这些关键词的资源将被排除，不会被打入图集

## 使用流程

### 1. 配置目录

通过菜单 `Tools/图集工具/配置面板` 打开配置窗口，设置：
- 源目录（需要打包的精灵资源目录）
- 输出目录（图集文件的保存位置）
- 排除目录（不需要打包的目录）

### 2. 资源导入

将图片资源放入配置的源目录下，工具会自动：
1. 将图片类型设置为 Sprite
2. 检查文件名是否包含空格
3. 检查是否存在同名资源冲突
4. 根据目录规则生成或更新对应的图集

### 3. 图集更新

当资源发生变化时：
- **新增资源**：自动添加到对应图集
- **删除资源**：自动从对应图集中移除
- **移动资源**：先删除旧路径，再添加到新路径

### 4. 手动操作

在配置窗口中，可以执行以下操作：
- **立即重新生成**：删除所有现有图集后重新生成
- **重新生成有变动的图集数据**：只重新生成有变化的图集
- **清空缓存**：清除内部缓存数据

## 图集命名规则

图集名称由根目录名和相对路径组成，使用下划线连接：

```
{RootFolderName}_{Directory1}_{Directory2}_...
```

例如：
- 源目录：`Assets/GameResRaw/Sprite`
- 资源路径：`Assets/GameResRaw/Sprite/UI/Buttons/btn_ok.png`
- 图集名称：`Sprite_UI_Buttons`

## 注意事项

1. **文件名不能包含空格**：工具会自动删除文件名包含空格的资源
2. **避免同名资源**：在同一目录树下，不同路径不能有同名资源
3. **自动更新依赖**：图集变动时，相关资源的引用会自动更新
4. **V2 格式兼容性**：V2 格式需要 Unity 2020.1+，旧版本请关闭此选项

## 代码引用

### 订阅图集变更事件

```csharp
// 获取配置实例
var config = AtlasConfiguration.Instance;

// 手动触发图集生成
EditorSpriteSaveInfo.ForceGenerateAll(true);  // 全量重新生成
EditorSpriteSaveInfo.ForceGenerateAll();       // 增量更新
```

### 处理特定资源

```csharp
// 导入精灵
EditorSpriteSaveInfo.OnImportSprite("Assets/Path/To/Sprite.png");

// 删除精灵
EditorSpriteSaveInfo.OnDeleteSprite("Assets/Path/To/Sprite.png");
```

## 常见问题

### Q: 为什么资源导入后图集没有更新？
A: 请检查：
1. `autoGenerate` 是否启用
2. 资源是否在配置的 `sourceAtlasRootDir` 目录下
3. 资源路径是否包含排除关键词

### Q: 如何排除某些资源不打入图集？
A: 有三种方式：
1. 将资源放入 `excludeFolder` 配置的目录
2. 在资源路径中包含 `excludeKeywords` 配置的关键词
3. 配置特定的目录为排除目录

### Q: 如何为特定目录生成单张图集？
A: 将目录添加到 `singleAtlasDir` 配置中，该目录下的每张图片都会生成独立的图集。

### Q: V2 格式和 V1 格式有什么区别？
A: V2 格式（Unity 2020.1+）提供：
- 更好的打包算法
- 支持运行时图集管理
- 改进的内存管理
- 更好的平台兼容性

## 更新日志

### v1.0
- 初始版本发布
- 支持自动图集生成
- 支持多目录配置
- 支持平台特定格式

---

**文件位置**：`Assets/Editor/AtlasMakerEditor/`

**配置路径**：`ProjectSettings/AtlasConfiguration.asset`
