# Unity Bootstrap Notes

This workspace is already a Unity project. Do not create a new project shell.

When bootstrapping a new system inside this repo:

1. place runtime code in the established `Assets/KachokGame/` structure when the task is part of the game
2. keep package changes minimal and update `Packages/manifest.json` only when required
3. add scenes and prefabs in the existing content layout rather than creating parallel top-level folders
4. preserve `.meta` files for moved or renamed assets
5. if the feature extends the workout interface, integrate with `WorkoutHUD`, `PlayerUI`, `WorkoutSession`, `StaminaSystem`, and the main scene instead of creating a parallel UI stack
