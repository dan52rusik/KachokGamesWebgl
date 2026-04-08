# Unity Script Authoring

Default assumptions for scripts in this project:

- use C#
- preserve namespace and assembly conventions already present in `Assets/`
- default namespace is `Tutorial`
- prefer focused MonoBehaviour components over oversized manager scripts unless the codebase already centralizes behavior
- keep serialized fields stable to avoid breaking inspector wiring
- keep `TextMeshProUGUI`, `Button`, `Slider`, and `Image` references serialized when they are assigned by scene or builder script

Checklist:

1. File name matches the public class name.
2. Serialized fields use clear names and minimal visibility.
3. Null-sensitive dependencies fail early or are guarded.
4. Runtime-only lookup is used only when serialized references are not practical.
5. Avoid hidden scene dependencies.
6. If a script is attached in `Assets/Scenes/SampleScene.unity`, keep the namespace/class path stable or update scene references intentionally.
