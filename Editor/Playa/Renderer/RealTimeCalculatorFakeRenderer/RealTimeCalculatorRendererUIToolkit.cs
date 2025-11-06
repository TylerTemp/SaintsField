#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.RealTimeCalculatorFakeRenderer
{
    public partial class RealTimeCalculatorRenderer
    {
        private string NameContainer() => $"saints-field--real-time-calculator--{GetName(FieldWithInfo)}";
        private string ClassResultContainer() => $"saints-field--native-property-field--{GetName(FieldWithInfo)}-result-container";

        private static StyleSheet _ussClassSaintsFieldEditingDisabledHide;
        private VisualElement _returnValueContainer;
        private object[] _parameterValues;

        private static string GetName(SaintsFieldWithInfo fieldWithInfo) => ObjectNames.NicifyVariableName(fieldWithInfo.MethodInfo.Name);

        private class DataPayload
        {
            public bool HasDrawer;
            public object Value;
            public bool IsGeneralCollection;
            public IReadOnlyList<object> OldCollection;
            public bool AlwaysCheckUpdate;
        }

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

            root.RegisterCallback<AttachToPanelEvent>(_ => UIToolkitUtils.LoopCheckOutOfScoopFoldout(root));

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
                            true,
                            Array.Empty<Attribute>(),
                            FieldWithInfo.Targets
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

            Type fieldType = methodInfo.ReturnType;
            bool isCollection = !typeof(UnityEngine.Object).IsAssignableFrom(fieldType) && (fieldType.IsArray || typeof(IEnumerable).IsAssignableFrom(fieldType));
            root.userData = new DataPayload
            {
                HasDrawer = false,
                Value = null,
                IsGeneralCollection = isCollection,
                OldCollection = null,
                // AlwaysCheckUpdate = isNestedField,
            };

            _onSearchFieldUIToolkit.AddListener(Search);
            root.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            if (hasParameters)
            {
                _returnValueContainer = new VisualElement();
                root.Add(_returnValueContainer);
            }
            else
            {
                _returnValueContainer = root;
            }

            return (root, true);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(GetName(FieldWithInfo), search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (root.style.display != display)
                {
                    root.style.display = display;
                }
            }
        }

        // private RichTextDrawer _richTextDrawer;

        // private bool _stillUpdateOnce;
        // private object _preValue = null;

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);
            if (_parameterValues == null)
            {
                return baseResult;
            }

            VisualElement container= root.Q<VisualElement>(NameContainer());

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            // Debug.Log(string.Join(", ", _parameterValues));

            object[] returnValues = FieldWithInfo.Targets.Select(t => methodInfo.Invoke(t, _parameterValues)).ToArray();

            Debug.Assert(_returnValueContainer != null);
            object value = returnValues[0];
            // Debug.Log($"returnValue={returnValue}");

            DataPayload userData = (DataPayload)container.userData;
            bool valueIsNull = RuntimeUtil.IsNull(value);
            bool isEqual;
            if (userData.AlwaysCheckUpdate)
            {
                isEqual = false;
            }
            else
            {
                isEqual = userData.HasDrawer && Util.GetIsEqual(userData.Value, value);
            }
            if(isEqual && userData.IsGeneralCollection)
            {
                IReadOnlyList<object> oldCollection = userData.OldCollection;
                if (oldCollection == null && valueIsNull)
                {
                }
                else if (oldCollection != null && valueIsNull)
                {
                    isEqual = false;
                }
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                else if (oldCollection == null && !valueIsNull)
                {
                    isEqual = false;
                }
                else
                {
                    isEqual = oldCollection.SequenceEqual(((IEnumerable)value).Cast<object>());
                    // Debug.Log($"sequence equal: {isEqual}");
                }
            }

            VisualElement fieldElementOrNull = _returnValueContainer.Children().FirstOrDefault();
            // Debug.Log($"isEqual={isEqual}/{value}");

            if (!isEqual)
            {
                // Debug.Log($"fieldElementOrNull={fieldElementOrNull?.name}");
                // Debug.Log($"native property update {userData.Value} -> {value}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"native property update {userData.Value} -> {value}");
#endif
                userData.Value = value;
                if (userData.IsGeneralCollection)
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (!RuntimeUtil.IsNull(value) && value is IEnumerable ie)
                    {
                        userData.OldCollection = ie.Cast<object>().ToArray();
                    }
                    else
                    {
                        userData.OldCollection = null;
                    }
                }

                Type type = RuntimeUtil.IsNull(value) ? methodInfo.ReturnType : value.GetType();
                (VisualElement result, bool isNestedField) = UIToolkitValueEdit(fieldElementOrNull, NoLabel? null: GetName(FieldWithInfo), type, value, null, _ => {}, false, InAnyHorizontalLayout, ReflectCache.GetCustomAttributes(FieldWithInfo.MethodInfo), FieldWithInfo.Targets);
                if(result!=null)
                {
                    if (isNestedField && result is Foldout { value: false } fo)
                    {
                        fo.RegisterCallback<AttachToPanelEvent>(_ => fo.value = true);
                        // fo.value = true;
                    }
                    // Debug.Log(
                    //     $"Not equal create for value={value}: {result}/{result == null}/{fieldElementOrNull}/{type}");
                    // result.name = NameResult();
                    _returnValueContainer.Clear();
                    _returnValueContainer.Add(result);
                    userData.HasDrawer = true;
                }
                else if(fieldElementOrNull == null)
                {
                    userData.HasDrawer = false;
                }

                // StyleEnum<DisplayStyle> displayStyle = child.style.display;
                // fieldElement.Clear();
                // fieldElement.userData = value;
                // fieldElement.Add(child = UIToolkitValueEdit(GetNiceName(FieldWithInfo), GetFieldType(FieldWithInfo), value, GetSetterOrNull(FieldWithInfo)));
                // child.style.display = displayStyle;
            }

            // bool returnIsNull = RuntimeUtil.IsNull(value);
            // bool preIsNull = RuntimeUtil.IsNull(_preValue);
            // if (returnIsNull && preIsNull)
            // {
            //     return baseResult;
            // }
            //
            // bool isCollection = !returnIsNull
            //                     && value is not UnityEngine.Object
            //                     && (value.GetType().IsArray || value is IEnumerable);
            //
            // bool isEqual;
            // if (isCollection)
            // {
            //     if (preIsNull)
            //     {
            //         isEqual = false;
            //     }
            //     else if (_preValue is IEnumerable preIe)
            //     {
            //         isEqual = preIe.Cast<object>().SequenceEqual(((IEnumerable)value).Cast<object>());
            //     }
            //     else
            //     {
            //         isEqual = Util.GetIsEqual(value, _preValue);
            //     }
            // }
            // else
            // {
            //     isEqual = Util.GetIsEqual(value, _preValue);
            // }
            //
            // // Debug.Log(isEqual);
            //
            // if (!isEqual)
            // {
            //     _preValue = value;
            //     string labelName = NoLabel ? null : GetName(FieldWithInfo);
            //     (VisualElement result, bool isNestedField) = UIToolkitValueEdit(
            //         _returnValueContainer.Children().FirstOrDefault(),
            //         labelName,
            //         methodInfo.ReturnType,
            //         value,
            //         null,
            //         _ => { },
            //         false,
            //         false
            //     );
            //     if (result != null)
            //     {
            //         if (isNestedField && result is Foldout { value: false } fo)
            //         {
            //             fo.RegisterCallback<AttachToPanelEvent>(_ => fo.value = true);
            //             // fo.value = true;
            //         }
            //         _returnValueContainer.Clear();
            //         _returnValueContainer.Add(result);
            //     }
            // }

            return baseResult;
        }
    }
}
#endif
