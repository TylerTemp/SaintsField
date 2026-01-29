#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.FieldContextMenuDrawer;
using SaintsField.Editor.Linq;
#if UNITY_2021_2_OR_NEWER
#endif
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer: IRichTextTagProvider
    {
        private const string ClassSaintsFieldPlaya = "saintsfield-playa";
        public const string ClassSaintsFieldEditingDisabled = "saintsfield-editing-disabled";
        public const string ClassSaintsFieldPlayaContainer = ClassSaintsFieldPlaya + "-container";

        private VisualElement _rootElement;

        private class ManipulatorHandler : CustomContextMenuUtils.IManipulatorHandler
        {
            private readonly AbsRenderer _absRenderer;
            private readonly HelpBox _helpBox;
            private readonly List<IEnumerator> _enumerators = new List<IEnumerator>();
            private IVisualElementScheduledItem _buttonTask;

            public ManipulatorHandler(AbsRenderer absRenderer, HelpBox helpBox)
            {
                _absRenderer = absRenderer;
                _helpBox = helpBox;
            }

            public void SetHelpBox(string error)
            {
                UIToolkitUtils.SetHelpBox(_helpBox, error);
            }

            public void SetIEnumerators(IReadOnlyCollection<IEnumerator> enumerators)
            {
                _buttonTask?.Pause();
                _enumerators.Clear();
                _enumerators.AddRange(enumerators);

                if (_enumerators.Count > 0)
                {
                    _buttonTask = _helpBox.schedule.Execute(() =>
                    {
                        HashSet<IEnumerator> completedEnumerators = new HashSet<IEnumerator>();

                        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                        foreach (IEnumerator enumerator in _enumerators)
                        {
                            if (!enumerator.MoveNext())
                            {
                                completedEnumerators.Add(enumerator);
                            }
                        }

                        _enumerators.RemoveAll(each  => completedEnumerators.Contains(each));
                    }).Every(1);
                }
            }

            public object GetParent()
            {
                return GetRefreshedTarget(_absRenderer.FieldWithInfo, _absRenderer.FieldWithInfo.Targets[0]).useTarget;
            }
        }

        public virtual VisualElement CreateVisualElement(VisualElement inspectorRoot)
        {
            int flexGrow;
            // if (InDirectHorizontalLayout)
            // {
            //     flexGrow = 1;
            // }
            // else
            // {
            //     flexGrow = InAnyHorizontalLayout ? 0 : 1;
            // }
            // Debug.Log(InDirectHorizontalLayout);
            if (InDirectHorizontalLayout)
            {
                flexGrow = 1;
            }
            else
            {
                flexGrow = InAnyHorizontalLayout ? 0 : 1;
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    // flexGrow = 1,
                    // flexGrow = InAnyHorizontalLayout? 0: 1,
                    // flexGrow = 1,
                    flexGrow = flexGrow,
                    flexShrink = 1,
                    // width = new StyleLength(Length.Percent(100)),
                },
                name = ToString(),
            };
            root.AddToClassList(ClassSaintsFieldPlaya);
            bool hasAnyChildren = false;

            (VisualElement target, bool targetNeedUpdate) = CreateTargetUIToolkit(inspectorRoot, root);
            if (target != null)
            {
                VisualElement targetContainer = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 0,

                        width = new StyleLength(Length.Percent(100)),
                    },
                };
                targetContainer.AddToClassList(ClassSaintsFieldPlayaContainer);
                targetContainer.Add(target);
                root.Add(targetContainer);
                hasAnyChildren = true;
            }

            if (!targetNeedUpdate)
            {
                GUIColorAttribute guiColor = FieldWithInfo.PlayaAttributes?.OfType<GUIColorAttribute>().FirstOrDefault();
                if (guiColor != null)
                {
                    if (guiColor.IsCallback)
                    {
                        targetNeedUpdate = true;
                    }
                    else  // we need to update at least once to apply the color
                    {
                        UIToolkitUtils.OnAttachToPanelOnce(root, _ =>
                        {
                            root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement));
                        });
                    }
                }
            }

            if (targetNeedUpdate)
            {
                UIToolkitUtils.OnAttachToPanelOnce(root, _ =>
                {
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement));
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement)).Every(100);
                });
            }
            if(targetNeedUpdate || hasAnyChildren)
            {
                root.Add(_helpBox = new HelpBox("", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                    },
                });

                #region ContextMenu

                CustomContextMenuAttribute[] customContextMenuAttributes =
                    (FieldWithInfo.PlayaAttributes ?? Array.Empty<IPlayaAttribute>())
                    .OfType<CustomContextMenuAttribute>()
                    .ToArray();

                // ReSharper disable once InvertIf
                if (customContextMenuAttributes.Length > 0)
                {
                    foreach ((CustomContextMenuAttribute customContextMenuAttribute, int index)  in customContextMenuAttributes.WithIndex())
                    {
                        HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                                display = DisplayStyle.None,
                            },
                        };
                        root.Add(helpBox);
                        CustomContextMenuUtils.AddManipulator(root, customContextMenuAttribute.FuncName, customContextMenuAttribute.MenuName, customContextMenuAttribute.MenuNameIsCallback, index == 0, FieldWithInfo.SerializedProperty, FieldWithInfo.FieldInfo, new ManipulatorHandler(this, helpBox));
                    }
                }

                #endregion

                return _rootElement = root;
            }

            return null;
        }

        protected abstract (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container);


        protected virtual PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            return UpdatePreCheckUIToolkitInternal(FieldWithInfo, _rootElement);
        }

        // protected PreCheckResult HelperOnUpdateUIToolKitRawBase()
        // {
        //     return UpdatePreCheckUIToolkit();
        // }

        // protected PreCheckResult UpdatePreCheckUIToolkit()
        // {
        //
        // }

        private Color _preColor;
        private HelpBox _helpBox;

        protected PreCheckResult UpdatePreCheckUIToolkitInternal(SaintsFieldWithInfo fieldWithInfo, VisualElement result)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(fieldWithInfo, false);
            // Debug.Log($"{preCheckResult.HasGuiColor}/{preCheckResult.GuiColor}");
            if(result.enabledSelf != !preCheckResult.IsDisabled)
            {
                result.SetEnabled(!preCheckResult.IsDisabled);
            }

            bool isShown = result.style.display != DisplayStyle.None;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_IS_SHOWN
            Debug.Log($"{fieldWithInfo} {result.name} isShown={isShown}, preCheckIsShown={preCheckResult.IsShown}");
#endif

            if(isShown != preCheckResult.IsShown)
            {
                result.style.display = preCheckResult.IsShown ? DisplayStyle.Flex : DisplayStyle.None;
            }

            ApplyGuiColor(result);

            UIToolkitUtils.SetHelpBox(_helpBox, preCheckResult.Error);

            return preCheckResult;
        }

        public static readonly Color ReColor = EColor.EditorSeparator.GetColor();

        // before set: useful for struct editing that C# will mess-up and change the value of the reference you have

        private static readonly Type[] SkipTypes = { typeof(IntPtr), typeof(UIntPtr), typeof(void) };

        public static bool SkipTypeDrawing(Type checkType)
        {
            foreach (Type disallowType in SkipTypes)
            {
                if (disallowType.IsAssignableFrom(checkType))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetDropdownTypeLabel(Type type)
        {
            return type == null
                ? "null"
                : $"{type.Name}: <color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>{type.Namespace}</color>";
        }

        public static (object rawMemberValue, object useTarget) GetRefreshedTarget(SaintsFieldWithInfo fieldWithInfo, object eachTarget)
        {
            bool isStruct = ReflectUtils.TypeIsStruct(eachTarget.GetType());
            object useTarget = eachTarget;
            object rawMemberValue = eachTarget;
            if (isStruct && fieldWithInfo.TargetParent != null && fieldWithInfo.TargetMemberInfo != null)
            {
                switch (fieldWithInfo.TargetMemberInfo)
                {
                    case FieldInfo fieldInfo:
                    {
                        try
                        {
                            useTarget = rawMemberValue =  fieldInfo.GetValue(fieldWithInfo.TargetParent);
                            if (fieldWithInfo.TargetMemberIndex != -1)
                            {
                                useTarget = GetCollectionIndex(useTarget, fieldWithInfo.TargetMemberIndex);
                            }
                            // Debug.Log($"useTarget={useTarget}");
                        }
                        catch (Exception e)
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogException(e);
#endif
                        }
                    }
                        break;
                    case PropertyInfo propertyInfo:
                    {
                        if (propertyInfo.CanRead)
                        {
                            try
                            {
                                useTarget = rawMemberValue = propertyInfo.GetValue(fieldWithInfo.TargetParent);
                                if (fieldWithInfo.TargetMemberIndex != -1)
                                {
                                    useTarget = GetCollectionIndex(useTarget, fieldWithInfo.TargetMemberIndex);
                                }
                            }
                            catch (Exception e)
                            {
#if SAINTSFIELD_DEBUG
                                Debug.LogException(e);
#endif
                            }
                        }
                    }
                        break;
                }
            }

            return (rawMemberValue, useTarget);
        }

        public string GetLabel()
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return FieldWithInfo.SerializedProperty.displayName;
                    }

                    return "";
                }
                case SaintsRenderType.NonSerializedField:
                {
                    if (FieldWithInfo.FieldInfo != null)
                    {
                        return ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name);
                    }

                    return "";
                }
                case SaintsRenderType.Method:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name);
                case SaintsRenderType.NativeProperty:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name);
                case SaintsRenderType.ClassStruct:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.ClassStructType.Name);
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        public string GetContainerType()
        {
            return GetTargetType().Name;
        }

        private Type GetTargetType()
        {
            return  FieldWithInfo.ClassStructType ?? FieldWithInfo.Targets[0].GetType();
        }

        public string GetContainerTypeBaseType()
        {
            return GetTargetType().BaseType?.Name ?? "";
        }

        public string GetIndex(string formatter)
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return "";
                    }

                    int propPath = SerializedUtils.PropertyPathIndex(FieldWithInfo.SerializedProperty.propertyPath);
                    return propPath < 0 ? "" : propPath.ToString();
                }
                case SaintsRenderType.NonSerializedField:
                case SaintsRenderType.Method:
                case SaintsRenderType.NativeProperty:
                case SaintsRenderType.ClassStruct:
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        public string GetField(string rawContent, string tagName, string tagValue)
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    if (!SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return "";
                    }

                    // string error = "";

                    (string error, int index, object value) result = Util.GetValue(FieldWithInfo.SerializedProperty, FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
                    // (string error, int index, object value) accResult = result;
                    if (result.error != "")
                    {
                        // error = result.error;
                    }
                    else
                    {
                        if (tagName == "field")
                        {
                        }
                        else
                        {
                            string revName = tagName["field.".Length..];

                            (string error, object result) getOfValue = Util.GetOf<object>(revName, null,
                                FieldWithInfo.SerializedProperty,
                                FieldWithInfo.FieldInfo, result.value, null);

                            // hasError = getOfValue.error != "";
                            // error = getOfValue.error;
                            result = (getOfValue.error, result.index, getOfValue.result);
                        }
                    }

                    // ReSharper disable once InvertIf
                    if (result.error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning(result.error);
#endif
                        return rawContent;
                    }

                    return RichTextDrawer.TagStringFormatter(result.value, tagValue);
                }
                case SaintsRenderType.NonSerializedField:
                case SaintsRenderType.Method:
                case SaintsRenderType.NativeProperty:
                case SaintsRenderType.ClassStruct:
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        protected void BackWriteCallback(object rawMemberValue, object useTarget)
        {
            bool isStruct = ReflectUtils.TypeIsStruct(FieldWithInfo.Targets[0].GetType());
            if (isStruct && FieldWithInfo.TargetParent != null && FieldWithInfo.TargetMemberInfo != null)
            {
                // Debug.Log($"write back {FieldWithInfo.TargetParent}:{FieldWithInfo.TargetMemberInfo.Name}");
                switch (FieldWithInfo.TargetMemberInfo)
                {
                    case FieldInfo fieldInfo:
                    {
                        if (FieldWithInfo.TargetMemberIndex != -1)
                        {
                            if(rawMemberValue != null)
                            {
                                Util.SetCollectionIndex(rawMemberValue, FieldWithInfo.TargetMemberIndex, useTarget);
                            }
                        }
                        else
                        {
                            try
                            {
                                fieldInfo.SetValue(FieldWithInfo.TargetParent, useTarget);
                            }
                            catch (Exception e)
                            {
#if SAINTSFIELD_DEBUG
                                Debug.LogException(e);
#endif
                            }
                        }
                    }
                        break;
                    case PropertyInfo propertyInfo:
                    {
                        if (propertyInfo.CanWrite)
                        {
                            if (FieldWithInfo.TargetMemberIndex != -1)
                            {
                                if(rawMemberValue != null)
                                {
                                    Util.SetCollectionIndex(rawMemberValue, FieldWithInfo.TargetMemberIndex,
                                        useTarget);
                                }
                            }
                            else
                            {
                                try
                                {
                                    propertyInfo.SetValue(FieldWithInfo.TargetParent, useTarget);
                                }
                                catch (Exception e)
                                {
#if SAINTSFIELD_DEBUG
                                    Debug.LogException(e);
#endif
                                }
                            }
                        }
                    }
                        break;
                }
            }
        }
    }
}
#endif
