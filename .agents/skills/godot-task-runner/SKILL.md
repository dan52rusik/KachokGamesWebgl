---
name: godot-task-runner
description: |
  Use this skill when the user wants to implement, repair, or verify a specific Godot 4 task such as a scene, script, mechanic, UI flow, bug fix, or visual check inside an existing project.
---

# Godot Task Runner

This skill adapts the original `godot-task` executor from `godogen` for Codex. It is the default implementation loop for concrete Godot work inside this workspace.

When bundled upstream docs mention `${CLAUDE_SKILL_DIR}`, interpret it as this skill's directory.

## Read progressively

Read these files only when needed:

| File | Purpose |
|------|---------|
| `references/validation-loop.md` | Core implementation and validation loop |
| `references/scene-builders.md` | Programmatic `.tscn` generation rules |
| `references/script-authoring.md` | Runtime GDScript rules and common quirks |
| `references/test-and-capture.md` | Test harnesses, screenshot capture, and visual checks |
| `references/source-map.md` | Pointers to the original `godogen` files for deeper detail |

## Inputs

Prefer to work from a task block in `PLAN.md`. If no plan exists, infer a temporary task block from the user's request and include:

- `Targets`
- `Goal`
- `Requirements`
- `Verify`

## Required workflow

1. Read `MEMORY.md` if present.
2. Inspect target files and `STRUCTURE.md` if present.
3. Implement scene builders first, then runtime scripts, when both are involved.
4. Import assets before referencing new textures, scenes, or GLBs.
5. Validate with Godot headless commands after each meaningful change.
6. Write a focused test harness when the task has visible or behavioral output.
7. Capture screenshots or evidence when visual verification matters.
8. Update `MEMORY.md` with project-specific findings.

## Execution principles

- Prefer deterministic scene builders over hand-editing large `.tscn` files.
- Use real validation over guesswork.
- Stop looping when fixes are not converging and report the architectural blocker clearly.
- Treat visual defects as real failures, not cosmetic trivia, when the task is visibly user-facing.

## Original source

This skill is adapted from the bundled upstream copy in:

- `references/original/SKILL.md`

Use the source-map reference for deeper local docs and scripts.
