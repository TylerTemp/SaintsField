#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer: ISaintsSerializedPropertyDrawer
    {
        public static VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual, ISaintsAttribute enumToggle, string label, SerializedProperty property, MemberInfo info, object parent)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                return new HelpBox($"Failed to get type for {property.propertyPath}", HelpBoxMessageType.Error);
            }

            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
                {
                    EnumMetaInfo enumMetaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    SerializedProperty subProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                    ButtonsULongElement ele = new ButtonsULongElement(enumMetaInfo, property, info, parent, newValue =>
                    {
                        subProp.longValue = Convert.ToInt64(newValue);
                        subProp.serializedObject.ApplyModifiedProperties();
                    });
                    ele.AddToClassList(ClassAllowDisable);
                    ele.BindProperty(subProp);

                    UnityEvent<bool> expandEvent = new UnityEvent<bool>();
                    ExpandableButtonsElement wrap = new ExpandableButtonsElement(ele, expandEvent);

                    ButtonsULongField r = new ButtonsULongField(label, wrap);
                    r.AddToClassList(DropdownFieldULong.alignedFieldUssClassName);

                    UIToolkitUtils.AddContextualMenuManipulator(r, subProp, () => { });

                    VisualElement root = new VisualElement();
                    root.Add(r);

                    ButtonsULongElement under = new ButtonsULongElement(enumMetaInfo, property, info, parent, newValue =>
                    {
                        subProp.longValue = Convert.ToInt64(newValue);
                        subProp.serializedObject.ApplyModifiedProperties();
                    })
                    {
                        style =
                        {
                            display = DisplayStyle.None,
                        },
                    };
                    under.hToggleButton.style.display = DisplayStyle.None;
                    under.AddToClassList(ClassAllowDisable);
                    under.BindProperty(subProp);
                    root.Add(under);

                    bool expanded = false;
                    ele.fillEmptyButton.clicked += OnExpandedChanged;
                    wrap.ExpandButton.clicked += OnExpandedChanged;

                    return root;

                    void OnExpandedChanged()
                    {
                        expandEvent.Invoke(expanded = !expanded);
                        under.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        ele.hToggleButton.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                        ele.hCheckAllButton.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        ele.hEmptyButton.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        foreach (Button button in ele.ToggleButtons)
                        {
                            button.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                        }
                    }
                }
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                {
                    EnumMetaInfo enumMetaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    SerializedProperty subProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                    ButtonsULongElement ele = new ButtonsULongElement(enumMetaInfo, property, info, parent, newValue =>
                    {
                        subProp.ulongValue = Convert.ToUInt64(newValue);
                        subProp.serializedObject.ApplyModifiedProperties();
                    });
                    ele.AddToClassList(ClassAllowDisable);
                    ele.BindProperty(subProp);

                    UnityEvent<bool> expandEvent = new UnityEvent<bool>();

                    ExpandableButtonsElement wrap = new ExpandableButtonsElement(ele, expandEvent);
                    ButtonsULongField r = new ButtonsULongField(label, wrap);
                    r.AddToClassList(DropdownFieldULong.alignedFieldUssClassName);

                    UIToolkitUtils.AddContextualMenuManipulator(r, subProp, () => { });

                    VisualElement root = new VisualElement();
                    root.Add(r);

                    ButtonsULongElement under = new ButtonsULongElement(enumMetaInfo, property, info, parent, newValue =>
                    {
                        subProp.ulongValue = Convert.ToUInt64(newValue);
                        subProp.serializedObject.ApplyModifiedProperties();
                    })
                    {
                        style =
                        {
                            display = DisplayStyle.None,
                        },
                    };
                    under.hToggleButton.style.display = DisplayStyle.None;
                    under.AddToClassList(ClassAllowDisable);
                    under.BindProperty(subProp);
                    root.Add(under);

                    bool expanded = false;
                    ele.fillEmptyButton.clicked += OnExpandedChanged;
                    wrap.ExpandButton.clicked += OnExpandedChanged;

                    return root;

                    void OnExpandedChanged()
                    {
                        expandEvent.Invoke(expanded = !expanded);
                        under.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        ele.hToggleButton.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                        ele.hCheckAllButton.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        ele.hEmptyButton.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        foreach (Button button in ele.ToggleButtons)
                        {
                            button.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                        }
                    }
                }
#endif
                case SaintsPropertyType.Undefined:
                default:
                    return null;
            }
        }
    }
}
#endif
