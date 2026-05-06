---
description: Implement tasks from an OpenSpec change using the project skill
argument-hint: change-name or context
---

Use the `openspec-apply-change` skill to implement an OpenSpec change.

Input from this shortcut:

```text
$ARGUMENTS
```

Rules for this shortcut:

1. Treat `$ARGUMENTS` as the optional change name or task context.
2. Do not duplicate the OpenSpec workflow here; follow `.codex/skills/openspec-apply-change/SKILL.md`.
3. If the skill is unavailable, report that the project is missing `openspec-apply-change`.
