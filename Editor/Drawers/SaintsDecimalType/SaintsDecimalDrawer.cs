using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsDecimal), true)]
    public partial class SaintsDecimalDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsDecimalField field = new SaintsDecimalField(GetPreferredLabel(property));
            field.DecimalTextField.AddToClassList(DecimalTextField.alignedFieldUssClassName);
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
                object newParent = SerializedUtils.GetAttributesAndDirectParent<Attribute>(property).parent;
                (string error, int _, object thisData) = Util.GetValue(property, info, newParent);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                    return;
                }
                // Debug.Log($"{newValue}/{thisData}");
                FieldInfo valueField = typeof(SaintsDecimal).GetField(nameof(SaintsDecimal.value), BindingFlags.Public | BindingFlags.Instance);
                // Debug.Log(valueField);
                decimal curCachedValue = (decimal)valueField!.GetValue(thisData);
                if(curCachedValue != newValue)
                {
                    valueField!.SetValue(thisData, newValue);
                    FieldInfo cacheField = typeof(SaintsDecimal).GetField(nameof(SaintsDecimal.cached),
                        BindingFlags.Public | BindingFlags.Instance);
                    cacheField!.SetValue(thisData, true);
                }
            });

            AddContextualMenuManipulator(field, property);
        }

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
    }
}
