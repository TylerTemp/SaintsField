using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static IEnumerable<int> Search(ISaintsDictionaryEditorTool dictionaryTool, SerializedProperty keysProp, SerializedProperty valuesProp,
            Type keyType, Type valueType,
            string keySearch, string valueSearch, bool defaultSearch, bool objectSearch, object parent, MethodInfo extraSearchMethod)
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

            IReadOnlyList<ListSearchToken> valueSearchTokens = SerializedUtils.ParseSearch(valueSearch).ToArray();
            Type pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);

            if (keySearchEmpty)
            {
                for (int index = 0; index < dictionaryTool.EditorSaintsKeys().Count; index++)
                {
                    bool matched = false;
                    if (defaultSearch)
                    {
                        matched = SerializedUtils.SearchArrayPropertyItem(valuesProp.GetArrayElementAtIndex(index),
                            valueSearchTokens, objectSearch);
                    }

                    if (!matched && extraSearchMethod != null)
                    {
                        matched = SearchExtra(
                            dictionaryTool,
                            index,
                            Array.Empty<ListSearchToken>(),
                            valueSearchTokens,
                            pairType,
                            parent,
                            extraSearchMethod);
                    }
                    yield return matched? index: -1;
                }

                yield break;
            }

            IReadOnlyList<ListSearchToken> keySearchTokens = SerializedUtils.ParseSearch(keySearch).ToArray();

            for (int index = 0; index < dictionaryTool.EditorSaintsKeys().Count; index++)
            {
                bool matched = false;
                if (defaultSearch)
                {
                    bool keyMatched = SerializedUtils.SearchArrayPropertyItem(keysProp.GetArrayElementAtIndex(index),
                        keySearchTokens, objectSearch);
                    bool bothMatched = keyMatched && SerializedUtils.SearchArrayPropertyItem(valuesProp.GetArrayElementAtIndex(index),
                            valueSearchTokens, objectSearch);

                    matched = bothMatched;
                }

                if (!matched && extraSearchMethod != null)
                {
                    matched = SearchExtra(
                        dictionaryTool,
                        index,
                        Array.Empty<ListSearchToken>(),
                        valueSearchTokens,
                        pairType,
                        parent,
                        extraSearchMethod);
                }

                yield return matched? index: -1;
            }
        }

        private static bool SearchExtra(
            ISaintsDictionaryEditorTool dictionaryTool, int index,
            IReadOnlyList<ListSearchToken> keySearchTokens, IReadOnlyList<ListSearchToken> valueSearchTokens,
            Type pairType,
            object parent,
            MethodInfo extraSearchMethod)
        {
            ISaintsWrapEditorTool key = dictionaryTool.EditorSaintsKeys()[index];
            ISaintsWrapEditorTool value = dictionaryTool.EditorSaintsValues()[index];
            object pair = Activator.CreateInstance(pairType, key.EditorGetValue(), value.EditorGetValue());
            object methodReturn = extraSearchMethod.Invoke(parent, new[]{pair, keySearchTokens, valueSearchTokens});
            return (bool)methodReturn;
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

        private static MethodInfo GetSearchMethodInfo(string methodName, Type targetType, Type keyType, Type valueType)
        {
            Type pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);

            Type tokenListType = typeof(IEnumerable<ListSearchToken>);

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(targetType))
            {
                foreach (MethodInfo methodInfo in eachType.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (methodInfo.Name != methodName)
                    {
                        continue;
                    }

                    if (methodInfo.ReturnParameter?.ParameterType != typeof(bool))
                    {
                        continue;
                    }

                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    if(methodParams.Length != 3)
                    {
                        continue;
                    }

                    bool tokenMatch = true;
                    foreach (ParameterInfo parameterInfo in new[]{methodParams[1], methodParams[2]})
                    {
                        if (!tokenListType.IsAssignableFrom(parameterInfo.ParameterType))
                        {
                            tokenMatch = false;
                            break;
                        }
                    }

                    if (!tokenMatch)
                    {
                        continue;
                    }

                    if (pairType.IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return methodInfo;
                    }
                }
            }

            return null;
        }
    }
}
