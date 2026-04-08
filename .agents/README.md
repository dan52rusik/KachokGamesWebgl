# Unity Agents Workspace

This `.agents` directory is configured for the current Unity project.

Available local skills:
- `unity-task-runner` for concrete implementation, repair, and verification tasks inside the existing project
- `unity-project-builder` for larger feature work, cleanup, and multi-file delivery planning

Project-specific notes:
- main gameplay code lives under `Assets/KachokGame/Scripts`
- the main playable scene in build settings is `Assets/Scenes/SampleScene.unity`
- a second sample scene also exists at `Assets/Floreswa/Scene/SampleScene.unity`, but it is not the primary gameplay target
- current gameplay namespace is `Tutorial`
- workout UI is centered around `WorkoutHUD`, `PlayerUI`, and `WorkoutHUDBuilder`
- the HUD canvas in scene text is named `WorkoutHUD_Canvas`

Validation priorities:
- preserve scene and prefab references when editing serialized Unity YAML
- prefer script changes under `Assets/KachokGame/Scripts` over large hand-edits to scene files
- when HUD structure changes are needed, prefer updating `Assets/KachokGame/Editor/WorkoutHUDBuilder.cs` and rebuilding in the Unity editor
