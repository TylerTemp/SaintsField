using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsDictionaryBase<,>), true)]
    [CustomPropertyDrawer(typeof(SaintsDictionaryAttribute), true)]
    public partial class SaintsDictionaryDrawer: SaintsPropertyDrawer
    {
        private static readonly Color WarningColor = new Color(0.8490566f, 0.3003738f, 0.3003738f);

        // [InitializeOnLoadMethod]
        // private static void AddSaintsPropertyInfoInjectAnimatorState()
        // {
        //     AddSaintsPropertyInfoInject((property, info, allAttributes) =>
        //     {
        //         if (allAttributes.Any(each => each is AnimatorStateAttribute))
        //         {
        //             return (null, null);
        //         }
        //
        //         if (property.propertyType != SerializedPropertyType.Generic)
        //         {
        //             return (null, null);
        //         }
        //         Type infoType = ReflectUtils.GetElementType(info.FieldType);
        //
        //
        //         if (!InterfaceTypes.All(interfaceType => interfaceType.IsAssignableFrom(infoType)))
        //         {
        //             return (null, null);
        //         }
        //
        //         AnimatorStateAttribute fakeAttribute = new AnimatorStateAttribute();
        //         return (fakeAttribute, typeof(AnimatorStateAttributeDrawer));
        //     });
        // }

        private static bool IncreaseArraySize(int newValue, SerializedProperty keyProp, SerializedProperty valueProp)
        {
            int keySize = keyProp.arraySize;
            if (keySize == newValue)
            {
                bool changed = false;
                // ReSharper disable once InvertIf
                if(valueProp.arraySize != newValue)
                {
                    changed = true;
                    valueProp.arraySize = newValue;
                }
                return changed;
            }

            keyProp.arraySize = newValue;
            valueProp.arraySize = newValue;
            // Debug.Log($"resize to {newValue}");
            return true;
        }

        private static void DecreaseArraySize(IReadOnlyList<int> indexReversed, SerializedProperty keyProp, SerializedProperty valueProp)
        {
            int curSize = keyProp.arraySize;
            foreach (int index in indexReversed.Where(each => each < curSize))
            {
                // Debug.Log($"Remove index {index}");
                keyProp.DeleteArrayElementAtIndex(index);
                valueProp.DeleteArrayElementAtIndex(index);
            }
        }

        private string _keysPropName;
        private string _valuesPropName;

        private (string, string) GetKeysValuesPropName(Type rawType)
        {
            // Type fieldType = ReflectUtils.GetElementType(rawType);

            // ReSharper disable once InvertIf
            if (_keysPropName == null)
            {
                // Debug.Log(rawType);
                _keysPropName = ReflectUtils.GetIWrapPropName(rawType, "EditorPropKeys");
                _valuesPropName = ReflectUtils.GetIWrapPropName(rawType, "EditorPropValues");
            }

            return (_keysPropName, _valuesPropName);
        }

        private static List<int> Search(SerializedProperty keysProp, SerializedProperty valuesProp, string keySearch, string valueSearch)
        {
            int size = keysProp.arraySize;

            List<int> results = string.IsNullOrEmpty(keySearch)
                ? Enumerable.Range(0, size).ToList()
                : SerializedUtils.SearchArrayProperty(keysProp, keySearch).ToList();
            // int[] valueResults = SerializedUtils.SearchArrayProperty(valuesProp, valueSearch).ToArray();
            if (string.IsNullOrEmpty(valueSearch))
            {
                return results;
            }

            int[] valueResults = SerializedUtils.SearchArrayProperty(valuesProp, valueSearch).ToArray();
            return results.Where(each => valueResults.Contains(each)).ToList();
        }

        private static string GetKeyLabel(SaintsDictionaryAttribute saintsDictionaryAttribute) => saintsDictionaryAttribute is null
            ? "Keys"
            : saintsDictionaryAttribute.KeyLabel;

        private static string GetValueLabel(SaintsDictionaryAttribute saintsDictionaryAttribute) => saintsDictionaryAttribute is null
            ? "Values"
            : saintsDictionaryAttribute.ValueLabel;

        private static bool GetNeedFlatten(SerializedProperty elementProp)
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

            return saintsRowAttribute is null;
        }
    }
}
