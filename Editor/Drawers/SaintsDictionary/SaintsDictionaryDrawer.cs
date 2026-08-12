using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsDictionary<,>), true)]
    [CustomPropertyDrawer(typeof(SaintsDictionaryAttribute), true)]
    public partial class SaintsDictionaryDrawer: SaintsPropertyDrawer
    {
        private static readonly Color WarningColor = new Color(0.8490566f, 0.3003738f, 0.3003738f);

        private static string SessionKeyColumnWidth(SerializedProperty property, bool isKey) =>
            $"{property.propertyPath}[{(isKey ? "key" : "value")}:width]";

        private static ResponsiveLength GetSessionColumnWidth(SerializedProperty property, bool isKey,
            ResponsiveLength fallback)
        {
            float percent = SessionState.GetFloat(SessionKeyColumnWidth(property, isKey), float.NaN);
            return !float.IsNaN(percent) && percent > 0f && percent < 100f
                ? new ResponsiveLength(ResponsiveType.Percent, percent)
                : fallback;
        }

        private static void SaveSessionColumnWidths(SerializedProperty property, float keyPixels, float valuePixels)
        {
            float totalPixels = keyPixels + valuePixels;
            if (float.IsNaN(totalPixels) || totalPixels <= 0f)
            {
                return;
            }

            SessionState.SetFloat(SessionKeyColumnWidth(property, true), keyPixels / totalPixels * 100f);
            SessionState.SetFloat(SessionKeyColumnWidth(property, false), valuePixels / totalPixels * 100f);
        }

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

            Debug.Assert(_keysPropName != null, $"Failed to find keys property name for {rawType}. Do you forget to define a `static string EditorPropKeys` (nameof(YourPropKeyList))?");
            Debug.Assert(_valuesPropName != null, $"Failed to find values property name for {rawType}. Do you forget to define a `static string EditorPropValues` (nameof(YourPropValueList))?");

            return (_keysPropName, _valuesPropName);
        }

        private static IEnumerable<int> Search(SerializedProperty keysProp, SerializedProperty valuesProp, string keySearch, string valueSearch, bool objectSearch)
        {
            int size = keysProp.arraySize;

            bool keySearchEmpty = string.IsNullOrEmpty(keySearch);
            bool valueSearchEmpty = string.IsNullOrEmpty(valueSearch);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (keySearchEmpty && valueSearchEmpty)
            {
                for (int index = 0; index < size; index++)
                {
                    yield return index;
                }
                yield break;
            }

            if (keySearchEmpty)
            {
                foreach (int index in SerializedUtils.SearchArrayProperty(valuesProp, valueSearch, objectSearch))
                {
                    yield return index;
                }

                yield break;
            }

            IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(valueSearch).ToArray();
            foreach (int index in SerializedUtils.SearchArrayProperty(keysProp, keySearch, objectSearch))
            {
                if (index == -1)
                {
                    yield return -1;
                }
                else
                {
                    SerializedProperty valueProp = valuesProp.GetArrayElementAtIndex(index);
                    HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                        .Select(_ => new HashSet<object>())
                        .ToArray();
                    bool all = true;
                    for (int tokenIndex = 0; tokenIndex < searchTokens.Count; tokenIndex++)
                    {
                        ListSearchToken search = searchTokens[tokenIndex];
                        HashSet<object> searchedObjects = searchedObjectsArray[tokenIndex];
                        // ReSharper disable once InvertIf
                        if (!SerializedUtils.SearchProp(valueProp, search.Token, objectSearch, searchedObjects))
                        {
                            all = false;
                            break;
                        }
                    }

                    if (all)
                    {
                        yield return index;
                    }
                    else
                    {
                        yield return -1;
                    }
                }
            }
        }

        private static string GetKeyLabel(SaintsDictionaryAttribute saintsDictionaryAttribute) => saintsDictionaryAttribute is null
            ? "Keys"
            : saintsDictionaryAttribute.KeyLabel;

        private static string GetValueLabel(SaintsDictionaryAttribute saintsDictionaryAttribute) => saintsDictionaryAttribute is null
            ? "Values"
            : saintsDictionaryAttribute.ValueLabel;

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
    }
}
