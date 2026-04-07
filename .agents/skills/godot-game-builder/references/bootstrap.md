# Workspace Bootstrap

Use this when the workspace does not yet contain a Godot project.

## Target layout

Create or preserve this structure:

```text
project.godot
STRUCTURE.md
PLAN.md
MEMORY.md
ASSETS.md              # optional until visuals matter
reference.png          # optional
scenes/
scripts/
test/
assets/
screenshots/
visual-qa/
```

`screenshots/` should contain an empty `.gdignore` so Godot does not import captured PNGs.

## Minimum bootstrap steps

1. Create `project.godot` with:
   - project name
   - a `run/main_scene`
   - viewport size
   - stretch settings
   - input actions needed by the current plan
2. Create `scenes/`, `scripts/`, `test/`, `assets/`, `screenshots/`, and `visual-qa/`.
3. Create empty or starter versions of:
   - `MEMORY.md`
   - `STRUCTURE.md`
   - `PLAN.md`
4. If the project is new, scaffold one valid main scene so `godot --headless --quit` can succeed early.

## Bootstrap content guidance

- `MEMORY.md` starts as a short project log. Add decisions and quirks as the project evolves.
- `STRUCTURE.md` should document scenes, scripts, signals, and input actions in a stable format.
- `PLAN.md` should be a task DAG or a numbered task list with dependencies when needed.

## Root-level conventions

- Use `scenes/build_*.gd` for programmatic scene generation when the scene is large or serialization is fragile.
- Use `scripts/*.gd` for runtime behavior.
- Use `test/test_<task_id>.gd` for per-task verification.
- Keep assets and screenshots out of version control if the user initializes git.

## Recommended gitignore entries

If a git repo is created, ignore:

```text
assets
screenshots
.godot
*.import
```

If the user wants to keep generated assets in git, remove `assets` from that list.
