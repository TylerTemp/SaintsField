#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using SaintsField.Playa;
using UnityEditor;
// ReSharper disable once RedundantUsingDirective
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ButtonFakeRenderer
{
    public partial class ButtonRenderer
    {
        private static string ButtonName(SaintsFieldWithInfo info) => $"{info.Targets[0]?.GetHashCode()}_{info.MethodInfo.Name}_{string.Join("_", info.MethodInfo.GetParameters().Select(each => each.Name))}__ButtonRenderer";
        // private static string ButtonLabelContainerName(MethodInfo methodInfo, object target) => $"{target?.GetHashCode()}_{methodInfo.Name}_{string.Join("_", methodInfo.GetParameters().Select(each => each.Name))}__ButtonLabelContainer";
        // private static string ButtonRotatorName(MethodInfo methodInfo, object target) => $"{target?.GetHashCode()}_{methodInfo.Name}_{string.Join("_", methodInfo.GetParameters().Select(each => each.Name))}__ButtonLabelContainer";

        private static StyleSheet _ussClassSaintsFieldEditingDisabledHide;

        public class ButtonUserData
        {
            public string Xml;
            public string Callback;
            public bool UpdateOneMoreTime;
            public RichTextDrawer RichTextDrawer;

            public List<Waiter> Enumerators = new List<Waiter>();
            public IVisualElementScheduledItem ButtonTask;
            public bool WaiterHasError = false;
            public bool WaiterHasFinished = false;
        }

        // private VisualElement _returnValueContainer;
        // private VisualElement _returnContainer;

        // private Button _buttonElement;


        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            FancyButton fancyButton = new FancyButton
            {
                name = ButtonName(FieldWithInfo),
            };

            // return (fancyButton, true);
            container.style.flexGrow = 1;

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            // Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));
            string buttonText = string.IsNullOrEmpty(_buttonAttribute.Label)
                                || _buttonAttribute.IsCallback ? ObjectNames.NicifyVariableName(methodInfo.Name) : _buttonAttribute.Label;
            // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
            ParameterInfo[] parameters = methodInfo.GetParameters();
            bool hasParameters = parameters.Length > 0;
            // List<VisualElement> parameterElements = new List<VisualElement>();
            object[] parameterValues = new object[parameters.Length];
            // VisualElement root = null;

            bool hasReturnValue = !_buttonAttribute.HideReturnValue
                && methodInfo.ReturnType != typeof(void)
                && !typeof(IEnumerator).IsAssignableFrom(methodInfo.ReturnType);

            string buttonId = $"{FieldWithInfo.Targets[0].GetHashCode()}.{methodInfo.Name}";

            if (hasParameters || hasReturnValue)
            {
                VisualElement parametersContainer = fancyButton.HasParameters();

                parametersContainer.RegisterCallback<AttachToPanelEvent>(_ => UIToolkitUtils.LoopCheckOutOfScoopFoldout(parametersContainer));

                foreach ((ParameterInfo parameterInfo, int index) in parameters.WithIndex())
                {
                    VisualElement paraContainer = new VisualElement
                    {
                        style =
                        {
                            marginRight = 4,
                        },
                    };
                    parametersContainer.Add(paraContainer);

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

                    Attribute[] attributes = parameterInfo.GetCustomAttributes().ToArray();

                    bool paraValueChanged = true;
                    paraContainer.schedule.Execute(() =>
                    {
                        if (!paraValueChanged)
                        {
                            return;
                        }
                        // Debug.Log($"para value changed: {parameterInfo.Name}={parameterValues[index]}, {paraContainer.Children().FirstOrDefault()}");
                        VisualElement r = UIToolkitEdit.UIToolkitValueEdit(
                            paraContainer.Children().FirstOrDefault(),
                            parameterInfo.Name,
                            paraType,
                            parameterValues[index],
                            null,
                            newValue =>
                            {
                                parameterValues[index] = newValue;
                                paraValueChanged = true;
                                fancyButton.ShowResult(false);
                                // Debug.Log($"param {index} set to {newValue}");
                            },
                            false,
                            InAnyHorizontalLayout,
                            attributes,
                            FieldWithInfo.Targets,
                            this,
                            $"{buttonId}.{parameterInfo.Name}"
                        ).result;
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
                Callback = _buttonAttribute.IsCallback ? _buttonAttribute.Label : "",
                UpdateOneMoreTime = true,
                Enumerators = new List<Waiter>(),
                WaiterHasError = false,
                WaiterHasFinished = false,
            };
            fancyButton.userData = buttonUserData;

            fancyButton.CloseButton.clicked += () =>
            {
                buttonUserData.ButtonTask?.Pause();

                fancyButton.StatusIndicator.EnsureLoading(false, 0);
                if (buttonUserData.Enumerators.Count > 0)
                {
                    fancyButton.StatusIndicator.PlayPause();
                }
                buttonUserData.Enumerators.Clear();
                fancyButton.ShowResult(false);
            };

            StatusIndicatorElement statusIndicatorElement = fancyButton.StatusIndicator;

            bool isStruct = ReflectUtils.TypeIsStruct(FieldWithInfo.Targets[0].GetType());

            fancyButton.MainButton.clicked += () =>
            {
                fancyButton.ShowResult(false);

                SaintsContext.SerializedProperty = _serializedProperty;
                int targetCount = FieldWithInfo.Targets.Count;
                object[] returnValues = new object[targetCount];
                Exception error = null;
                for (int index = 0; index < targetCount; index++)
                {
                    object eachTarget = FieldWithInfo.Targets[index];
                    (object rawMemberValue, object useTarget) = GetRefreshedTarget(FieldWithInfo, eachTarget);

                    object result;
                    try
                    {
                        result = methodInfo.Invoke(useTarget, parameterValues);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        error = e;
                        break;
                    }

                    returnValues[index] = result;
                    if (isStruct)
                    {
                        BackWriteCallback(rawMemberValue, useTarget);
                    }
                }

                if (error != null)
                {
                    statusIndicatorElement.PlayError();
                    VisualElement resultContainer = fancyButton.ShowResult(true);
                    resultContainer.Clear();
                    resultContainer.Add(MakeErrorBox(error));
                    return;
                }

                if (hasReturnValue)
                {
                    // Debug.Assert(_returnValueContainer != null);
                    VisualElement returnValueContainer = fancyButton.ShowResult(true);
                    returnValueContainer.Clear();
                    object returnValue = returnValues[0];
                    VisualElement r = UIToolkitEdit.UIToolkitValueEdit(
                        returnValueContainer.Children().FirstOrDefault(),
                        "<color=green>[return]</color>",
                        methodInfo.ReturnType,
                        returnValue,
                        null,
                        _ => { },
                        false,
                        InAnyHorizontalLayout,
                        ReflectCache.GetCustomAttributes(FieldWithInfo.MethodInfo),
                        FieldWithInfo.Targets,
                        this,
                        $"{buttonId}.[return]"
                    ).result;
                    if (r != null)
                    {
                        if (r is Foldout { value: false } fo)
                        {
                            fo.RegisterCallback<AttachToPanelEvent>(_ => fo.value = true);
                            // fo.value = true;
                        }

                        fancyButton.ShowResult(true);
                        // if (_returnContainer.style.display != DisplayStyle.Flex)
                        // {
                        //     _returnContainer.style.display = DisplayStyle.Flex;
                        // }

                        returnValueContainer.Add(r);
                    }
                }

                buttonUserData.Enumerators.Clear();
                foreach (IEnumerator enumerator in returnValues.OfType<IEnumerator>())
                {
                    Waiter waiter = new Waiter(enumerator);
                    buttonUserData.Enumerators.Add(waiter);
                }

                if (buttonUserData.Enumerators.Count == 0)
                {
                    statusIndicatorElement.PlayOk();
                }
                else
                {
                    statusIndicatorElement.PlayLoading();
                    fancyButton.ShowCloseButton(true);
                }

                buttonUserData.ButtonTask?.Pause();

                if (buttonUserData.Enumerators.Count > 0)
                {
                    // ButtonUserData buttonUserData = (ButtonUserData) buttonElement.userData;
                    buttonUserData.ButtonTask = fancyButton.schedule.Execute(() =>
                    {
                        List<Waiter> finishedEnumerators = new List<Waiter>();
                        int oldCounter = buttonUserData.Enumerators.Count;
                        float progress = -1f;
                        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                        foreach (Waiter waiter in buttonUserData.Enumerators)
                        {
                            waiter.Update();

                            if (!waiter.Done())
                            {
                                if (waiter.Waitable != null)
                                {
                                    float curProcess = waiter.Waitable.Progress;
                                    progress = Mathf.Max(progress, curProcess);
                                }

                                continue;
                            }

                            bool moveNext;
                            bool thisHasMoveError = false;
                            try
                            {
                                moveNext = waiter.Enumerator.MoveNext();
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e.InnerException ?? e);
                                moveNext = false;
                                thisHasMoveError = true;

                                VisualElement result = fancyButton.ShowResult(true);
                                // Debug.Log("show error result");
                                result.Add(MakeErrorBox(e));
                                buttonUserData.WaiterHasError = true;
                            }

                            if (thisHasMoveError)
                            {
                                waiter.Waitable = null;
                            }
                            else
                            {
                                waiter.CheckCurrent();
                            }

                            // Debug.Log(bindEnumerator.Current);
                            // ReSharper disable once InvertIf
                            if (!moveNext)
                            {
                                finishedEnumerators.Add(waiter);

                                if(!thisHasMoveError)
                                {
                                    buttonUserData.WaiterHasFinished = true;
                                }
                            }
                        }

                        buttonUserData.Enumerators.RemoveAll(each => finishedEnumerators.Contains(each));

                        bool stillHaveRunner = buttonUserData.Enumerators.Count > 0;
                        statusIndicatorElement.EnsureLoading(stillHaveRunner, progress);

                        // ReSharper disable once InvertIf
                        if (!stillHaveRunner)
                        {
                            buttonUserData.ButtonTask?.Pause();

                            // ReSharper disable once InvertIf
                            if (oldCounter > 0)  // last ones finished
                            {
                                if (buttonUserData.WaiterHasError)
                                {
                                    if(buttonUserData.WaiterHasFinished)
                                    {
                                        statusIndicatorElement.PlayWarning();
                                    }
                                    else
                                    {
                                        statusIndicatorElement.PlayError();
                                    }
                                }
                                else
                                {
                                    statusIndicatorElement.PlayOk();
                                    fancyButton.ShowResult(false);
                                }
                            }
                        }
                    }).Every(1);
                }
            };

            fancyButton.MainLabel.Clear();
            foreach (VisualElement element in new RichTextDrawer().DrawChunksUIToolKit(RichTextDrawer.ParseRichXmlWithProvider(buttonText, this)))
            {
                fancyButton.MainLabel.Add(element);
            }

            bool needUpdate = _buttonAttribute.IsCallback;

            if (!needUpdate)
            {
                needUpdate = FieldWithInfo.PlayaAttributes.Count(each =>
                    // ReSharper disable once MergeIntoLogicalPattern
                    each is ShowIfAttribute
                    || each is EnableIfAttribute
                    || each is DisableIfAttribute
                    || each is IPlayaMethodBindAttribute) > 0;
            }

            string methodNameFriendly = ObjectNames.NicifyVariableName(methodInfo.Name);

            _onSearchFieldUIToolkit.AddListener(Search);
            fancyButton.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            return (fancyButton, needUpdate);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(methodNameFriendly, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (fancyButton.style.display != display)
                {
                    fancyButton.style.display = display;
                }
            }
        }

        private static VisualElement MakeErrorBox(Exception error)
        {
            return new HelpBox(error.InnerException?.Message ?? error.Message, HelpBoxMessageType.Error)
            {
                style =
                {
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    // borderTopWidth = 0,
                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    borderBottomWidth = 0,
                    backgroundColor = Color.clear,
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                },
            };
        }

        // private RichTextDrawer _richTextDrawer;

        // private bool _stillUpdateOnce;

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);

            FancyButton fancyButton;
            try
            {
                fancyButton = root.Q<FancyButton>(name: ButtonName(FieldWithInfo));
            }
            catch (NullReferenceException)
            {
                return baseResult;
            }
            catch (ObjectDisposedException)
            {
                return baseResult;
            }

            if (fancyButton == null)
            {
                return baseResult;
            }

            ButtonUserData buttonUserData = (ButtonUserData) fancyButton.userData;

            string labelCallback = buttonUserData.Callback;
            // ReSharper disable once InvertIf
            if(!string.IsNullOrEmpty(labelCallback))
            {
                (string error, MemberInfo _, string result) = Util.GetOf<string>(labelCallback, null,
                    FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0], null);
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
                VisualElement buttonLabelContainer = fancyButton.MainLabel;
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
                    RichTextDrawer.ParseRichXmlWithProvider(result, this));

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
