## 1. Skill Scaffold

- [x] 1.1 Create `.codex/skills/lframework-dev/` and required `references/` directory.
- [x] 1.2 Create `SKILL.md` with valid Codex skill frontmatter for `name: lframework-dev`.
- [x] 1.3 Add a concise skill overview explaining that `references/` is the primary distilled knowledge source.

## 2. Source Review

- [x] 2.1 Review `Books/LFramework框架解析教程/*.md` and extract module-level source material.
- [x] 2.2 Review `Assets/LFramework/Runtime/Component` subfolders to verify module names, key types, and lifecycle APIs.
- [x] 2.3 Review `Assets/GameScripts/GameLogic/Component` extension modules and identify which runtime references they merge into.
- [x] 2.4 Review `GameEntry`, `LFrameworkEntry`, `ILFrameworkModule`, and relevant Core files needed for architecture and lifecycle guidance.

## 3. Runtime Module References

- [x] 3.1 Create `references/audio.md` covering Runtime Audio and `GameLogic/Component/Audio/AudioExtension.cs`.
- [x] 3.2 Create `references/base.md` covering Runtime Base.
- [x] 3.3 Create `references/config.md` covering Runtime Config and `UpdateConfig`.
- [x] 3.4 Create `references/datanode.md` covering Runtime DataNode.
- [x] 3.5 Create `references/debugger.md` covering Runtime Debugger.
- [x] 3.6 Create `references/event.md` covering Runtime Event and `GameLogic/Component/Event/EventContainer.cs`.
- [x] 3.7 Create `references/fsm.md` covering Runtime Fsm.
- [x] 3.8 Create `references/localization.md` covering Runtime Localization.
- [x] 3.9 Create `references/objectpool.md` covering Runtime ObjectPool.
- [x] 3.10 Create `references/procedure.md` covering Runtime Procedure.
- [x] 3.11 Create `references/reference-pool.md` covering Runtime ReferencePoolComponent and Core ReferencePool usage.
- [x] 3.12 Create `references/resource.md` covering Runtime Resource and `GameLogic/Component/Resource/ResourceContainer.cs`.
- [x] 3.13 Create `references/scene.md` covering Runtime Scene and `ResourceComponent.Scene` interaction.
- [x] 3.14 Create `references/setting.md` covering Runtime Setting.
- [x] 3.15 Create `references/timer.md` covering Runtime Timer.
- [x] 3.16 Create `references/unity-wrapper.md` covering Runtime UnityWrapper.

## 4. Standalone GameLogic References

- [x] 4.1 Create `references/datatable.md` covering `DataTableComponent`, Luban `Tables`, and DataTable resource loading requirements.
- [x] 4.2 Create `references/singleton.md` covering `SingletonComponent`, `Singleton<T>`, `SingletonBehaviour<T>`, and release behavior.
- [x] 4.3 Create `references/ui.md` covering `UIComponent`, `UIGroup`, `UIForm`, `UIFormLogic`, `UguiForm`, `UIWidget`, `UIWidgetContainer`, and `UIExtension`.

## 5. Cross-cutting References

- [x] 5.1 Create `references/architecture.md` covering the project layers and `RootComponent -> LFrameworkEntry -> ILFrameworkModule -> GameEntry` flow.
- [x] 5.2 Create `references/module-lifecycle.md` covering registration, interface-only module access, priority ordering, update order, and shutdown order.
- [x] 5.3 Create `references/workflow-recipes.md` covering startup, resource load/release, scene switch, UI open/close, audio playback, event cleanup, low-memory, and shutdown flows.
- [x] 5.4 Create `references/extension-practices.md` covering how to add or extend LFramework modules safely.
- [x] 5.5 Create `references/troubleshooting.md` covering common mistakes and source-verification policy.

## 6. Skill Routing and Red Lines

- [x] 6.1 Add a task-to-reference routing table in `SKILL.md`.
- [x] 6.2 Add LFramework-specific coding red lines for module access, resources, events, UI lifecycle, reference pooling, and DataTable loading.
- [x] 6.3 Add a conflict policy that tells Codex to verify actual C# signatures and prefer source code when references disagree.
- [x] 6.4 Confirm `SKILL.md` does not include incompatible TEngine-specific guidance such as `GameModule.XXX` or `AddUIEvent`.

## 7. Verification

- [x] 7.1 Verify all required reference files from the spec exist under `.codex/skills/lframework-dev/references/`.
- [x] 7.2 Verify each reference names its source folder or source files and includes responsibility, lifecycle, dependencies, usage, and cleanup or extension guidance where applicable.
- [x] 7.3 Verify no files under `Assets/`, `Packages/`, or `ProjectSettings/` were modified.
- [x] 7.4 Run OpenSpec status/validation for `create-lframework-dev-skill` and confirm the change is apply-ready or complete.
