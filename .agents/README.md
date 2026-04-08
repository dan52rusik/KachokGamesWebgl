# Unity Agents Workspace

This `.agents` directory is configured for a Unity project.

Available local skills:
- `unity-task-runner` for concrete implementation, repair, and verification tasks inside the existing Unity project
- `unity-project-builder` for larger feature work, project-wide scaffolding, and delivery planning

Project assumptions:
- Unity project root contains `Assets/`, `Packages/`, and `ProjectSettings/`
- gameplay code lives under `Assets/`
- validation should prefer Unity-safe checks such as compile state, scene/prefab reference review, and targeted Play Mode or Edit Mode tests when present
