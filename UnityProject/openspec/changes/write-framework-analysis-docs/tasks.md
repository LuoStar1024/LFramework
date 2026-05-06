## 1. Documentation Scaffold

- [x] 1.1 Create `Books/LFramework框架解析教程/` as the tutorial root directory.
- [x] 1.2 Create `00-阅读指南.md` with tutorial purpose, source coverage, reading order, and file index.
- [x] 1.3 Create all planned numbered Markdown files so the document set is visibly split by module topic.

## 2. Architecture and Core Chapters

- [x] 2.1 Write `01-整体架构与启动流程.md` covering `RootComponent`, `LFrameworkEntry`, `ILFrameworkModule`, module registration, priority ordering, shutdown, low-memory handling, and `GameEntry` access.
- [x] 2.2 Write `02-Core基础设施.md` covering Core entry types, data structures, reference pool, event pool, task pool, variables, utilities, exceptions, and logging.
- [x] 2.3 Add simple diagrams or structured flow descriptions for module update order, event dispatch, reference pool acquire/release, and task pool scheduling.

## 3. Runtime Component Chapters

- [x] 3.1 Write `03-运行时组件-基础服务.md` covering Base, Config, UpdateConfig, DataNode, Timer, Setting, and ReferencePoolComponent.
- [x] 3.2 Write `04-运行时组件-资源场景音频.md` covering Resource/YooAsset integration, asset pool, callbacks, scene loading, audio groups, audio agents, and play parameters.
- [x] 3.3 Write `05-运行时组件-状态机流程对象池.md` covering FSM, Procedure, ObjectPool, object lifecycle, pool modes, and cross-dependencies.
- [x] 3.4 Write `06-运行时组件-调试本地化Unity桥接.md` covering Debugger windows, Localization/I2 integration boundaries, and UnityWrapper coroutine/AOT-preserve behavior.

## 4. GameLogic Component Chapters

- [x] 4.1 Write `07-GameLogic组件-入口UI单例.md` covering `GameEntry` component caching context, UI manager, UI groups, UI forms, UI lifecycle, UI pooling, widgets, and singleton management.
- [x] 4.2 Write `08-GameLogic组件-配置表资源事件音频扩展.md` covering DataTable/Luban loading, ResourceContainer, EventContainer, AudioExtension, UIExtension, and related `AssetUtility` usage.
- [x] 4.3 Clearly distinguish project extension behavior from lower-level framework behavior in every GameLogic chapter.

## 5. Cross-module Tutorial Chapters

- [x] 5.1 Write `09-典型业务流程串讲.md` covering startup, resource package initialization, asset loading, scene switching, UI opening/closing, audio playback, event cleanup, and resource release.
- [x] 5.2 Write `10-扩展模块实践建议.md` covering how to add a new module, choose an interface, set priority, register with `LFrameworkEntry`, manage references, and avoid lifecycle pitfalls.

## 6. Review and Verification

- [x] 6.1 Verify all requested source roots are represented in the tutorial files.
- [x] 6.2 Verify the tutorial is multi-file and no single file attempts to contain the whole tutorial.
- [x] 6.3 Check Markdown headings, file links, code identifiers, and terminology consistency.
- [x] 6.4 Confirm no runtime code or non-documentation assets were modified during implementation.
