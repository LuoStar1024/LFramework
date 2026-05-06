# Config

Source paths:
- `Assets/LFramework/Runtime/Component/Config/ConfigComponent.cs`
- `Assets/LFramework/Runtime/Component/Config/IConfigManager.cs`
- `Assets/LFramework/Runtime/Component/Config/UpdateConfig.cs`
- `Assets/Launcher/Res/Configs/UpdateConfig.asset`

`ConfigComponent` registers `IConfigManager` and exposes the `UpdateConfig` ScriptableObject. It is the configuration source for resource update paths, HybridCLR hot-update DLL configuration, and update behavior.

Responsibility:

- Provide `UpdateConfig` to runtime modules.
- Centralize update style, notice policy, resource URL, fallback URL, and DLL metadata.
- Support the resource module before package initialization.

Lifecycle:

- `Awake()` registers `IConfigManager`.
- `OnInit()` and `OnUpdate()` are light in the current implementation.
- `Shutdown()` does not own external resources.

Dependencies:

- `ResourceComponent.Start()` reads `Config.UpdateConfig.GetResDownLoadPath()` and fallback paths.
- HybridCLR-related editor/runtime configuration uses `UpdateConfig` metadata.

Usage:

- Use `GameEntry.Config.UpdateConfig` in GameLogic after `GameEntry` is initialized.
- Runtime code can use `LFrameworkEntry.GetModule<IConfigManager>()`.
- Treat `UpdateConfig` as serialized Unity data; avoid changing asset shape casually.

Extension guidance:

- Add new update settings to `UpdateConfig` when they are global and serialized.
- Keep environment-specific URL logic inside `UpdateConfig` helpers.
- If resource initialization fails, verify Config is registered before Resource starts and that the asset reference is assigned.
