using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Drawers.GuidDrawer;
using SaintsField.Editor.Drawers.SaintsDecimalType;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsSerializedActualDrawerDrawer
{
    public partial class SaintsSerializedActualDrawerAttributeDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            if (DateTimeAttributeDrawer.IsSerializedActualDateTime(property))
            {
                return DateTimeAttributeDrawer.GetSerializedActualFieldHeight(property, label);
            }

            if (TimeSpanAttributeDrawer.IsSerializedActualTimeSpan(property))
            {
                return TimeSpanAttributeDrawer.GetSerializedActualFieldHeight(property, label, index, info);
            }

            if (GuidAttributeDrawer.IsSerializedActualGuid(property))
            {
                return GuidAttributeDrawer.GetSerializedActualFieldHeight(property, label);
            }

            if (SaintsDecimalDrawer.IsSerializedActualDecimal(property))
            {
                return SaintsDecimalDrawer.GetSerializedActualFieldHeight(property, label);
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            if (DateTimeAttributeDrawer.IsSerializedActualDateTime(property) &&
                DateTimeAttributeDrawer.DrawSerializedActualField(position, property, label,
                    newValue => TriggerChangedIMGUI(property, newValue)))
            {
                return;
            }

            if (TimeSpanAttributeDrawer.IsSerializedActualTimeSpan(property) &&
                TimeSpanAttributeDrawer.DrawSerializedActualField(position, property, label, index, info,
                    newValue => TriggerChangedIMGUI(property, newValue)))
            {
                return;
            }

            if (GuidAttributeDrawer.IsSerializedActualGuid(property) &&
                GuidAttributeDrawer.DrawSerializedActualField(position, property, label, index,
                    newValue => TriggerChangedIMGUI(property, newValue)))
            {
                return;
            }

            if (SaintsDecimalDrawer.IsSerializedActualDecimal(property) &&
                SaintsDecimalDrawer.DrawSerializedActualField(position, property, label,
                    newValue => TriggerChangedIMGUI(property, newValue)))
            {
                return;
            }

            RawDefaultDrawer(position, property, allAttributes, label, info);
        }
    }
}
