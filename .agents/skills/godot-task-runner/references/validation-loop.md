# Validation Loop

Use this loop for each concrete Godot task.

## Sequence

1. Analyze the task and its `Targets`.
2. Import new or changed assets before scenes reference them.
3. Generate or update scenes.
4. Generate or update runtime scripts.
5. Run a quick parse check.
6. Fix reported errors.
7. Add a test harness when the task needs proof.
8. Capture screenshots if the task is visible.
9. Record any project-specific workaround in `MEMORY.md`.

## Core commands

```bash
godot --headless --import
godot --headless --script scenes/build_<name>.gd
godot --headless --quit 2>&1
```

If the machine lacks GNU `timeout`, use `gtimeout` on macOS or a fallback wrapper.

## Error recovery

When Godot reports an error:

1. extract the file path and line number,
2. fix the smallest responsible area,
3. rerun validation before making unrelated edits.

Common categories:

- `Parser Error` means GDScript syntax or typing trouble
- `Invalid call` often means wrong node type or wrong API
- `Cannot infer type` often means Variant inference from `:=`
- hung scene builder usually means missing `quit()`

## When to stop iterating

Stop and escalate or replan when:

- the same class of bug repeats after multiple fixes,
- the architecture is clearly wrong,
- the required asset or scene structure does not exist yet,
- the task should be split before continuing.
