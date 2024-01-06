using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BelowButtonAttribute))]
    public class BelowButtonAttributeDrawer: DecButtonAttributeDrawer
    {

        #region IMGUI
        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute) => EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: ImGuiHelpBox.GetHeight(DisplayError, width, MessageType.Error));


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute);

            if (DisplayError != "")
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, DisplayError, MessageType.Error);
            }

            return leftRect;
        }
        #endregion

        #region UIToolkit

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
            visualElement.Add(DrawUIToolkit(property, saintsAttribute, index, parent, container));
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));
            return visualElement;
        }
        #endregion
    }
}
