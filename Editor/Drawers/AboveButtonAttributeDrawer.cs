using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AboveButtonAttribute))]
    public class AboveButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute) => EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: HelpBox.GetHeight(DisplayError, width, MessageType.Error));


        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute);

            if (DisplayError != "")
            {
                leftRect = HelpBox.Draw(leftRect, DisplayError, MessageType.Error);
            }

            return leftRect;
        }
    }
}
