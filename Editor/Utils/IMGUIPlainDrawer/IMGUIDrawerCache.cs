using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIDrawerCache
    {
        public readonly struct DrawerId: IEquatable<DrawerId>
        {
            public readonly string PropertyId;
            public readonly int SameAttributeTypeIndex;

            public DrawerId(SerializedProperty property, int sameAttributeTypeIndex)
            {
                PropertyId = SerializedUtils.GetUniqueId(property);
                SameAttributeTypeIndex = sameAttributeTypeIndex;
            }

            public bool Equals(DrawerId other)
            {
                return PropertyId == other.PropertyId && SameAttributeTypeIndex == other.SameAttributeTypeIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is DrawerId other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(PropertyId, SameAttributeTypeIndex);
            }
        }

        public static Dictionary<DrawerId, PropertyDrawer> CachedDrawers = new Dictionary<DrawerId, PropertyDrawer>();
        public static Dictionary<DrawerId, ReorderableList> CachedReorderableList = new Dictionary<DrawerId, ReorderableList>();
    }
}
