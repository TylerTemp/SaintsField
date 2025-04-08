using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer
    {
        public void RenderIMGUI(float width)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo, true);
            float height = GetHeightIMGUI(width);
            if (height <= Mathf.Epsilon)
            {
                return;
            }
            Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(rect, preCheckResult);
        }

        private static IReadOnlyList<(string, List<IPlayaAttribute>)> GroupAttributesIMGUI(IEnumerable<IPlayaAttribute> playaAttributes)
        {
            List<(string, List<IPlayaAttribute>)> groupWithAttributes = new List<(string, List<IPlayaAttribute>)>();
            Dictionary<string, List<IPlayaAttribute>> groupToAttributes = new Dictionary<string, List<IPlayaAttribute>>();

            foreach (IPlayaAttribute playaAttribute in playaAttributes)
            {
                string groupBy = "";
                if (playaAttribute is IPlayaIMGUIGroupBy imguiGroupBy)
                {
                    groupBy = imguiGroupBy.GroupBy;
                }

                if (string.IsNullOrEmpty(groupBy))
                {
                    groupWithAttributes.Add(("", new List<IPlayaAttribute>(){playaAttribute}));
                }
                else
                {
                    if (!groupToAttributes.TryGetValue(groupBy, out List<IPlayaAttribute> list))
                    {
                        list = new List<IPlayaAttribute>();
                        groupToAttributes.Add(groupBy, list);
                        groupWithAttributes.Add((groupBy, list));
                    }

                    list.Add(playaAttribute);
                }
            }

            return groupWithAttributes;
        }

        protected virtual void RenderAboveIMGUI()
        {
        }

        protected abstract void RenderTargetIMGUI(float width, PreCheckResult preCheckResult);

        protected virtual void RenderBelowIMGUI()
        {
        }

        public virtual float GetHeightIMGUI(float width)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo, true);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            return GetAboveHeightIMGUI(width) + GetFieldHeightIMGUI(width, preCheckResult) + GetBelowHeightIMGUI(width);
        }

        protected virtual float GetAboveHeightIMGUI(float width)
        {
            // float totalHeight = 0f;
            // foreach ((string _, List<IPlayaAttribute> attributes) in GroupAttributesIMGUI(FieldWithInfo.PlayaAttributes))
            // {
            //     float maxHeight = 0f;
            //     float useWidth = attributes.Count == 1? width: width / attributes.Count;
            //     foreach (IPlayaAttribute playaAttribute in attributes)
            //     {
            //         switch (playaAttribute)
            //         {
            //             case PlayaInfoBoxAttribute infoBoxAttribute:
            //                 if(!infoBoxAttribute.Below)
            //                 {
            //                     maxHeight = Mathf.Max(maxHeight, GetInfoBoxHeightIMGUI(useWidth, infoBoxAttribute));
            //                 }
            //                 break;
            //         }
            //     }
            //
            //     totalHeight += maxHeight;
            // }
            // return totalHeight;
            return 0;
        }



        protected abstract float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult);

        protected virtual float GetBelowHeightIMGUI(float width)
        {
            // float totalHeight = 0f;
            // foreach ((string _, List<IPlayaAttribute> attributes) in GroupAttributesIMGUI(FieldWithInfo.PlayaAttributes))
            // {
            //     List<float> accHeight = new List<float>();
            //     float useWidth = attributes.Count == 1? width: width / attributes.Count;
            //     foreach (IPlayaAttribute playaAttribute in attributes)
            //     {
            //         switch (playaAttribute)
            //         {
            //             case PlayaInfoBoxAttribute infoBoxAttribute:
            //                 if(infoBoxAttribute.Below)
            //                 {
            //                     accHeight.Add(GetInfoBoxHeightIMGUI(useWidth, infoBoxAttribute));
            //                 }
            //                 break;
            //         }
            //     }
            //
            //     totalHeight += accHeight.DefaultIfEmpty(0).Max();
            // }
            // return totalHeight;
            return 0;
        }

        public void RenderPositionIMGUI(Rect position)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo, true);
            Rect aboveRect = RenderAbovePosition(position);
            float targetHeight = GetFieldHeightIMGUI(position.width, preCheckResult);
            (Rect targetRect, Rect belowRect) = RectUtils.SplitHeightRect(aboveRect, targetHeight);
            RenderPositionTargetIMGUI(targetRect, preCheckResult);
            RenderPositionBelow(belowRect);
        }

        protected virtual Rect RenderAbovePosition(Rect position)
        {
            return position;
        }

        protected abstract void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult);

        protected virtual void RenderPositionBelow(Rect position)
        {
        }

        protected static float FieldHeight(object value, string label)
        {
            if (value == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            Type valueType = value.GetType();

            if (valueType == typeof(bool)
                || valueType == typeof(sbyte)
                || valueType == typeof(byte)
                || valueType == typeof(short)
                || valueType == typeof(ushort)
                || valueType == typeof(int)
                || valueType == typeof(uint)
                || valueType == typeof(long)
                || valueType == typeof(ulong)
                || valueType == typeof(float)
                || valueType == typeof(double)
                || valueType == typeof(string)
                || valueType == typeof(Vector2)
                || valueType == typeof(Vector3)
                || valueType == typeof(Vector4)
                || valueType == typeof(Vector2Int)
                || valueType == typeof(Vector3Int)
                || valueType == typeof(Color)
                || valueType == typeof(Bounds)
                || valueType == typeof(Rect)
                || valueType == typeof(RectInt)
                || typeof(UnityEngine.Object).IsAssignableFrom(valueType)
                || valueType.BaseType == typeof(Enum)
                || valueType.BaseType == typeof(TypeInfo))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (Array.Exists(valueType.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                float resultHeight = EditorGUIUtility.singleLineHeight;

                // ReSharper disable once AssignNullToNotNullAttribute
                object[] kvPairs = (value as IEnumerable).Cast<object>().ToArray();

                const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

                foreach ((object kvPair, int index) in kvPairs.WithIndex())
                {
                    Type kvPairType = kvPair.GetType();
                    PropertyInfo keyProp = kvPairType.GetProperty("Key", bindAttr);
                    if (keyProp == null)
                    {
                        string errMsg = $"Failed to obtain key on element {index}: {kvPair}";
                        resultHeight += ImGuiHelpBox.GetHeight(errMsg, EditorGUIUtility.currentViewWidth, MessageType.Error);
                        continue;
                    }
                    PropertyInfo valueProp = kvPairType.GetProperty("Value", bindAttr);
                    if (valueProp == null)
                    {
                        string errMsg = $"Failed to obtain value on element {index}: {kvPair}";
                        resultHeight += ImGuiHelpBox.GetHeight(errMsg, EditorGUIUtility.currentViewWidth, MessageType.Error);
                        continue;
                    }

                    object dictKey = keyProp.GetValue(kvPair);
                    resultHeight += FieldHeight(dictKey, $"{dictKey}");

                    object dictValue = valueProp.GetValue(kvPair);
                    resultHeight += FieldHeight(dictValue, $"{dictValue}");
                }

                return resultHeight;
                // return new HelpBox($"IDictionary {valueType}", HelpBoxMessageType.Error);
            }

            if (value is IEnumerable enumerableValue)
            {
                return EditorGUIUtility.singleLineHeight + enumerableValue.Cast<object>().Select((each, index) => FieldHeight(each, $"Element {index}")).Sum();
            }

            {  // generic fields & properties
                float resultHeight = EditorGUIUtility.singleLineHeight;

                const BindingFlags bindAttrNormal =
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

                foreach (FieldInfo fieldInfo in valueType.GetFields(bindAttrNormal))
                {
                    object fieldValue = fieldInfo.GetValue(value);
                    resultHeight += FieldHeight(fieldValue, fieldInfo.Name);
                }

                foreach (PropertyInfo propertyInfo in valueType.GetProperties(bindAttrNormal))
                {
                    object propertyValue = propertyInfo.GetValue(value);
                    resultHeight += FieldHeight(propertyValue, propertyInfo.Name);
                }

                return resultHeight;
            }
        }

        protected static object FieldPosition(Rect position, object value, string label, Type type=null, bool disabled=true)
        {
            using (new EditorGUI.DisabledScope(disabled))
            {
                if (type == null && value == null)
                {
                    Rect rt = position;
                    EditorGUI.DrawRect(new Rect(rt)
                    {
                        x = rt.x + EditorGUIUtility.labelWidth,
                        width = rt.width - EditorGUIUtility.labelWidth,
                    }, EColor.CharcoalGray.GetColor() * new Color(1, 1,1, 0.2f));
                    EditorGUI.LabelField(rt, label, "null", EditorStyles.label);
                    return null;
                }

                // bool isDrawn = true;
                Type valueType = type ?? value.GetType();

                if (valueType == typeof(bool))
                {
                    return EditorGUI.Toggle(position, label, (bool)value);
                }

                if (valueType == typeof(sbyte))
                {
                    return EditorGUI.IntField(position, label, (sbyte)value);
                }
                if (valueType == typeof(byte))
                {
                    return EditorGUI.IntField(position, label, (byte)value);
                }
                if (valueType == typeof(short))
                {
                    return EditorGUI.IntField(position, label, (short)value);
                }
                if (valueType == typeof(ushort))
                {
                    return EditorGUI.IntField(position, label, (ushort)value);
                }
                if (valueType == typeof(int))
                {
                    return EditorGUI.IntField(position, label, (int)value);
                }
                if (valueType == typeof(uint))
                {
                    return EditorGUI.LongField(position, label, (uint)value);
                }
                if (valueType == typeof(long))
                {
                    return EditorGUI.LongField(position, label, (long)value);
                }
                if (valueType == typeof(ulong))
                {
                    return EditorGUI.TextField(position, label, ((ulong)value).ToString());
                }
                if (valueType == typeof(float))
                {
                    return EditorGUI.FloatField(position, label, (float)value);
                }
                if (valueType == typeof(double))
                {
                    return EditorGUI.DoubleField(position, label, (double)value);
                }
                if (valueType == typeof(string))
                {
                    return EditorGUI.TextField(position, label, (string)value);
                }
                if (valueType == typeof(Vector2))
                {
                    return EditorGUI.Vector2Field(position, label, (Vector2)value);
                }
                if (valueType == typeof(Vector3))
                {
                    return EditorGUI.Vector3Field(position, label, (Vector3)value);
                }
                if (valueType == typeof(Vector4))
                {
                    return EditorGUI.Vector4Field(position, label, (Vector4)value);
                }
                if (valueType == typeof(Vector2Int))
                {
                    return EditorGUI.Vector2IntField(position, label, (Vector2Int)value);
                }
                if (valueType == typeof(Vector3Int))
                {
                    return EditorGUI.Vector3IntField(position, label, (Vector3Int)value);
                }
                if (valueType == typeof(Color))
                {
                    return EditorGUI.ColorField(position, label, (Color)value);
                }
                if (valueType == typeof(Bounds))
                {
                    return EditorGUI.BoundsField(position, label, (Bounds)value);
                }
                if (valueType == typeof(Rect))
                {
                    return EditorGUI.RectField(position, label, (Rect)value);
                }
                if (valueType == typeof(RectInt))
                {
                    return EditorGUI.RectIntField(position, label, (RectInt)value);
                }
                if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    return EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, valueType, true);
                }
                if (valueType.BaseType == typeof(Enum))
                {
                    return EditorGUI.EnumPopup(position, label, (Enum)value);
                }
                if (valueType.BaseType == typeof(TypeInfo))
                {
                    return EditorGUI.TextField(position, label, value.ToString());
                }
                if (Array.Exists(valueType.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                    };

                    // ReSharper disable once AssignNullToNotNullAttribute
                    object[] kvPairs = (value as IEnumerable).Cast<object>().ToArray();

                    (Rect labelRect, Rect leftRect) =
                        RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

                    EditorGUI.LabelField(labelRect, $"{label} <i>(Dictionary x{kvPairs.Length})</i>", style);

                    const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                                  BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

                    Rect accRect = leftRect;
                    foreach ((object kvPair, int index) in kvPairs.WithIndex())
                    {
                        Type kvPairType = kvPair.GetType();
                        PropertyInfo keyProp = kvPairType.GetProperty("Key", bindAttr);
                        if (keyProp == null)
                        {
                            string errMsg = $"Failed to obtain key on element {index}: {kvPair}";
                            float height = ImGuiHelpBox.GetHeight(errMsg, position.width, MessageType.Error);
                            (Rect helpRect, Rect nowLeftRect) = RectUtils.SplitHeightRect(accRect, height);
                            accRect = nowLeftRect;
                            EditorGUI.HelpBox(helpRect, errMsg, MessageType.Error);
                            continue;
                        }
                        PropertyInfo valueProp = kvPairType.GetProperty("Value", bindAttr);
                        if (valueProp == null)
                        {
                            string errMsg = $"Failed to obtain value on element {index}: {kvPair}";
                            float height = ImGuiHelpBox.GetHeight(errMsg, position.width, MessageType.Error);
                            (Rect helpRect, Rect nowLeftRect) = RectUtils.SplitHeightRect(accRect, height);
                            accRect = nowLeftRect;
                            EditorGUI.HelpBox(helpRect, errMsg, MessageType.Error);
                            continue;
                        }

                        object dictKey = keyProp.GetValue(kvPair);
                        string dictKeyLabel = $"{dictKey}";
                        float dictKeyHeight = FieldHeight(dictKey, $"{dictKey}");
                        (Rect dictKeyUseRect, Rect dictLeftRect) = RectUtils.SplitHeightRect(accRect, dictKeyHeight);
                        accRect = dictLeftRect;
                        FieldPosition(new Rect(dictKeyUseRect)
                        {
                            x = dictKeyUseRect.x + SaintsPropertyDrawer.IndentWidth,
                            width = dictKeyUseRect.width - SaintsPropertyDrawer.IndentWidth,
                        }, dictKey, dictKeyLabel);

                        object dictValue = valueProp.GetValue(kvPair);
                        string dictValueLabel = $"{dictValue}";
                        float dictValueHeight = FieldHeight(dictValue, $"{dictValue}");
                        (Rect dictValueUseRect, Rect dictValueLeftRect) = RectUtils.SplitHeightRect(accRect, dictValueHeight);
                        accRect = dictValueLeftRect;
                        FieldPosition(new Rect(dictValueUseRect)
                        {
                            x = dictValueUseRect.x + SaintsPropertyDrawer.IndentWidth,
                            width = dictValueUseRect.width - SaintsPropertyDrawer.IndentWidth,
                        }, dictValue, dictValueLabel);
                    }

                    return null;
                    // return new HelpBox($"IDictionary {valueType}", HelpBoxMessageType.Error);
                }
                if (value is IEnumerable enumerableValue)
                {
                    (object value, int index)[] valueIndexed = enumerableValue.Cast<object>().WithIndex().ToArray();

                    (Rect labelRect, Rect listRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(labelRect, label);

                    float numWidth = Mathf.Max(30,
                        EditorStyles.textField.CalcSize(new GUIContent($"{valueIndexed.Length}")).x);

                    Rect numRect = new Rect(labelRect)
                    {
                        width = numWidth,
                        x = labelRect.x + labelRect.width - numWidth,
                    };

                    EditorGUI.IntField(numRect, valueIndexed.Length);

                    GUI.Box(listRect, GUIContent.none);

                    float lisAccY = 0;
                    using(new EditorGUI.IndentLevelScope())
                    {
                        List<object> results = new List<object>();
                        foreach ((object item, int index) in valueIndexed)
                        {
                            string thisLabel = $"Element {index}";
                            float thisHeight = FieldHeight(item, thisLabel);
                            Rect thisRect = new Rect(listRect)
                            {
                                y = listRect.y + lisAccY,
                                height = thisHeight,
                            };
                            lisAccY += thisHeight;

                            results.Add(FieldPosition(thisRect, item, thisLabel));
                        }

                        return results;
                    }

                }

                {
                    if (RuntimeUtil.IsNull(value))
                    {
                        return null;
                    }

                    // fallback draw properties and fields
                    (Rect labelRect, Rect leftRect) =
                        RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

                    EditorGUI.LabelField(labelRect, label);

                    List<(object value, string label, Type type)> listResult = new List<(object value, string label, Type type)>();

                    const BindingFlags bindAttrNormal =
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

                    // Debug.Log($"valueType={valueType}");

                    foreach (FieldInfo fieldInfo in valueType.GetFields(bindAttrNormal))
                    {
                        object fieldValue = fieldInfo.GetValue(value);
                        listResult.Add((fieldValue, fieldInfo.Name, fieldInfo.FieldType));
                    }

                    foreach (PropertyInfo propertyInfo in valueType.GetProperties(bindAttrNormal))
                    {
                        object propertyValue = propertyInfo.GetValue(value);
                        listResult.Add((propertyValue, propertyInfo.Name, propertyInfo.PropertyType));
                    }

                    Rect accRect = leftRect;
                    foreach ((object eachValue, string eachLabel, Type eachType) in listResult)
                    {
                        float height = FieldHeight(eachValue, eachLabel);
                        (Rect useRect, Rect newLeftRect) = RectUtils.SplitHeightRect(accRect, height);
                        accRect = newLeftRect;
                        Rect thisRect = new Rect(useRect)
                        {
                            x = useRect.x + SaintsPropertyDrawer.IndentWidth,
                            width = useRect.width - SaintsPropertyDrawer.IndentWidth,
                        };

                        FieldPosition(thisRect, eachValue, eachLabel, eachType);
                    }
                }

                return null;

                // return isDrawn;
            }
        }
    }
}
