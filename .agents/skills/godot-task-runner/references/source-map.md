# Source Map

This Codex skill is intentionally lean. Use these bundled upstream files when you need the fuller, battle-tested version of a specific subtopic.

Interpret `${CLAUDE_SKILL_DIR}` in those bundled upstream files as the root of the corresponding local skill directory.

## Orchestrator source

- `../../godot-game-builder/references/original/SKILL.md`
- `../../godot-game-builder/references/original/scaffold.md`
- `../../godot-game-builder/references/original/decomposer.md`
- `../../godot-game-builder/references/original/asset-planner.md`

## Task executor source

- `references/original/SKILL.md`
- `references/original/scene-generation.md`
- `references/original/script-generation.md`
- `references/original/test-harness.md`
- `references/original/capture.md`
- `references/original/visual-qa.md`
- `references/original/quirks.md`
- `references/original/gdscript.md`

## API docs bootstrap

If you want the generated Godot API reference, run:

```bash
bash tools/ensure_doc_api.sh
```

That will populate `doc_api/` beside this skill from the Godot source tree.
