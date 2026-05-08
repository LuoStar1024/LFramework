# 配置表

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.DataTable` 访问 Luban 生成的 `Tables`，不要直接实例化 `Tables` 或手动拼接配置表资源路径。
- `GameEntry.DataTable.TbUIForm.Get(uiFormId)`：读取 UI 窗口配置，常用于 `UIExtension` 根据配置构建 UI 资源路径、分组名和窗口打开参数。
- `GameEntry.DataTable.TbSound.Get(soundId)`：读取音频配置，常用于 `AudioExtension` 根据配置构建 BGM、音效或 UI 音效资源路径和播放参数。

## 注意事项

- 优先使用 `GameEntry.DataTable` 访问。
- `Tables` 下新增表的访问入口由 Luban 生成；新增或变更表后需要重新生成 `DataTableCode`，再按生成的 `TbXxx` 属性访问。
- 新增表驱动的业务封装时，优先把表查询放在 GameLogic 扩展方法或业务管理器中；Runtime 框架层不要依赖 `GameEntry.DataTable` 或 Luban 生成代码。
- 修改表结构或表数据时，修改源表定义/数据并重新生成 Luban 代码，不要手工修改 `Assets/GameScripts/GameDataTable/DataTableCode/` 下的生成文件。

## IDataTableManager API 速查

仅在框架集成代码或 `GameEntry` 初始化阶段优先考虑直接使用 `IDataTableManager`。业务代码通常使用 `GameEntry.DataTable`。

- `Tables Tables { get; }`：获取 Luban 生成的 `Tables` 实例；首次访问时触发 `DataTableComponent.Load()`。
- `Tables.TbUIForm.Get(id)`：按 id 获取 UI 窗口配置。
- `Tables.TbSound.Get(id)`：按 id 获取音频配置。

## 源码路径

- `Assets/GameScripts/GameLogic/Component/DataTable/DataTableComponent.cs`
- `Assets/GameScripts/GameDataTable/IDataTableManager.cs`
- `Assets/GameScripts/GameDataTable/DataTableCode/Tables.cs`
- `Assets/Editor/LubanTools/LubanTools.cs`
