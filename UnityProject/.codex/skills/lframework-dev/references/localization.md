# Localization

Source paths:
- `Assets/LFramework/Runtime/Component/Localization/LocalizationComponent.cs`
- `Assets/LFramework/Runtime/Component/Localization/ILocalizationManager.cs`
- `Assets/LFramework/Runtime/Component/Localization/Language.cs`
- `Assets/LFramework/Runtime/Component/Localization/I2Localization/**`

`LocalizationComponent` registers `ILocalizationManager` and bridges project code to I2 Localization.

Responsibility:

- Track current language.
- Provide localized string access and language switching.
- Load localization resources through the resource manager where needed.
- Encapsulate I2-specific integration details.

Lifecycle:

- `Awake()` registers `ILocalizationManager`.
- `Start()` / runtime initialization obtains `IResourceManager` where needed.
- `OnInit()` initializes local state.
- `Shutdown()` clears module-owned state.

Dependencies:

- Depends on `IResourceManager` for resource-backed localization assets.
- Uses I2 Localization types in the `I2Localization` folder.

Usage:

- Use `GameEntry.Localization` in GameLogic after GameEntry initialization.
- Keep UI text refresh logic aligned with language changes.
- Do not spread direct I2 internals into unrelated gameplay systems unless necessary.

Extension guidance:

- Add language enum values carefully; verify I2 data and serialized assets agree.
- If localization assets are loaded asynchronously, define the owner and release point.
- For missing terms, check both project localization data and the component's term lookup path.
