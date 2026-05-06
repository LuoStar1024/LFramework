---
description: Propose a new OpenSpec change using the project skill
argument-hint: change-name or feature description
---

Use the `openspec-propose` skill to create a new OpenSpec change and generate the required artifacts.

Input from this shortcut:

```text
$ARGUMENTS
```

Rules for this shortcut:

1. Treat `$ARGUMENTS` as the optional change name or feature/fix description.
2. Do not duplicate the OpenSpec workflow here; follow `.codex/skills/openspec-propose/SKILL.md`.
3. If the skill is unavailable, report that the project is missing `openspec-propose`.
