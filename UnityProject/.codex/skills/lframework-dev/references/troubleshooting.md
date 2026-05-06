# Troubleshooting

Source paths:
- affected module source files
- `Books/LFramework框架解析教程/*.md`

## Source Verification Policy

If a reference and source disagree, source wins. Verify with PowerShell search in this repository, for example:

```powershell
Select-String -Path "Assets\**\*.cs" -Pattern "LoadAsset<T>"
```

Check exact overloads before editing resource, UI, audio, FSM, and DataTable calls.

## Common Issues

### `GetModule<T>()` throws that type is not interface

Cause: code used a concrete component class as `T`.

Fix: use the registered manager interface, such as `IResourceManager`, `IUIManager`, or `IDataTableManager`.

### Module not found

Cause: module component did not run `Awake()`, is missing from scene, or `GameEntry` is used before initialization.

Fix: verify the component exists in the startup scene and registration happens before access. For business code, prefer access after `GameEntry.Start()`.

### Data table asset is not loaded

Cause: `GameEntry.DataTable` triggered lazy `Tables` loading before DataTable `TextAsset`s were loaded into ResourceComponent's asset pool.

Fix: ensure DataTable assets are preloaded through Resource before first table access.

### Resource leak or stale asset

Cause: direct `LoadAsset<T>()` without `UnloadAsset()`, or `ResourceContainer` not released.

Fix: use `ResourceContainer` for owner-scoped loads and release it in owner cleanup.

### Event handler fires after owner closed

Cause: direct subscription was not unsubscribed, or `EventContainer` was not released.

Fix: release `EventContainer` in `OnClose()`, `OnRecycle()`, or manager `Release()`.

### UI instance pool state breaks

Cause: UI GameObject was destroyed directly.

Fix: close UI through `GameEntry.UI.CloseUIForm()` or `UguiForm.Close()`.

### ReferencePool returns stale data

Cause: `IReference.Clear()` did not reset all fields.

Fix: update `Clear()` and verify every acquire has one release.

### FSM/procedure carries old state

Cause: old FSM data or procedure FSM was not destroyed before reinitialization.

Fix: follow `GameEntry.Start()` pattern and clear or replace FSM data intentionally.

### Audio settings do not persist

Cause: mute/volume changed without `GameEntry.Setting.Save()` or group names do not match DataTable/config.

Fix: use `AudioExtension` helpers and verify group names.
