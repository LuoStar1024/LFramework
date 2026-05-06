---
name: lframework-dev
description: LFramework Unity game framework development guidance for Codex CLI. Use for tasks involving LFramework, GameEntry, LFrameworkEntry, ILFrameworkModule, Runtime Component modules, GameLogic UI, DataTable, Singleton, ResourceContainer, EventContainer, ReferencePool, YooAsset resource loading, scene loading, audio, FSM, Procedure, ObjectPool, Luban Tables, HybridCLR configuration, UniTask workflows, module extension, lifecycle cleanup, troubleshooting, or code review in this project.
---

# LFramework Development

Use this skill for LFramework-specific Unity work in this repository. The distilled knowledge source is `references/`; do not route normal tasks back to `Books/` unless a reference is missing or obviously stale.

## Core Rules

1. Prefer `GameEntry.Xxx` in GameLogic/business code. Use `LFrameworkEntry.GetModule<I...>()` inside framework integration code or when `GameEntry` is not initialized.
2. `LFrameworkEntry.GetModule<T>()` and `RegisterModule<T>()` require interface types. Do not use concrete component classes as `T`.
3. Objects created through `ReferencePool.Acquire<T>()` must implement `IReference`, clear all retained fields in `Clear()`, and be released with `ReferencePool.Release()`.
4. Resource ownership must be explicit. Prefer `ResourceContainer` for owner-scoped loads; direct `GameEntry.Resource.LoadAsset<T>()` calls need matching `UnloadAsset()`.
5. Event ownership must be explicit. Prefer `EventContainer` for lifecycle-bound subscriptions; release it so `UnsubscribeAll()` runs.
6. UI logic must follow `UIFormLogic` / `UguiForm` lifecycle. Bind data in `OnOpen()`, release subscriptions/resources in `OnClose()` or `OnRecycle()`, and do not bypass `UIComponent` to destroy pooled UI instances.
7. `DataTableComponent` lazy-loads Luban `Tables`; DataTable `TextAsset`s must already be loaded into the resource pool before `GameEntry.DataTable` access.
8. When a reference and source disagree, verify the actual C# signature with source search and prefer the implementation.

## Reference Routing

| Task | Read |
|---|---|
| Project architecture, startup, layer boundaries | `references/architecture.md`, `references/module-lifecycle.md` |
| Add or change a framework module | `references/module-lifecycle.md`, `references/extension-practices.md` |
| Base app settings and runtime flags | `references/base.md` |
| Update config, HybridCLR DLL config, resource URLs | `references/config.md` |
| Data tree/state values | `references/datanode.md` |
| Debug overlay, runtime inspection | `references/debugger.md`, `references/troubleshooting.md` |
| Event publish/subscribe or lifecycle subscriptions | `references/event.md` |
| FSM states or FSM data | `references/fsm.md` |
| Localization/I2 integration | `references/localization.md` |
| Object pooling | `references/objectpool.md`, `references/reference-pool.md` |
| Procedure/game flow states | `references/procedure.md`, `references/fsm.md` |
| ReferencePool or `IReference` objects | `references/reference-pool.md` |
| YooAsset package, asset loading, release | `references/resource.md` |
| Scene loading/unloading | `references/scene.md`, `references/resource.md` |
| Player settings persistence | `references/setting.md` |
| Timers/delayed callbacks | `references/timer.md` |
| Audio playback, BGM/SFX helpers | `references/audio.md` |
| Coroutine wrapper or AOT preserve bridge | `references/unity-wrapper.md` |
| Luban tables/config access | `references/datatable.md`, `references/resource.md` |
| Singleton managers | `references/singleton.md` |
| UI forms, groups, widgets, UI assets | `references/ui.md`, `references/resource.md`, `references/event.md` |
| End-to-end runtime flow | `references/workflow-recipes.md` |
| Failure analysis | `references/troubleshooting.md` plus the affected module reference |

## Source Verification

References are concise by design. Before editing code, inspect the affected `.cs` files when:

- the task depends on exact overloads, enum values, serialized fields, or callback parameters;
- the reference and current source appear to conflict;
- Unity serialization, generated Luban code, YooAsset, or HybridCLR behavior is involved.

Keep changes scoped to the requested module and avoid modifying generated DataTable code unless the user explicitly asks for generated output changes.
