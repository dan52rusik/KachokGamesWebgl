# Scene Builders

Use programmatic scene builders for larger or fragile scenes. They should generate `.tscn` files through Godot itself instead of hand-editing serialized scene text.

## Required shape

The builder should:

1. `extend SceneTree`
2. use `_initialize()` as the entry point
3. create the full node hierarchy
4. set owners so nodes serialize correctly
5. attach scripts with `set_script()` when needed
6. save via `PackedScene.pack()` and `ResourceSaver.save()`
7. call `quit()` on success and failure paths

## Ownership rule

After all `add_child()` calls, set owners recursively from the root. This prevents nodes from disappearing from the saved `.tscn`.

When using instantiated scenes or GLBs, do not recurse into their internals if they already have `scene_file_path`; keep them as references.

## Safety checks

- count nodes before packing,
- instantiate the packed scene for a quick sanity check if the scene is non-trivial,
- fail fast if `pack()` or `save()` returns a non-OK error.

## Practical patterns

- build child scenes first, then parent scenes that instance them
- attach runtime scripts in the scene builder, not during gameplay setup
- use simple collision primitives for imported meshes instead of expensive generated collision

## Original deep reference

For the full upstream guidance, read:

- `references/original/scene-generation.md`
