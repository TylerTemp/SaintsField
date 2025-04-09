#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Playa.Renderer
{
    public partial class MethodRenderer
    {
        private static string ButtonName(MethodInfo methodInfo, object target) => $"{target?.GetHashCode()}_{methodInfo.Name}_{string.Join("_", methodInfo.GetParameters().Select(each => each.Name))}__ButtonRenderer";
        private static string ButtonLabelContainerName(MethodInfo methodInfo, object target) => $"{target?.GetHashCode()}_{methodInfo.Name}_{string.Join("_", methodInfo.GetParameters().Select(each => each.Name))}__ButtonLabelContainer";
        // private static string ButtonRotatorName(MethodInfo methodInfo, object target) => $"{target?.GetHashCode()}_{methodInfo.Name}_{string.Join("_", methodInfo.GetParameters().Select(each => each.Name))}__ButtonLabelContainer";

        private static StyleSheet _ussClassSaintsFieldEditingDisabledHide;

        private class ButtonUserData
        {
            public string Xml;
            public string Callback;
            public bool UpdateOneMoreTime;
            public RichTextDrawer RichTextDrawer;

            public IEnumerator Enumerator;
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            container.style.flexGrow = 1;
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
                return (null, methodBindAttributes.Count > 0);
            }
            // Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) || buttonAttribute.IsCallback ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;
            // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
            ParameterInfo[] parameters = methodInfo.GetParameters();
            bool hasParameters = parameters.Length > 0;
            // List<VisualElement> parameterElements = new List<VisualElement>();
            object[] parameterValues = new object[parameters.Length];
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
                    name = "saintsfield-button-params",
                };

                _ussClassSaintsFieldEditingDisabledHide ??= Util.LoadResource<StyleSheet>("UIToolkit/ClassSaintsFieldEditingDisabledHide.uss");
                root.styleSheets.Add(_ussClassSaintsFieldEditingDisabledHide);

                HashSet<Toggle> savedToggles = new HashSet<Toggle>();
                root.schedule.Execute(() =>
                {
                    // Debug.Log("check");
                    SaintsRendererGroup.CheckOutOfScoopFoldout(root, savedToggles);
                }).Every(200);

                foreach ((ParameterInfo parameterInfo, int index) in parameters.WithIndex())
                {
                    VisualElement paraContainer = new VisualElement();
                    root.Add(paraContainer);

                    Type paraType = parameterInfo.ParameterType;
                    object paraValue;
                    if(parameterInfo.HasDefaultValue)
                    {
                        paraValue = parameterInfo.DefaultValue;
                    }
                    else
                    {
                        paraValue = paraType.IsValueType ? Activator.CreateInstance(paraType) : null;
                    }
                    parameterValues[index] = paraValue;

                    bool paraValueChanged = true;
                    paraContainer.schedule.Execute(() =>
                    {
                        if (!paraValueChanged)
                        {
                            return;
                        }

                        VisualElement r = UIToolkitValueEdit(
                            paraContainer.Children().FirstOrDefault(),
                            parameterInfo.Name,
                            paraType,
                            paraValue,
                            null,
                            newValue =>
                            {
                                paraValue = parameterValues[index] = newValue;
                                paraValueChanged = true;
                            },
                            false,
                            true
                        );
                        // ReSharper disable once InvertIf
                        if (r != null)
                        {
                            paraContainer.Clear();
                            paraContainer.Add(r);
                        }

                        paraValueChanged = false;
                    }).Every(100);
                }

            }

            ButtonUserData buttonUserData = new ButtonUserData
            {
                Xml = buttonText,
                Callback = buttonAttribute.IsCallback ? buttonAttribute.Label : "",
                UpdateOneMoreTime = true,
            };
            Button buttonElement = null;
            IVisualElementScheduledItem buttonTask = null;
            Image buttonRotator = new Image
            {
                image = Util.LoadResource<Texture2D>("refresh.png"),
                style =
                {
                    position = Position.Absolute,
                    width = EditorGUIUtility.singleLineHeight - 2,
                    height = EditorGUIUtility.singleLineHeight - 2,
                    left = 1,
                    top = 1,
                    opacity = 0.5f,
                    display = DisplayStyle.None,
                },
                tintColor = EColor.Lime.GetColor(),
                // name = ButtonRotatorName(FieldWithInfo.MethodInfo, FieldWithInfo.Target),
            };
            UIToolkitUtils.KeepRotate(buttonRotator);
            buttonRotator.schedule.Execute(() => UIToolkitUtils.TriggerRotate(buttonRotator)).StartingIn(200);

            buttonElement = new Button(() =>
            {
                // object[] paraValues = parameterElements.Select(each => each.GetType().GetProperty("value")!.GetValue(each)).ToArray();
                object returnValue = methodInfo.Invoke(target, parameterValues);
                // ReSharper disable once InvertIf
                if (returnValue is IEnumerator enumerator)
                {
                    // ButtonUserData buttonUserData = (ButtonUserData) buttonElement.userData;
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    buttonUserData.Enumerator = enumerator;
                    buttonTask?.Pause();
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    buttonTask = buttonElement.schedule.Execute(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once InvertIf
                        // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                        if (buttonUserData.Enumerator is IEnumerator bindEnumerator)
                        {
                            bool show = true;
                            if (!bindEnumerator.MoveNext())
                            {
                                show = false;
                                // ReSharper disable once AccessToModifiedClosure
                                // ReSharper disable once PossibleNullReferenceException
                                buttonTask?.Pause();
                            }

                            DisplayStyle style = show? DisplayStyle.Flex : DisplayStyle.None;
                            if(buttonRotator.style.display != style)
                            {
                                buttonRotator.style.display = style;
                            }
                        }
                    }).Every(1);
                }
            })
            {
                text = "",
                enableRichText = true,
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    position = Position.Relative,
                },
                name = ButtonName(FieldWithInfo.MethodInfo, FieldWithInfo.Target),
                userData = buttonUserData,
            };

            // if (!string.IsNullOrEmpty(buttonAttribute.Label))
            // {
            //     buttonElement.text = "";
            //     buttonElement.Clear();
            //     foreach (VisualElement element in (new RichTextDrawer()).DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(buttonText,
            //                  FieldWithInfo.MethodInfo.Name, FieldWithInfo.MethodInfo, FieldWithInfo.Target)))
            //     {
            //         buttonElement.Add(element);
            //     }
            // }

            buttonElement.Clear();
            VisualElement buttonLabelContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                },
                name = ButtonLabelContainerName(FieldWithInfo.MethodInfo, FieldWithInfo.Target),
            };
            buttonElement.Add(buttonLabelContainer);
            foreach (VisualElement element in new RichTextDrawer().DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(buttonText,
                         FieldWithInfo.MethodInfo.Name, null, FieldWithInfo.MethodInfo, FieldWithInfo.Target)))
            {
                buttonLabelContainer.Add(element);
            }

            // buttonLabelContainer.RegisterCallback<AttachToPanelEvent>(_ =>
            //     UIToolkitUtils.TriggerRotate(buttonLabelContainer));
            // UIToolkitUtils.TriggerRotate(buttonLabelContainer);
            // buttonRotator.transform.rotation = Quaternion.Euler(0, 0, 180);
            // buttonRotator.AddToClassList("saints-rotate-360");

            buttonElement.Add(buttonRotator);

            bool needUpdate = buttonAttribute.IsCallback;

            if (!needUpdate)
            {
                needUpdate = FieldWithInfo.PlayaAttributes.Count(each =>
                    // ReSharper disable once MergeIntoLogicalPattern
                    each is PlayaShowIfAttribute
                    || each is PlayaEnableIfAttribute
                    || each is PlayaDisableIfAttribute
                    || each is IPlayaMethodBindAttribute) > 0;
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

        // private RichTextDrawer _richTextDrawer;

        // private bool _stillUpdateOnce;

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in FieldWithInfo.PlayaAttributes.OfType<IPlayaMethodBindAttribute>())
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"button click {playaMethodBindAttribute}");
#endif
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            Button buttonElement;
            try
            {
                buttonElement = root.Q<Button>(name: ButtonName(FieldWithInfo.MethodInfo, FieldWithInfo.Target));
            }
            catch (NullReferenceException)
            {
                return baseResult;
            }
            catch (ObjectDisposedException)
            {
                return baseResult;
            }

            if (buttonElement == null)
            {
                return baseResult;
            }

            ButtonUserData buttonUserData = (ButtonUserData) buttonElement.userData;

            string labelCallback = buttonUserData.Callback;
            // ReSharper disable once InvertIf
            if(!string.IsNullOrEmpty(labelCallback))
            {
                (string error, string result) = Util.GetOf<string>(labelCallback, null,
                    FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, FieldWithInfo.Target);
                // Debug.Log($"{error}/{result}");
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    return baseResult;
                }

                bool noNeedUpdate;
                if (buttonUserData.Xml == result)
                {
                    if (buttonUserData.UpdateOneMoreTime)
                    {
                        noNeedUpdate = false;
                        buttonUserData.UpdateOneMoreTime = false;
                    }
                    else
                    {
                        noNeedUpdate = true;
                    }
                }
                else
                {
                    noNeedUpdate = true;
                    buttonUserData.Xml = result;
                    buttonUserData.UpdateOneMoreTime = true;
                }

                if (noNeedUpdate)
                {
                    return baseResult;
                }

                // buttonElement.text = "";
                // buttonElement.Clear();
                VisualElement buttonLabelContainer = root.Q<VisualElement>(name: ButtonLabelContainerName(FieldWithInfo.MethodInfo, FieldWithInfo.Target));
                buttonLabelContainer.Clear();

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (result == "")
                {
                    // buttonElement.Add(new Label(" "));
                    return baseResult;
                }

                if (result is null)
                {
                    buttonLabelContainer.Add(new Label(ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name)));
                    return baseResult;
                }

                buttonUserData.RichTextDrawer ??= new RichTextDrawer();

                IEnumerable<VisualElement> chunks = buttonUserData.RichTextDrawer.DrawChunksUIToolKit(
                    RichTextDrawer.ParseRichXml(result,
                        FieldWithInfo.MethodInfo.Name, null, FieldWithInfo.MethodInfo, FieldWithInfo.Target));

                foreach (VisualElement chunk in chunks)
                {
                    buttonLabelContainer.Add(chunk);
                }

                return baseResult;
            }

            return baseResult;
        }
    }
}
#endif
