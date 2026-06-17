using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsWrapTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(BaseWrap<>), false)]
    // [CustomPropertyDrawer(typeof(SaintsWrap<>), true)]
    public partial class BaseWrapDrawer: SaintsPropertyDrawer
    {
        protected static Type GetWrapFieldType(SerializedProperty property, FieldInfo info)
        {
            return SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
        }

        protected static Type GetWrappedValueType(SerializedProperty property, FieldInfo info)
        {
            Type wrapFieldType = GetWrapFieldType(property, info);
            return wrapFieldType?.GetGenericArguments()[0];
        }

        public static (SerializedProperty realProp, FieldInfo realInfo) GetBasicInfo(SerializedProperty property, FieldInfo info)
        {
            string propName = "value";
            SerializedProperty wrapTypeProp = property.FindPropertyRelative("wrapType");
            if (wrapTypeProp != null)
            {
                switch ((WrapType)wrapTypeProp.intValue)
                {
                    case WrapType.Field:
                        propName = "valueField";
                        break;
                    case WrapType.Array:
                        propName = "valueArray";
                        break;
                    case WrapType.List:
                        propName = "valueList";
                        break;
                }
            }

            SerializedProperty realProp = property.FindPropertyRelative(propName) ??
                                          SerializedUtils.FindPropertyByAutoPropertyName(property, propName);
            Debug.Assert(realProp != null, property.propertyPath);

            Type wrapFieldType = GetWrapFieldType(property, info);
            FieldInfo realInfo = wrapFieldType?.GetField(propName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            return (realProp, realInfo);
        }
    }
}
