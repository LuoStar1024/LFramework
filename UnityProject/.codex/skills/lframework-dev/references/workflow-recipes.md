# Workflow Recipes

Source paths:
- `Books/LFramework框架解析教程/09-典型业务流程串讲.md`
- `Assets/LFramework/Runtime/Component/RootComponent.cs`
- `Assets/GameScripts/GameLogic/Base/GameEntry.cs`
- affected module source files per flow

## Startup

```text
Unity scene loads
  -> RootComponent.Awake()
  -> module Awake() registers interfaces
  -> LFrameworkEntry calls module.OnInit()
  -> RootComponent.Update() calls LFrameworkEntry.OnUpdate()
  -> GameEntry.Start() reinitializes game procedures and caches modules
```

Use `module-lifecycle.md` when editing registration, priority, or initialization order.

## Resource Load and Release

Preferred owner-scoped pattern:

```text
ResourceContainer.Create(owner)
  -> LoadAsset<T>()
  -> use asset
  -> ReferencePool.Release(container)
  -> UnloadAllAssets() and cancel pending loads
```

Direct `GameEntry.Resource.LoadAsset<T>()` requires direct `UnloadAsset()`.

## Scene Switch

```text
Procedure sets target scene data
  -> ChangeState<ProcedureChangeScene>()
  -> GameEntry.Scene.LoadScene(AssetUtility.GetSceneAsset(name))
  -> SceneComponent delegates to ResourceComponent.LoadScene()
  -> YooAsset loads additive scene
  -> SceneComponent refreshes order and main camera
```

Unload through `GameEntry.Scene.UnloadScene()` so SceneComponent tracking remains correct.

## UI Open/Close

```text
GameEntry.UI.OpenUIForm(id)
  -> UIExtension reads TbUIForm
  -> AssetUtility builds prefab path
  -> UIComponent loads/reuses instance
  -> UIFormLogic.OnOpen()

CloseUIForm()
  -> UIFormLogic.OnClose()
  -> recycle
  -> object pool unspawn
```

UI-owned resources and event containers should be released in close/recycle lifecycle.

## Audio Playback

```text
GameEntry.Audio.PlayBgm(id)
  -> TbSound lookup
  -> PlayAudioParams.Create()
  -> AssetUtility builds clip path
  -> AudioComponent loads clip
  -> AudioGroup selects AudioAgent
```

Use audio extension methods so BGM serial id, mute, and volume settings stay consistent.

## Event Cleanup

Use `EventContainer` for lifecycle owners:

```text
Create -> Subscribe many -> ReferencePool.Release(container)
```

This prevents handlers from surviving after UI/managers release.

## Low Memory and Shutdown

Low memory releases unused object pool entries and forces unused resource unload. Shutdown destroys the root object, then `LFrameworkEntry.Shutdown()` calls modules in reverse priority order and finally clears `ReferencePool`.
