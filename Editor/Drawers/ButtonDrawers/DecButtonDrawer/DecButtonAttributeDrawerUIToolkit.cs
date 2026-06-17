#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections;
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
        private static string ClassLabelError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelError";
        private static string ClassExecError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ExecError";
        protected static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__Button";

        protected abstract void CleanResult(VisualElement container, SerializedProperty property, int index);
        protected abstract void AppendErrorResult(VisualElement container, SerializedProperty property, int index, string error);
        protected abstract void AppendInvokeResult(VisualElement container, SerializedProperty property, int index, MethodInfo methodInfo, object parent, object result);

        protected static VisualElement DrawUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            ButtonRenderer.ButtonUserData buttonUserData = new ButtonRenderer.ButtonUserData
            {
                Xml = decButtonAttribute.ButtonLabel ?? ObjectNames.NicifyVariableName(decButtonAttribute.FuncName),
                Callback = decButtonAttribute.IsCallback? decButtonAttribute.ButtonLabel: "",
                UpdateOneMoreTime = true,
                Enumerators = new List<Waiter>(),
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
                buttonUserData.Enumerators.Clear();
                buttonUserData.ButtonTask?.Pause();
                buttonUserData.WaiterHasError = false;
                buttonUserData.WaiterHasFinished = false;

                // string buttonError = "";
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                // HashSet<IEnumerator> enumerators = (HashSet<IEnumerator>)buttonElement.userData;
                List<string> errors = new List<string>();
                List<object> results = new List<object>();
                MethodInfo usedMethodInfo = null;
                foreach ((string eachError, MemberInfo memberInfo, object buttonResult) in CallButtonFunc(property,
                             ((DecButtonAttribute)saintsAttribute).FuncName, info, parent))
                {
                    // Debug.Log($"{eachError}/{buttonResult}");
                    if (eachError == "")
                    {
                        usedMethodInfo = (MethodInfo)memberInfo;
                        results.Add(buttonResult);
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
                foreach (object result in results)
                {
                    if (result is IEnumerator ie)
                    {
                        buttonUserData.Enumerators.Add(new Waiter(ie));
                    }
                    else if (!decButtonAttribute.HideReturnValue)
                    {
                        if(result != null && result.GetType() != typeof(void))
                        {
                            Debug.Assert(usedMethodInfo != null);
                            refreshedParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                            AppendInvokeResult(container, property, index, usedMethodInfo, refreshedParent, result);
                        }
                    }
                }

                if (buttonUserData.Enumerators.Count <= 0)
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
                            Debug.LogException(e);
                            moveNext = false;
                            thisHasMoveError = true;
                            buttonUserData.WaiterHasError = true;

                            AppendErrorResult(container, property, index, e.InnerException?.Message ?? e.Message);
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
                        if (!moveNext)
                        {
                            finishedEnumerators.Add(waiter);
                            if (!thisHasMoveError)
                            {
                                buttonUserData.WaiterHasFinished = true;
                            }
                        }
                    }

                    buttonUserData.Enumerators.RemoveAll(each => finishedEnumerators.Contains(each));

                    bool stillHaveRunner = buttonUserData.Enumerators.Count > 0;
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
                if (buttonUserData.Enumerators.Count > 0)
                {
                    fancyButton.StatusIndicator.PlayPause();
                }

                buttonUserData.Enumerators.Clear();
                buttonUserData.ButtonTask?.Pause();
            };

            // Image buttonRotator = container.Q<Image>(name: NameButtonRotator(property, index));
            // // UIToolkitUtils.TriggerRotate(buttonRotator);
            // UIToolkitUtils.SetKeepRotate(buttonRotator);
            // buttonRotator.schedule.Execute(() => UIToolkitUtils.TriggerRotate(buttonRotator));
            // Debug.Log("TriggerRotate");
        }

        protected abstract bool HasResult(VisualElement container, SerializedProperty property, int index);

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
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            ButtonRenderer.ButtonUserData buttonUserData = (ButtonRenderer.ButtonUserData)fancyButton.userData;

            string labelCallback = buttonUserData.Callback;
            bool noNeedUpdate = true;
            string useXml = buttonUserData.Xml;
            object parent = null;
            if(!string.IsNullOrEmpty(labelCallback))
            {
                parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                (string xmlError, string newXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, parent);
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
            parent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            IEnumerable<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(useXml, property.displayName, property, info, parent);
            foreach (VisualElement visualElement in RichTextDrawer.DrawChunksUIToolKit(richChunks))
            {
                fancyButton.MainLabel.Add(visualElement);
            }

            // if (parent == null)
            // {
            //     return;
            // }

            // VisualElement labelContainer = container.Query<VisualElement>(className: ClassLabelContainer(property, index)).First();
            // string oldXml = (string)labelContainer.userData;
            // DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;
            //
            // object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            // (string xmlError, string newXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, parent);
            //
            // // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            // if (newXml == null)
            // {
            //     newXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
            // }
            //
            // HelpBox helpBox = container.Query<HelpBox>(className: ClassLabelError(property, index)).First();
            // helpBox.style.display = xmlError == ""? DisplayStyle.None: DisplayStyle.Flex;
            // helpBox.text = xmlError;
            //
            // if (oldXml == newXml)
            // {
            //     return;
            // }
            //
            // // Debug.Log($"update xml={newXml}");
            //
            // labelContainer.userData = newXml;
            // labelContainer.Clear();
            // IEnumerable<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(newXml, property.displayName, property, info, parent);
            // foreach (VisualElement visualElement in RichTextDrawer.DrawChunksUIToolKit(richChunks))
            // {
            //     labelContainer.Add(visualElement);
            // }
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
