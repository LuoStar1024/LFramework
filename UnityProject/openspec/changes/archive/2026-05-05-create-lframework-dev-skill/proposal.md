## Why

Codex CLI currently has OpenSpec workflow skills, but lacks a project-specific LFramework development skill. This makes future coding tasks depend on ad hoc reading of `Books/LFramework框架解析教程` and source files instead of a reusable, module-routed knowledge base.

## What Changes

- Add a Codex skill named `lframework-dev` under `.codex/skills/`.
- Create a `SKILL.md` that routes LFramework development tasks to focused reference documents.
- Split the existing `Books/LFramework框架解析教程` material into module-level `references/*.md` files based on `Assets/LFramework/Runtime/Component` subfolders.
- Merge `Assets/GameScripts/GameLogic/Component` extension modules into the matching runtime module references where appropriate.
- Keep `DataTable`, `Singleton`, and `UI` as standalone module references.
- Include architecture, module lifecycle, workflow, extension practice, and troubleshooting references to support cross-module work.
- Do not change framework runtime code or game logic behavior.

## Capabilities

### New Capabilities

- `lframework-dev-skill`: Provides a Codex CLI skill for LFramework development with module-level reference routing, project-specific red lines, and implementation guidance.

### Modified Capabilities

- None.

## Impact

- Adds files under `.codex/skills/lframework-dev/`.
- Uses `Books/LFramework框架解析教程` and existing source code as source material for distilled references.
- Affects future Codex guidance and implementation quality, but does not affect Unity assets, C# runtime behavior, build output, or public APIs.
