using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.DropdownDrawer;
// using SaintsField.Editor.Playa.Renderer.ChipListRenderer.ChipsInput;
using SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// using ChipsInputElement = SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput.ChipsInputElement;
// using OverflowWrapperElement = SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput.OverflowWrapperElement;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer
{
    public partial class ChipsAttributeRenderer: IDeletableChipDisplayResolver
    {
        private class ChipsInputField : BaseField<UnityEngine.Object>
        {
            public ChipsInputField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        protected override bool AllowGuiColor => true;
        public override void OnDestroyUIToolkit()
        {
            _overlayHost?.UnregisterCallback<PointerDownEvent>(OnOverlayPointerDown, TrickleDown.TrickleDown);
            _overflowWrapperElement?.RemoveFromHierarchy();
        }

        private ChipsInputField _chipsInputField;
        private ChipsInputElement _chipsInputElement;
        private HelpBox _helpBox;
        private OverflowWrapperElement _overflowWrapperElement;
        private VisualElement _overlayHost;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot, VisualElement container)
        {
            VisualElement root = new VisualElement();

            _chipsInputElement = new ChipsInputElement();
            _chipsInputField = new ChipsInputField(GetFriendlyName(FieldWithInfo), _chipsInputElement);
            if (InAnyHorizontalLayout)
            {
                _chipsInputField.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                _chipsInputField.AddToClassList(ChipsInputField.alignedFieldUssClassName);
            }

            root.Add(_chipsInputField);
            // chipElement.BindProperty(FieldWithInfo.SerializedProperty);

            SerializedProperty property = FieldWithInfo.SerializedProperty;
            _chipsInputElement.BindProp(property);

            #region Array Size
            SerializedProperty arraySize = property.FindPropertyRelative("Array.size");
            _chipsInputElement.TrackPropertyValue(arraySize, _ => OnArraySizeChanged(property));
            OnArraySizeChanged(property);
            #endregion

            // search
            _chipsInputElement.OnSearchEvent.AddListener(OnSearchChanged);

            _chipsInputElement.BubbleNavigationMoveEvent.AddListener(evt =>
            {
                _treeDropdownElement?.OnNavigationMove(evt);
            });
            _chipsInputElement.OnEnterKey.AddListener(() =>
            {
                _treeDropdownElement?.OnEnterKey();
            });

            _chipsInputElement.SetInputToLastChild();
            _chipsInputElement.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (evt.target == _chipsInputElement)
                {
                    UIToolkitUtils.Unbind(_chipsInputElement);
                    _chipsInputElement.OnSearchEvent.RemoveListener(OnSearchChanged);
                }
            });

            _helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            root.Add(_helpBox);
            _chipsInputElement.OnErrorEvent.AddListener(exception =>
            {
                UIToolkitUtils.SetHelpBox(_helpBox, exception?.InnerException?.Message ?? exception?.Message ?? "");
                if (exception != null)
                {
                    _pullingMetaInfo = false;
                    _createDropdownWhenMetaReady = false;
                    _pendingCreateDropdown = false;
                }
            });

            PullMetaInfo(false);

            _overflowWrapperElement = new OverflowWrapperElement
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            // root.Add(_overflowWrapperElement);

            UIToolkitUtils.OnAttachToPanelOnceWithEnsure(root, () =>
            {
                Debug.Assert(root.panel != null);

                VisualElement overlayHost = inspectorRoot;
                while (overlayHost.parent != null && overlayHost.parent != root.panel.visualTree)
                {
                    overlayHost = overlayHost.parent;
                }

                _overlayHost = overlayHost;
                _overlayHost.Add(_overflowWrapperElement);
                _overlayHost.RegisterCallback<PointerDownEvent>(OnOverlayPointerDown, TrickleDown.TrickleDown);
            });

            return (root, false);
        }

        private void OnOverlayPointerDown(PointerDownEvent evt)
        {
            if (_treeDropdownElement == null || evt.target is not VisualElement target)
            {
                return;
            }

            bool insideInput = target == _chipsInputElement || _chipsInputElement.Contains(target);
            bool insideDropdown = target == _overflowWrapperElement || _overflowWrapperElement.Contains(target);
            if (!insideInput && !insideDropdown)
            {
                CloseDropdown();
            }
        }

        private void CloseDropdown()
        {
            _overflowWrapperElement.Clear();
            _overflowWrapperElement.style.display = DisplayStyle.None;
            _pendingCreateDropdown = false;
            _treeDropdownElement = null;
        }

        private bool _pendingCreateDropdown;
        private string _search;
        private SaintsTreeDropdownElement _treeDropdownElement;
        private bool _addElementShift;
        private AdvancedDropdownMetaInfo? _metaInfo;
        private bool _pullingMetaInfo;
        private bool _createDropdownWhenMetaReady;

        private void OnSearchChanged(bool newSearch, string searchContent)
        {
            _search = searchContent;

            UIToolkitUtils.SetHelpBox(_helpBox, "");
            if (_treeDropdownElement == null && !_pendingCreateDropdown)
            {
                _pendingCreateDropdown = true;
                PullMetaInfo(true);
            }
            else if (!_pendingCreateDropdown && _treeDropdownElement != null)
            {
                if(_treeDropdownElement.ToolbarSearchField.value != _search)
                {
                    _treeDropdownElement.ToolbarSearchField.value = _search;
                }
            }

        }

        private void PullMetaInfo(bool createDropdown)
        {
            _createDropdownWhenMetaReady |= createDropdown;
            if (_pullingMetaInfo)
            {
                return;
            }

            _pullingMetaInfo = true;
            AdvancedDropdownAttributeDrawer.GetMetaInfoAsync(
                _chipsInputElement,
                metaInfo =>
                {
                    _pullingMetaInfo = false;
                    bool shouldCreateDropdown = _createDropdownWhenMetaReady;
                    _createDropdownWhenMetaReady = false;

                    SerializedProperty arrayProperty = FieldWithInfo.SerializedProperty;
                    if (!SerializedUtils.IsOk(arrayProperty))
                    {
                        _pendingCreateDropdown = false;
                        return;
                    }

                    _metaInfo = metaInfo;
                    UIToolkitUtils.SetHelpBox(_helpBox, metaInfo.Error);
                    OnArraySizeChanged(arrayProperty);

                    if (shouldCreateDropdown)
                    {
                        CreateDropdown(metaInfo);
                    }
                },
                FieldWithInfo.SerializedProperty,
                _attribute,
                (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                FieldWithInfo.Targets[0],
                false
            );
        }

        private void CreateDropdown(AdvancedDropdownMetaInfo metaInfo)
        {
            _pendingCreateDropdown = false;
            if (metaInfo.Error != "")
            {
                return;
            }

            SerializedProperty arrayProperty = FieldWithInfo.SerializedProperty;
            if (!SerializedUtils.IsOk(arrayProperty))
            {
                return;
            }

            var (uniqueError, uniqueDropdown) = AdvancedDropdownAttributeDrawer.GetUniqueListForArray(
                metaInfo.DropdownListValue,
                _attribute.EUnique,
                arrayProperty,
                (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                FieldWithInfo.Targets[0]);
            if (uniqueError != "")
            {
                UIToolkitUtils.SetHelpBox(_helpBox, uniqueError);
                return;
            }

            metaInfo.DropdownListValue = uniqueDropdown;
            metaInfo.CurValues = Array.Empty<object>();

            _treeDropdownElement = new SaintsTreeDropdownElement(metaInfo, false, false);
            _treeDropdownElement.ToolbarSearchField.style.display = DisplayStyle.None;

            _treeDropdownElement.OnClickedEvent.AddListener((value, _, _) =>
            {
                SerializedProperty arrayProperty = FieldWithInfo.SerializedProperty;
                if (!SerializedUtils.IsOk(arrayProperty))
                {
                    CloseDropdown();
                    return;
                }

                int newIndex = Mathf.Clamp(_chipsInputElement.GetInputIndex(), 0, arrayProperty.arraySize);
                int appendedIndex = arrayProperty.arraySize;
                arrayProperty.arraySize++;
                if (newIndex != appendedIndex)
                {
                    arrayProperty.MoveArrayElement(appendedIndex, newIndex);
                }
                SerializedProperty elementProperty = arrayProperty.GetArrayElementAtIndex(newIndex);
                Util.SignPropertyValue(elementProperty,
                    (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                    FieldWithInfo.Targets[0], value);
                _addElementShift = true;
                arrayProperty.serializedObject.ApplyModifiedProperties();
                CloseDropdown();
            });

            _overflowWrapperElement.Clear();
            _overflowWrapperElement.style.display = DisplayStyle.Flex;
            _overflowWrapperElement.BringToFront();
            _overflowWrapperElement.Add(_treeDropdownElement);
            _overflowWrapperElement.AnchorTo(_chipsInputField);

            _chipsInputElement.schedule.Execute(() => _chipsInputElement.InputFocus());

            if (!string.IsNullOrEmpty(_search))
            {
                UIToolkitUtils.OnAttachToPanelOnceWithEnsure(_treeDropdownElement, () =>
                {
                    if(_treeDropdownElement.ToolbarSearchField.value != _search)
                    {
                        _treeDropdownElement.ToolbarSearchField.value = _search;
                    }
                });
            }
        }

        private void OnArraySizeChanged(SerializedProperty arrayProp)
        {
            if (!SerializedUtils.IsOk(arrayProp))
            {
                return;
            }

            int newSize = arrayProp.arraySize;
            int index = 0;
            foreach (DeletableChip chip in _chipsInputElement.GetOrCreateChip(newSize))
            {
                chip.BindProp(arrayProp, index, this);
                index++;
            }

            if (_addElementShift)
            {
                _chipsInputElement.ActualInputMove(false);
                _addElementShift = false;
            }
        }

        public string GetDisplay(object value)
        {
            if (!_metaInfo.HasValue || _metaInfo.Value.Error != "" || _metaInfo.Value.DropdownListValue == null)
            {
                return $"{value}";
            }

            (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStacks, string display) =
                AdvancedDropdownUtil.GetSelected(value, Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                    _metaInfo.Value.DropdownListValue);

            return selectStacks.Count == 0
                ? $"{value}"
                : string.Join("/", selectStacks.Skip(1).Select(each => each.Display).Append(display));
        }
    }
}
