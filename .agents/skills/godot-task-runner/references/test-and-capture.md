# Test And Capture

Visible Godot tasks should end with evidence, not just source code.

## Test harness

Write a per-task test script like `test/test_T1.gd`.

Recommended shape:

- `extends SceneTree`
- use `_initialize()` for setup
- use `_process(delta)` for orchestration when needed
- print `ASSERT PASS:` or `ASSERT FAIL:` lines for behavior that is hard to see on screen

The harness should prove the actual task, not just open the scene.

## Screenshot capture

Prefer storing task evidence under `screenshots/<task_name>/`.

Useful pattern:

```bash
godot --write-movie screenshots/<task>/frame.png --fixed-fps 10 --quit-after 60 --script test/test_<task>.gd
```

For static scenes, lower FPS is fine. For movement or physics, keep a reasonable fixed FPS so the simulation behaves correctly.

## Visual checking

If the task is visibly user-facing, inspect screenshots for:

- layout problems,
- wrong scale,
- clipped geometry,
- placeholder assets that should have been replaced,
- broken camera framing,
- UI overlap or text overflow.

If `reference.png` exists, compare against it for palette, density, and framing consistency.

## Optional automated VQA

The original repo includes a Gemini-based visual QA helper. In this workspace, Gemini is the only API integration we expect to use, and only for visual QA.

Use Gemini when:

- the task is strongly visual,
- screenshots would benefit from a second-pass visual review,
- the user has provided `GEMINI_API_KEY` or `GOOGLE_API_KEY`.

Do not block on Gemini. If the key or dependency is missing, continue with:

- manual screenshot inspection,
- code and scene review,
- user review when artistic preference is subjective.

Original sources bundled locally:

- `references/original/test-harness.md`
- `references/original/capture.md`
- `references/original/visual-qa.md`
- `scripts/visual_qa.py`
