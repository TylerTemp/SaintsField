using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer: SaintsPropertyDrawer
    {
        private struct MetaInfo
        {
            public string Error;

            public float Min;  // dynamic
            public float Max;  // dynamic
            public Color Color;
            public Color BackgroundColor;
            public string Title;
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, float curValue, object parent)
        {
            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute) saintsAttribute;

            float min = progressBarAttribute.Min;
            if(progressBarAttribute.MinCallback != null)
            {
                (string error, float value) = Util.GetCallbackFloat(parent, progressBarAttribute.MinCallback);
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
                (string error, float value) = Util.GetCallbackFloat(parent, progressBarAttribute.MaxCallback);
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
                (string error, Color value) = GetCallbackColor(parent, progressBarAttribute.ColorCallback);
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
            if(progressBarAttribute.BackgroundColorCallback != null)
            {
                (string error, Color value) = GetCallbackColor(parent, progressBarAttribute.BackgroundColorCallback);
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

            string title;
            if (progressBarAttribute.TitleCallback != null)
            {
                const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly;

                MethodInfo methodInfo = parent.GetType().GetMethod(progressBarAttribute.TitleCallback, bindAttr);

                if (methodInfo == null)
                {
                    return new MetaInfo
                    {
                        Error = $"Can not find method `{progressBarAttribute.TitleCallback}` on `{parent}`",
                        Max = 100f,
                    };
                }

                try
                {
                    title = (string)methodInfo.Invoke(parent,
                        new object[]{curValue, property.displayName});
                }
                catch (TargetInvocationException e)
                {
                    Debug.Assert(e.InnerException != null);
                    Debug.LogException(e);
                    return new MetaInfo
                    {
                        Error = e.InnerException.Message,
                        Max = 100f,
                    };
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return new MetaInfo
                    {
                        Error = e.Message,
                        Max = 100f,
                    };
                }
            }
            else
            {
                title = $"{curValue} / {max}";
            }

            return new MetaInfo
            {
                Error = "",
                Min = min,
                Max = max,
                Color = color,
                BackgroundColor = backgroundColor,
                Title = title,
            };
        }

        public static (string error, Color value) GetCallbackColor(object target, string by)
        {
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (found.Item1 == ReflectUtils.GetPropType.NotFound)
            {
                return ($"No field or method named `{by}` found on `{target}`", Color.white);
            }

            if (found.Item1 == ReflectUtils.GetPropType.Property)
            {
                return ObjToColor(((PropertyInfo)found.Item2).GetValue(target));
            }
            if (found.Item1 == ReflectUtils.GetPropType.Field)
            {
                return ObjToColor(((FieldInfo)found.Item2).GetValue(target));
            }
            // ReSharper disable once InvertIf
            if (found.Item1 == ReflectUtils.GetPropType.Method)
            {
                MethodInfo methodInfo = (MethodInfo)found.Item2;
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                Debug.Assert(methodParams.All(p => p.IsOptional));
                // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                return ObjToColor(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
            }
            throw new ArgumentOutOfRangeException(nameof(found), found, null);
        }

        private static (string error, Color color) ObjToColor(object obj)
        {
            if (obj is Color color)
            {
                return ("", color);
            }
            if (obj is string str)
            {
                return ("", Colors.GetColorByStringPresent(str));
            }

            if (obj is EColor eColor)
            {
                return ("", eColor.GetColor());
            }

            return ($"target is not a color: {obj}", Color.white);
        }

        #region IMGUI
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            object parent)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive, position);
            Rect fieldRect = EditorGUI.PrefixLabel(position, controlId, label);
            // EditorGUI.DrawRect(position, Color.yellow);
            EditorGUI.DrawRect(fieldRect, EColor.Blue.GetColor());

            float curValue = property.floatValue;
            float percent = curValue / 100f;
            Rect fillRect = RectUtils.SplitWidthRect(fieldRect, fieldRect.width * percent).leftRect;

            EditorGUI.DrawRect(fillRect, EColor.Green.GetColor());

            Event e = Event.current;
            // Debug.Log($"{e.isMouse}, {e.mousePosition}");
            // ReSharper disable once InvertIf
            // Debug.Log($"{e.type} {e.isMouse}, {e.button}, {e.mousePosition}");

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && fieldRect.Contains(e.mousePosition))
            {
                float newValue = (e.mousePosition.x - fieldRect.x) / fieldRect.width * 100f;
                property.floatValue = newValue;
                SetValueChanged(property);
            }

            EditorGUI.DropShadowLabel(fieldRect, $"{curValue:0.00}%");

            // if(e.type == EventType.MouseDrag && )

            // if (position.Contains(e.mousePosition))
            // {
            //     Debug.Log($"cap: {e.type} {e.isMouse}, {e.button}, {e.mousePosition}");
            // }
        }
        #endregion

        #region UI Toolkit

        private class UIToolkitPayload
        {
            public bool isMouseDown;
            public readonly VisualElement Background;
            public readonly VisualElement Progress;
            public MetaInfo metaInfo;

            public UIToolkitPayload(VisualElement background, VisualElement progress, MetaInfo metaInfo)
            {
                Background = background;
                Progress = progress;
                isMouseDown = false;
                this.metaInfo = metaInfo;
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(
                property,
                saintsAttribute,
                property.propertyType == SerializedPropertyType.Integer? property.intValue: property.floatValue ,
                parent);

            ProgressBar progressBar = new ProgressBar
            {
                title = metaInfo.Title,
                lowValue = 0,
                highValue = metaInfo.Max - metaInfo.Min,
                value = property.floatValue,

                // style =
                // {
                //     color = EColor.Green.GetColor(),
                //     backgroundColor = EColor.Green.GetColor(),
                // },
            };

            Type type = typeof(AbstractProgressBar);
            FieldInfo backgroundFieldInfo = type.GetField("m_Background", BindingFlags.NonPublic | BindingFlags.Instance);

            VisualElement background = null;
            if (backgroundFieldInfo != null)
            {
                background = (VisualElement) backgroundFieldInfo.GetValue(progressBar);
                // background.style.backgroundColor = EColor.Aqua.GetColor();
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

            progressBar.CapturePointer(0);
            progressBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                ((UIToolkitPayload)progressBar.userData).isMouseDown = true;
                // Debug.Log($"down: {evt.pointerId}");
            });
            progressBar.RegisterCallback<PointerUpEvent>(evt =>
            {
                ((UIToolkitPayload)progressBar.userData).isMouseDown = false;
                // Debug.Log($"up: {evt.pointerId}");
            });
            progressBar.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                ((UIToolkitPayload)progressBar.userData).isMouseDown = true;
                // Debug.Log($"leave: {evt.pointerId}");
            });
            progressBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                // Debug.Log(evt.localPosition);
                // Debug.Log(evt.pointerId);
                // Debug.Log(progressBar.HasPointerCapture(0));

                float curWidth = progressBar.resolvedStyle.width;
                if(float.IsNaN(curWidth))
                {
                    return;
                }

                UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;
                if (!uiToolkitPayload.isMouseDown)
                {
                    return;
                }

                float curValue = evt.localPosition.x / curWidth * 100f + uiToolkitPayload.metaInfo.Min;
                progressBar.value = curValue;
            });

            // Debug.Log(progressBar.resolvedStyle.width);
            //
            progressBar.RegisterValueChangedCallback(evt =>
            {
                property.floatValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
                progressBar.title = evt.newValue.ToString();
                Debug.Log(evt.newValue);
            });
            //
            // progressBar.schedule.Execute(() =>
            // {
            //     progressBar.value += 2f;
            // }).Every(75).Until(() => progressBar.value >= 100f);
            //
            return progressBar;

            // return new Slider();
        }
        #endregion
    }
}
