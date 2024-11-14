using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using System;
using System.ComponentModel;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using UnityEngine;
using UnityEngine.Assertions.Must;


namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : SaintsPropertyDrawer
    {
        #region IMGUI

        private static readonly Dictionary<string, Vector2> IdToMinMaxRange = new Dictionary<string, Vector2>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

#if UNITY_2019_2_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
#if UNITY_2019_3_OR_NEWER
        [InitializeOnEnterPlayMode]
#endif
        private static void ImGuiClearSharedData() => IdToMinMaxRange.Clear();

        private string _error = "";
        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            IdToMinMaxRange.Remove(_cacheKey);
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _cacheKey = GetKey(property);

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            _error = metaInfo.Error;
            if (_error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue = metaInfo.MinValue;
            float maxValue = metaInfo.MaxValue;

            float labelWidth = label.text == ""? 0: EditorGUIUtility.labelWidth;

            float leftFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.x, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.x, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth);
            leftFieldWidth += 5f;
            float rightFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.y, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.y, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth);

            // float floatFieldWidth = EditorGUIUtility.fieldWidth;
            float sliderWidth = position.width - labelWidth - leftFieldWidth - rightFieldWidth;
            const float sliderPadding = 4f;

            (Rect labelWithMinFieldRect, Rect fieldRect) = RectUtils.SplitWidthRect(position, labelWidth + leftFieldWidth);

            (Rect sliderRect, Rect field3Rect) = RectUtils.SplitWidthRect(new Rect(fieldRect)
            {
                x = fieldRect.x + sliderPadding,
            }, sliderWidth - sliderPadding);

            (Rect maxFloatFieldRect, Rect _) = RectUtils.SplitWidthRect(new Rect(field3Rect)
            {
                x = field3Rect.x +sliderPadding,
            }, rightFieldWidth);

            bool freeInput = minMaxSliderAttribute.FreeInput;
            // Draw the slider
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 sliderValue = property.vector2Value;

                if(IdToMinMaxRange.TryGetValue(GetKey(property), out Vector2 freeRange))
                {
                    minValue = Mathf.Min(minValue, freeInput? freeRange.x: minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, freeInput? freeRange.y: maxValue, sliderValue.y);
                    freeRange = new Vector2(minValue, maxValue);
                }
                else
                {
                    minValue = Mathf.Min(minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, sliderValue.y);
                    IdToMinMaxRange[GetKey(property)] = freeRange = new Vector2(minValue, maxValue);
                }

                bool hasChange = false;
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
                    if(changed.changed)
                    {
                        Vector2 v = AdjustFloatSliderInput(sliderValue, minMaxSliderAttribute.Step, minValue, maxValue);
                        sliderValue.x = v.x;
                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float sliderX = EditorGUI.FloatField(labelWithMinFieldRect, label, sliderValue.x);
                    if(changed.changed)
                    {
                        // sliderValue.x = minMaxSliderAttribute.FreeInput? sliderX: Mathf.Clamp(sliderX, minValue, Mathf.Min(maxValue, sliderValue.y));
                        Vector2 v = AdjustFloatInput(sliderX, sliderValue.y, minMaxSliderAttribute.Step, minValue, maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.x < minValue)
                        {
                            freeRange.x = v.x;
                        }
                        sliderValue.x = v.x;
                        hasChange = true;
                    }
                }

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float sliderY = EditorGUI.FloatField(maxFloatFieldRect, sliderValue.y);
                    if(changed.changed)
                    {
                        // sliderValue.y = minMaxSliderAttribute.FreeInput? sliderY: Mathf.Clamp(sliderY, Mathf.Max(minValue, sliderValue.x), maxValue);
                        Vector2 v = AdjustFloatInput(sliderY, sliderValue.x, minMaxSliderAttribute.Step, minValue, maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.y > maxValue)
                        {
                            freeRange.y = v.y;
                        }
                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                if (hasChange)
                {
                    property.vector2Value = sliderValue;
                    onGUIPayload.SetValue(sliderValue);
                    property.serializedObject.ApplyModifiedProperties();
                }
                IdToMinMaxRange[GetKey(property)] = freeRange;
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                Vector2 sliderValue = property.vector2IntValue;

                if(IdToMinMaxRange.TryGetValue(GetKey(property), out Vector2 freeRange))
                {
                    minValue = Mathf.Min(minValue, freeInput? freeRange.x: minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, freeInput? freeRange.y: maxValue, sliderValue.y);
                    freeRange = new Vector2(minValue, maxValue);
                }
                else
                {
                    minValue = Mathf.Min(minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, sliderValue.y);
                    IdToMinMaxRange[GetKey(property)] = freeRange = new Vector2(minValue, maxValue);
                }

                bool hasChange = false;
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
                    if(changed.changed)
                    {
                        Vector2Int v = AdjustIntSliderInput(sliderValue, minMaxSliderAttribute.Step, minValue, maxValue);
                        sliderValue.x = v.x;
                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int sliderX = EditorGUI.IntField(labelWithMinFieldRect, label, (int)sliderValue.x);
                    if(changed.changed)
                    {
                        // sliderValue.x = minMaxSliderAttribute.FreeInput? sliderX: Mathf.Clamp(sliderX, minValue, Mathf.Min(maxValue, sliderValue.y));
                        Vector2Int v = AdjustIntInput(sliderX, (int)sliderValue.y, minMaxSliderAttribute.Step, minValue, maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.x < minValue)
                        {
                            freeRange.x = v.x;
                        }
                        sliderValue.x = v.x;
                        hasChange = true;
                    }
                }

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int sliderY = EditorGUI.IntField(maxFloatFieldRect, (int)sliderValue.y);
                    if(changed.changed)
                    {
                        // sliderValue.y = minMaxSliderAttribute.FreeInput? sliderY: Mathf.Clamp(sliderY, Mathf.Max(minValue, sliderValue.x), maxValue);
                        Vector2Int v = AdjustIntInput(sliderY, (int)sliderValue.x, minMaxSliderAttribute.Step, minValue, maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.y > maxValue)
                        {
                            freeRange.y = v.y;
                        }
                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                if (hasChange)
                {
                    property.vector2IntValue = new Vector2Int((int)sliderValue.x, (int) sliderValue.y);
                    onGUIPayload.SetValue(sliderValue);
                    property.serializedObject.ApplyModifiedProperties();
                }
                IdToMinMaxRange[GetKey(property)] = freeRange;
            }

            // ClickFocus(labelWithMinFieldRect, _fieldControlName);
        }

        private static float GetNumberFieldWidth(float value, float minWidth, float maxWidth) => GetFieldWidth($"{value}", minWidth, maxWidth);
        private static float GetNumberFieldWidth(int value, float minWidth, float maxWidth) => GetFieldWidth($"{value}", minWidth, maxWidth);

        private static float GetFieldWidth(string content, float minWidth, float maxWidth)
        {
            float actualWidth = EditorStyles.numberField.CalcSize(new GUIContent(content)).x;
            if (minWidth > 0 && actualWidth < minWidth)
            {
                return minWidth;
            }

            if (maxWidth > 0 && actualWidth > maxWidth)
            {
                return maxWidth;
            }

            return actualWidth;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public float MinValue;
            public float MaxValue;
            // ReSharper enable InconsistentNaming

            public override string ToString() => $"Meta(min={MinValue}, max={MaxValue}, error={Error ?? "null"})";
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parentTarget)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return new MetaInfo
                {
                    Error = $"Expect Vector2 or Vector2Int, get {property.propertyType}",
                    MinValue = 0,
                    MaxValue = 1,
                };
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue;
            if (minMaxSliderAttribute.MinCallback == null)
            {
                minValue = minMaxSliderAttribute.Min;
            }
            else
            {
                (string getError, float getValue) =
                    Util.GetOf(minMaxSliderAttribute.MinCallback, 0f, property, info, parentTarget);
                // Debug.Log($"get min {getValue} with error {getError}, name={minMaxSliderAttribute.MinCallback} target={parentTarget}/directGet={parentTarget.GetType().GetField(minMaxSliderAttribute.MinCallback).GetValue(parentTarget)}");
                if (!string.IsNullOrEmpty(getError))
                {
                    return new MetaInfo
                    {
                        Error = getError,
                        MinValue = 0,
                        MaxValue = 1,
                    };
                }
                minValue = getValue;
            }

            float maxValue;
            if (minMaxSliderAttribute.MaxCallback == null)
            {
                maxValue = minMaxSliderAttribute.Max;
            }
            else
            {
                (string getError, float getValue) = Util.GetOf(minMaxSliderAttribute.MaxCallback, 0f, property, info, parentTarget);
                if (!string.IsNullOrEmpty(getError))
                {
                    return new MetaInfo
                    {
                        Error = getError,
                        MinValue = 0,
                        MaxValue = 1,
                    };
                }
                maxValue = getValue;
            }

            if (minValue > maxValue)
            {
                return new MetaInfo
                {
                    Error = $"invalid min ({minValue}) max ({maxValue}) value",
                    MinValue = 0,
                    MaxValue = 1,
                };
            }

            if (minMaxSliderAttribute.FreeInput)
            {
                if(property.propertyType == SerializedPropertyType.Vector2)
                {
                    Vector2 curValue = property.vector2Value;
                    minValue = Mathf.Min(minValue, curValue.x);
                    maxValue = Mathf.Max(maxValue, curValue.y);
                }
                else
                {
                    Vector2Int curValue = property.vector2IntValue;
                    minValue = Mathf.Min(minValue, curValue.x);
                    maxValue = Mathf.Max(maxValue, curValue.y);
                }
            }

            return new MetaInfo
            {
                Error = "",
                MinValue = minValue,
                MaxValue = maxValue,
            };
        }


        private static Vector2Int AdjustIntSliderInput(Vector2 changedNewValue, float step, float min, float max)
        {
            if (step <= 0f)
            {
                return new Vector2Int(Mathf.RoundToInt(changedNewValue.x), Mathf.RoundToInt(changedNewValue.y));
            }

            int startStep = Mathf.RoundToInt((changedNewValue.x - min) / step);
            int startValue = Mathf.RoundToInt(min + startStep * Mathf.RoundToInt(step));

            float distance = changedNewValue.y - changedNewValue.x;

            int endValue = Mathf.RoundToInt(startValue + Mathf.RoundToInt(distance / step) * step);
            if (endValue > max)
            {
                endValue = Mathf.RoundToInt(endValue - step);
            }

            return new Vector2Int(startValue, endValue);
        }

        private static Vector2 AdjustFloatSliderInput(Vector2 changedNewValue, float step, float min, float max)
        {
            if (step <= 0f)
            {
                return changedNewValue;
            }

            float startValue = min + Mathf.RoundToInt((changedNewValue.x - min) / step) * step;

            float distance = changedNewValue.y - changedNewValue.x;

            float endValue = startValue + Mathf.RoundToInt(distance / step) * step;
            if (endValue > max)
            {
                endValue -= step;
            }

            return new Vector2(startValue, endValue);
        }

        private static Vector2Int AdjustIntInput(int newValue, int value, float step, float minValue, float maxValue, bool free)
        {
            int startValue = Mathf.Min(newValue, value);
            int endValue = Mathf.Max(newValue, value);
            if (step < 0)
            {
                return free
                    ? new Vector2Int(startValue, endValue)
                    : new Vector2Int(Mathf.RoundToInt(Mathf.Max(startValue, minValue)), Mathf.RoundToInt(Mathf.Min(endValue, maxValue)));
            }

            int startSteppedValue =
                Mathf.RoundToInt(minValue + Mathf.RoundToInt(Mathf.RoundToInt((startValue - minValue) / step) * step));
            if (!free && startSteppedValue < minValue)
            {
                startSteppedValue = Mathf.RoundToInt(minValue);
            }
            int endSteppedValue = startSteppedValue + Mathf.RoundToInt(Mathf.RoundToInt((endValue - startValue) / step) * step);
            if (!free && endSteppedValue > maxValue)
            {
                endSteppedValue = startSteppedValue + Mathf.FloorToInt(Mathf.FloorToInt((maxValue - startSteppedValue) / step) * step);
            }

            return new Vector2Int(startSteppedValue, endSteppedValue);
        }

        private static Vector2 AdjustFloatInput(float newValue, float value, float step, float minValue, float maxValue, bool free)
        {
            float startValue = Mathf.Min(newValue, value);
            float endValue = Mathf.Max(newValue, value);
            if (step < 0)
            {
                return free
                    ? new Vector2(startValue, endValue)
                    : new Vector2(Mathf.Max(startValue, minValue), Mathf.Min(endValue, maxValue));
            }

            float startSteppedValue = minValue + Mathf.RoundToInt((startValue - minValue) / step) * step;
            if (!free && startSteppedValue < minValue)
            {
                startSteppedValue = minValue;
            }
            float endSteppedValue = startSteppedValue + (Mathf.RoundToInt((endValue - startValue) / step) * step);
            if (!free && endSteppedValue > maxValue)
            {
                endSteppedValue = startSteppedValue + (Mathf.FloorToInt((maxValue - startSteppedValue) / step) * step);
            }

            return new Vector2(startSteppedValue, endSteppedValue);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        // ReSharper disable once MemberCanBePrivate.Global
        public class MinMaxSliderField : BaseField<Vector2>
        {
            public MinMaxSliderField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameSlider(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_Slider";
        private static string NameMinInteger(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MinIntegerField";
        private static string NameMaxInteger(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MaxIntegerField";
        private static string NameMinFloat(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MinFloatField";
        private static string NameMaxFloat(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MaxFloatField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_HelpBox";

        private const int InputWidth = 50;

        private record UserData
        {
            public float FreeMin;
            public float FreeMax;
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return null;
            }

            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Vector2 sliderValue = isInt ? property.vector2IntValue : property.vector2Value;

            MinMaxSlider minMaxSlider = new MinMaxSlider(sliderValue.x, sliderValue.y, Mathf.Min(sliderValue.x, sliderValue.y), Mathf.Max(sliderValue.x, sliderValue.y))
            {
                name = NameSlider(property),
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    paddingLeft = 5,
                    paddingRight = 5,
                },
                userData = new UserData
                {
                    FreeMin = sliderValue.x,
                    FreeMax = sliderValue.y,
                },
            };

            if (isInt)
            {
                Vector2Int curValue = property.vector2IntValue;

                root.Add(new IntegerField
                {
                    isDelayed = true,
                    value = curValue.x,
                    name = NameMinInteger(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
                root.Add(minMaxSlider);
                root.Add(new IntegerField
                {
                    isDelayed = true,
                    value = curValue.y,
                    name = NameMaxInteger(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
            }
            else
            {
                Vector2 curValue = property.vector2Value;
                // slider.SetValueWithoutNotify(curValue);

                root.Add(new FloatField
                {
                    isDelayed = true,
                    value = curValue.x,
                    name = NameMinFloat(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
                root.Add(minMaxSlider);
                root.Add(new FloatField
                {
                    isDelayed = true,
                    value = curValue.y,
                    name = NameMaxFloat(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
            }

            MinMaxSliderField minMaxSliderField = new MinMaxSliderField(property.displayName, root);
            minMaxSliderField.labelElement.style.overflow = Overflow.Hidden;
            minMaxSliderField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);

            minMaxSliderField.AddToClassList(ClassAllowDisable);

            return minMaxSliderField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            MinMaxSlider minMaxSlider = container.Q<MinMaxSlider>(NameSlider(property));
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            UserData userData = (UserData)minMaxSlider.userData;

            // That's the KEY!!!!!!!!!!!!!
            minMaxSlider.TrackPropertyValue(property, p =>
            {
                Vector2 newValue = property.propertyType == SerializedPropertyType.Vector2Int ? p.vector2IntValue : p.vector2Value;

                if (newValue.y < newValue.x)
                {
                    newValue.y = newValue.x;
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                Debug.Log($"slider update value {newValue} while self range {minMaxSlider.lowLimit}~{minMaxSlider.highLimit}");
#endif
                if (minMaxSlider.lowLimit > newValue.x)
                {
                    minMaxSlider.lowLimit = newValue.x;
                }

                if (minMaxSlider.highLimit < newValue.y)
                {
                    minMaxSlider.highLimit = newValue.y;
                }

                minMaxSlider.SetValueWithoutNotify(newValue);
            });

            MetaInfo metaInfo = GetMetaInfo(property, minMaxSliderAttribute, info, parent);
            userData.FreeMin = metaInfo.MinValue;
            userData.FreeMax = metaInfo.MaxValue;
            // Debug.Log($"{minMaxSlider.highLimit}~{minMaxSlider.lowLimit}: {metaInfo.MinValue}~{metaInfo.MaxValue}");
            if (metaInfo.MinValue >= minMaxSlider.highLimit)
            {
                minMaxSlider.highLimit = metaInfo.MaxValue;
                minMaxSlider.lowLimit = metaInfo.MinValue;
            }
            else
            {
                minMaxSlider.lowLimit = metaInfo.MinValue;
                minMaxSlider.highLimit = metaInfo.MaxValue;
            }

            AdjustFreeRangeHighAndLow(isInt? property.vector2IntValue: property.vector2Value, property, minMaxSliderAttribute, container, info, parent);

            if (isInt)
            {
                IntegerField minIntField = container.Q<IntegerField>(NameMinInteger(property));
                IntegerField maxIntField = container.Q<IntegerField>(NameMaxInteger(property));
                minMaxSlider.RegisterValueChangedCallback(changed =>
                {
                    float min = userData.FreeMin;
                    float max = userData.FreeMax;
                    Vector2Int inputValue = AdjustIntSliderInput(changed.newValue, minMaxSliderAttribute.Step, min, max);
                    ApplyIntValue(property, inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                });
                minIntField.RegisterValueChangedCallback(changed =>
                {
                    int newValue = changed.newValue;
                    Vector2Int inputValue = AdjustIntInput(newValue, maxIntField.value, minMaxSliderAttribute.Step, (int)userData.FreeMin, (int)userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyIntValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                    // if (minMaxSliderAttribute.FreeInput)
                    // {
                    //     userData.FreeMin = Mathf.Min(userData.FreeMin, inputValue.x);
                    //     userData.FreeMax = Mathf.Max(userData.FreeMax, inputValue.y);
                    //     // Debug.Log($"update min max {userData.FreeMin}~{userData.FreeMax}");
                    // }
                });
                maxIntField.RegisterValueChangedCallback(changed =>
                {

                    int newValue = changed.newValue;
                    Vector2Int inputValue = AdjustIntInput(newValue, minIntField.value, minMaxSliderAttribute.Step, (int)userData.FreeMin, (int)userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyIntValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                    // if (minMaxSliderAttribute.FreeInput)
                    // {
                    //     userData.FreeMin = Mathf.Min(userData.FreeMin, inputValue.x);
                    //     userData.FreeMax = Mathf.Max(userData.FreeMax, inputValue.y);
                    //     // Debug.Log($"update min max {userData.FreeMin}~{userData.FreeMax}");
                    // }
                });
            }
            else
            {
                FloatField minFloatField = container.Q<FloatField>(NameMinFloat(property));
                FloatField maxFloatField = container.Q<FloatField>(NameMaxFloat(property));

                minMaxSlider.RegisterValueChangedCallback(changed =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                    Debug.Log($"slider changed: {changed.newValue}");
#endif
                    float min = userData.FreeMin;
                    float max = userData.FreeMax;
                    Vector2 inputValue = AdjustFloatSliderInput(changed.newValue, minMaxSliderAttribute.Step, min, max);
                    ApplyFloatValue(property, inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                });
                minFloatField.RegisterValueChangedCallback(changed =>
                {
                    float newValue = changed.newValue;
                    Vector2 inputValue = AdjustFloatInput(newValue, maxFloatField.value, minMaxSliderAttribute.Step, userData.FreeMin, userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyFloatValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                    Debug.Log($"minFloat onChange changed={newValue}, maxValue={maxFloatField.value}, inputValue={inputValue}, {userData.FreeMin}, {userData.FreeMax}");
#endif
                });
                maxFloatField.RegisterValueChangedCallback(changed =>
                {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                    Debug.Log($"changed={changed.newValue}, minValue={minFloatField.value}");
#endif
                    float newValue = changed.newValue;
                    Vector2 inputValue = AdjustFloatInput(newValue, minFloatField.value, minMaxSliderAttribute.Step, userData.FreeMin, userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyFloatValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                    // if (minMaxSliderAttribute.FreeInput)
                    // {
                    //     userData.FreeMin = Mathf.Min(userData.FreeMin, inputValue.x);
                    //     userData.FreeMax = Mathf.Max(userData.FreeMax, inputValue.y);
                    // }
                });
            }
        }

        // TODO: TrackPropertyValue is better than this
        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            AdjustFreeRangeHighAndLow(isInt? property.vector2IntValue: property.vector2Value, property, minMaxSliderAttribute, container, info, parent);

            if (isInt)
            {
                Vector2Int value = property.vector2IntValue;
                IntegerField minIntField = container.Q<IntegerField>(NameMinInteger(property));
                IntegerField maxIntField = container.Q<IntegerField>(NameMaxInteger(property));
                if (minIntField.value != value.x)
                {
                    minIntField.SetValueWithoutNotify(value.x);
                }

                if (maxIntField.value != value.y)
                {
                    maxIntField.SetValueWithoutNotify(value.y);
                }
            }
            else
            {
                Vector2 value = property.vector2Value;
                FloatField minFloatField = container.Q<FloatField>(NameMinFloat(property));
                FloatField maxFloatField = container.Q<FloatField>(NameMaxFloat(property));
                if (!Mathf.Approximately(minFloatField.value, value.x))
                {
                    minFloatField.SetValueWithoutNotify(value.x);
                }

                if (!Mathf.Approximately(maxFloatField.value, value.y))
                {
                    maxFloatField.SetValueWithoutNotify(value.y);
                }
            }
        }

        private static void AdjustFreeRangeHighAndLow(Vector2 newValue, SerializedProperty property, MinMaxSliderAttribute minMaxSliderAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;
            MinMaxSlider minMaxSlider = container.Q<MinMaxSlider>(NameSlider(property));
            UserData userData = (UserData)minMaxSlider.userData;

            MetaInfo metaInfo = GetMetaInfo(property, minMaxSliderAttribute, info, parent);

            if (isInt)
            {
                Vector2Int value = property.vector2IntValue;
                Vector2Int sliderValue = value.y < value.x
                    ? new Vector2Int(value.x, value.x)
                    : value;

                // if the new value is out of range, let's expand the low/high limit. But we need to stick to the `step` for low if it's a external change
                // high is free, we don't care about it because the setting value process with handle it
                // this is not fun, at all
                float step = minMaxSliderAttribute.Step;
                if (minMaxSliderAttribute.FreeInput)
                {
                    int leftValue = Mathf.RoundToInt(sliderValue.x);
                    float originMin = userData.FreeMin;
                    // Debug.Log($"originMin={originMin}, metaInfo.MinValue={metaInfo.MinValue} leftValue={leftValue}");
                    bool leftValueOutOfRange = leftValue < originMin;
                    bool leftMetaAdjust = false;
                    if(metaInfo.Error == "")
                    {
                        int intMetaMinValue = Mathf.RoundToInt(metaInfo.MinValue);
                        if(intMetaMinValue > leftValue)
                        {
                            leftValue = intMetaMinValue;
                            leftMetaAdjust = true;
                        }
                        // else if(intMetaMinValue > originMin && intMetaMinValue < leftValue)
                        // {
                        //     leftMetaAdjust = true;
                        // }
                    }

                    if (leftValueOutOfRange || leftMetaAdjust)
                    {
                        if(leftValueOutOfRange && step > 0)
                        {
                            int newMin = Mathf.RoundToInt(originMin + Mathf.FloorToInt((leftValue - originMin) / step) * step);
                            minMaxSlider.lowLimit = userData.FreeMin = newMin;
                        }
                        else
                        {
                            minMaxSlider.lowLimit = userData.FreeMin = Mathf.Min(leftValue, metaInfo.MinValue);
                        }
                    }

                    float rightValue = sliderValue.y;
                    float useMax = Mathf.Max(rightValue, userData.FreeMax, metaInfo.MaxValue);
                    if (!Mathf.Approximately(minMaxSlider.highLimit, useMax))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = useMax;
                    }
                }
                else if(metaInfo.Error == "")  // for non-free input, the limit is fixed
                {
                    if(!Mathf.Approximately(minMaxSlider.lowLimit, metaInfo.MinValue))
                    {
                        minMaxSlider.lowLimit = userData.FreeMin = metaInfo.MinValue;
                    }
                    if(!Mathf.Approximately(minMaxSlider.highLimit, metaInfo.MaxValue))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = metaInfo.MaxValue;
                    }
                }
            }
            else
            {
                Vector2 value = property.vector2Value;

                Vector2 sliderValue = value.y < value.x
                    ? new Vector2(value.x, value.x)
                    : value;

                // if the new value is out of range, let's expand the low/high limit. But we need to stick to the `step` for low if it's a external change
                // high is free, we don't care about it because the setting value process with handle it
                // this is not fun, at all
                float step = minMaxSliderAttribute.Step;
                if (minMaxSliderAttribute.FreeInput)
                {
                    float leftValue = sliderValue.x;
                    float originMin = userData.FreeMin;
                    // Debug.Log($"originMin={originMin}, metaInfo.MinValue={metaInfo.MinValue} leftValue={leftValue}");
                    bool leftValueOutOfRange = leftValue < originMin;
                    bool leftMetaAdjust = false;
                    if(metaInfo.Error == "")
                    {
                        if(metaInfo.MinValue > leftValue)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                            Debug.Log($"leftMetaAdjust as metaInfo.MinValue {metaInfo.MinValue} > leftValue {leftValue}");
#endif
                            leftValue = metaInfo.MinValue;
                            leftMetaAdjust = true;
                        }
//                         else if(metaInfo.MinValue > originMin && metaInfo.MinValue < leftValue)
//                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
//                             Debug.Log($"leftMetaAdjust as metaInfo.MinValue {metaInfo.MinValue} out of {leftValue} ~ {originMin}");
// #endif
//                             leftMetaAdjust = true;
//                         }
                    }

                    if (leftValueOutOfRange || leftMetaAdjust)
                    {
                        if(leftValueOutOfRange && step > 0)
                        {
                            float newMin = originMin + Mathf.FloorToInt((leftValue - originMin) / step) * step;
                            minMaxSlider.lowLimit = userData.FreeMin = newMin;
                        }
                        else
                        {
                            minMaxSlider.lowLimit = userData.FreeMin = Mathf.Min(leftValue, metaInfo.MinValue);
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"lowLimit changed {minMaxSlider.lowLimit}: leftValueOutOfRange={leftValueOutOfRange}, leftMetaAdjust={leftMetaAdjust}");
#endif
                    }

                    float rightValue = sliderValue.y;
                    float useMax = Mathf.Max(rightValue, userData.FreeMax, metaInfo.MaxValue);
                    if (!Mathf.Approximately(minMaxSlider.highLimit, useMax))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = useMax;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"set highLimit to {useMax}, rightValue={rightValue}, userData.FreeMax={userData.FreeMax}, metaInfo.MaxValue={metaInfo.MaxValue}");
#endif
                    }
                }
                else if(metaInfo.Error == "")  // for non-free input, the limit is fixed
                {
                    if(!Mathf.Approximately(minMaxSlider.lowLimit, metaInfo.MinValue))
                    {
                        minMaxSlider.lowLimit = userData.FreeMin = metaInfo.MinValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"lowLimit changed {minMaxSlider.lowLimit}");
#endif

                    }
                    if(!Mathf.Approximately(minMaxSlider.highLimit, metaInfo.MaxValue))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = metaInfo.MaxValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"highLimit changed {minMaxSlider.highLimit}");
#endif

                    }
                }
            }
        }

        private static void ApplyIntValue(SerializedProperty property, Vector2Int sliderValue, Action<object> onValueChangedCallback, MinMaxSliderAttribute sliderAttribute, VisualElement container, FieldInfo info, object parent)
        {
            Vector2Int vector2IntValue = sliderValue;

            AdjustFreeRangeHighAndLow(sliderValue, property, sliderAttribute, container, info, parent);
            property.vector2IntValue = vector2IntValue;
            property.serializedObject.ApplyModifiedProperties();
            onValueChangedCallback.Invoke(vector2IntValue);

            // if (freeInput)
            // {
            //     if(slider.lowLimit > vector2IntValue.x)
            //     {
            //         // Debug.Log($"free adjust low {slider.lowLimit} -> {vector2IntValue.x}");
            //         slider.lowLimit = vector2IntValue.x;
            //     }
            //     if(slider.highLimit < vector2IntValue.y)
            //     {
            //         // Debug.Log($"free adjust high {slider.highLimit} -> {vector2IntValue.y}");
            //         slider.highLimit = vector2IntValue.y;
            //     }
            // }

            // slider.SetValueWithoutNotify(vector2IntValue);
            // minField.SetValueWithoutNotify(vector2IntValue.x);
            // maxField.SetValueWithoutNotify(vector2IntValue.y);
        }

        private static void ApplyFloatValue(SerializedProperty property, Vector2 sliderValue, Action<object> onValueChangedCallback, MinMaxSliderAttribute sliderAttribute, VisualElement container, FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
            Debug.Log($"apply  {sliderValue}");
#endif
            AdjustFreeRangeHighAndLow(sliderValue, property, sliderAttribute, container, info, parent);
            property.vector2Value = sliderValue;
            // Debug.Log($"apply float {vector2Value}");
            property.serializedObject.ApplyModifiedProperties();
            onValueChangedCallback.Invoke(sliderValue);

            // if (freeInput)
            // {
            //     if(slider.minValue > sliderValue.x)
            //     {
            //         slider.lowLimit = sliderValue.x;
            //     }
            //     if(slider.maxValue < sliderValue.y)
            //     {
            //         slider.highLimit = sliderValue.y;
            //     }
            // }

            // slider.SetValueWithoutNotify(sliderValue);
            // minField.SetValueWithoutNotify(sliderValue.x);
            // maxField.SetValueWithoutNotify(sliderValue.y);
        }

        #endregion

#endif
    }
}
