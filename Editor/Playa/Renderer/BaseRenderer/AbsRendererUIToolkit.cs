#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.FieldContextMenuDrawer;
using SaintsField.Editor.Drawers.GUIColor;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer
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
            return UpdatePreCheckUIToolkitInternal(FieldWithInfo, root);
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



        public static string GetDropdownTypeLabel(Type type)
        {
            return type == null
                ? "null"
                : $"{type.Name}: <color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>{type.Namespace}</color>";
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
#pragma warning disable CS0168 // Variable is declared but never used
                            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                            {
                                // ignored
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
#pragma warning disable CS0168 // Variable is declared but never used
                                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                                {
                                    // ignored
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
        public string ApplyGuiColor(VisualElement result)
        {
            if (!HasGuiColor())
            {
                return "";
            }
            (string error, Color color) = GUIColorAttributeDrawer.GetColor(_guiColorAttribute, FieldWithInfo.SerializedProperty,
                (MemberInfo)FieldWithInfo.FieldInfo ?? (MemberInfo)FieldWithInfo.PropertyInfo ?? FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0]);

            if (error == "")
            {
                UIToolkitUtils.ApplyColor(result, color);
            }

            return error;
        }
    }
}
#endif
