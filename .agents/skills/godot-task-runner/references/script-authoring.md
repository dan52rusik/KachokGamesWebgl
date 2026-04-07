# Script Authoring

Runtime scripts define behavior. Keep them small, typed where useful, and aligned with the node they attach to.

## Baseline rules

- `extends` must match the node type the script attaches to
- prefer `@onready` node refs over repeated `get_node()` calls
- connect signals in `_ready()`
- use the input actions declared in `project.godot` and `STRUCTURE.md`
- avoid `preload()` for resources that may not exist yet; use `load()`

## Type inference pitfalls

Be careful with `:=` in GDScript. Prefer explicit typing or plain `=` when:

- calling `instantiate()`,
- reading array or dictionary values,
- using polymorphic math helpers like `abs`, `clamp`, `max`, `min`, `lerp`, `move_toward`, or `randf_range`.

## Common Godot quirks to remember

- `Camera2D` should use `make_current()`, not `current`
- test harness cameras may need to disable gameplay cameras every frame
- `_ready()` on instantiated content does not fire during scene-builder `_initialize()`
- changing collisions inside physics callbacks often needs `set_deferred()`
- `queue_free()` can be too late when immediate replacement is needed in tests; sometimes `free()` is the right tool

## Original deep references

Read these bundled files for precise patterns and more edge cases:

- `references/original/script-generation.md`
- `references/original/gdscript.md`
- `references/original/quirks.md`
