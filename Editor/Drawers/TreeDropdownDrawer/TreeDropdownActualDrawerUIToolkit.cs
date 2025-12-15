using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer: ISaintsSerializedActualDrawer
    {
        private Type _enumType;

        public VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual, ISaintsAttribute _, string label, SerializedProperty property, object parent)
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
                    _enumType = enumMetaInfo.EnumType;
                    DropdownButtonLongElement ele = new DropdownButtonLongElement(enumMetaInfo);
                    SerializedProperty subProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                    // ele.BindProperty(subProp);
                    ele.bindingPath = subProp.propertyPath;

                    DropdownFieldLong r = new DropdownFieldLong(label, ele);
                    r.AddToClassList(DropdownFieldLong.alignedFieldUssClassName);
                    r.AddToClassList(ClassAllowDisable);

                    UIToolkitUtils.AddContextualMenuManipulator(r, subProp, () => { });

                    return r;
                }
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                {
                    EnumMetaInfo enumMetaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    _enumType = enumMetaInfo.EnumType;
                    DropdownButtonULongElement ele = new DropdownButtonULongElement(enumMetaInfo);
                    SerializedProperty subProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                    Debug.Log($"bind {targetType} to {subProp.propertyPath}");
#endif
                    // ele.BindProperty(subProp);
                    ele.bindingPath = subProp.propertyPath;

                    DropdownFieldULong r = new DropdownFieldULong(label, ele);
                    r.AddToClassList(DropdownFieldULong.alignedFieldUssClassName);
                    r.AddToClassList(ClassAllowDisable);

                    UIToolkitUtils.AddContextualMenuManipulator(r, subProp, () => { });

                    // ele.Button.clicked += () => ClickDropdown(ele.Button, enumMetaInfo, Enum.ToObject(enumMetaInfo.EnumType, subProp.ulongValue), v =>
                    // {
                    //     ulong uv = (ulong)v;
                    //     subProp.ulongValue = uv;
                    //     subProp.serializedObject.ApplyModifiedProperties();
                    // });

                    return r;
                }
#endif
                case SaintsPropertyType.Undefined:
                default:
                    return null;
            }
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;
            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
                {
                    SerializedProperty actualProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                    container.TrackPropertyValue(actualProp, _ => onValueChangedCallback.Invoke(Enum.ToObject(_enumType, actualProp.longValue)));
                }
                    break;
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                {
                    SerializedProperty actualProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                    container.TrackPropertyValue(actualProp, _ => onValueChangedCallback.Invoke(Enum.ToObject(_enumType, actualProp.ulongValue)));
                }
                    break;
#endif
                case SaintsPropertyType.Undefined:
                case SaintsPropertyType.ClassOrStruct:
                case SaintsPropertyType.Interface:
                case SaintsPropertyType.DateTime:
                case SaintsPropertyType.TimeSpan:
                case SaintsPropertyType.Guid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }
    }
}
