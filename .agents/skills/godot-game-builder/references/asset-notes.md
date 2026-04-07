# Asset Notes

This workspace uses a manual asset workflow by default:

- Gemini API is optional and used only for visual QA.
- 2D image generation is manual unless the user later chooses to automate it.
- image-to-3D is manual unless the user later chooses to automate it.

## Default policy

Prefer this order:

1. gameplay and architecture
2. scene readability
3. visual consistency
4. asset polish

## Good fallback options

- 2D prototypes: `ColorRect`, `Polygon2D`, `Line2D`, simple sprites, bitmap fonts
- 3D prototypes: `BoxMesh`, `SphereMesh`, `CylinderMesh`, `PlaneMesh`, procedural materials
- UI: native Godot controls with consistent spacing and typography
- Environment: gradients, fog, sky materials, tiled placeholder textures

## Manual asset workflow

When an asset step is reached, guide the user explicitly rather than vaguely. The instruction should include:

1. what to generate
2. where to generate it
3. the exact prompt to paste
4. any style or camera constraints
5. the expected output format
6. the exact destination path in the project

Use this structure in your user-facing instructions:

```markdown
Asset: <name>
Purpose: <where it is used in-game>
Tool: <site or app the user should open>
Prompt:
<pasteable prompt>
Download format: PNG / WEBP / GLB
Save as: assets/img/<file>.png
Notes: transparent background / white background / front view / no text / etc.
```

## Manual 2D image generation guidance

For manually generated 2D images:

- ask for PNG when transparency matters
- ask for a plain white or plain solid background when the image may later be converted to 3D
- ask for no UI text baked into the image unless the image is a title screen
- ask for centered subject and clean silhouette for props or characters
- specify framing: front view, side view, top-down, isometric, close-up, and so on

Prefer prompts that describe:

- subject
- shape language
- materials
- mood
- palette
- framing
- exclusions

## Manual image-to-3D guidance

When a 3D model is needed but we are not using an API:

- first prepare a 2D source image suited for conversion
- prefer one clear subject on a clean solid background
- avoid motion blur, heavy shadows, clutter, text, or multiple objects
- tell the user to export the resulting model as `GLB` when available
- store it under `assets/glb/`

Recommended user flow:

1. generate the source image
2. review silhouette and proportions
3. upload that image to the 3D conversion site
4. download `GLB`
5. place both the source image and model in the project for traceability

## What to save in `ASSETS.md`

For each asset, include:

- name
- purpose
- intended in-game size
- source type: placeholder, user-generated, provided, or generated
- target file path
- any prompt notes needed for regeneration

## When to introduce `ASSETS.md`

Create `ASSETS.md` whenever any of these become important:

- multiple art assets need consistent sizing,
- the user cares about a specific visual direction,
- a later art pass needs a manifest,
- task execution needs explicit paths and scale notes.

Include at minimum:

- art direction summary,
- asset name,
- intended in-game size,
- target path,
- current status: placeholder, provided, or generated.

## Gemini usage

Gemini is optional and is only for visual QA:

- comparing screenshots against the intended look,
- spotting layout or art issues,
- helping catch visual regressions.

It is not required for the project to function.

## Optional advanced path

If the user later wants fuller parity with the upstream asset pipeline, use the local bundled copies:

- `references/original/asset-gen.md`
- `references/original/rembg.md`
- `tools/`
