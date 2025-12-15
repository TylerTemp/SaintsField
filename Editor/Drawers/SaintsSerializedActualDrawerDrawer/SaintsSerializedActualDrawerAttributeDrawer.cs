using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer;
using SaintsField.Editor.Drawers.GuidDrawer;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saintsfield.Editor.Drawers.SaintsSerializedActualDrawerDrawer
{
    [CustomPropertyDrawer(typeof(SaintsSerializedActualDrawerAttribute), false)]
    public class SaintsSerializedActualDrawerAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private ISaintsSerializedActualDrawer _actualDrawer;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            string label = GetPreferredLabel(property);
            if (!string.IsNullOrEmpty(label) && label.EndsWith("__Saints Serialized__"))
            {
                label = label[..^"__Saints Serialized__".Length];
            }
            SaintsSerializedActualAttribute saintsSerializedActual = ReflectCache.GetCustomAttributes<SaintsSerializedActualAttribute>(info).First();

            // return new Label(GetPreferredLabel(property));
            (VisualElement element, ISaintsSerializedActualDrawer actualDrawer) = RenderSerializedActual(
                saintsSerializedActual,
                label,
                property,
                info,
                InHorizontalLayout,
                parent
            );
            _actualDrawer = actualDrawer;
            return element ?? UnityFallbackUIToolkit(info, property, allAttributes.Where(each => each is not SaintsSerializedActualDrawerAttribute).ToArray(), container, label, SaintsPropertyDrawers, parent);
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _actualDrawer?.OnAwakeActualDrawer(property, saintsAttribute, index, allAttributes, container, onValueChangedCallback, info, parent);
        }

        private (VisualElement element, ISaintsSerializedActualDrawer actualDrawer) RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual,
            string label, SerializedProperty property, FieldInfo serInfo, bool inHorizontalLayout, object parent)
        {
            // Debug.Log(property.propertyPath);
            Attribute[] attributes = ReflectCache.GetCustomAttributes(serInfo);

            EnumToggleButtonsAttribute enumToggle = null;
            FlagsTreeDropdownAttribute flagsTreeDropdownAttribute = null;
            FlagsDropdownAttribute flagsDropdownAttribute = null;
            DateTimeAttribute dateTimeAttribute = null;
            TimeSpanAttribute timeSpanAttribute = null;
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
                    case DateTimeAttribute dt:
                        dateTimeAttribute = dt;
                        break;
                    case TimeSpanAttribute ts:
                        timeSpanAttribute = ts;
                        break;
                }
            }

            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
#endif
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                    Debug.Log($"saintsrow serInfo={serInfo.Name} attrs = {string.Join(", ", attributes.Select(a => a.GetType().Name))}");
#endif

                    if (enumToggle != null)
                    {
                        EnumToggleButtonsAttributeDrawer enumToggleButtonsAttributeDrawer = (EnumToggleButtonsAttributeDrawer)MakePropertyDrawer(typeof(EnumToggleButtonsAttributeDrawer), serInfo, enumToggle, label);
                        // return EnumToggleButtonsAttributeDrawer.RenderSerializedActual(saintsSerializedActual, enumToggle, label, property, serInfo, parent, this);
                        VisualElement element = enumToggleButtonsAttributeDrawer.RenderSerializedActual(saintsSerializedActual,
                            enumToggle,
                            label, property, serInfo, parent, this);
                        return (element, element == null? null: enumToggleButtonsAttributeDrawer);
                    }

                    TreeDropdownAttributeDrawer treeDropdownAttributeDrawer =
                        (TreeDropdownAttributeDrawer)MakePropertyDrawer(typeof(TreeDropdownAttributeDrawer), serInfo, (Attribute)flagsTreeDropdownAttribute ?? flagsDropdownAttribute, label);
                    VisualElement treeElement = treeDropdownAttributeDrawer.RenderSerializedActual(saintsSerializedActual,
                        (ISaintsAttribute)flagsTreeDropdownAttribute ?? flagsDropdownAttribute, label, property,
                        parent);
                    return (treeElement, treeElement == null? null: treeDropdownAttributeDrawer);
                }
                case SaintsPropertyType.Interface:
                {
                    // SaintsInterfaceDrawer drawer = (SaintsInterfaceDrawer)MakePropertyDrawer(typeof(SaintsInterfaceDrawer), serInfo, null, label);
                    // VisualElement element = drawer.RenderSerializedActual(saintsSerializedActual,
                    //     label, property, attributes, inHorizontalLayout, serInfo, parent);
                    return (SaintsInterfaceDrawer.RenderSerializedActual(saintsSerializedActual,
                        label, property, attributes, inHorizontalLayout, serInfo, parent), null);
                }
                case SaintsPropertyType.DateTime:
                {
                    VisualElement element = DateTimeAttributeDrawer.RenderSerializedActual(dateTimeAttribute, label, property, inHorizontalLayout);
                    return (element, element == null? null: (DateTimeAttributeDrawer)MakePropertyDrawer(typeof(DateTimeAttributeDrawer), serInfo, dateTimeAttribute, label));
                    // return DateTimeAttributeDrawer.RenderSerializedActual(dateTimeAttribute, label, property, inHorizontalLayout);
                }
                case SaintsPropertyType.TimeSpan:
                {
                    VisualElement element = TimeSpanAttributeDrawer.RenderSerializedActual(label, property, attributes, inHorizontalLayout);
                    return (element, element == null? null: (TimeSpanAttributeDrawer)MakePropertyDrawer(typeof(TimeSpanAttributeDrawer), serInfo, timeSpanAttribute, label));
                    // return TimeSpanAttributeDrawer.RenderSerializedActual(timeSpanAttribute, label, property, attributes, inHorizontalLayout);
                }

                case SaintsPropertyType.Guid:
                {
                    VisualElement element = GuidAttributeDrawer.RenderSerializedActual(label, property, inHorizontalLayout);
                    return (element, element == null? null: (GuidAttributeDrawer)MakePropertyDrawer(typeof(GuidAttributeDrawer), serInfo, null, label));
                    // return GuidAttributeDrawer.RenderSerializedActual(label, property, inHorizontalLayout);
                }
                // case SaintsPropertyType.Undefined:
                // case SaintsPropertyType.ClassOrStruct:
                default:
                    return (new HelpBox($"{propertyType} is not supported", HelpBoxMessageType.Error), null);
            }
            // return (new HelpBox($"{propertyType} is not supported", HelpBoxMessageType.Error), null);
        }

    }
}
