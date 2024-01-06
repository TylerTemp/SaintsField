using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AboveButtonAttribute))]
    public class AboveButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute) => EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: ImGuiHelpBox.GetHeight(DisplayError, width, MessageType.Error));


        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute);

            if (DisplayError != "")
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, DisplayError, MessageType.Error);
            }

            return leftRect;
        }

        #region UIToolkit

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            visualElement.Add(DrawUIToolkit(property, saintsAttribute, index, parent, container));
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));
            return visualElement;
        }

        #endregion
    }
}
