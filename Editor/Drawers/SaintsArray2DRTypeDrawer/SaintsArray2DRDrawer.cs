using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsArray2DRTypeDrawer
{
    // ReSharper disable once InconsistentNaming
    [CustomPropertyDrawer(typeof(SaintsArray2DRAttribute), true)]
    [CustomPropertyDrawer(typeof(SaintsArray2DR<>), true)]
    public partial class SaintsArray2DRDrawer: SaintsPropertyDrawer
    {
        private string _propName;

        private string GetPropName(Type rawType)
        {
            // Type fieldType = ReflectUtils.GetElementType(rawType);

            // ReSharper disable once InvertIf
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_propName == null)
            {
                _propName = ReflectUtils.GetIWrapPropName(rawType);
            }

            Debug.Assert(_propName != null, $"Failed to find property name for {rawType}. Do you forget to define a `static string EditorPropertyName` (nameof(YourPropList))?");

            return _propName;
        }

        private static SerializedProperty FindPropertyCompact(SerializedProperty property, string propValuesNameCompact)
        {
            SerializedProperty prop = property.FindPropertyRelative(propValuesNameCompact);
            if (prop != null)
            {
                return prop;
            }

            SerializedProperty accProp = property;
            foreach (string propSegName in propValuesNameCompact.Split('.'))
            {
                SerializedProperty findProp = accProp.FindPropertyRelative(propSegName) ?? SerializedUtils.FindPropertyByAutoPropertyName(accProp, propSegName);
                Debug.Assert(findProp != null, $"Failed to find prop {propSegName} in {accProp.propertyPath}");
                accProp = findProp;
            }

            return accProp;
        }

        private static (FieldInfo targetInfo, object targetParent) GetTargetInfo(string propNameCompact, Type type, object saintsSerValue)
        {

            // object keysIterTarget = info.GetValue(parent);
            object keysIterTarget = saintsSerValue;
            List<object> keysParents = new List<object>(3)
            {
                saintsSerValue,
            };
            Type keysParentType = type;
            FieldInfo keysField = null;
            // Debug.Log($"propKeysNameCompact={propNameCompact}");
            foreach (string propKeysName in propNameCompact.Split('.'))
            {
                // Debug.Log($"propKeysName={propKeysName}");

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (Type each in ReflectUtils.GetSelfAndBaseTypesFromType(keysParentType))
                {
                    FieldInfo field = each.GetField(propKeysName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (field == null)
                    {
                        continue;
                    }

                    // Debug.Log($"field={field}; keysField={keysField}");

                    keysField = field;
                    keysParentType = keysField.FieldType;
                    keysIterTarget = keysField.GetValue(keysIterTarget);
                    keysParents.Add(keysIterTarget);
                    // Debug.Log($"Prop {propKeysName} Add parents = {keysIterTarget}/{keysIterTarget.GetType()}");
                    // Debug.Log($"set keysField={keysField}/keysParentType={keysParentType}/keysIterTarget={keysIterTarget}");
                    break;
                }

                Debug.Assert(keysField != null, $"Failed to get key {propKeysName} from {keysIterTarget}");
            }

            int keysParentsCount = keysParents.Count;

            object keysParent = keysParentsCount >= 2? keysParents[keysParentsCount - 2]: keysParents[0];

            return (keysField, keysParent);
        }

    }
}
