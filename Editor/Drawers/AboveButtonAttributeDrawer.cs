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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            visualElement.Add(DrawUIToolkit(property, saintsAttribute, index, info, parent, container));
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));

            visualElement.AddToClassList(ClassAllowDisable);
            return visualElement;
        }

        #endregion

#endif
    }
}
