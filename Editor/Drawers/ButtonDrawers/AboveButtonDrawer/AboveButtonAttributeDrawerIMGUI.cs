using System.Reflection;
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
            UpdateButtonLabelIMGUI(property, saintsAttribute, index, info, parent);
            return GetButtonHeightIMGUI() + GetResultHeightIMGUI(property, index, width);
        }


        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute, index, info, parent);
            return DrawResultIMGUI(leftRect, property, index);
        }
    }
}
