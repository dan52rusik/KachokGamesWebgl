# Unity Delivery Pipeline

Use this flow for larger Unity feature work:

1. map the feature to concrete assets and systems
2. identify serialized-risk areas: scenes, prefabs, animator controllers, ScriptableObjects
3. implement code-first where possible
4. apply narrow serialized edits only where Unity wiring requires them
5. validate incrementally:
   - compile-sensitive code changes first
   - then scene/prefab reference integrity
   - then targeted tests if available
6. finish with a short manual editor checklist when visual or inspector validation is still needed
