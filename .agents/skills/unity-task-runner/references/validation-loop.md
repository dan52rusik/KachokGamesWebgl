# Unity Validation Loop

Use this loop for each concrete Unity task.

1. Confirm the exact asset or code entry points involved.
2. Implement the code or serialized asset change.
3. Run the fastest relevant validation available.
4. If validation fails, fix the narrow cause first and re-run.

Recommended checks:

- inspect generated `.csproj` references when assembly boundaries matter
- run targeted tests under `Assets/Tests/` if present
- inspect scene or prefab YAML for broken `guid` / `fileID` references after serialized edits
- verify that new scripts match Unity file/class naming expectations
- for this project, confirm that `Assets/Scenes/SampleScene.unity` still points to valid `Assembly-CSharp::Tutorial.*` components after script or HUD changes
- when modifying workout UI, check both `WorkoutHUD_Canvas` in scene text and `Assets/KachokGame/Editor/WorkoutHUDBuilder.cs`

Manual editor checks may still be required for:

- prefab overrides
- animation transitions
- event bindings in inspector-only fields
- layout and Canvas behavior
- running the menu item defined by `WorkoutHUDBuilder` when the HUD was changed structurally
