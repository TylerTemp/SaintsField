using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ButtonDrawers.BelowButtonDrawer
{
    public partial class BelowButtonAttributeDrawer
    {
        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            UpdateButtonLabelIMGUI(property, saintsAttribute, index, info, parent);
            return GetButtonHeightIMGUI() + GetResultHeightIMGUI(property, index, width);
        }


        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute, index, info, parent);
            return DrawResultIMGUI(leftRect, property, index);
        }

    }
}
