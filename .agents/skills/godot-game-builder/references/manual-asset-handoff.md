# Manual Asset Handoff

Use this reference when the project needs the user to manually create art assets outside Codex.

## Goal

Give the user a clean handoff with no ambiguity. The user should know:

- which site to open,
- what prompt to paste,
- what result to pick,
- which format to download,
- what exact filename to use,
- where to put the file in the project.

## Response template for a 2D asset

```markdown
Asset: <asset name>
Used for: <where it appears in the game>
Open: <tool/site name>

Prompt:
<pasteable prompt>

Choose an output that:
- matches the requested silhouette and framing
- has no text or watermark
- keeps the subject clean and readable at game scale

Download format: PNG
Save to: assets/img/<file>.png
```

## Response template for image-to-3D

```markdown
Asset: <asset name>
Used for: <where it appears in the game>

Step 1. Generate source image
Open: <image tool/site>
Prompt:
<pasteable prompt for the source image>
Download format: PNG
Save to: assets/img/<source>.png

Step 2. Convert to 3D
Open: <3D conversion site>
Upload: assets/img/<source>.png
Pick the cleanest result with the right silhouette
Download format: GLB
Save to: assets/glb/<model>.glb
```

## Prompt rules for 2D source images

- one clear subject
- simple readable silhouette
- no extra props unless needed
- no text, labels, or watermark
- solid or plain background if the image may later go to 3D conversion
- describe camera angle explicitly

## Prompt rules for image-to-3D source images

Prefer prompts that produce:

- centered object
- front or three-quarter view
- clean edges
- low clutter
- stable proportions
- minimal cast shadow

Avoid:

- busy backgrounds
- multiple objects
- dramatic motion
- extreme perspective distortion
- tiny details that will not survive conversion

## Filename conventions

- 2D images: `assets/img/<asset_name>.png`
- 3D models: `assets/glb/<asset_name>.glb`
- if there are variants, suffix them clearly: `_v2`, `_front`, `_damaged`

## Regeneration guidance

When the first result is weak, explain why and give a revised prompt. Mention one or two concrete corrections, such as:

- "make the silhouette chunkier"
- "remove background clutter"
- "use a pure white background"
- "front-facing instead of angled"
- "less detail, bolder shape language"
