# UI

Source paths:
- `Assets/GameScripts/GameLogic/Component/UI/UIComponent.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIGroup.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIForm.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIFormLogic.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UguiForm.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIWidget.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIWidgetContainer.cs`
- `Assets/GameScripts/GameLogic/Component/UI/UIExtension.cs`

`UIComponent` is a GameLogic module that registers `IUIManager` and coordinates UI groups, form loading, form lifecycle, pooling, and release. `UguiForm` is the project-facing base class for UGUI screens.

Responsibility:

- Manage UI groups and depth.
- Open UI forms by asset name/group or by table id through `UIExtension`.
- Load UI prefabs through `IResourceManager`.
- Reuse form instances through `IObjectPoolManager`.
- Forward lifecycle to `UIFormLogic`.
- Manage widgets through `UIWidgetContainer`.

Form lifecycle:

```text
UIComponent.OpenUIForm()
  -> load prefab
  -> UIForm.OnInit()
  -> UIGroup.AddUIForm()
  -> UIForm.OnOpen()
  -> UIFormLogic.OnOpen()

CloseUIForm()
  -> UIGroup.RemoveUIForm()
  -> UIForm.OnClose()
  -> recycle queue
  -> OnRecycle()
  -> instance pool Unspawn()
```

Business UI guidance:

- Inherit screens from `UguiForm` or `UIFormLogic`.
- Bind user data in `OnOpen(object userData)`.
- Release events/resources in `OnClose()` or `OnRecycle()`.
- Use `LoadAssetAsync<T>()`, `UnloadAsset()`, and `UnloadAllAssets()` on `UguiForm` for UI-owned resources.
- Use `Subscribe()` helpers on `UguiForm`; they are backed by `EventContainer`.

Widgets:

- Add widgets with `AddUIWidget()`.
- Open static widgets in form `OnOpen()`.
- Use `DynamicOpenUIWidget()` when depth refresh is needed.
- Close/remove widgets through `UguiForm` so `UIWidgetContainer` remains consistent.

Do not directly destroy UI instances. Always close through `GameEntry.UI.CloseUIForm()` or `UguiForm.Close()`.
