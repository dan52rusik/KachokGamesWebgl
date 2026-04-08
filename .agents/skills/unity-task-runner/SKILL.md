---
name: unity-task-runner
description: |
  Use this skill when the user wants to implement, repair, or verify a specific Unity task such as gameplay logic, scene wiring, UI flow, prefab hookup, bug fix, or targeted test inside an existing Unity project.
---

# Unity Task Runner

This skill is the default execution loop for focused Unity work in this workspace.

## Use It For

- updating C# scripts under `Assets/`
- adjusting Unity scenes, prefabs, ScriptableObjects, and UI
- fixing regressions or null-reference wiring issues
- adding or repairing narrow Edit Mode / Play Mode tests

## Workflow

1. Identify the concrete files involved before editing.
2. Read the relevant scene, prefab, and script references narrowly instead of scanning the whole repo.
3. Make the smallest coherent change that resolves the task.
4. Preserve existing serialized references in Unity YAML files whenever possible.
5. Validate using the strongest available local signal:
   - C# compile success from Unity-generated project structure
   - targeted tests if they already exist or are clearly warranted
   - scene/prefab reference review for serialized changes
6. Report what changed, what was validated, and any remaining manual Unity-editor step.

## Project Map

- `Assets/Scenes/` for scenes
- `Assets/` for scripts, prefabs, materials, and gameplay content
- `Packages/manifest.json` for dependencies
- `ProjectSettings/` for project-wide configuration

## Guardrails

- Prefer editing code over hand-editing complex serialized Unity YAML unless the change is mechanical and safe.
- If a scene or prefab must be edited as text, change only the narrow serialized block required.
- Do not rename or regenerate `.meta` files unless the task explicitly requires asset identity changes.
- Keep user-authored inspector wiring intact.
