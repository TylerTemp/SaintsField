#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Playa.Renderer
{
    public partial class MethodRenderer
    {
        private static string ButtonName(SerializedProperty property) => $"{SerializedUtils.GetUniqueId(property)}__ButtonRenderer";

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            // Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));

            ButtonAttribute buttonAttribute = null;
            List<IPlayaMethodBindAttribute> methodBindAttributes = new List<IPlayaMethodBindAttribute>();

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                if(playaAttribute is ButtonAttribute button)
                {
                    buttonAttribute = button;
                }
                else if(playaAttribute is IPlayaMethodBindAttribute methodBindAttribute)
                {
                    methodBindAttributes.Add(methodBindAttribute);
                }
            }

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in methodBindAttributes)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"button click {playaMethodBindAttribute}");
#endif
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            if (buttonAttribute == null)
            {
                return (null, false);
            }
            // Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;
            // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
            ParameterInfo[] parameters = methodInfo.GetParameters();
            bool hasParameters = parameters.Length > 0;
            List<VisualElement> parameterElements = new List<VisualElement>();
            VisualElement root = null;

            if (hasParameters)
            {
                root = new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f),
                        borderTopWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftColor = EColor.MidnightAsh.GetColor(),
                        borderRightColor = EColor.MidnightAsh.GetColor(),
                        borderTopColor = EColor.MidnightAsh.GetColor(),
                        borderBottomColor = EColor.MidnightAsh.GetColor(),
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3,
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        marginTop = 1,
                        marginBottom = 1,
                        marginLeft = 3,
                        marginRight = 3,
                        paddingTop = 3,
                    },
                };

                foreach (ParameterInfo parameterInfo in parameters)
                {
                    VisualElement element = UIToolkitLayout(GetParameterDefaultValue(parameterInfo),
                        ObjectNames.NicifyVariableName(parameterInfo.Name), parameterInfo.ParameterType);
                    element.style.marginRight = 3;
                    element.SetEnabled(true);
                    parameterElements.Add(element);
                    root.Add(element);
                }

            }

            Button buttonElement = null;
            IVisualElementScheduledItem buttonTask = null;
            buttonElement = new Button(() =>
            {
                object[] paraValues = parameterElements.Select(each => each.GetType().GetProperty("value")!.GetValue(each)).ToArray();
                object returnValue = methodInfo.Invoke(target, paraValues);
                // ReSharper disable once InvertIf
                if (returnValue is System.Collections.IEnumerator enumerator)
                {
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    buttonElement.userData = enumerator;
                    buttonTask?.Pause();
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    buttonTask = buttonElement.schedule.Execute(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        // ReSharper disable once PossibleNullReferenceException
                        if (buttonElement.userData is System.Collections.IEnumerator bindEnumerator)
                        {
                            if (!bindEnumerator.MoveNext())
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                // ReSharper disable once PossibleNullReferenceException
                                buttonTask?.Pause();
                            }
                        }
                    }).Every(1);
                }
            })
            {
                text = buttonText,
                enableRichText = true,
                style =
                {
                    flexGrow = 1,
                },
                name = ButtonName(FieldWithInfo.SerializedProperty),
            };
            bool needUpdate = buttonAttribute.IsCallback;

            if (!needUpdate)
            {
                needUpdate = FieldWithInfo.PlayaAttributes.Count(each =>
                    // ReSharper disable once MergeIntoLogicalPattern
                    each is PlayaShowIfAttribute || each is PlayaEnableIfAttribute ||
                    each is PlayaDisableIfAttribute) > 0;
            }

            if (!hasParameters)
            {
                return (buttonElement, needUpdate);
            }
            buttonElement.style.marginTop = buttonElement.style.marginBottom = buttonElement.style.marginLeft = buttonElement.style.marginRight = 0;
            buttonElement.style.borderTopLeftRadius = buttonElement.style.borderTopRightRadius = 0;
            buttonElement.style.borderLeftWidth = buttonElement.style.borderRightWidth = buttonElement.style.borderBottomWidth = 0;
            root.Add(buttonElement);
            return (root, needUpdate);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);

            ButtonAttribute buttonAttribute = FieldWithInfo.PlayaAttributes.OfType<ButtonAttribute>().FirstOrDefault();
            if (buttonAttribute == null)
            {
                return baseResult;
            }

            if (!buttonAttribute.IsCallback)
            {
                return baseResult;
            }

            Button buttonElement;
            try
            {
                buttonElement = root.Q<Button>(name: ButtonName(FieldWithInfo.SerializedProperty));
            }
            catch (NullReferenceException)
            {
                return baseResult;
            }
            catch (ObjectDisposedException)
            {
                return baseResult;
            }

            string labelCallback = buttonAttribute.Label;
            var r = Util.GetOf<string>(labelCallback);

            return baseResult;
        }
    }
}
#endif
