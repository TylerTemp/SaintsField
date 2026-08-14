#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LeftToggleDrawer
{
    public partial class LeftToggleAttributeDrawer
    {

        private static string NameLeftToggle(SerializedProperty property) => $"{property.propertyPath}__LeftToggle";
        // private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__LeftToggle_Label";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // string label = GetPreferredLabel(property);
            // Debug.Log(label);
            LeftToggleField toggle = new LeftToggleField(GetPreferredLabel(property))
            {
                name = NameLeftToggle(property),
                // style =
                // {
                //     flexDirection = FlexDirection.RowReverse,
                //     justifyContent = Justify.FlexEnd,
                // },
                bindingPath = property.propertyPath,
            };

            // toggle.styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/LeftToggle.uss"));
            toggle.AddToClassList(ClassAllowDisable);
            if (!string.IsNullOrEmpty(property.tooltip) && toggle.labelElement != null)
            {
                toggle.labelElement.tooltip = property.tooltip;
            }

            return toggle;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<LeftToggleField>(NameLeftToggle(property)).RegisterValueChangedCallback(evt =>
            {
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, evt.newValue);
                onValueChangedCallback.Invoke(evt.newValue);
            });
        }

        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, string labelOrNull, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried,
        //     RichTextDrawer richTextDrawer)
        // {
        //     bool noLabel = string.IsNullOrEmpty(labelOrNull);
        //     if (noLabel)
        //     {
        //
        //     }
        //     else
        //     {
        //
        //     }
        // }
    }
}
#endif
