## Context

The requested documentation covers three source roots that represent different layers of the project:

- `Assets/LFramework/Runtime/Core`: low-level framework primitives such as module registration, reference pooling, event pooling, task scheduling, variables, logging, and utilities.
- `Assets/LFramework/Runtime/Component`: Unity-facing framework modules such as resource, scene, audio, FSM, procedure, object pool, setting, timer, data node, localization, debugger, and Unity wrapper.
- `Assets/GameScripts/GameLogic/Component`: project-side extensions such as UI, singleton management, data table access, resource containers, event containers, and audio/UI helper extensions.

The documentation must be useful as a tutorial, not only as an API inventory. It should explain the actual lifecycle and dependency flow discovered in code, including `RootComponent -> LFrameworkEntry -> ILFrameworkModule`, module priority ordering, `GameEntry` static module access, resource/object-pool coupling, and UI/audio/data-table usage patterns.

## Goals / Non-Goals

**Goals:**

- Produce a multi-file Markdown document set under `Books/`.
- Organize documentation by module category and source ownership.
- Explain each module's responsibility, key classes/interfaces, lifecycle, dependencies, usage patterns, and extension points.
- Include cross-module walkthroughs that connect modules into real runtime flows.
- Keep the docs maintainable with an index file and focused per-topic files.

**Non-Goals:**

- Do not modify runtime C# code.
- Do not add tests, prefabs, ScriptableObjects, assets, package dependencies, or generated data.
- Do not document unrelated directories outside the requested three roots except brief references needed to explain `GameEntry`, `IDataTableManager`, `AssetUtility`, or constants used by the requested modules.
- Do not produce a single monolithic tutorial file.

## Decisions

### Decision: Split Documentation by Reader Workflow

The document set will use an index plus focused module files:

- `Books/LFramework框架解析教程/00-阅读指南.md`
- `Books/LFramework框架解析教程/01-整体架构与启动流程.md`
- `Books/LFramework框架解析教程/02-Core基础设施.md`
- `Books/LFramework框架解析教程/03-运行时组件-基础服务.md`
- `Books/LFramework框架解析教程/04-运行时组件-资源场景音频.md`
- `Books/LFramework框架解析教程/05-运行时组件-状态机流程对象池.md`
- `Books/LFramework框架解析教程/06-运行时组件-调试本地化Unity桥接.md`
- `Books/LFramework框架解析教程/07-GameLogic组件-入口UI单例.md`
- `Books/LFramework框架解析教程/08-GameLogic组件-配置表资源事件音频扩展.md`
- `Books/LFramework框架解析教程/09-典型业务流程串讲.md`
- `Books/LFramework框架解析教程/10-扩展模块实践建议.md`

Alternative considered: one large `LFramework框架解析教程.md`. This was rejected because the user explicitly asked to split the documentation into multiple files and because the module count is high enough that a single file would be harder to navigate.

### Decision: Keep Documentation Close to the Existing Architecture

Each module chapter will follow the code's existing boundaries instead of inventing a new abstraction hierarchy:

- Core primitives stay together.
- Unity framework components stay under Runtime Component chapters.
- Project-level extensions stay under GameLogic chapters.

Alternative considered: reorganizing around gameplay workflows only. This was rejected because the user asked to distinguish modules, and maintainers need source-to-document traceability.

### Decision: Include Cross-Module Flow Documents

The module files explain each subsystem independently, while `09-典型业务流程串讲.md` connects them through runtime scenarios:

- framework startup and module registration
- resource package initialization and asset loading
- scene loading and active scene refresh
- UI open/close/recycle
- audio group setup and playback
- event subscription cleanup
- resource container cleanup

Alternative considered: embedding all flows into individual module chapters. This was rejected because many flows cross module boundaries and are easier to understand in one narrative file.

### Decision: Treat Generated or Third-Party-Heavy Areas as Integration Notes

Large embedded systems such as I2Localization and YooAsset integration will be explained at the integration level: what this project wraps, how the wrapper is initialized, what public surface the rest of the project consumes, and where lifecycle ownership sits. The docs will not rewrite external plugin manuals.

Alternative considered: documenting every I2Localization file in depth. This was rejected because it would dilute the framework tutorial and exceed the requested module-level analysis.

## Risks / Trade-offs

- Risk: Documentation may drift as modules evolve. Mitigation: include file path references and keep module files scoped so future edits are localized.
- Risk: Too much API listing can make the tutorial hard to read. Mitigation: emphasize lifecycle, dependencies, and common usage first; list APIs only where they clarify usage.
- Risk: Some requested modules depend on nearby files outside the three roots, such as `GameEntry`, `IDataTableManager`, `AssetUtility`, and constants. Mitigation: mention these only as supporting context and clearly mark them as related entry or utility context.
- Risk: Chinese file names may be awkward in tooling on some environments. Mitigation: use UTF-8 Markdown files and keep names short with numeric prefixes.
