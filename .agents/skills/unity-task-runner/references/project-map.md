# Kachok Project Map

Primary runtime targets:

- `Assets/Scenes/SampleScene.unity`
- `Assets/KachokGame/Scripts/Player.cs`
- `Assets/KachokGame/Scripts/PlayerUI.cs`
- `Assets/KachokGame/Scripts/WorkoutHUD.cs`
- `Assets/KachokGame/Scripts/WorkoutSession.cs`
- `Assets/KachokGame/Scripts/StaminaSystem.cs`
- `Assets/KachokGame/Scripts/BodyMorphSystem.cs`
- `Assets/KachokGame/Scripts/DumbbellWorkout.cs`

Primary editor/UI generation target:

- `Assets/KachokGame/Editor/WorkoutHUDBuilder.cs`

Known UI split:

- `PlayerUI` controls hearts and tree counter
- `WorkoutHUD` controls the workout canvas, tabs, bars, rest, and results
- `WorkoutHUDBuilder` is the structural builder for the workout canvas

Known scene details:

- build settings currently include only `Assets/Scenes/SampleScene.unity`
- scene text includes a `WorkoutHUD_Canvas` object with `Assembly-CSharp::Tutorial.WorkoutHUD`
- `DumbbellWorkout` holds a serialized `workoutHUD` reference in scene data

Practical editing rule:

- if a request changes how the workout HUD is laid out or which widgets exist, update the builder script and then reconcile scene references
- if a request changes only behavior or text, prefer editing the C# scripts first
