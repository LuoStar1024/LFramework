# DataTable

Source paths:
- `Assets/GameScripts/GameLogic/Component/DataTable/DataTableComponent.cs`
- `Assets/GameScripts/GameDataTable/IDataTableManager.cs`
- `Assets/GameScripts/GameDataTable/DataTableCode/Tables.cs`
- `Assets/GameResRaw/DataTable/*.json`
- `Assets/Editor/LubanTools/LubanTools.cs`

`DataTableComponent` is a GameLogic module that registers `IDataTableManager` and exposes Luban-generated `Tables`.

Responsibility:

- Lazy-load `Tables` on first access.
- Choose binary or JSON loader depending on generated `Tables.SetDefaultLoader` signature.
- Resolve table asset paths through `AssetUtility.GetDataTableAsset(file)`.
- Load table `TextAsset`s from the existing resource pool via `GameEntry.Resource.LoadExistAsset<TextAsset>()`.

Lifecycle:

- `Awake()` registers `IDataTableManager`.
- `Tables` property calls private `Load()` once.
- `OnInit()`, `OnUpdate()`, and `Shutdown()` are light in current implementation.

Critical dependency:

DataTable files must already be loaded by the Resource module before `GameEntry.DataTable` is first accessed. If not, `DataTableComponent` throws:

```text
Data table asset is not loaded: <assetPath>
```

Usage:

```csharp
var uiForm = GameEntry.DataTable.TbUIForm.Get(id);
var sound = GameEntry.DataTable.TbSound.Get(id);
```

Do not modify generated files under `Assets/GameScripts/GameDataTable/DataTableCode/` by hand. Change source table definitions/data and regenerate when a schema/data update is required.

Cleanup and extension:

- Treat `Tables` as a long-lived runtime config cache.
- For new table-driven helpers, keep DataTable lookup in GameLogic extension methods or managers, not in lower Runtime modules.
- Verify whether the project is using JSON or binary generated loaders before changing loader code.
