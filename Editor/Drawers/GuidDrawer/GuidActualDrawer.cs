using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.GuidDrawer
{
    public partial class GuidAttributeDrawer: ISaintsSerializedActualDrawer
    {
        public static VisualElement RenderSerializedActual(string label, SerializedProperty property, bool inHorizontal)
        {
            VisualElement r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.stringValue)), label);
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(GuidStringField.alignedFieldUssClassName);
            }

            UIToolkitUtils.AddContextualMenuManipulator(r, property, () => {});

            return r;
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(nameof(SaintsSerializedProperty.stringValue));
            container.TrackPropertyValue(prop, _ =>
            {
                if (Guid.TryParse(prop.stringValue, out Guid fineGuid))
                {
                    onValueChangedCallback.Invoke(fineGuid);
                }
            });
        }
    }
}
