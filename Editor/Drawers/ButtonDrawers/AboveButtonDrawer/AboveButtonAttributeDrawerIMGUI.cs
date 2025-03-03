using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ButtonDrawers.AboveButtonDrawer
{
    public partial class AboveButtonAttributeDrawer
    {
        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string displayError = GetDisplayError(property);
            return EditorGUIUtility.singleLineHeight +
                   (displayError == "" ? 0 : ImGuiHelpBox.GetHeight(displayError, width, MessageType.Error));
        }


        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
