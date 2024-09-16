namespace SaintsField.SaintsXPathParser
{
    // *   `::ancestor`
    // *   `::ancestor-inside-prefab`
    // *   `::ancestor-or-self`
    // *   `::ancestor-or-self-inside-prefab`
    // *   `::parent`
    // *   `::parent-or-self`
    // *   `::parent-or-self-inside-prefab`
    public enum NodeTest
    {
        None,
        Ancestor,
        AncestorInsidePrefab,
        AncestorOrSelf,
        AncestorOrSelfInsidePrefab,
        Parent,
        ParentOrSelf,
        ParentOrSelfInsidePrefab,
    }
}
