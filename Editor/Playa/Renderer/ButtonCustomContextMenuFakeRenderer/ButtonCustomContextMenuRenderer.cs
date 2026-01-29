using System;
using System.Collections;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ButtonCustomContextMenuFakeRenderer
{
    public class ButtonCustomContextMenuRenderer: AbsRenderer
    {
        private readonly CustomContextMenuAttribute _customContextMenuAttribute;
        public ButtonCustomContextMenuRenderer(CustomContextMenuAttribute customContextMenuAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _customContextMenuAttribute = customContextMenuAttribute;
        }

        // private HelpBox _helpBox;
        private IVisualElementScheduledItem _buttonTask;
        private IEnumerator _enumerator;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
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

            inspectorRoot.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string useMenuName = _customContextMenuAttribute.FuncName ?? _customContextMenuAttribute.MenuName ?? GetFriendlyName(FieldWithInfo);
                DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;
                if (_customContextMenuAttribute.MenuNameIsCallback)
                {
                    (string error, object result) = Util.GetOf<object>(
                        _customContextMenuAttribute.FuncName ?? _customContextMenuAttribute.MenuName,
                        null, FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, GetRefreshedTarget(FieldWithInfo, FieldWithInfo.Targets[0]).useTarget, null);
                    if (error != "")
                    {
                        UIToolkitUtils.SetHelpBox(helpBox, error);
                    }
                    else
                    {
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (result is null or "") //  don't do menu this time
                        {
                            return;
                        }

                        if (result is ValueTuple<string, EContextMenuStatus> sb)
                        {
                            useMenuName = sb.Item1;

                            if (useMenuName is null or "") //  don't do menu this time
                            {
                                return;
                            }

                            status = sb.Item2 switch
                            {
                                EContextMenuStatus.Normal => DropdownMenuAction.Status.Normal,
                                EContextMenuStatus.Checked => DropdownMenuAction.Status.Checked,
                                EContextMenuStatus.Disabled => DropdownMenuAction.Status.Disabled,
                                _ => throw new ArgumentOutOfRangeException(nameof(sb.Item2), sb.Item2, null),
                            };
                            // isChecked = sb.Item2;
                            // isDisabled = sb.Item3;
                        }
                        else
                        {
                            useMenuName = result.ToString();
                        }
                    }
                }

                evt.menu.AppendAction(useMenuName, _ =>
                {
                    _buttonTask?.Pause();

                    string buttonError = "";
                    object buttonResult = null;
                    (object rawMemberValue, object useTarget) = GetRefreshedTarget(FieldWithInfo, FieldWithInfo.Targets[0]);
                    try
                    {
                        buttonResult = FieldWithInfo.MethodInfo.Invoke(
                            useTarget,
                            Array.Empty<object>());
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        buttonError = e.InnerException?.Message ?? e.Message;
                    }

                    if (buttonError == "")
                    {
                        BackWriteCallback(rawMemberValue, useTarget);
                    }
                    UIToolkitUtils.SetHelpBox(helpBox, buttonError);

                    _buttonTask?.Pause();
                    if (buttonResult is IEnumerator enumerator)
                    {
                        _enumerator = enumerator;
                        _buttonTask = helpBox.schedule.Execute(() =>
                        {
                            if (_enumerator == null)
                            {
                                return;
                            }

                            // ReSharper disable once InvertIf
                            if (!_enumerator.MoveNext())
                            {
                                _enumerator = null;
                                _buttonTask?.Pause();
                                _buttonTask = null;
                            }
                        }).Every(1);
                    }

                }, status);
            }));

            // Debug.Log($"Add menu for {container}");
            // CustomContextMenuUtils.AddManipulator(container, _customContextMenuAttribute.FuncName, _customContextMenuAttribute.MenuName, _customContextMenuAttribute.MenuNameIsCallback, false, FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, this);
            return (helpBox, false);
        }


        protected override bool AllowGuiColor => true;
        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }
    }
}
