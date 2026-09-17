#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.ButtonFakeRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public partial class DecButtonAttributeDrawer
    {
        // private static string ClassLabelError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelError";
        // private static string ClassExecError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ExecError";
        protected static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__Button";

        protected abstract void CleanResult(VisualElement container, SerializedProperty property, int index);
        protected abstract void AppendErrorResult(VisualElement container, SerializedProperty property, int index, string error);
        protected abstract void AppendInvokeResult(VisualElement container, SerializedProperty property, int index,
            MethodInfo methodInfo, Type returnType, object parent, object result);

        protected static VisualElement DrawUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            ButtonRenderer.ButtonUserData buttonUserData = new ButtonRenderer.ButtonUserData
            {
                Xml = decButtonAttribute.ButtonLabel ?? ObjectNames.NicifyVariableName(decButtonAttribute.FuncName),
                Callback = decButtonAttribute.IsCallback? decButtonAttribute.ButtonLabel: "",
                UpdateOneMoreTime = true,
                MethodRunners = new List<MethodRunnerUtil.Payload>(),
            };
            FancyButton fancyButton = new FancyButton
            {
                name = NameButton(property, index),
                userData = buttonUserData,
            };
            fancyButton.MainLabel.Add(new Label(buttonUserData.Xml));
            fancyButton.AddToClassList(ClassAllowDisable);

            return fancyButton;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            ButtonRenderer.ButtonUserData buttonUserData = (ButtonRenderer.ButtonUserData)fancyButton.userData;
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            fancyButton.MainButton.clicked += () =>
            {
                fancyButton.ShowResult(false);
                CleanResult(container, property, index);
                buttonUserData.MethodRunners.Clear();
                buttonUserData.ButtonTask?.Pause();
                buttonUserData.WaiterHasError = false;
                buttonUserData.WaiterHasFinished = false;
                buttonUserData.WaiterHasCancel = false;
                buttonUserData.WaiterHasReturnValue = false;

                // string buttonError = "";
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                // HashSet<IEnumerator> enumerators = (HashSet<IEnumerator>)buttonElement.userData;
                List<string> errors = new List<string>();
                List<(MethodInfo methodInfo, object result)> results = new List<(MethodInfo methodInfo, object result)>();
                foreach ((string eachError, MemberInfo memberInfo, object buttonResult) in CallButtonFunc(property,
                             ((DecButtonAttribute)saintsAttribute).FuncName, info, parent))
                {
                    // Debug.Log($"{eachError}/{buttonResult}/{memberInfo.Name}");
                    if (eachError == "")
                    {
                        results.Add(((MethodInfo)memberInfo, buttonResult));
                    }
                    else
                    {
                        errors.Add(eachError);
                    }
                }

                foreach (string error in errors)
                {
                    AppendErrorResult(container, property, index, error);
                }

                object refreshedParent = null;
                Dictionary<MethodRunnerUtil.Payload, object> runnerReturnTargets =
                    new Dictionary<MethodRunnerUtil.Payload, object>();
                foreach ((MethodInfo methodInfo, object result) in results)
                {
                    MethodRunnerUtil.Payload methodRunner = MethodRunnerUtil.FromResult(methodInfo, result);
                    (bool hasReturnValue, MethodRunnerUtil.ReturnValueInfo returnValue) =
                        MethodRunnerUtil.GetReturnValue(methodRunner);
                    if (methodRunner.Status == MethodRunnerUtil.RunStatus.Pending)
                    {
                        buttonUserData.MethodRunners.Add(methodRunner);
                        if (!decButtonAttribute.HideReturnValue)
                        {
                            refreshedParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                            runnerReturnTargets[methodRunner] = refreshedParent;
                        }
                    }
                    else if (!decButtonAttribute.HideReturnValue
                             && hasReturnValue
                             && returnValue.Value != null)
                    {
                        refreshedParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        AppendInvokeResult(container, property, index, methodInfo, returnValue.Type, refreshedParent,
                            returnValue.Value);
                    }
                }

                if (buttonUserData.MethodRunners.Count <= 0)
                {
                    if (errors.Count > 0)
                    {
                        fancyButton.StatusIndicator.PlayError();
                    }
                    else
                    {
                        fancyButton.StatusIndicator.PlayOk();
                    }

                    return;
                }

                fancyButton.ShowCloseButton(true);
                fancyButton.StatusIndicator.PlayLoading();
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                buttonUserData.ButtonTask = fancyButton.schedule.Execute(() =>
                {
                    List<MethodRunnerUtil.Payload> finishedRunners = new List<MethodRunnerUtil.Payload>();
                    int oldCounter = buttonUserData.MethodRunners.Count;
                    float progress = -1f;
                    // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                    foreach (MethodRunnerUtil.Payload methodRunner in buttonUserData.MethodRunners)
                    {
                        MethodRunnerUtil.RunStatus status = MethodRunnerUtil.Tick(methodRunner);
                        if (status == MethodRunnerUtil.RunStatus.Pending)
                        {
                            float curProcess = methodRunner.Progress;
                            if (curProcess >= 0)
                            {
                                progress = Mathf.Max(progress, curProcess);
                            }

                            continue;
                        }

                        if (methodRunner.Exception != null)
                        {
                            Debug.LogException(methodRunner.Exception.InnerException ?? methodRunner.Exception);
                            buttonUserData.WaiterHasError = true;

                            AppendErrorResult(container, property, index,
                                methodRunner.Exception.InnerException?.Message ?? methodRunner.Exception.Message);
                        }

                        (bool hasReturnValue, MethodRunnerUtil.ReturnValueInfo returnValue) =
                            MethodRunnerUtil.GetReturnValue(methodRunner);
                        if (runnerReturnTargets.TryGetValue(methodRunner, out object returnTarget)
                            && hasReturnValue)
                        {
                            AppendInvokeResult(container, property, index, methodRunner.MethodInfo, returnValue.Type,
                                returnTarget, returnValue.Value);
                            buttonUserData.WaiterHasReturnValue = true;
                        }

                        finishedRunners.Add(methodRunner);
                        if (status == MethodRunnerUtil.RunStatus.Completed)
                        {
                            buttonUserData.WaiterHasFinished = true;
                        }
                        else if (status == MethodRunnerUtil.RunStatus.Cancelled)
                        {
                            buttonUserData.WaiterHasCancel = true;
                        }
                    }

                    buttonUserData.MethodRunners.RemoveAll(each => finishedRunners.Contains(each));

                    bool stillHaveRunner = buttonUserData.MethodRunners.Count > 0;
                    fancyButton.StatusIndicator.EnsureLoading(stillHaveRunner, progress);

                    if (!stillHaveRunner)
                    {
                        buttonUserData.ButtonTask?.Pause();

                        if (!HasResult(container, property, index))
                        {
                            fancyButton.ShowCloseButton(false);
                        }

                        if (oldCounter > 0)
                        {
                            if (buttonUserData.WaiterHasError)
                            {
                                if (buttonUserData.WaiterHasFinished)
                                {
                                    fancyButton.StatusIndicator.PlayWarning();
                                }
                                else
                                {
                                    fancyButton.StatusIndicator.PlayError();
                                }
                            }
                            else if (buttonUserData.WaiterHasCancel)
                            {
                                fancyButton.StatusIndicator.PlayPause();
                            }
                            else
                            {
                                fancyButton.StatusIndicator.PlayOk();
                            }
                        }
                    }
                }).Every(1);
            };

            fancyButton.CloseButton.clicked += () =>
            {
                fancyButton.StatusIndicator.EnsureLoading(false, 0);
                if (buttonUserData.MethodRunners.Count > 0)
                {
                    fancyButton.StatusIndicator.PlayPause();
                }

                buttonUserData.MethodRunners.Clear();
                buttonUserData.ButtonTask?.Pause();
            };

            // Image buttonRotator = container.Q<Image>(name: NameButtonRotator(property, index));
            // // UIToolkitUtils.TriggerRotate(buttonRotator);
            // UIToolkitUtils.SetKeepRotate(buttonRotator);
            // buttonRotator.schedule.Execute(() => UIToolkitUtils.TriggerRotate(buttonRotator));
            // Debug.Log("TriggerRotate");
        }

        protected abstract bool HasResult(VisualElement container, SerializedProperty property, int index);

        // protected static HelpBox DrawLabelError(SerializedProperty property, int index) => DrawError(ClassLabelError(property, index));
        //
        // protected static HelpBox DrawExecError(SerializedProperty property, int index) => DrawError(ClassExecError(property, index));

        // private static HelpBox DrawError(string className)
        // {
        //     HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
        //     {
        //         style =
        //         {
        //             display = DisplayStyle.None,
        //         },
        //     };
        //     helpBox.AddToClassList(className);
        //     helpBox.AddToClassList(ClassAllowDisable);
        //     return helpBox;
        // }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            if (!SerializedUtils.IsOk(property))
            {
                return;
            }

            object reParent = null;

            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            ButtonRenderer.ButtonUserData buttonUserData = (ButtonRenderer.ButtonUserData)fancyButton.userData;

            (string error, bool show, object reParent) showResult = GetShowResult(property, saintsAttribute, allAttributes, info);
            if (showResult.error != string.Empty)  // TODO: error handling in UI
            {
                Debug.LogError(showResult.error);
                UIToolkitUtils.SetDisplayStyle(fancyButton, DisplayStyle.Flex);
                return;
            }

            UIToolkitUtils.SetDisplayStyle(fancyButton, showResult.show? DisplayStyle.Flex: DisplayStyle.None);

            if (showResult.reParent != null)
            {
                reParent = showResult.reParent;
            }

            (string error, bool disable, object reParent) disableResult = GetDisableResult(property, saintsAttribute, allAttributes, info, reParent);
            if (disableResult.error != string.Empty)  // TODO: error handling in UI
            {
                Debug.LogError(disableResult.error);
                fancyButton.SetEnabled(false);
                return;
            }
            fancyButton.SetEnabled(!disableResult.disable);

            reParent = disableResult.reParent ?? reParent;

            string labelCallback = buttonUserData.Callback;
            bool noNeedUpdate = true;
            string useXml = buttonUserData.Xml;
            if(!string.IsNullOrEmpty(labelCallback))
            {
                reParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

                (string xmlError, string newXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, reParent);
                if (!string.IsNullOrEmpty(xmlError))
                {
                    Debug.LogError(xmlError);
                }

                // Debug.Log($"{xmlError}/{newXml}");
                if (string.IsNullOrEmpty(newXml))
                {
                    newXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
                }

                string oldXml = buttonUserData.Xml;
                if (oldXml == newXml)
                {
                    if (buttonUserData.UpdateOneMoreTime)
                    {
                        noNeedUpdate = false;
                        buttonUserData.UpdateOneMoreTime = false;
                    }
                    else if (newXml != null && newXml.Contains("<field"))
                    {
                        noNeedUpdate = false;
                    }
                }
                else
                {
                    noNeedUpdate = false;
                }

                useXml = newXml;
            }
            else if ((buttonUserData.Xml != null && buttonUserData.Xml.Contains("<field")) ||
                     buttonUserData.UpdateOneMoreTime)
            {
                noNeedUpdate = false;
            }

            if (noNeedUpdate)
            {
                return;
            }

            buttonUserData.UpdateOneMoreTime = false;
            buttonUserData.Xml = useXml;

            fancyButton.MainLabel.Clear();
            // parent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            IEnumerable<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXmlWithProvider(useXml, this);
            foreach (VisualElement visualElement in RichTextDrawer.DrawChunksUIToolKit(richChunks))
            {
                fancyButton.MainLabel.Add(visualElement);
            }
        }

        protected static VisualElement MakeErrorBox(string error)
        {
            return new HelpBox(error, HelpBoxMessageType.Error)
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
    }
}
#endif
