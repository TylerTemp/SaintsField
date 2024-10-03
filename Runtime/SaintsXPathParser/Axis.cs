#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser
{
    // *   `::ancestor`
    // *   `::ancestor-inside-prefab`
    // *   `::ancestor-or-self`
    // *   `::ancestor-or-self-inside-prefab`
    // *   `::parent`
    // *   `::parent-or-self`
    // *   `::parent-or-self-inside-prefab`
    // *   `::scene-root`
    // *   `::prefab-root`
    // *   `::resources`
    // *   `::asset`
    public enum Axis
    {
        None,
        Ancestor,
        AncestorInsidePrefab,
        AncestorOrSelf,
        AncestorOrSelfInsidePrefab,
        Parent,
        ParentOrSelf,
        ParentOrSelfInsidePrefab,
        SceneRoot,
        PrefabRoot,
        Resources,
        Asset,
    }
}
#endif
