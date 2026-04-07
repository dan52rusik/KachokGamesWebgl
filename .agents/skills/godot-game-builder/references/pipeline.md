# Pipeline

This is the adapted orchestration loop for Codex.

## Resume behavior

Before making a new plan, check whether these already exist:

- `PLAN.md`
- `STRUCTURE.md`
- `MEMORY.md`

If they exist, resume from them unless the user explicitly asks for a fresh rebuild.

## Planning protocol

Write `PLAN.md` so each task includes:

```markdown
## 1. Task name
- **Status:** pending
- **Targets:** scenes/main.tscn, scripts/player_controller.gd
- **Depends on:** none
- **Goal:** ...
- **Requirements:** ...
- **Verify:** ...
```

Allowed `Status` values:

- `pending`
- `in_progress`
- `done`
- `done (partial)`
- `skipped`

## Architecture protocol

Write `STRUCTURE.md` in full after major architectural changes. It should cover:

- dimension: 2D or 3D
- input actions
- scenes and root node types
- scripts and what they attach to
- signals emitted and received
- asset hints or sizing notes when relevant

## Execution loop

For each ready task:

1. mark it `in_progress` in `PLAN.md`
2. implement the task using the `godot-task-runner` workflow
3. run Godot validation and capture evidence when the task has visible output
4. update `MEMORY.md` with anything worth preserving
5. mark task `done`, `done (partial)`, or replan as needed

## Replanning triggers

Stop and rework the plan when you discover:

- the architecture is wrong,
- a missing asset blocks the chosen approach,
- repeated fixes are not converging,
- the current task scope is too broad or too narrow,
- a foundational scene or script should be rebuilt before continuing.

## Asset strategy

Do not make asset generation a hard dependency for implementation. The pipeline should still produce a working prototype with:

- primitive meshes,
- placeholder sprites,
- flat-color UI,
- procedural backgrounds,
- text-only `ASSETS.md` notes for a later art pass.

## Original source mapping

When you need the deeper upstream guidance, consult the local bundled copies:

- `references/original/SKILL.md`
- `references/original/scaffold.md`
- `references/original/decomposer.md`
- `references/original/asset-planner.md`
