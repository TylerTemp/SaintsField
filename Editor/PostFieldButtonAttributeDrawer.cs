using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{

    [CustomPropertyDrawer(typeof(PostFieldButtonAttribute))]
    public class PostFieldButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            object target = property.serializedObject.targetObject;
            string labelXml = GetButtonLabelXml((DecButtonAttribute)saintsAttribute, target, target.GetType());
            return Mathf.Min(position.width, Mathf.Max(10, RichTextDrawer.GetWidth(label, position.height, RichTextDrawer.ParseRichXml(labelXml, label.text))));
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            // Debug.Log($"draw below {position}");
            // return Draw(position, property, label, saintsAttribute);
            // float width = GetPostFieldWidth(position, property, label, saintsAttribute);
            // (Rect useRect, Rect leftRect) = RectUtils.SplitWidthRect(position, width);
            Draw(position, property, label, saintsAttribute);
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return DisplayError != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return DisplayError == "" ? 0 : HelpBox.GetHeight(DisplayError, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            DisplayError == ""
                ? position
                : HelpBox.Draw(position, DisplayError, MessageType.Error);
    }
}
