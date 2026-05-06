# lframework-dev-skill Specification

## Purpose
TBD - created by archiving change create-lframework-dev-skill. Update Purpose after archive.
## Requirements
### Requirement: Codex Skill Package
The system SHALL provide a repository-local Codex skill named `lframework-dev` under `.codex/skills/lframework-dev/` with a valid `SKILL.md`.

#### Scenario: Skill metadata is available
- **WHEN** Codex loads repository-local skills
- **THEN** the `lframework-dev` skill has YAML frontmatter with `name: lframework-dev` and a description that mentions LFramework development triggers

#### Scenario: Skill body routes work
- **WHEN** Codex uses `lframework-dev` for a framework task
- **THEN** `SKILL.md` directs Codex to the relevant module reference files instead of requiring broad ad hoc reading of Books

### Requirement: Runtime Component Module References
The skill SHALL include one reference document for each module subfolder under `Assets/LFramework/Runtime/Component`.

#### Scenario: Runtime module reference set exists
- **WHEN** the skill is implemented
- **THEN** `references/` contains `audio.md`, `base.md`, `config.md`, `datanode.md`, `debugger.md`, `event.md`, `fsm.md`, `localization.md`, `objectpool.md`, `procedure.md`, `reference-pool.md`, `resource.md`, `scene.md`, `setting.md`, `timer.md`, and `unity-wrapper.md`

#### Scenario: Runtime module references are source-aligned
- **WHEN** a module reference is opened
- **THEN** it identifies the corresponding `Assets/LFramework/Runtime/Component/<Module>` source folder, module responsibility, key types, lifecycle, dependencies, common usage, and safe extension or cleanup rules

### Requirement: GameLogic Extension Integration
The skill SHALL merge GameLogic extension modules into the matching runtime module references, except for DataTable, Singleton, and UI.

#### Scenario: Extension modules are merged into matching references
- **WHEN** the reference set is implemented
- **THEN** `GameLogic/Component/Event/EventContainer.cs` is covered in `event.md`, `GameLogic/Component/Resource/ResourceContainer.cs` is covered in `resource.md`, and `GameLogic/Component/Audio/AudioExtension.cs` is covered in `audio.md`

#### Scenario: Standalone GameLogic modules have standalone references
- **WHEN** the reference set is implemented
- **THEN** `DataTable`, `Singleton`, and `UI` are documented as `datatable.md`, `singleton.md`, and `ui.md`

### Requirement: Cross-cutting LFramework References
The skill SHALL include cross-cutting references for architecture, lifecycle, workflows, extension practices, and troubleshooting.

#### Scenario: Cross-cutting references exist
- **WHEN** the skill is implemented
- **THEN** `references/` contains `architecture.md`, `module-lifecycle.md`, `workflow-recipes.md`, `extension-practices.md`, and `troubleshooting.md`

#### Scenario: Architecture guidance captures project entry flow
- **WHEN** Codex reads architecture or lifecycle guidance
- **THEN** it can identify the `RootComponent -> LFrameworkEntry -> ILFrameworkModule -> GameEntry` relationship and the interface-based module access rule

### Requirement: Project-specific Guidance
The skill SHALL encode LFramework-specific development red lines and MUST NOT import incompatible TEngine-specific rules.

#### Scenario: LFramework module access is correct
- **WHEN** Codex writes or reviews LFramework code using the skill
- **THEN** it prefers `GameEntry.Xxx` for business code, uses `LFrameworkEntry.GetModule<I...>()` for framework integration where appropriate, and treats `GetModule<T>()` / `RegisterModule<T>()` type parameters as interfaces

#### Scenario: Lifecycle ownership is explicit
- **WHEN** Codex handles resources, events, UI, or reference-pooled objects using the skill
- **THEN** it accounts for `ResourceContainer`, `EventContainer`, `UIFormLogic` lifecycle methods, `IReference.Clear()`, and `ReferencePool.Release()` cleanup ownership

#### Scenario: Incompatible TEngine rules are absent
- **WHEN** the skill guidance is inspected
- **THEN** it does not instruct Codex to use TEngine-only entry points or APIs such as `GameModule.XXX` or `AddUIEvent`

### Requirement: Source Material and Verification Policy
The skill SHALL treat distilled references as the normal knowledge source while preserving a policy to verify against source code when references and code conflict.

#### Scenario: References are distilled from project material
- **WHEN** implementation produces reference files
- **THEN** they are based on `Books/LFramework框架解析教程/*.md` and targeted source inspection of the corresponding module folders

#### Scenario: Source code wins on conflict
- **WHEN** Codex finds a mismatch between a reference and actual C# APIs
- **THEN** the skill instructs Codex to verify signatures in source code and prefer the actual implementation

### Requirement: Non-runtime Change
The implementation SHALL only add or update Codex skill and OpenSpec planning artifacts and SHALL NOT modify Unity runtime code, assets, prefabs, package dependencies, or generated data.

#### Scenario: Runtime project remains unchanged
- **WHEN** the change is implemented
- **THEN** no files under `Assets/`, `Packages/`, or `ProjectSettings/` are modified as part of this skill creation

