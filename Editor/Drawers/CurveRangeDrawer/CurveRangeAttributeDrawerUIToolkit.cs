#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.CurveRangeDrawer
{
    public partial class CurveRangeAttributeDrawer
    {
        private static string NameCurveField(SerializedProperty property) => $"{property.propertyPath}__CurveRange";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            CurveRangeAttribute curveRangeAttribute = (CurveRangeAttribute)saintsAttribute;
            CurveField createFieldElement = new CurveField(GetPreferredLabel(property))
            {
                value = property.animationCurveValue,
                ranges = GetRanges(curveRangeAttribute),
                name = NameCurveField(property),
            };

            Type type = typeof(CurveField);
            FieldInfo colorFieldInfo = type.GetField("m_CurveColor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (colorFieldInfo != null)
            {
                colorFieldInfo.SetValue(createFieldElement, curveRangeAttribute.Color.GetColor());
            }

            createFieldElement.AddToClassList(ClassAllowDisable);
            createFieldElement.AddToClassList("unity-base-field__aligned");
            createFieldElement.BindProperty(property);

            return createFieldElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<CurveField>(NameCurveField(property)).RegisterValueChangedCallback(v =>
            {
                // property.animationCurveValue = v.newValue;
                // property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(v.newValue);
            });
        }
    }
}
#endif
