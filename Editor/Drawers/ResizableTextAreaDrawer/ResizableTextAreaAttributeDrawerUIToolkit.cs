#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {

        // private static string NameLabelPlaceholder(SerializedProperty property) =>
        //     $"{property.propertyPath}__ResizableTextArea_LabelPlaceholder";

        // private static string NameResizable(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";
        // private static string NameTextArea(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            ResizableTextAreaAttribute resizableTextAreaAttribute = saintsAttribute as ResizableTextAreaAttribute
                                                                    ?? new ResizableTextAreaAttribute();

            ResizableTextField result = new ResizableTextField(GetPreferredLabel(property), GetMinHeight(resizableTextAreaAttribute))
            {
                bindingPath = property.propertyPath,
            };
            result.AddToClassList(ClassAllowDisable);
            if (resizableTextAreaAttribute.Inline)
            {
                result.AddToClassList(ResizableTextField.alignedFieldUssClassName);
            }
            else
            {
                result.ToHorizontal();
            }

            // no need for this:
            // UIToolkitUtils.AddContextualMenuManipulator(r.labelElement, property, () => Util.PropertyChangedCallback(property, info, null));

            return result;
        }

        private static float GetMinHeight(ResizableTextAreaAttribute resizableTextAreaAttribute) =>
            resizableTextAreaAttribute.MinRow > 1
                ? resizableTextAreaAttribute.MinRow * 15
                : 0;
    }
}
#endif
