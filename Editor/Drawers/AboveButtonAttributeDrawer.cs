using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AboveButtonAttribute))]
    public class AboveButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: ImGuiHelpBox.GetHeight(DisplayError, width, MessageType.Error));


        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute, parent);

            if (DisplayError != "")
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, DisplayError, MessageType.Error);
            }

            return leftRect;
        }

#if UNITY_2021_3_OR_NEWER

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

#endif
    }
}
