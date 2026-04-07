---
name: godot-game-builder
description: |
  Use this skill when the user wants to create, build, generate, or significantly update a complete Godot 4 game or prototype in the current workspace from a natural-language description.
---

# Godot Game Builder

This is a Codex-native adaptation of the `godogen` workflow in this workspace. It keeps the strong parts of the original pipeline: document-driven planning, Godot headless validation, and task-by-task delivery.

## What changed from the original

- Do not rely on `CLAUDE.md`, `.claude/skills/`, or `Skill(skill="...")`.
- Run the orchestration directly in Codex in the current workspace.
- Use Gemini as the only optional API integration, and only for visual QA. Treat image generation and 3D conversion as a manual user workflow unless the user explicitly asks to automate them later.
- Reuse the bundled `references/original/` docs and `tools/` helpers when deeper detail is needed.
- When bundled upstream docs mention `${CLAUDE_SKILL_DIR}`, interpret it as this skill's directory.

## Read progressively

Read these files only when their phase begins:

| File | Purpose |
|------|---------|
| `references/bootstrap.md` | Bootstrapping a fresh Godot workspace and required files |
| `references/pipeline.md` | Full orchestration loop and project document protocol |
| `references/asset-notes.md` | Manual asset workflow: what to generate, where to save it, and how to guide the user |
| `references/manual-asset-handoff.md` | Exact user-facing format for manual image generation and image-to-3D handoff |

## Core artifacts

Maintain these project files at the workspace root:

- `project.godot` — Godot project config
- `STRUCTURE.md` — architecture map of scenes, scripts, signals, and inputs
- `PLAN.md` — task list with status and verification criteria
- `MEMORY.md` — discoveries, workarounds, and project-specific quirks
- `ASSETS.md` — asset manifest when art direction or asset placement matters
- `reference.png` — optional visual target when available

## Pipeline

1. If `project.godot` does not exist, read `references/bootstrap.md` and create a minimal but valid Godot project layout.
2. Read `references/pipeline.md` and create or update `PLAN.md`, `STRUCTURE.md`, and `MEMORY.md`.
3. If the user gave visual direction, create `ASSETS.md` and optionally `reference.png`. For 2D art or image-to-3D steps, prepare user-ready prompts and explicit save instructions rather than assuming automatic generation.
4. Execute the plan task by task. For each task, follow the `godot-task-runner` workflow rather than free-styling implementation.
5. Replan when a task reveals an architectural issue, missing asset, or broken assumption.
6. Resume from existing `PLAN.md` and `MEMORY.md` instead of restarting unless the user explicitly asks for a reset.

## Operating rules

- Prefer a small number of substantial tasks over many tiny ones.
- Always include `Goal`, `Requirements`, `Verify`, `Status`, and `Targets` in each task block.
- Preserve project state between tasks through the files above, not through conversational memory alone.
- Verify with real Godot commands whenever possible: import, headless parse check, test harness, screenshot capture.
- If you need the full upstream guidance, read the local copies in:
  - `references/original/`
  - `../godot-task-runner/references/original/`

## Default stance on assets

Do not make paid image or 3D APIs a dependency. Use one of:

- clean primitive/procedural visuals,
- placeholder textures or meshes with correct sizing,
- user-supplied or manually generated art,
- later asset pass after gameplay works.

When the project reaches an asset step, explain clearly:

- what asset is needed,
- whether it is 2D or image-to-3D,
- what prompt the user should paste,
- which site or tool to use,
- what file format to download,
- where to place it in the project.

The priority is always: playable structure first, visual polish second.
