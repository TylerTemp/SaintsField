using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    public abstract class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
        protected string _error = "";
        private string _execError = "";

        protected string DisplayError {
            get
            {
                if (_error != "" && _execError != "")
                {
                    return $"{_error}\n\n{_execError}";
                }
                return $"{_error}{_execError}";
            }
        }

        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
        // private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~DecButtonAttributeDrawer()
        {
            RichTextDrawer.Dispose();
        }

        // protected float GetExtraHeight(SerializedProperty property, GUIContent label,
        //     float width,
        //     ISaintsAttribute saintsAttribute)
        // {
        //     float result = EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: HelpBox.GetHeight(DisplayError, width));
        //     // Debug.Log($"AboveButtonAttributeDrawer.GetAboveExtraHeight={result}/{DisplayError}");
        //     return result;
        // }

        private const BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                                          BindingFlags.Public | BindingFlags.DeclaredOnly;

        #region IMGUI
        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, object target)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            // object target = GetParentTarget(property);
            Type objType = target.GetType();
            (string error, string buttonLabelXml) = GetButtonLabelXml(decButtonAttribute, target, objType);
            _error = error;

            if (GUI.Button(buttonRect, string.Empty))
            {
                _execError = CallButtonFunc(decButtonAttribute, target, objType);
            }

            IReadOnlyList<RichTextDrawer.RichTextChunk> richChunks;
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (buttonLabelXml is null)
            {
                buttonLabelXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
                richChunks = new[]
                {
                    new RichTextDrawer.RichTextChunk
                    {
                        IsIcon = false,
                        Content = buttonLabelXml,
                    },
                };
            }
            else
            {
                richChunks = RichTextDrawer.ParseRichXml(buttonLabelXml, label.text).ToArray();
            }

            // GetWidth
            float textWidth = RichTextDrawer.GetWidth(label, buttonRect.height, richChunks);
            Rect labelRect = buttonRect;
            if (textWidth < labelRect.width)
            {
                float space = (labelRect.width - textWidth) / 2f;
                labelRect.x += space;
            }
            RichTextDrawer.DrawChunks(labelRect, label, richChunks);

            return leftRect;

        }
        #endregion

        private static string CallButtonFunc(DecButtonAttribute aboveButtonAttribute, object target, Type objType)
        {
            MethodInfo callMethodInfo = objType.GetMethod(aboveButtonAttribute.FuncName, BindAttr);

            if (callMethodInfo == null)
            {
                return $"No field or method named `{aboveButtonAttribute.FuncName}` found on `{target}`";
            }

            ParameterInfo[] methodParams = callMethodInfo.GetParameters();
            Debug.Assert(methodParams.All(p => p.IsOptional));
            try
            {
                callMethodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
            }
            catch (TargetInvocationException e)
            {
                Debug.LogException(e);

                Debug.Assert(e.InnerException != null);
                return e.InnerException.Message;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return e.Message;
            }

            return "";
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        // private static string ClassButton(SerializedProperty property) => $"{property.propertyPath}__Button";
        private static string ClassLabelContainer(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelContainer";
        private static string ClassLabelError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelError";
        private static string ClassExecError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ExecError";

        protected static VisualElement DrawUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, object parent, VisualElement container)
        {
            Button button = new Button(() =>
            {
                string error = CallButtonFunc((DecButtonAttribute) saintsAttribute, parent, parent.GetType());
                HelpBox helpBox = container.Query<HelpBox>(className: ClassExecError(property, index)).First();
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = error;
            })
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                },
            };

            VisualElement labelContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    // flexGrow = 1,
                    justifyContent = Justify.Center,  // horizontal
                    alignItems = Align.Center,  // vertical
                },
                userData = "",
            };
            labelContainer.AddToClassList(ClassLabelContainer(property, index));
            // labelContainer.Add(new Label("test label"));

            button.Add(labelContainer);
            // button.AddToClassList();
            return button;
        }

        protected static HelpBox DrawLabelError(SerializedProperty property, int index) => DrawError(ClassLabelError(property, index));

        protected static HelpBox DrawExecError(SerializedProperty property, int index) => DrawError(ClassExecError(property, index));

        private static HelpBox DrawError(string className)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(className);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            if (parent == null)
            {
                return;
            }

            VisualElement labelContainer = container.Query<VisualElement>(className: ClassLabelContainer(property, index)).First();
            string oldXml = (string)labelContainer.userData;
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;
            (string error, string newXml) = GetButtonLabelXml(decButtonAttribute, parent, parent.GetType());

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (newXml == null)
            {
                newXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
            }

            HelpBox helpBox = container.Query<HelpBox>(className: ClassLabelError(property, index)).First();
            helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
            helpBox.text = error;

            if (oldXml == newXml)
            {
                return;
            }

            // Debug.Log($"update xml={newXml}");

            labelContainer.userData = newXml;
            labelContainer.Clear();
            IEnumerable<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(newXml, property.displayName);
            foreach (VisualElement visualElement in RichTextDrawer.DrawChunksUIToolKit(richChunks))
            {
                labelContainer.Add(visualElement);
            }
        }

        #endregion

#endif

        protected static (string error, string label) GetButtonLabelXml(DecButtonAttribute decButtonAttribute, object target, Type objType)
        {
            if (!decButtonAttribute.IsCallback)
            {
                return ("", decButtonAttribute.ButtonLabel);
            }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(objType, decButtonAttribute.ButtonLabel);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    string error = $"No field or method named `{decButtonAttribute.ButtonLabel}` found on `{target}`";
                    return (error, decButtonAttribute.ButtonLabel);
                }
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo findFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    object value = findFieldInfo.GetValue(target);
                    return ("", value == null ? string.Empty : value.ToString());
                }
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo propertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    object value = propertyInfo.GetValue(target);
                    return ("", value == null ? string.Empty : value.ToString());
                }
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(string));
                    // ReSharper disable once InvertIf
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        string error = $"Return type of callback method `{decButtonAttribute.ButtonLabel}` should be string";
                        return (error, decButtonAttribute.ButtonLabel);
                    }

                    // _error = "";
                    return
                        ("", (string)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }
        }
    }
}
