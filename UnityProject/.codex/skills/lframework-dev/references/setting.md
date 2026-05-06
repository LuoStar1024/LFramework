# Setting

Source paths:
- `Assets/LFramework/Runtime/Component/Setting/SettingComponent.cs`
- `Assets/LFramework/Runtime/Component/Setting/ISettingManager.cs`

`SettingComponent` registers `ISettingManager` and provides persisted settings through Unity `PlayerPrefs`.

Responsibility:

- Load and save settings.
- Get, set, remove, and enumerate bool/int/float/string/object values.
- Serialize object values through the framework utility layer.

Lifecycle:

- `Awake()` registers `ISettingManager`.
- `OnInit()` initializes setting state.
- `Shutdown()` saves or clears module-owned state according to implementation.

Usage:

```csharp
GameEntry.Setting.SetFloat(key, value);
GameEntry.Setting.Save();
```

Use overloads with default values when a setting may not exist. For object settings, verify serialization support for the object type.

Dependencies:

- Debugger settings windows read/write through `ISettingManager`.
- Audio extension stores mute and volume values through Setting.

Cleanup and safety:

- Call `Save()` after important user-facing setting changes.
- Keep setting names stable; renaming keys loses existing player preferences unless migration is added.
- Avoid storing large objects in PlayerPrefs-backed settings.
