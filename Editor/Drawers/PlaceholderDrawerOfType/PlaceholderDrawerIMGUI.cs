using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.PlaceholderDrawerOfType
{
    public partial class PlaceholderDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
            return 0;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
        }
    }
}
