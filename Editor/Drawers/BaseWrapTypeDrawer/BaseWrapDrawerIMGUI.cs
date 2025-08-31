using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.BaseWrapTypeDrawer
{
    public partial class BaseWrapDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
            (SerializedProperty realProp, FieldInfo _) = GetBasicInfo(property, info);
            return EditorGUI.GetPropertyHeight(realProp, GUIContent.none, true);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            (SerializedProperty realProp, FieldInfo _) = GetBasicInfo(property, info);
            EditorGUI.PropertyField(position, realProp, GUIContent.none, true);
        }
    }
}
