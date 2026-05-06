# Debugger

Source paths:
- `Assets/LFramework/Runtime/Component/Debugger/DebuggerComponent.cs`
- `Assets/LFramework/Runtime/Component/Debugger/DebuggerComponent.*.cs`
- `Assets/LFramework/Runtime/Component/Debugger/IDebuggerManager.cs`
- `Assets/Launcher/Res/Configs/DebuggerSkin.guiskin`

`DebuggerComponent` registers `IDebuggerManager` and renders a runtime IMGUI debugger overlay with grouped windows.

Responsibility:

- Toggle and draw runtime diagnostic windows.
- Display FPS, environment, system, screen, quality, profiler, runtime memory, scene, object pool, reference pool, and log information.
- Provide operations such as resource cleanup and shutdown/restart actions through debugger windows.

Lifecycle:

- `Awake()` registers `IDebuggerManager`.
- `OnInit()` builds debugger windows.
- `OnUpdate()` updates window state.
- `Shutdown()` shuts down the debugger window root and child windows.

Dependencies:

- Reads `IBaseManager`, `IResourceManager`, `IObjectPoolManager`, `ISettingManager`, and other managers for diagnostics.
- Uses `ReferencePool` for log nodes.

Usage:

- Keep debugger usage development-focused.
- Business logic should not depend on debugger window internals.
- Use the ReferencePool and ObjectPool windows to diagnose leaked pooled objects.

Extension guidance:

- Add new windows as debugger-only views with no gameplay side effects.
- Avoid expensive allocations in per-frame debugger drawing.
- For production builds, verify debugger activation policy before exposing operations.
