using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer;
using SaintsField.Editor.Drawers.GuidDrawer;
using SaintsField.Editor.Drawers.SaintsDecimalType;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsSerializedActualDrawerDrawer
{
    public partial class SaintsSerializedActualDrawerAttributeDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        private EnumToggleButtonsAttributeDrawer _enumToggleButtonsAttributeDrawer;
        private TreeDropdownAttributeDrawer _treeDropdownAttributeDrawer;
        private SaintsInterfaceDrawer _saintsInterfaceDrawer;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            GUIContent actualLabel = GetActualLabel(property, label);
            SaintsSerializedActualAttribute saintsSerializedActual = GetSerializedActualAttribute(info);
            if (saintsSerializedActual == null)
            {
                return ErrorHeight($"{nameof(SaintsSerializedActualAttribute)} not found", width);
            }

            return GetSerializedActualFieldHeight(saintsSerializedActual, property, actualLabel, width, index, info,
                parent);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            GUIContent actualLabel = GetActualLabel(property, label);
            SaintsSerializedActualAttribute saintsSerializedActual = GetSerializedActualAttribute(info);
            if (saintsSerializedActual == null)
            {
                DrawError(position, $"{nameof(SaintsSerializedActualAttribute)} not found");
                DrawOverrideRichText(position, actualLabel, overrideRichTextChunks);
                return;
            }

            DrawSerializedActualField(position, saintsSerializedActual, property, actualLabel, info, parent);
            DrawOverrideRichText(position, actualLabel, overrideRichTextChunks);
        }

        private float GetSerializedActualFieldHeight(SaintsSerializedActualAttribute saintsSerializedActual,
            SerializedProperty property, GUIContent label, float width, int index, FieldInfo info, object parent)
        {
            (string error, SaintsPropertyType propertyType) = GetPropertyType(property);
            if (error != "")
            {
                return ErrorHeight(error, width);
            }

            Attribute[] attributes = ReflectCache.GetCustomAttributes(info);
            (EnumToggleButtonsAttribute enumToggle, FlagsTreeDropdownAttribute flagsTreeDropdownAttribute,
                FlagsDropdownAttribute flagsDropdownAttribute) = GetAttributeSpecificDrawers(attributes);

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
#endif
                    if (enumToggle != null)
                    {
                        return GetEnumToggleButtonsDrawer(info, enumToggle, label.text)
                            .GetSerializedActualFieldHeight(saintsSerializedActual, enumToggle, property, label, width,
                                parent, this);
                    }

                    return GetTreeDropdownDrawer(info,
                            (Attribute)flagsTreeDropdownAttribute ?? flagsDropdownAttribute, label.text)
                        .GetSerializedActualFieldHeight(saintsSerializedActual, property, label, width, parent);
                case SaintsPropertyType.Interface:
                    return GetSaintsInterfaceDrawer(info, label.text)
                        .GetSerializedActualFieldHeight(saintsSerializedActual, property, label, width, info, parent);
                case SaintsPropertyType.DateTime:
                    return DateTimeAttributeDrawer.GetSerializedActualFieldHeight(property, label);
                case SaintsPropertyType.TimeSpan:
                    return TimeSpanAttributeDrawer.GetSerializedActualFieldHeight(property, label, index, info);
                case SaintsPropertyType.Guid:
                    return GuidAttributeDrawer.GetSerializedActualFieldHeight(property, label);
                case SaintsPropertyType.Decimal:
                    return SaintsDecimalDrawer.GetSerializedActualFieldHeight(property, label);
                default:
                    return ErrorHeight($"{propertyType} is not supported", width);
            }
        }

        private void DrawSerializedActualField(Rect position, SaintsSerializedActualAttribute saintsSerializedActual,
            SerializedProperty property, GUIContent label, FieldInfo info, object parent)
        {
            (string error, SaintsPropertyType propertyType) = GetPropertyType(property);
            if (error != "")
            {
                DrawError(position, error);
                return;
            }

            Attribute[] attributes = ReflectCache.GetCustomAttributes(info);
            (EnumToggleButtonsAttribute enumToggle, FlagsTreeDropdownAttribute flagsTreeDropdownAttribute,
                FlagsDropdownAttribute flagsDropdownAttribute) = GetAttributeSpecificDrawers(attributes);

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
#endif
                    if (enumToggle != null)
                    {
                        GetEnumToggleButtonsDrawer(info, enumToggle, label.text)
                            .DrawSerializedActualField(position, saintsSerializedActual, enumToggle, property, label,
                                parent, this, newValue => TriggerChangedIMGUI(property, newValue));
                        return;
                    }

                    GetTreeDropdownDrawer(info,
                            (Attribute)flagsTreeDropdownAttribute ?? flagsDropdownAttribute, label.text)
                        .DrawSerializedActualField(position, saintsSerializedActual, property, label, parent, this,
                            newValue => TriggerChangedIMGUI(property, newValue));
                    return;
                case SaintsPropertyType.Interface:
                    GetSaintsInterfaceDrawer(info, label.text)
                        .DrawSerializedActualField(position, saintsSerializedActual, property, label, info, parent);
                    return;
                case SaintsPropertyType.DateTime:
                    if (DateTimeAttributeDrawer.DrawSerializedActualField(position, property, label,
                            newValue => TriggerChangedIMGUI(property, newValue)))
                    {
                        return;
                    }

                    break;
                case SaintsPropertyType.TimeSpan:
                    if (TimeSpanAttributeDrawer.DrawSerializedActualField(position, property, label, info,
                            newValue => TriggerChangedIMGUI(property, newValue)))
                    {
                        return;
                    }

                    break;
                case SaintsPropertyType.Guid:
                    if (GuidAttributeDrawer.DrawSerializedActualField(position, property, label,
                            newValue => TriggerChangedIMGUI(property, newValue)))
                    {
                        return;
                    }

                    break;
                case SaintsPropertyType.Decimal:
                    if (SaintsDecimalDrawer.DrawSerializedActualField(position, property, label,
                            newValue => TriggerChangedIMGUI(property, newValue)))
                    {
                        return;
                    }

                    break;
                default:
                    DrawError(position, $"{propertyType} is not supported");
                    return;
            }

            DrawError(position, $"{propertyType} serialized actual value is invalid");
        }

        private static SaintsSerializedActualAttribute GetSerializedActualAttribute(FieldInfo info) =>
            ReflectCache.GetCustomAttributes<SaintsSerializedActualAttribute>(info).FirstOrDefault();

        private GUIContent GetActualLabel(SerializedProperty property, GUIContent label)
        {
            string actualLabel = GetPreferredLabel(property);
            if (string.IsNullOrEmpty(actualLabel))
            {
                actualLabel = label?.text ?? "";
            }

            if (!string.IsNullOrEmpty(actualLabel) && actualLabel.EndsWith(Util.SaintsSerializedLabelSuffix))
            {
                actualLabel = actualLabel[..^Util.SaintsSerializedLabelSuffix.Length];
            }

            return new GUIContent(label)
            {
                text = actualLabel,
            };
        }

        private static (string error, SaintsPropertyType propertyType) GetPropertyType(SerializedProperty property)
        {
            SerializedProperty propertyTypeProperty =
                property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType));
            if (propertyTypeProperty == null)
            {
                return ($"propertyType not found in {property.propertyPath}", SaintsPropertyType.Undefined);
            }

            return ("", (SaintsPropertyType)propertyTypeProperty.intValue);
        }

        private static (EnumToggleButtonsAttribute enumToggle, FlagsTreeDropdownAttribute flagsTreeDropdownAttribute,
            FlagsDropdownAttribute flagsDropdownAttribute) GetAttributeSpecificDrawers(
                IReadOnlyList<Attribute> attributes)
        {
            EnumToggleButtonsAttribute enumToggle = null;
            FlagsTreeDropdownAttribute flagsTreeDropdownAttribute = null;
            FlagsDropdownAttribute flagsDropdownAttribute = null;
            foreach (Attribute attr in attributes)
            {
                switch (attr)
                {
                    case EnumToggleButtonsAttribute et:
                        enumToggle = et;
                        break;
                    case FlagsTreeDropdownAttribute ftd:
                        flagsTreeDropdownAttribute = ftd;
                        break;
                    case FlagsDropdownAttribute fd:
                        flagsDropdownAttribute = fd;
                        break;
                }
            }

            return (enumToggle, flagsTreeDropdownAttribute, flagsDropdownAttribute);
        }

        private EnumToggleButtonsAttributeDrawer GetEnumToggleButtonsDrawer(FieldInfo info,
            EnumToggleButtonsAttribute enumToggleButtonsAttribute, string label)
        {
            return _enumToggleButtonsAttributeDrawer ??=
                (EnumToggleButtonsAttributeDrawer)MakePropertyDrawer(typeof(EnumToggleButtonsAttributeDrawer), info,
                    enumToggleButtonsAttribute, label);
        }

        private TreeDropdownAttributeDrawer GetTreeDropdownDrawer(FieldInfo info, Attribute attribute, string label)
        {
            return _treeDropdownAttributeDrawer ??=
                (TreeDropdownAttributeDrawer)MakePropertyDrawer(typeof(TreeDropdownAttributeDrawer), info, attribute,
                    label);
        }

        private SaintsInterfaceDrawer GetSaintsInterfaceDrawer(FieldInfo info, string label)
        {
            return _saintsInterfaceDrawer ??=
                (SaintsInterfaceDrawer)MakePropertyDrawer(typeof(SaintsInterfaceDrawer), info, null, label);
        }

        private static float ErrorHeight(string error, float width) =>
            ImGuiHelpBox.GetHeight(error, Mathf.Max(1f, width), MessageType.Error);

        private static void DrawError(Rect position, string error)
        {
            ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
