#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.MathematicsHalfUShort;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Mathematics
{
    [CustomPropertyDrawer(typeof(half3))]
    public class Half3Drawer: SaintsPropertyDrawer
    {
        private static string FieldName(SerializedProperty prop) => $"saints-field-mathematics-half3--{SerializedUtils.GetUniqueId(prop)}";

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            MultiHalfsField field = new MultiHalfsField(GetPreferredLabel(property), 3)
            {
                name = FieldName(property),
                value = new[]
                {
                    property.FindPropertyRelative(nameof(half3.x)).FindPropertyRelative(nameof(half.value)).intValue,
                    property.FindPropertyRelative(nameof(half3.y)).FindPropertyRelative(nameof(half.value)).intValue,
                    property.FindPropertyRelative(nameof(half3.z)).FindPropertyRelative(nameof(half.value)).intValue,
                },
            };

            field.AddToClassList(MathematicsHalfUShortField.alignedFieldUssClassName);
            if (!string.IsNullOrEmpty(property.tooltip) && field.labelElement != null)
            {
                field.labelElement.tooltip = property.tooltip;
            }
            // field.BindProperty(prop);
            return field;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            MultiHalfsField field = container.Q<MultiHalfsField>(name: FieldName(property));
            field.RegisterValueChangedCallback(evt =>
            {
                int[] newValue = evt.newValue;
                bool changed = false;

                SerializedProperty prop1 = property.FindPropertyRelative(nameof(half3.x)).FindPropertyRelative(nameof(half.value));
                if(prop1.intValue != newValue[0])
                {
                    changed = true;
                    prop1.intValue = newValue[0];
                }

                SerializedProperty prop2 = property.FindPropertyRelative(nameof(half3.y)).FindPropertyRelative(nameof(half.value));
                if (prop2.intValue != newValue[1])
                {
                    changed = true;
                    prop2.intValue = newValue[1];
                }

                SerializedProperty prop3 = property.FindPropertyRelative(nameof(half3.z)).FindPropertyRelative(nameof(half.value));
                if (prop3.intValue != newValue[2])
                {
                    changed = true;
                    prop3.intValue = newValue[2];
                }

                if (!changed)
                {
                    return;
                }

                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(new half3
                {
                    x = new half{value = (ushort)newValue[0]},
                    y = new half{value = (ushort)newValue[1]},
                    z = new half{value = (ushort)newValue[2]},
                });
            });
            field.TrackPropertyValue(property, p =>
            {
                int[] newValue = {
                    property.FindPropertyRelative(nameof(half3.x)).FindPropertyRelative(nameof(half.value)).intValue,
                    property.FindPropertyRelative(nameof(half3.y)).FindPropertyRelative(nameof(half.value)).intValue,
                    property.FindPropertyRelative(nameof(half3.z)).FindPropertyRelative(nameof(half.value)).intValue,
                };
                field.SetValueWithoutNotify(newValue);
                // Debug.Log("changed");
            });
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});
        }
    }
}
#endif
