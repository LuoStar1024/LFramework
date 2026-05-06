---
description: Archive a completed OpenSpec change using the project skill
argument-hint: change-name
---

Use the `openspec-archive-change` skill to archive a completed OpenSpec change.

Input from this shortcut:

```text
$ARGUMENTS
```

Rules for this shortcut:

1. Treat `$ARGUMENTS` as the optional change name.
2. Do not duplicate the OpenSpec workflow here; follow `.codex/skills/openspec-archive-change/SKILL.md`.
3. If the skill is unavailable, report that the project is missing `openspec-archive-change`.
