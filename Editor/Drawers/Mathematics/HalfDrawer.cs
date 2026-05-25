#if UNITY_2021_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.MathematicsHalfUShort;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Mathematics
{
    [CustomPropertyDrawer(typeof(half))]
    public class HalfDrawer: SaintsPropertyDrawer
    {
        private static string FieldName(SerializedProperty prop) => $"saints-field-mathematics-half--{SerializedUtils.GetUniqueId(prop)}";

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(nameof(half.value));
            MathematicsHalfUShortField field = new MathematicsHalfUShortField(GetPreferredLabel(property))
            {
                bindingPath = prop.propertyPath,
                name = FieldName(property),
            };

            field.AddToClassList(MathematicsHalfUShortField.alignedFieldUssClassName);
            // field.BindProperty(prop);
            return field;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            MathematicsHalfUShortField field = container.Q<MathematicsHalfUShortField>(name: FieldName(property));
            field.RegisterValueChangedCallback(evt => onValueChangedCallback.Invoke(new half { value = (ushort)evt.newValue }));
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});
        }
    }
}
#endif
