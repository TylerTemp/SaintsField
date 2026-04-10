using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public partial class SaintsDecimalDrawer: ISaintsSerializedActualDrawer
    {
        public static VisualElement RenderSerializedActual(string label, SerializedProperty property, bool inHorizontalLayout)
        {
            SaintsDecimalFieldActual field = new SaintsDecimalFieldActual(label);
            if (inHorizontalLayout)
            {
                field.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                field.DecimalTextField.AddToClassList(DecimalTextField.alignedFieldUssClassName);
            }
            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(field);
            return emptyPrefabOverrideElement;
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsDecimalFieldActual field = container.Q<SaintsDecimalFieldActual>();
            field.ManuallyBindProperty(property, newValue =>
            {
                onValueChangedCallback.Invoke(newValue);
                // object newParent = SerializedUtils.GetAttributesAndDirectParent<Attribute>(property).parent;
                // (string error, int _, object thisData) = Util.GetValue(property, info, newParent);
                // if (!string.IsNullOrEmpty(error))
                // {
                //     Debug.LogError(error);
                //     return;
                // }
                // Debug.Log($"{newValue}/{thisData}");
                // FieldInfo valueField = typeof(SaintsDecimal).GetField(nameof(SaintsDecimal.value), BindingFlags.Public | BindingFlags.Instance);
                // Debug.Log(valueField);
                // decimal curCachedValue = (decimal)valueField!.GetValue(thisData);
                // if(curCachedValue != newValue)
                // {
                //     valueField!.SetValue(thisData, newValue);
                //     FieldInfo cacheField = typeof(SaintsDecimal).GetField(nameof(SaintsDecimal.cached),
                //         BindingFlags.Public | BindingFlags.Instance);
                //     cacheField!.SetValue(thisData, true);
                // }
            });

            AddContextualMenuManipulator(field, property);
        }


    }
}
