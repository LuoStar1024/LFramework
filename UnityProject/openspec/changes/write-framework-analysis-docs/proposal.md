## Why

The framework currently has substantial runtime infrastructure spread across Core, built-in Components, and GameLogic extension Components, but there is no module-by-module tutorial that explains how the pieces fit together. This change creates structured documentation so maintainers can understand startup flow, module contracts, lifecycle rules, and common usage paths before modifying or extending the framework.

## What Changes

- Add a multi-file framework analysis tutorial under `Books/`.
- Split the tutorial by module category instead of producing one oversized document.
- Cover the three requested source areas:
  - `Assets/LFramework/Runtime/Core`
  - `Assets/LFramework/Runtime/Component`
  - `Assets/GameScripts/GameLogic/Component`
- Include architecture overview, module lifecycle, dependency relationships, key APIs, usage patterns, and extension guidance.
- Include cross-module walkthroughs for typical flows such as startup, resource loading, scene switching, UI opening, audio playback, event cleanup, and object/resource release.
- Do not change runtime code, APIs, prefabs, assets, package dependencies, or generated data.

## Capabilities

### New Capabilities

- `framework-analysis-docs`: Provides a complete, module-separated tutorial document set for the LFramework runtime and GameLogic component architecture.

### Modified Capabilities

- None.

## Impact

- Affected files: new Markdown documentation files under `Books/`.
- Affected systems: documentation only.
- Runtime code impact: none.
- API compatibility impact: none.
- Dependencies: none.
