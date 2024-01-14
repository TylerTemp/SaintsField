using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{

    [CustomPropertyDrawer(typeof(PostFieldButtonAttribute))]
    public class PostFieldButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        private const float PaddingWidth = 3f;

        #region IMGUI

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            object target = property.serializedObject.targetObject;
            (string error, string labelXml) = GetButtonLabelXml((DecButtonAttribute)saintsAttribute, target, target.GetType());
            _error = error;
            return PaddingWidth*2 + Mathf.Min(position.width, Mathf.Max(10, RichTextDrawer.GetWidth(label, position.height, RichTextDrawer.ParseRichXml(labelXml, label.text))));
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            // Debug.Log($"draw below {position}");
            // return Draw(position, property, label, saintsAttribute);
            // float width = GetPostFieldWidth(position, property, label, saintsAttribute);
            // (Rect useRect, Rect leftRect) = RectUtils.SplitWidthRect(position, width);
            Draw(position, property, label, saintsAttribute);
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return DisplayError != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return DisplayError == "" ? 0 : ImGuiHelpBox.GetHeight(DisplayError, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            DisplayError == ""
                ? position
                : ImGuiHelpBox.Draw(position, DisplayError, MessageType.Error);
        #endregion

        #region UIToolkit

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            VisualElement element = DrawUIToolkit(property, saintsAttribute, index, parent, container);
            element.style.flexGrow = StyleKeyword.Null;
            return element;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));
            return visualElement;
        }

        #endregion
    }
}
