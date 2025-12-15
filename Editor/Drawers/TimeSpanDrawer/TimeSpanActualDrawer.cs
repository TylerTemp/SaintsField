using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
    public partial class TimeSpanAttributeDrawer: ISaintsSerializedActualDrawer
    {
        public static VisualElement RenderSerializedActual(string label, SerializedProperty property, IReadOnlyList<Attribute> allAttributes, bool inHorizontal)
        {
            VisualElement r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue)), label, allAttributes.Any(each => each is DefaultExpandAttribute));
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(TimeSpanField.alignedFieldUssClassName);
            }

            return r;
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
            container.TrackPropertyValue(prop, _ => onValueChangedCallback.Invoke(new TimeSpan(prop.longValue)));
        }
    }
}
