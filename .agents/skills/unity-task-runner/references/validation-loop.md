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

Manual editor checks may still be required for:

- prefab overrides
- animation transitions
- event bindings in inspector-only fields
- layout and Canvas behavior
