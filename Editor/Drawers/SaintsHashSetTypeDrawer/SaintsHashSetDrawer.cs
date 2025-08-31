using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsHashSetBase<>), true)]
    [CustomPropertyDrawer(typeof(SaintsHashSetAttribute), true)]
    public partial class SaintsHashSetDrawer: SaintsPropertyDrawer
    {
        private static readonly Color WarningColor = new Color(0.8490566f, 0.3003738f, 0.3003738f);

        private static bool IncreaseArraySize(int newValue, SerializedProperty prop)
        {
            int propSize = prop.arraySize;
            if (propSize == newValue)
            {
                return false;
            }

            prop.arraySize = newValue;
            return true;
        }

        private static void DecreaseArraySize(IReadOnlyList<int> indexReversed, SerializedProperty prop)
        {
            int curSize = prop.arraySize;
            foreach (int index in indexReversed.Where(each => each < curSize))
            {
                // Debug.Log($"Remove index {index}");
                prop.DeleteArrayElementAtIndex(index);
            }
        }

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

            Debug.Assert(_propName != null, $"Failed to find property name for {rawType}. Do you froget to define a `static string EditorPropertyName` (nameof(YourPropList))?");

            return _propName;
        }

        private static bool GetNeedFlatten(SerializedProperty elementProp, Type baseType)
        {
            if (elementProp.propertyType != SerializedPropertyType.Generic)
            {
                return false;
            }

            (PropertyAttribute[] valuesAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(elementProp);
            // Debug.Log($"{string.Join<PropertyAttribute>(",", valuesAttributes)}");
            // AboveRichLabelAttribute aboveRichLabelAttributes = valuesAttributes.OfType<AboveRichLabelAttribute>().FirstOrDefault();
            SaintsRowAttribute saintsRowAttribute =
                valuesAttributes.OfType<SaintsRowAttribute>().FirstOrDefault();

            if (saintsRowAttribute is null)
            {
                // check if it has a 3rd party drawer
                bool drawer = valuesAttributes.Any(eachAttr =>
                    PropertyAttributeToPropertyDrawers.TryGetValue(eachAttr.GetType(), out IReadOnlyList<PropertyDrawerInfo> d) &&
                    d.Any(each => !each.IsSaints));

                if (drawer)
                {
                    return false;
                }

                // check if it has a type drawer
                Type typeDrawer = FindTypeDrawerAny(baseType);
                // Debug.Log($"{baseType}:{typeDrawer}");
                if (typeDrawer != null)
                {
                    return false;
                }
            }

            return true;
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

        private static IEnumerable<int> Search(SerializedProperty wrapProp, string searchText)
        {
            int size = wrapProp.arraySize;

            bool searchEmpty = string.IsNullOrEmpty(searchText);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (searchEmpty)
            {
                for (int index = 0; index < size; index++)
                {
                    yield return index;
                }
                yield break;
            }

            foreach (int index in SerializedUtils.SearchArrayProperty(wrapProp, searchText))
            {
                yield return index;
            }
        }
    }
}
