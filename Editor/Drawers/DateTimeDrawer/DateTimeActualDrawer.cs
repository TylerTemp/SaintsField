using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public partial class DateTimeAttributeDrawer: ISaintsSerializedActualDrawer
    {
        public static DateTimeField RenderSerializedActual(ISaintsAttribute dateTimeAttribute, string label, SerializedProperty property, bool inHorizontal)
        {
            DateTimeField r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue)), label);
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(DateTimeField.alignedFieldUssClassName);
            }

            return r;
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
            container.TrackPropertyValue(prop, _ => onValueChangedCallback.Invoke(new DateTime(prop.longValue)));
        }
    }
}
