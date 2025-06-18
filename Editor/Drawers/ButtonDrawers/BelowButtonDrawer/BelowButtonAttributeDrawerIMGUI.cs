using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
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
            string displayError = GetDisplayError(property);
            return EditorGUIUtility.singleLineHeight +
                   (displayError == "" ? 0 : ImGuiHelpBox.GetHeight(displayError, width, MessageType.Error));
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
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute, info, parent);

            string displayError = GetDisplayError(property);

            if (displayError != "")
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, displayError, MessageType.Error);
            }

            return leftRect;
        }

    }
}
