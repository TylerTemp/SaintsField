using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
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
    public class SaintsDecimalDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsDecimalField field = new SaintsDecimalField(GetPreferredLabel(property));
            field.DecimalTextField.AddToClassList(DecimalTextField.alignedFieldUssClassName);
            return field;
            // SaintsDecimalElement element = new SaintsDecimalElement();
            // SaintsDecimalField field = new SaintsDecimalField(GetPreferredLabel(property), element);
            // field.AddToClassList(SaintsDecimalField.alignedFieldUssClassName);
            // return field;
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
        }
    }
}
