## ADDED Requirements

### Requirement: Multi-file Tutorial Structure
The documentation set SHALL be written as multiple Markdown files under `Books/LFramework框架解析教程/`, with a numbered reading guide or index that links the tutorial files.

#### Scenario: Documentation is split by topic
- **WHEN** the documentation is generated
- **THEN** it is stored as multiple focused Markdown files instead of one monolithic tutorial file

#### Scenario: Reader can navigate the tutorial
- **WHEN** a reader opens the first tutorial file
- **THEN** it provides a clear reading order and links or references to the other tutorial files

### Requirement: Requested Source Roots Coverage
The documentation set SHALL cover the modules found under `Assets/LFramework/Runtime/Core`, `Assets/LFramework/Runtime/Component`, and `Assets/GameScripts/GameLogic/Component`.

#### Scenario: Core modules are covered
- **WHEN** a reader checks the Core tutorial chapter
- **THEN** it explains module entry, data structures, reference pool, event pool, task pool, variables, utilities, exceptions, and logging

#### Scenario: Runtime components are covered
- **WHEN** a reader checks the Runtime Component tutorial chapters
- **THEN** they explain the built-in framework components including base, config, event, object pool, resource, scene, FSM, procedure, audio, data node, timer, setting, localization, debugger, reference pool component, and Unity wrapper

#### Scenario: GameLogic components are covered
- **WHEN** a reader checks the GameLogic Component tutorial chapters
- **THEN** they explain UI, singleton, data table, event container, resource container, and audio extension modules

### Requirement: Module-level Explanation
Each module section SHALL describe the module's responsibility, important classes or interfaces, lifecycle behavior, dependencies, and common usage or extension guidance.

#### Scenario: Module explanation is actionable
- **WHEN** a maintainer reads a module section
- **THEN** they can identify what the module owns, how it is initialized or updated, which other modules it depends on, and how to use or extend it safely

### Requirement: Runtime Flow Explanation
The documentation set SHALL include cross-module walkthroughs for common runtime flows that connect the modules into actual behavior.

#### Scenario: Startup flow is explained
- **WHEN** a reader opens the runtime flow tutorial
- **THEN** it explains how `RootComponent`, `LFrameworkEntry`, `ILFrameworkModule`, module priorities, and `GameEntry` work together during startup

#### Scenario: Resource and UI flow is explained
- **WHEN** a reader opens the runtime flow tutorial
- **THEN** it explains how resource loading, object pooling, UI opening, UI closing, and resource release interact

#### Scenario: Scene and audio flow is explained
- **WHEN** a reader opens the runtime flow tutorial
- **THEN** it explains how scene loading/unloading and audio playback use the resource and callback systems

### Requirement: Documentation-only Change
The implementation SHALL only add or update Markdown documentation under `Books/` and SHALL NOT modify runtime code, APIs, prefabs, assets, package dependencies, or generated data.

#### Scenario: Runtime code remains unchanged
- **WHEN** the documentation implementation is completed
- **THEN** no files outside `Books/` are changed except OpenSpec planning artifacts
