## Context

The project already contains `Books/LFramework框架解析教程/`, which explains LFramework architecture and modules for human readers. Codex CLI currently has `.codex/skills/openspec-*` only, so framework development tasks do not have a reusable project-specific knowledge source.

The new skill will be a Codex-native equivalent of the TEngine `tengine-dev` pattern: a concise `SKILL.md` with task routing, plus focused `references/*.md` files that are loaded only when relevant. The references will be distilled from Books and verified against the actual source layout, especially `Assets/LFramework/Runtime/Component` and `Assets/GameScripts/GameLogic/Component`.

## Goals / Non-Goals

**Goals:**

- Create `.codex/skills/lframework-dev/` with a valid `SKILL.md`.
- Create module-level references for each subfolder under `Assets/LFramework/Runtime/Component`.
- Merge GameLogic extension modules into matching runtime references where they extend that module.
- Keep `DataTable`, `Singleton`, and `UI` as standalone references.
- Preserve cross-cutting references for architecture, module lifecycle, runtime workflows, extension guidance, and troubleshooting.
- Make future Codex implementation tasks route to the minimum relevant references.

**Non-Goals:**

- Do not modify runtime C# code, Unity assets, prefabs, generated data, or package dependencies.
- Do not keep `Books/` as the runtime lookup target for the skill.
- Do not copy TEngine-specific rules such as `GameModule.XXX`, `AddUIEvent`, or TEngine hotfix paths into LFramework guidance.
- Do not create scripts unless deterministic automation becomes necessary during implementation.

## Decisions

### Decision: Use Codex skill format under `.codex/skills/lframework-dev`

The skill will live in the repository-local `.codex/skills/` directory so it is versioned with the project and available to Codex in this workspace. The required file is `SKILL.md`; optional resources are stored under `references/`.

Alternative considered: install a global skill under the user Codex home. This was rejected because this guidance is project-specific and should follow the repository.

### Decision: Route by actual LFramework module folders

The primary reference split will match each subfolder under `Assets/LFramework/Runtime/Component`:

- `audio.md`
- `base.md`
- `config.md`
- `datanode.md`
- `debugger.md`
- `event.md`
- `fsm.md`
- `localization.md`
- `objectpool.md`
- `procedure.md`
- `reference-pool.md`
- `resource.md`
- `scene.md`
- `setting.md`
- `timer.md`
- `unity-wrapper.md`

This keeps each reference aligned with a concrete source ownership boundary.

Alternative considered: split only by Books chapters. This was rejected because Books chapters group multiple modules together and are too coarse for Codex task routing.

### Decision: Merge GameLogic extension modules into matching references, except standalone modules

`Assets/GameScripts/GameLogic/Component/Event/EventContainer.cs` will be covered in `event.md`, `ResourceContainer.cs` in `resource.md`, and `AudioExtension.cs` in `audio.md`. `DataTable`, `Singleton`, and `UI` will remain standalone as:

- `datatable.md`
- `singleton.md`
- `ui.md`

These standalone modules have enough project-side behavior and APIs to require independent routing.

Alternative considered: create one reference per GameLogic subfolder. This was rejected for extension modules because it would split closely related lifecycle guidance across files.

### Decision: Keep cross-cutting references separate from module references

The skill will also include:

- `architecture.md` for `RootComponent -> LFrameworkEntry -> ILFrameworkModule -> GameEntry`.
- `module-lifecycle.md` for registration, interface access, priority, update, and shutdown.
- `workflow-recipes.md` for startup, resource, scene, UI, audio, event, and shutdown flows.
- `extension-practices.md` for adding or extending modules.
- `troubleshooting.md` for common failure patterns.

These files prevent duplication in module references while supporting architecture and debugging tasks.

Alternative considered: put cross-cutting guidance entirely in `SKILL.md`. This was rejected because `SKILL.md` should stay concise and references should be loaded progressively.

### Decision: Use Books as source material but references as the skill contract

Implementation will read `Books/LFramework框架解析教程/*.md` and relevant source files, then write distilled references. After creation, `SKILL.md` will route to `references/*.md`; it will not instruct Codex to use Books as the primary lookup path.

Alternative considered: make `SKILL.md` route directly to Books files. This was rejected by the user and would keep the old coarse chapter grouping.

## Risks / Trade-offs

- Risk: Some module references may duplicate lifecycle rules. Mitigation: keep common rules in `module-lifecycle.md` and link module docs to it conceptually through the routing table.
- Risk: Distilled references can drift from source. Mitigation: include file path anchors and instruct Codex to verify actual signatures with source search when references and code conflict.
- Risk: Too many references can make routing harder. Mitigation: keep `SKILL.md` routing table explicit and task-oriented.
- Risk: Books may omit details needed for module-level guidance. Mitigation: supplement with targeted reads of source files in the corresponding module folders during implementation.
