using System;

namespace SaintsField.Editor.Utils.RuntimeSave
{
    [Serializable]
    public struct GameObjectHierarchyPath
    {
        // the name of the node in hierarchy, e.g. for `/root/hero/hand`, it'll be separated as "root", "hero", "hand"
        public string nodeName;
        // in case there are same named node on the same leve, this is the index of the node
        // e.g
        // root
        // |- Hero
        // |- Separator
        // |- Hero
        // |- ...
        // under "root", for "Hero" node, the first value will be 0, the second "Hero" will be 1 (not 2, we ignore those different names)
        public int sameNameIndex;

        public GameObjectHierarchyPath(string nodeName, int sameNameIndex)
        {
            this.nodeName = nodeName;
            this.sameNameIndex = sameNameIndex;
        }
    }
}
