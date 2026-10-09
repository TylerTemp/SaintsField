using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.DropdownDrawer;
// using SaintsField.Editor.Playa.Renderer.ChipListRenderer.ChipsInput;
using SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput;
using SaintsField.Editor.UIToolkitElements;
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
        // private class ChipsInputField : BaseField<UnityEngine.Object>
        // {
        //     public ChipsInputField(string label, VisualElement visualInput) : base(label, visualInput)
        //     {
        //     }
        // }

        protected override bool AllowGuiColor => true;
        public override void OnDestroyUIToolkit()
        {
            _overlayHost?.UnregisterCallback<PointerDownEvent>(OnOverlayPointerDown, TrickleDown.TrickleDown);
            _overflowWrapperElement?.RemoveFromHierarchy();
        }

        private EmptyPrefabOverrideField _chipsInputField;
        private ChipsInputElement _chipsInputElement;
        private HelpBox _helpBox;
        private OverflowWrapperElement _overflowWrapperElement;
        private VisualElement _overlayHost;

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            VisualElement root = new VisualElement();
            SerializedProperty property = FieldWithInfo.SerializedProperty;
            Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
                                                           FieldWithInfo.PropertyInfo.PropertyType);

            _chipsInputElement = new ChipsInputElement();
            _chipsInputField = new EmptyPrefabOverrideField(GetFriendlyName(FieldWithInfo), _chipsInputElement, FieldWithInfo.SerializedProperty);
            _chipsInputField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);
            UIToolkitUtils.AddContextualMenuManipulator(_chipsInputField, FieldWithInfo.SerializedProperty, () => {});
            _chipsInputField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Clear", _ =>
                {
                    FieldWithInfo.SerializedProperty.arraySize = 0;
                    FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                });
            }));
            UIToolkitUtils.AddContextualMenuReset(_chipsInputField, FieldWithInfo.SerializedProperty, FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);

            if (InAnyHorizontalLayout)
            {
                _chipsInputField.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                _chipsInputField.AddToClassList(EmptyPrefabOverrideField.alignedFieldUssClassName);
            }

            root.Add(_chipsInputField);
            _chipsInputElement.BindProp(property);

            void Search(string text) => UIToolkitUtils.SetDisplayStyle(root,
                Util.UnityDefaultSimpleSearch(property.displayName, text)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);
            OnSearchFieldUIToolkit.AddListener(Search);
            UIToolkitUtils.OnAttachToPanelOnceWithEnsure(root, () =>
            {
                OnSearchFieldUIToolkit.RemoveListener(Search);
                OnSearchFieldUIToolkit.AddListener(Search);
            });
            root.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));

            _chipsInputField.RegisterCallback<DragEnterEvent>(_ => SetDragVisualMode(elementType));
            _chipsInputField.RegisterCallback<DragLeaveEvent>(_ =>
                DragAndDrop.visualMode = DragAndDropVisualMode.None);
            _chipsInputField.RegisterCallback<DragUpdatedEvent>(_ => SetDragVisualMode(elementType));
            _chipsInputField.RegisterCallback<DragPerformEvent>(evt =>
            {
                if (!DropUIToolkit(elementType, property))
                {
                    return;
                }

                property.serializedObject.ApplyModifiedProperties();
                UpdateBindChips(property);
                evt.StopPropagation();
            });

            void SetDragVisualMode(Type dropElementType)
            {
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, dropElementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            }

            #region Array Size
            SerializedProperty arraySize = property.FindPropertyRelative("Array.size");
            _chipsInputElement.TrackPropertyValue(arraySize, _ => UpdateBindChips(property));
            UpdateBindChips(property);
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
            _chipsInputElement.OnCancelledEvent.AddListener(() =>
            {
                _pullingMetaInfo = false;
                _createDropdownWhenMetaReady = false;
                _pendingCreateDropdown = false;
            });

            PullMetaInfo("", false);

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

                VisualElement overlayHost = root;
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
        private bool _pullingMetaInfo;
        private bool _createDropdownWhenMetaReady;

        private void OnSearchChanged(bool newSearch, string searchContent)
        {
            _search = searchContent;

            UIToolkitUtils.SetHelpBox(_helpBox, "");

            if (_isTypingSearchCallback)
            {
                CloseDropdown();
                _pendingCreateDropdown = true;
                PullMetaInfo(searchContent, true);
                return;
            }

            if (_treeDropdownElement == null && !_pendingCreateDropdown)
            {
                _pendingCreateDropdown = true;
                PullMetaInfo(searchContent, true);
            }
            else if (!_pendingCreateDropdown && _treeDropdownElement != null)
            {
                if(_treeDropdownElement.ToolbarSearchField.value != _search)
                {
                    _treeDropdownElement.ToolbarSearchField.value = _search;
                }
            }

        }

        // private bool _isTypingSearchCallbackChecked = false;
        private bool _isTypingSearchCallback;

        private void PullMetaInfo(string searchString, bool createDropdown)
        {
            _createDropdownWhenMetaReady |= createDropdown;
            if (_pullingMetaInfo)
            {
                if (!_isTypingSearchCallback)
                {
                    return;
                }

                // A typing callback must fetch the latest query, even while the previous one is pending.
                _chipsInputElement.StopTrack();
            }

            _pullingMetaInfo = true;

            FetchMetaInfo(new object[] { searchString }, false);
            return;

            void FetchMetaInfo(IReadOnlyList<object> overrideParams, bool isFallback)
            {
                AdvancedDropdownAttributeDrawer.GetMetaInfoAsync(
                    _chipsInputElement,
                    metaInfo =>
                    {
                        SerializedProperty arrayProperty = FieldWithInfo.SerializedProperty;
                        if (!SerializedUtils.IsOk(arrayProperty))
                        {
                            _pendingCreateDropdown = false;
                            return;
                        }

                        if (!_isTypingSearchCallback
                            && !isFallback
                            && metaInfo is { Error: "", MemberInfo: MethodInfo memberInfo })
                        {
                            bool matchedStringParamUsed = false;
                            bool matchedStringParam = false;
                            ParameterInfo[] methodParams = memberInfo.GetParameters();
                            for (int index = 0; index < memberInfo.GetParameters().Length; index++)
                            {
                                ParameterInfo parameterInfo = methodParams[index];
                                bool isStringParam = parameterInfo.ParameterType == typeof(string);
                                if (parameterInfo.IsOptional)
                                {
                                    if (isStringParam)
                                    {
                                        if (matchedStringParamUsed)
                                        {
                                            matchedStringParam = false;
                                            break;
                                        }

                                        matchedStringParam = true;
                                    }
                                }
                                else
                                {
                                    if (isStringParam)
                                    {
                                        if (matchedStringParamUsed)
                                        {
                                            matchedStringParam = false;
                                            break;
                                        }

                                        matchedStringParam = true;
                                        matchedStringParamUsed = true;
                                    }
                                    else
                                    {
                                        matchedStringParam = false;
                                        break;
                                    }
                                }
                            }

                            _isTypingSearchCallback = matchedStringParam;
                        }

                        if (!isFallback && metaInfo.Error != "")
                        {
                            FetchMetaInfo(null, true);
                            return;
                        }

                        _pullingMetaInfo = false;
                        if (_isTypingSearchCallback && _createDropdownWhenMetaReady && searchString != _search)
                        {
                            // The input may change before the initial fetch identifies a typing callback.
                            PullMetaInfo(_search, true);
                            return;
                        }

                        bool shouldCreateDropdown = _createDropdownWhenMetaReady;
                        _createDropdownWhenMetaReady = false;

                        CacheDisplayInfo(metaInfo.DropdownListValue);
                        UIToolkitUtils.SetHelpBox(_helpBox, metaInfo.Error);
                        UpdateBindChips(arrayProperty);

                        if (shouldCreateDropdown)
                        {
                            CreateDropdown(metaInfo);
                        }
                    },
                    FieldWithInfo.SerializedProperty,
                    _attribute,
                    (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                    FieldWithInfo.Targets[0],
                    false,
                    overrideParams: overrideParams
                );
            }
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

            (string uniqueError, IDropdown uniqueDropdown) = AdvancedDropdownAttributeDrawer.GetUniqueListForArray(
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
                SerializedProperty arrProp = FieldWithInfo.SerializedProperty;
                if (!SerializedUtils.IsOk(arrProp))
                {
                    CloseDropdown();
                    return;
                }

                int newIndex = Mathf.Clamp(_chipsInputElement.GetInputIndex(), 0, arrProp.arraySize);
                int appendedIndex = arrProp.arraySize;
                arrProp.arraySize++;
                if (newIndex != appendedIndex)
                {
                    arrProp.MoveArrayElement(appendedIndex, newIndex);
                }
                SerializedProperty elementProperty = arrProp.GetArrayElementAtIndex(newIndex);
                Util.SignPropertyValue(elementProperty,
                    (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                    FieldWithInfo.Targets[0], value);
                _addElementShift = true;
                arrProp.serializedObject.ApplyModifiedProperties();
                // _chipsInputElement.InputFocus();
                // _chipsInputField.schedule.Execute(() => CloseDropdown());
                CloseDropdown();
            });

            _overflowWrapperElement.Clear();
            _overflowWrapperElement.style.display = DisplayStyle.Flex;
            _overflowWrapperElement.BringToFront();
            _overflowWrapperElement.Add(_treeDropdownElement);
            _overflowWrapperElement.AnchorTo(_chipsInputField);

            if (!_isTypingSearchCallback)
            {
                _chipsInputElement.schedule.Execute(() => _chipsInputElement.InputFocus());
            }

            if (!_isTypingSearchCallback && !string.IsNullOrEmpty(_search))
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

        private void UpdateBindChips(SerializedProperty arrayProp)
        {
            if (!SerializedUtils.IsOk(arrayProp))
            {
                return;
            }

            int newSize = arrayProp.arraySize;
            int index = 0;
            MemberInfo fieldInfo = (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo;
            object parent = FieldWithInfo.Targets[0];
            foreach (DeletableChip chip in _chipsInputElement.GetOrCreateChip(newSize))
            {
                chip.BindProp(arrayProp, index, this, fieldInfo, parent);
                index++;
            }

            if (_addElementShift)
            {
                _chipsInputElement.ActualInputMove(false);
                _addElementShift = false;
            }
        }

        public readonly struct DisplayInfo
        {
            public readonly string NameWithPath;
            public readonly string Icon;
            public readonly Color? Color;

            public DisplayInfo(string nameWithPath, string icon, Color? color)
            {
                NameWithPath = nameWithPath;
                Icon = icon;
                Color = color;
            }
        }

        private readonly Dictionary<object, DisplayInfo> _cachedValueToDisplayInfo = new Dictionary<object, DisplayInfo>();
        private DisplayInfo? _cachedNullDisplayInfo;

        private void CacheDisplayInfo(IDropdown dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            foreach (AdvancedDropdownAttributeDrawer.FlattenInfo option in AdvancedDropdownAttributeDrawer.Flatten(dropdown))
            {
                DisplayInfo displayInfo = new DisplayInfo(
                    string.Join("/", option.stackDisplays),
                    option.icon,
                    option.color);
                if (option.value == null)
                {
                    _cachedNullDisplayInfo = displayInfo;
                    continue;
                }

                _cachedValueToDisplayInfo[option.value] = displayInfo;
            }
        }

        public (string nameWithPath, string icon, Color? color) GetDisplay(object value)
        {
            if (value == null)
            {
                return _cachedNullDisplayInfo.HasValue
                    ? (_cachedNullDisplayInfo.Value.NameWithPath, _cachedNullDisplayInfo.Value.Icon, _cachedNullDisplayInfo.Value.Color)
                    : ("", null, null);
            }

            if (_cachedValueToDisplayInfo.TryGetValue(value, out DisplayInfo displayInfo))
            {
                return (displayInfo.NameWithPath, displayInfo.Icon, displayInfo.Color);
            }
            return ($"{value}", null, null);
        }
    }
}
