---
name: unity-project-builder
description: |
  Use this skill when the user wants to create, scaffold, or significantly update a Unity project or a large feature set from a natural-language description.
---

# Unity Project Builder

This skill handles larger Unity work that spans multiple files or systems.

## Use It For

- scaffolding a new feature area inside `Assets/`
- building a gameplay slice across scenes, prefabs, and scripts
- organizing delivery for UI, combat, progression, menus, or save systems
- project-wide cleanup of obsolete structures

## Workflow

1. Define the feature scope and affected Unity areas.
2. Create a short implementation plan grouped by system.
3. Execute task-by-task, using `unity-task-runner` style discipline for each concrete change.
4. Prefer incremental delivery that keeps the project compiling.
5. Validate each slice with targeted checks rather than deferring all validation to the end.

## Expected Outputs

- updated scripts under `Assets/`
- any required scenes, prefabs, materials, or ScriptableObjects
- tests only where they materially reduce regression risk
- a concise note of any Unity-editor-only follow-up

## Guardrails

- preserve GUID stability by keeping existing assets and `.meta` files where possible
- avoid broad scene rewrites when a prefab or component extraction will do
- do not introduce package dependencies unless justified by the task
