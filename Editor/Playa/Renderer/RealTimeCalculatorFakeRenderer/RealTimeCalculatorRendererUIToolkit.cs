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

namespace SaintsField.Editor.Playa.Renderer.RealTimeCalculatorFakeRenderer
{
    public partial class RealTimeCalculatorRenderer
    {
        private string NameContainer() => $"saints-field--real-time-calculator--{GetName(FieldWithInfo)}";

        private static StyleSheet _ussClassSaintsFieldEditingDisabledHide;
        private VisualElement _returnValueContainer;
        private object[] _parameterValues;

        private static string GetName(SaintsFieldWithInfo fieldWithInfo) => ObjectNames.NicifyVariableName(fieldWithInfo.MethodInfo.Name);

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            container.style.flexGrow = 1;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ParameterInfo[] parameters = methodInfo.GetParameters();
            bool hasParameters = parameters.Length > 0;
            // List<VisualElement> parameterElements = new List<VisualElement>();
            _parameterValues = new object[parameters.Length];
            VisualElement root;
            if (hasParameters)
            {
                root =  new VisualElement
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
                    name = NameContainer(),
                };
            }
            else
            {
                root = new VisualElement
                {
                    style =
                    {
                        borderLeftWidth = 2,
                        borderRightWidth = 2,
                        borderLeftColor = EColor.EditorEmphasized.GetColor(),
                        borderRightColor = EColor.EditorEmphasized.GetColor(),
                        borderTopLeftRadius = 3,
                        borderBottomLeftRadius = 3,
                        marginLeft = 1,
                        marginRight = 1,
                    },
                    name = NameContainer(),
                };
            }

            HashSet<Toggle> savedToggles = new HashSet<Toggle>();
            root.schedule.Execute(() =>
            {
                SaintsRendererGroup.CheckOutOfScoopFoldout(root, savedToggles);
            }).Every(200);

            if (hasParameters)
            {
                _ussClassSaintsFieldEditingDisabledHide ??= Util.LoadResource<StyleSheet>("UIToolkit/ClassSaintsFieldEditingDisabledHide.uss");
                root.styleSheets.Add(_ussClassSaintsFieldEditingDisabledHide);

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
                    _parameterValues[index] = paraValue;

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
                                paraValue = _parameterValues[index] = newValue;
                                paraValueChanged = true;
                            },
                            false,
                            true
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

                root.Add(new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(1, 1, 1, 0.2f),
                        height = 1,
                        marginTop = 3,
                        marginBottom = 3,
                    }
                });
            }


            // _onSearchFieldUIToolkit.AddListener(Search);
            // container.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            _returnValueContainer = new VisualElement();
            root.Add(_returnValueContainer);


            return (root, true);

            // void Search(string search)
            // {
            //     DisplayStyle display = Util.UnityDefaultSimpleSearch(methodNameFriendly, search)
            //         ? DisplayStyle.Flex
            //         : DisplayStyle.None;
            //
            //     if (buttonElement.style.display != display)
            //     {
            //         buttonElement.style.display = display;
            //     }
            // }
        }

        // private RichTextDrawer _richTextDrawer;

        // private bool _stillUpdateOnce;

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);
            if (_parameterValues == null)
            {
                return baseResult;
            }

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            object[] returnValues = FieldWithInfo.Targets.Select(t => methodInfo.Invoke(t, _parameterValues)).ToArray();

            Debug.Assert(_returnValueContainer != null);
            object returnValue = returnValues[0];
            string labelName = NoLabel ? null : GetName(FieldWithInfo);
            (VisualElement result, bool isNestedField) = UIToolkitValueEdit(
                _returnValueContainer.Children().FirstOrDefault(),
                labelName,
                methodInfo.ReturnType,
                returnValue,
                null,
                _ => { },
                false,
                false
            );
            if (result != null)
            {
                if (isNestedField && result is Foldout { value: false } fo)
                {
                    fo.RegisterCallback<AttachToPanelEvent>(_ => fo.value = true);
                    // fo.value = true;
                }
                _returnValueContainer.Clear();
                _returnValueContainer.Add(result);
            }

            return baseResult;
        }
    }
}
