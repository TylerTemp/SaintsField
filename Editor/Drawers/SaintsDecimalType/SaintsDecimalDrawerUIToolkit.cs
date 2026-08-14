#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public partial class SaintsDecimalDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static void AddContextualMenuManipulator(SaintsDecimalFieldAbs field, SerializedProperty property)
        {
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});

            field.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction($"Copy \"{field.value}\"", _ =>
                {
                    EditorGUIUtility.systemCopyBuffer = $"{field.value}";
                });

                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (decimal.TryParse(clipboardText, out decimal value))
                {
                    evt.menu.AppendAction($"Paste \"{clipboardText}\"", _ =>
                    {
                        field.value = value;
                    });
                }
            }));
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsDecimalField field = new SaintsDecimalField(GetPreferredLabel(property));

            field.AddToClassList(SaintsDecimalField.alignedFieldUssClassName);
            if (!string.IsNullOrEmpty(property.tooltip) && field.DecimalTextField.labelElement != null)
            {
                field.DecimalTextField.labelElement.tooltip = property.tooltip;
            }
            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(field);
            return emptyPrefabOverrideElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsDecimalField field = container.Q<SaintsDecimalField>();
            // int propIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            field.ManuallyBindProperty(property, newValue =>
            {
                string error = UpdateCachedDecimalValue(property, info, newValue);
                if (error != "")
                {
                    Debug.LogError(error);
                }
            });

            AddContextualMenuManipulator(field, property);
        }
    }
}
#endif
