using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer: SaintsPropertyDrawer
    {
        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;

            public float Min;  // dynamic
            public float Max;  // dynamic
            public Color Color;
            public Color BackgroundColor;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute) saintsAttribute;

            float min = progressBarAttribute.Min;
            if(progressBarAttribute.MinCallback != null)
            {
                (string error, float value) = Util.GetOf(progressBarAttribute.MinCallback, 0f, property, info, parent);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                    };
                }
                min = value;
            }

            float max = progressBarAttribute.Max;
            if(progressBarAttribute.MaxCallback != null)
            {
                (string error, float value) = Util.GetOf(progressBarAttribute.MaxCallback, 0f, property, info, parent);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                    };
                }
                max = value;
            }

            Color color = progressBarAttribute.Color.GetColor();

            if(progressBarAttribute.ColorCallback != null)
            {
                (string error, Color value) =
                    GetCallbackColor(progressBarAttribute.ColorCallback, color, property, info, parent);

                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                    };
                }
                color = value;
            }

            Color backgroundColor = progressBarAttribute.BackgroundColor.GetColor();
            // ReSharper disable once InvertIf
            if(progressBarAttribute.BackgroundColorCallback != null)
            {
                (string error, Color value) = GetCallbackColor(progressBarAttribute.BackgroundColorCallback, backgroundColor, property, info, parent);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                    };
                }
                backgroundColor = value;
            }

            return new MetaInfo
            {
                Error = "",
                Min = min,
                Max = max,
                Color = color,
                BackgroundColor = backgroundColor,
                // Title = title,
            };
        }

        private static (string error, Color value) GetCallbackColor(string by, Color defaultValue, SerializedProperty property, FieldInfo fieldInfo, object target)
        {
            (string error, object value) = Util.GetOf<object>(by, defaultValue, property, fieldInfo, target);
            return error != ""
                ? (error, defaultValue)
                : ObjToColor(value);
        }

        private static (string error, Color color) ObjToColor(object obj)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (obj)
            {
                case Color color:
                    return ("", color);
                case string str:
                    return ("", Colors.GetColorByStringPresent(str));
                case EColor eColor:
                    return ("", eColor.GetColor());
                default:
                    return ($"target is not a color: {obj}", Color.white);
            }
        }

        private static (string error, string title) GetTitle(SerializedProperty property, string titleCallback, float step, float curValue, float minValue, float maxValue, object parent)
        {
            if (titleCallback == null)
            {
                if(property.propertyType == SerializedPropertyType.Integer)
                {
                    return ("", $"{(int)curValue} / {(int)maxValue}");
                }

                if (step <= 0)
                {
                    return ("", $"{curValue} / {maxValue}");
                }

                string valueStr = step.ToString(System.Globalization.CultureInfo.InvariantCulture);
                int decimalPointIndex = valueStr.IndexOf(System.Globalization.CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);

                int decimalPlaces = 0;

                if (decimalPointIndex >= 0)
                {
                    decimalPlaces = valueStr.Length - decimalPointIndex - 1;
                }

                // string formatValue = curValue.ToString("F" + decimalPlaces);
                string formatValue = curValue.ToString($"0.{new string('#', decimalPlaces)}");
                // Debug.Log($"curValue={curValue}, format={formatValue}");

                return ("", $"{formatValue} / {maxValue}");
            }

            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(parent);
            types.Reverse();

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Type type in types)
            {
                MethodInfo methodInfo = type.GetMethod(titleCallback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                string title;
                try
                {
                    title = (string)methodInfo.Invoke(parent,
                        new object[] { curValue, minValue, maxValue, property.displayName });
                }
                catch (TargetInvocationException e)
                {
                    Debug.Assert(e.InnerException != null);
                    Debug.LogException(e);
                    return (e.InnerException.Message, null);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return (e.Message, null);
                }

                return ("", title);
            }

            return ($"Can not find method `{titleCallback}` on `{parent}`", null);
        }

        private static float BoundValue(float curValue, float minValue, float maxValue, float step, bool isInt)
        {
            float wrapCurValue = isInt
                ? Mathf.RoundToInt(curValue)
                : curValue;

            return step <= 0
                ? Mathf.Clamp(wrapCurValue, minValue, maxValue)
                : Util.BoundFloatStep(wrapCurValue, minValue, maxValue, step);
        }

        #region IMGUI

        private string _imGuiError = "";

        // private bool _imgGuiMousePressed;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private readonly Dictionary<string, bool> inArrayMousePressed = new Dictionary<string, bool>();

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload,
            FieldInfo info,
            object parent)
        {
            string arrayKey = $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";
            if(!inArrayMousePressed.ContainsKey(arrayKey))
            {
                inArrayMousePressed[arrayKey] = false;
            }

            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

            int controlId = GUIUtility.GetControlID(FocusType.Passive, position);
            // Debug.Log(label.text.Length);
            Rect fieldRect = EditorGUI.PrefixLabel(position, controlId, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - fieldRect.width,
            };
            // EditorGUI.DrawRect(position, Color.yellow);

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            _imGuiError = metaInfo.Error;

            EditorGUI.DrawRect(fieldRect, metaInfo.BackgroundColor);

            bool isInt = property.propertyType == SerializedPropertyType.Integer;
            float curValue = isInt
                ? property.intValue
                : property.floatValue;

            // float percent = Mathf.Clamp01(curValue / (metaInfo.Max - metaInfo.Min));
            float percent = Mathf.InverseLerp(metaInfo.Min, metaInfo.Max, curValue);
            // Debug.Log($"percent={percent:P}");
            Rect fillRect = new Rect(fieldRect)
            {
                width = fieldRect.width * percent,
            };

            EditorGUI.DrawRect(fillRect, metaInfo.Color);

            if (GUI.enabled)
            {
                EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
                EditorGUIUtility.AddCursorRect(fieldRect, MouseCursor.SlideArrow);
            }

            Event e = Event.current;

// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
//             Debug.Log($"{e.isMouse}, {e.mousePosition}");
// #endif
            // ReSharper disable once InvertIf
            // Debug.Log($"{e.type} {e.isMouse}, {e.button}, {e.mousePosition}");

            if(e.type == EventType.MouseUp && e.button == 0)
            {
                // GUIUtility.hotControl = 0;
                // Debug.Log($"UP!");
                inArrayMousePressed[arrayKey] = false;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
                Debug.Log($"mouse up {property.propertyPath}: {inArrayMousePressed[arrayKey]}");
#endif
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // arrayMousePressed[arrayIndex] = position.Contains(e.mousePosition);
                inArrayMousePressed[arrayKey] = position.Contains(e.mousePosition);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
                Debug.Log($"mouse down {position}: {inArrayMousePressed[arrayKey]}/{property.propertyPath}");
#endif
            }

            (string titleError, string title) = GetTitle(property, progressBarAttribute.TitleCallback, progressBarAttribute.Step, curValue, metaInfo.Min, metaInfo.Max, parent);
            if(_imGuiError == "")
            {
                _imGuiError = titleError;
            }

            // string title = null;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
            Debug.Log($"{property.propertyPath}/{inArrayMousePressed[arrayKey]}/{GetHashCode()}");
#endif

            if (GUI.enabled && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && inArrayMousePressed[arrayKey])
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
                Debug.Log($"{property.propertyPath}/{inArrayMousePressed[arrayKey]}");
#endif
                float newPercent = (e.mousePosition.x - fieldRect.x) / fieldRect.width;
                float newValue = Mathf.Lerp(metaInfo.Min, metaInfo.Max, newPercent);
                float boundValue = BoundValue(newValue, metaInfo.Min, metaInfo.Max, progressBarAttribute.Step, isInt);

                // Debug.Log($"boundValue={boundValue}, newValue={newValue}");

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (boundValue != curValue)
                {
                    if (isInt)
                    {
                        property.intValue = (int)boundValue;
                        onGUIPayload.SetValue((int)boundValue);
                    }
                    else
                    {
                        property.floatValue = boundValue;
                        onGUIPayload.SetValue(boundValue);
                    }
                    if(ExpandableIMGUIScoop.IsInScoop)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }

                    (string titleError, string title) changedTitle = GetTitle(property, progressBarAttribute.TitleCallback, progressBarAttribute.Step, boundValue, metaInfo.Min, metaInfo.Max, parent);
                    if (_imGuiError == "")
                    {
                        _imGuiError = changedTitle.titleError;
                    }

                    title = changedTitle.title;

                    // Debug.Log($"value={newValue}, title={title}");
                }
            }

            // _imGuiError = titleError;
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUI.DropShadowLabel(fieldRect, title);
            }
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit

        private class UIToolkitPayload
        {
            // ReSharper disable once InconsistentNaming
            public readonly VisualElement Background;
            // ReSharper disable once InconsistentNaming
            public readonly VisualElement Progress;
            public MetaInfo metaInfo;

            public UIToolkitPayload(VisualElement background, VisualElement progress, MetaInfo metaInfo)
            {
                Background = background;
                Progress = progress;
                this.metaInfo = metaInfo;
            }
        }

        public class ProgressBarField : BaseField<float>
        {
            public ProgressBarField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameProgressBar(SerializedProperty property) => $"{property.propertyPath}__ProgressBar";
        // private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__ProgressBar_Label";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__ProgressBar_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            #region ProgrssBar

            float propValue = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : property.floatValue;

            ProgressBar progressBar = new ProgressBar
            {
                name = NameProgressBar(property),

                title = GetTitle(property, progressBarAttribute.TitleCallback, progressBarAttribute.Step, propValue, metaInfo.Min, metaInfo.Max, parent).title,
                lowValue = 0,
                highValue = metaInfo.Max - metaInfo.Min,
                value = propValue - metaInfo.Min,

                style =
                {
                    flexGrow = 1,
                },
            };

            Type type = typeof(AbstractProgressBar);
            FieldInfo backgroundFieldInfo = type.GetField("m_Background", BindingFlags.NonPublic | BindingFlags.Instance);

            VisualElement background = null;
            if (backgroundFieldInfo != null)
            {
                background = (VisualElement) backgroundFieldInfo.GetValue(progressBar);
                background.style.backgroundColor = metaInfo.BackgroundColor;
            }

            FieldInfo progressFieldInfo = type.GetField("m_Progress", BindingFlags.NonPublic | BindingFlags.Instance);
            VisualElement progress = null;
            if(progressFieldInfo != null)
            {
                progress = (VisualElement) progressFieldInfo.GetValue(progressBar);
                progress.style.backgroundColor = metaInfo.Color;
            }

            progressBar.userData = new UIToolkitPayload(background, progress, metaInfo);
            #endregion


            ProgressBarField progressBarField = new ProgressBarField(property.displayName, progressBar)
            {
                name = NameProgressBar(property),
            };
            progressBarField.AddToClassList(ProgressBarField.alignedFieldUssClassName);

            progressBarField.AddToClassList(ClassAllowDisable);

            return progressBarField;

        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            ProgressBar progressBar = container.Q<ProgressBar>(NameProgressBar(property));

            progressBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                progressBar.CapturePointer(0);
                OnProgressBarInteract(property, (ProgressBarAttribute)saintsAttribute, container, progressBar, evt.localPosition, onValueChangedCallback, info, parent);
            });
            progressBar.RegisterCallback<PointerUpEvent>(_ =>
            {
                progressBar.ReleasePointer(0);
            });
            progressBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if(progressBar.HasPointerCapture(0))
                {
                    OnProgressBarInteract(property, (ProgressBarAttribute)saintsAttribute, container, progressBar,
                        evt.localPosition, onValueChangedCallback, info, parent);
                }
            });
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            ProgressBar progressBar = container.Q<ProgressBar>(NameProgressBar(property));
            UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;
            MetaInfo oldMetaInfo = uiToolkitPayload.metaInfo;

            bool changed = false;
            string error = metaInfo.Error;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(metaInfo.Min != oldMetaInfo.Min
               // ReSharper disable once CompareOfFloatsByEqualityOperator
               || metaInfo.Max != oldMetaInfo.Max)
            {
                changed = true;
                progressBar.highValue = metaInfo.Max - metaInfo.Min;
                float propValue = property.propertyType == SerializedPropertyType.Integer
                    ? property.intValue
                    : property.floatValue;

                ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

                if(propValue < metaInfo.Min || propValue > metaInfo.Max)
                {
                    // Debug.Log($"update change: {metaInfo.Min} <= {propValue} <= {metaInfo.Max}");
                    propValue = ChangeValue(property, progressBarAttribute, container, progressBar, Mathf.Clamp(propValue, metaInfo.Min, metaInfo.Max), metaInfo.Min, metaInfo.Max,
                        onValueChanged, info, parent);
                    // Debug.Log($"now prop = {propValue}");
                }

                progressBar.value = propValue - metaInfo.Min;

                (string titleError, string title) = GetTitle(property, progressBarAttribute.TitleCallback, progressBarAttribute.Step, propValue, metaInfo.Min, metaInfo.Max, parent);
                // Debug.Log($"update change title {title}");
                progressBar.title = title;
                if (titleError != "")
                {
                    error = titleError;
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
            Debug.Log($"oldColor={oldMetaInfo.Color}/newColor={metaInfo.Color}, progress={uiToolkitPayload.Progress}");
#endif
            if(metaInfo.Color != oldMetaInfo.Color && uiToolkitPayload.Progress != null)
            {
                changed = true;
                uiToolkitPayload.Progress.style.backgroundColor = metaInfo.Color;
            }

            if(metaInfo.BackgroundColor != oldMetaInfo.BackgroundColor && uiToolkitPayload.Background != null)
            {
                changed = true;
                uiToolkitPayload.Background.style.backgroundColor = metaInfo.BackgroundColor;
            }

            if (changed)
            {
                // progressBar.userData = metaInfo;
                uiToolkitPayload.metaInfo = metaInfo;
            }

            UpdateHelpBox(property, container, error);
        }

        private static void OnProgressBarInteract(SerializedProperty property, ProgressBarAttribute progressBarAttribute, VisualElement container, ProgressBar progressBar, Vector3 mousePosition, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            float curWidth = progressBar.resolvedStyle.width;
            if(float.IsNaN(curWidth))
            {
                return;
            }

            UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;

            float curValue = Mathf.Lerp(uiToolkitPayload.metaInfo.Min, uiToolkitPayload.metaInfo.Max, mousePosition.x / curWidth);
            // Debug.Log(curValue);
            ChangeValue(property, progressBarAttribute, container, progressBar, curValue, uiToolkitPayload.metaInfo.Min, uiToolkitPayload.metaInfo.Max, onValueChangedCallback, info, parent);
        }

        private static float ChangeValue(SerializedProperty property, ProgressBarAttribute progressBarAttribute, VisualElement container, ProgressBar progressBar, float curValue, float minValue, float maxValue, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;

            bool isInt = property.propertyType == SerializedPropertyType.Integer;

            float newValue = BoundValue(curValue, minValue, maxValue, progressBarAttribute.Step, isInt);

            // float wrapNewValue =

            // Debug.Log($"curValue={curValue}, newValue={newValue}");
            float propValue = isInt
                ? property.intValue
                : property.floatValue;

            // Debug.Log($"curValue={curValue}, newValue={newValue}, propValue={propValue}");
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (propValue == newValue)
            {
                return propValue;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int intValue = (int)newValue;
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, intValue);
                property.intValue = intValue;
                property.serializedObject.ApplyModifiedProperties();
                progressBar.SetValueWithoutNotify(intValue - minValue);
                onValueChangedCallback.Invoke(intValue);
            }
            else
            {
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
                property.floatValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
                progressBar.SetValueWithoutNotify(newValue - minValue);
                onValueChangedCallback.Invoke(newValue);
            }

            (string error, string title) = GetTitle(property, progressBarAttribute.TitleCallback, progressBarAttribute.Step, newValue, minValue, maxValue, parent);
            // Debug.Log($"change title to {title}");
            progressBar.title = title;
            UpdateHelpBox(property, container, error);

            return newValue;
        }

        private static void UpdateHelpBox(SerializedProperty property, VisualElement container, string error)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            if (helpBox.text == error)
            {
                return;
            }
            helpBox.text = error;
            helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        #endregion

#endif
    }
}
