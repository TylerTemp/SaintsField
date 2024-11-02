using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;
using UnityAdvancedDropdown = UnityEditor.IMGUI.Controls.AdvancedDropdown;
using UnityAdvancedDropdownItem = UnityEditor.IMGUI.Controls.AdvancedDropdownItem;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    #region IMGUI Pop

    public class SaintsAdvancedDropdown : UnityAdvancedDropdown
    {
        private readonly IAdvancedDropdownList _dropdownListValue;

        private readonly Dictionary<UnityAdvancedDropdownItem, object> _itemToValue = new Dictionary<UnityAdvancedDropdownItem, object>();
        private readonly Action<object> _setValueCallback;
        private readonly Func<string, Texture2D> _getIconCallback;
        private readonly Rect _showRect;

        public SaintsAdvancedDropdown(IAdvancedDropdownList dropdownListValue, Vector2 size, Rect showRect, AdvancedDropdownState state, Action<object> setValueCallback, Func<string, Texture2D> getIconCallback) : base(state)
        {
            _dropdownListValue = dropdownListValue;
            _setValueCallback = setValueCallback;
            _getIconCallback = getIconCallback;
            _showRect = showRect;

            minimumSize = size;
        }

        protected override UnityAdvancedDropdownItem BuildRoot()
        {
            UnityAdvancedDropdownItem root = MakeUnityAdvancedDropdownItem(_dropdownListValue);

            if(_dropdownListValue.children.Count == 0)
            {
                // root.AddChild(new UnityAdvancedDropdownItem("Empty"));
                return root;
            }

            MakeChildren(root, _dropdownListValue.children);

            return root;
        }

        private UnityAdvancedDropdownItem MakeUnityAdvancedDropdownItem(IAdvancedDropdownList item)
        {
            // if (item.isSeparator)
            // {
            //     return new UnityAdvancedDropdownItem("SEPARATOR");
            // }

            return new UnityAdvancedDropdownItem(item.displayName)
            {
                icon = string.IsNullOrEmpty(item.icon) ? null : _getIconCallback(item.icon),
                enabled = !item.disabled,
            };
        }

        private void MakeChildren(UnityAdvancedDropdownItem parent, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList childItem in children)
            {
                if (childItem.isSeparator)
                {
                    parent.AddSeparator();
                }
                else if (childItem.children.Count == 0)
                {
                    // Debug.Log($"{parent.name}/{childItem.displayName}");
                    UnityAdvancedDropdownItem item = MakeUnityAdvancedDropdownItem(childItem);
                    _itemToValue[item] = childItem.value;
                    // Debug.Log($"add {childItem.displayName} => {childItem.value}");
                    parent.AddChild(item);
                }
                else
                {
                    UnityAdvancedDropdownItem subParent = MakeUnityAdvancedDropdownItem(childItem);
                    // Debug.Log($"{parent.name}/{childItem.displayName}[...]");
                    MakeChildren(subParent, childItem.children);
                    parent.AddChild(subParent);
                }
            }
        }

        protected override void ItemSelected(UnityAdvancedDropdownItem item)
        {
            if (!item.enabled)  // WTF Unity?
            {
                // Show(new Rect(_showRect)
                // {
                //     y = 0,
                //     height = 0,
                // });
                // Show(new Rect(_showRect)
                // {
                //     x = 0,
                //     y = -_showRect.y - _showRect.height,
                //     height = 0,
                // });

                // ReSharper disable once InvertIf
                if(_bindWindowPos)
                {
                    Show(_showRect);
                    EditorWindow curFocusedWindow = EditorWindow.focusedWindow;
                    if (curFocusedWindow == null || curFocusedWindow.GetType().ToString() !=
                        "UnityEditor.IMGUI.Controls.AdvancedDropdownWindow")
                    {
                        return;
                    }
                    curFocusedWindow.position = _windowPosition;
                }

                return;
            }

            // Debug.Log($"select {item.name}: {(_itemToValue.TryGetValue(item, out object r) ? r.ToString() : "[NULL]")}");
            if (_itemToValue.TryGetValue(item, out object result))
            {
                _setValueCallback(result);
            }
        }

        private bool _bindWindowPos;
        // private EditorWindow _thisEditorWindow;
        private Rect _windowPosition;

        // hack for Unity allow to click on disabled item...
        public void BindWindowPosition()
        {
            if (_bindWindowPos)
            {
                return;
            }

            EditorWindow window = EditorWindow.focusedWindow;
            if (window == null || window.GetType().ToString() != "UnityEditor.IMGUI.Controls.AdvancedDropdownWindow")
            {
                return;
            }

            _bindWindowPos = true;
            _windowPosition = window.position;
        }
    }

    #endregion

    #region UIToolkit Pop
#if UNITY_2021_3_OR_NEWER
    public class SaintsAdvancedDropdownUiToolkit : PopupWindowContent
    {
        private readonly float _width;
        private readonly AdvancedDropdownAttributeDrawer.MetaInfo _metaInfo;
        private readonly Action<string, object> _setValue;

        // ReSharper disable once InconsistentNaming
        private readonly UnityEvent<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>> GoToStackEvent =
            new UnityEvent<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>>();

        private readonly bool _isFlat;
        private readonly float _maxHeight;

        public SaintsAdvancedDropdownUiToolkit(AdvancedDropdownAttributeDrawer.MetaInfo metaInfo, float width, float maxHeight, Action<string, object> setValue)
        {
            _width = width;
            _metaInfo = metaInfo;
            _setValue = setValue;
            _maxHeight = maxHeight;

            _isFlat = metaInfo.DropdownListValue.All(each => each.ChildCount() == 0);
        }

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }

        //Set the window size
        public override Vector2 GetWindowSize()
        {
            return new Vector2(_width, 90);
        }

        public override void OnOpen()
        {
            VisualElement element = CloneTree();
            editorWindow.rootVisualElement.Add(element);
        }

        private IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> _curPageStack = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();

        private VisualElement CloneTree()
        {
            StyleSheet ussStyle = Util.LoadResource<StyleSheet>("UIToolkit/SaintsAdvancedDropdown/Style.uss");
            StyleSheet hackSliderStyle = Util.LoadResource<StyleSheet>("UIToolkit/SaintsAdvancedDropdown/HackSliderStyle.uss");

            VisualTreeAsset popUpAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/Popup.uxml");
            VisualElement root = popUpAsset.CloneTree();

            root.styleSheets.Add(ussStyle);

            // root.Q<Image>(name: "saintsfield-advanced-dropdown-search-clean-image").image = Util.LoadResource<Texture2D>("classic-close.png");

            VisualTreeAsset separatorAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/Separator.uxml");

            VisualTreeAsset itemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/ItemRow.uxml");

            VisualElement scrollViewContainer = root.Q<VisualElement>("saintsfield-advanced-dropdown-scroll-view-container");
            ScrollView scrollView = root.Q<ScrollView>();
            scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeoUpdateWindowSize);

            // scrollView.contentContainer.style.borderBottomWidth = 1;
            // scrollView.contentContainer.style.borderBottomColor = Color.green;

            // Texture2D icon = RichTextDrawer.LoadTexture("eye.png");
            Texture2D next = Util.LoadResource<Texture2D>("arrow-next.png");
            Texture2D checkGroup = Util.LoadResource<Texture2D>("arrow-right.png");
            Texture2D check = Util.LoadResource<Texture2D>("check.png");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
            Debug.Log($"selectStack={string.Join("->", _metaInfo.SelectStacks.Select(each => $"{each.Display}/{each.Index}"))}");
#endif

            ToolbarBreadcrumbs toolbarBreadcrumbs = root.Q<ToolbarBreadcrumbs>();
            if (_isFlat)
            {
                toolbarBreadcrumbs.style.display = DisplayStyle.None;
            }

            GoToStackEvent.AddListener(newStack =>
            {
                if (_curPageStack.SequenceEqual(newStack))
                {
                    return;
                }

                bool animRightToLeft = _curPageStack.Count <= newStack.Count;

                SwapToolbarBreadcrumbs(toolbarBreadcrumbs, newStack, GoToStackEvent.Invoke);
                SwapPage(
                    _metaInfo.DropdownListValue,
                    scrollViewContainer,
                    _metaInfo.SelectStacks,
                    newStack,
                    animRightToLeft,
                    separatorAsset,
                    itemAsset,
                    next,
                    check,
                    checkGroup,
                    hackSliderStyle,
                    GoToStackEvent.Invoke,
                    (newDisplay, newValue) =>
                    {
                        _setValue(newDisplay, newValue);
                        editorWindow.Close();
                    });

                _curPageStack = newStack;
            });
            GoToStackEvent.Invoke(_metaInfo.SelectStacks.Count == 0
                ? new []{new AdvancedDropdownAttributeDrawer.SelectStack
                {
                    Display = _metaInfo.DropdownListValue.displayName,
                    Index = -1,
                }}
                : _metaInfo.SelectStacks);

            // search
            ToolbarSearchField toolbarSearchField = root.Q<ToolbarSearchField>();
            IReadOnlyList<(string stackDisplay, string display, string icon, bool disabled, object value)> flattenOptions = AdvancedDropdownAttributeDrawer.Flatten("", _metaInfo.DropdownListValue).ToArray();
            Dictionary<string, VisualElement> stackDisplayToElement = new Dictionary<string, VisualElement>();
            toolbarSearchField.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log($"search {evt.newValue}");
                string[] searchFragments = evt.newValue
                    .Split()
                    .Where(each => each != "")
                    .Select(each => each.ToLower())
                    .ToArray();
                // foreach (string searchFragment in searchFragments)
                // {
                //     Debug.Log($"`{searchFragment}`");
                // }
                if (searchFragments.Length == 0)
                {
                    if(!_isFlat)
                    {
                        toolbarBreadcrumbs.style.display = DisplayStyle.Flex;
                    }

                    AdvancedDropdownAttributeDrawer.SelectStack[] curPageStack = _curPageStack.ToArray();
                    _curPageStack = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
                    GoToStackEvent.Invoke(curPageStack);
                    return;
                }

                toolbarBreadcrumbs.style.display = DisplayStyle.None;

                IEnumerable<(string stackDisplay, string display, string icon, bool disabled, object value)> matchedOptions = flattenOptions.Where(each =>
                {
                    string lowerDisplay = each.display.ToLower();
                    return searchFragments.All(fragment => lowerDisplay.Contains(fragment));
                });

                bool hasMatch = false;

                scrollView.Clear();
                foreach ((string stackDisplay, string display, string icon, bool disabled, object value) in matchedOptions)
                {
                    hasMatch = true;
                    if (!stackDisplayToElement.TryGetValue(stackDisplay, out VisualElement elementItem))
                    {
                        stackDisplayToElement[stackDisplay] = elementItem = itemAsset.CloneTree();

                        Button itemContainer =
                            elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

                        Image selectImage = itemContainer.Q<Image>("item-checked-image");
                        selectImage.image = check;

                        itemContainer.Q<Label>("item-content").text = display;

                        bool curSelect = _metaInfo.SelectStacks.Count > 0 && Util.GetIsEqual(_metaInfo.CurValue, value);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                        Debug.Log($"curSelect={curSelect}, _metaInfo.SelectStacks.Count={_metaInfo.SelectStacks.Count}, _metaInfo.CurValue={_metaInfo.CurValue}, value={value}, _metaInfo.CurValue == value: {_metaInfo.CurValue == value}");
#endif

                        if(!string.IsNullOrEmpty(icon))
                        {
                            itemContainer.Q<Image>("item-icon-image").image = Util.LoadResource<Texture2D>(icon);
                        }

                        if (disabled)
                        {
                            itemContainer.SetEnabled(false);
                            itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-disabled");
                            itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                        }

                        if (curSelect)
                        {
                            itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                            Debug.Log($"cur selected: {value}");
#endif
                            selectImage.visible = true;
                            itemContainer.pickingMode = PickingMode.Ignore;
                        }
                        else
                        {
                            itemContainer.clicked += () =>
                            {
                                _setValue(stackDisplay, value);
                                editorWindow.Close();
                            };
                        }
                    }

                    scrollView.Add(elementItem);
                }

                if (!hasMatch)
                {
                    scrollView.Add(new Label("No match")
                    {
                        style =
                        {
                            width = Length.Percent(100),
                            unityTextAlign = TextAnchor.MiddleCenter,
                        },
                    });
                }
            });

            root.RegisterCallback<AttachToPanelEvent>(_ => root.Q<TextField>().Q("unity-text-input").Focus());

            return root;
        }

        // private bool _delayUpdateSize;
        // private float _delayUpdateHeight;

        // private struct DelayUpdateSize
        // {
        //     public bool IsDelay;
        //     public float Height;
        // }

        // Yep, hack around...
        private void GeoUpdateWindowSize(GeometryChangedEvent evt)
        {
            ScrollView scrollView = editorWindow.rootVisualElement.Q<ScrollView>();
            VisualElement contentContainer = scrollView.contentContainer;
            // var height = contentContainer.resolvedStyle.height;

            VisualElement toolbarSearchContainer = editorWindow.rootVisualElement.Q<VisualElement>("saintsfield-advanced-dropdown-search-container");
            ToolbarBreadcrumbs toolbarBreadcrumbs = editorWindow.rootVisualElement.Q<ToolbarBreadcrumbs>();

            float height = contentContainer.resolvedStyle.height + toolbarSearchContainer.resolvedStyle.height + toolbarBreadcrumbs.resolvedStyle.height + 8;

            editorWindow.maxSize = editorWindow.minSize = new Vector2(_width, Mathf.Min(height, _maxHeight));
            // editorWindow.maxSize = editorWindow.minSize = new Vector2(_width, height);
            // VisualElement target = (VisualElement)evt.target;
            // // float newHeight = evt.newRect.height + 5;
            // float newHeight = target.resolvedStyle.height + 1;
            //
            // DelayUpdateSize delayUpdateSize = (DelayUpdateSize)target.userData;
            //
            // target.userData = new DelayUpdateSize
            // {
            //     IsDelay = true,
            //     Height = newHeight,
            // };
            //
            // if (delayUpdateSize.IsDelay)
            // {
            //     Debug.Log($"delay to {newHeight}");
            //     return;
            // }
            //
            // Debug.Log($"will update {newHeight}");
            // ((VisualElement) evt.target).schedule.Execute(() =>
            // {
            //     DelayUpdateSize newSize = (DelayUpdateSize)target.userData;
            //     Debug.Log($"start to update {newSize.Height}");
            //     editorWindow.maxSize = editorWindow.minSize = new Vector2(_width, newSize.Height);
            //     // _delayUpdateSize = false;
            //     target.userData = new DelayUpdateSize
            //     {
            //         IsDelay = false,
            //         Height = newSize.Height,
            //     };
            // });
        }

        private static void SwapToolbarBreadcrumbs(ToolbarBreadcrumbs toolbarBreadcrumbs, IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> pageStack, Action<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>> goToStack)
        {
            while (toolbarBreadcrumbs.childCount > 0)
            {
                toolbarBreadcrumbs.PopItem();
            }

            // IAdvancedDropdownList target = dropdownList;
            foreach ((AdvancedDropdownAttributeDrawer.SelectStack stack, int stackDepth) in pageStack.WithIndex())
            {
                // int curStackDepth = stackDepth;
                AdvancedDropdownAttributeDrawer.SelectStack[] curStack = pageStack.Take(stackDepth+1).ToArray();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                Debug.Log($"push {stack.Display}: {string.Join("->", curStack.Select(each => $"{each.Display}/{each.Index}"))}");
#endif
                toolbarBreadcrumbs.PushItem(stack.Display, () =>
                {
                    goToStack(curStack);
                    // SwapToolbarBreadcrumbs(toolbarBreadcrumbs, dropdownList, selectStack.Take(stackDepth).ToArray());
                });
            }
        }

        private static void SwapPage(IAdvancedDropdownList mainDropdownList,
            VisualElement scrollViewContainer,
            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStack,
            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> pageStack,
            bool animRightToLeft,
            VisualTreeAsset separatorAsset,
            VisualTreeAsset itemAsset,
            // ReSharper disable once SuggestBaseTypeForParameter
            Texture2D next,
            Texture2D check,
            Texture2D checkGroup,
            StyleSheet hackSliderStyle,
            Action<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>> goToStack, Action<string, object> setValue)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
            Debug.Log($"selectStack={string.Join("->", selectStack.Select(each => $"{each.Display}/{each.Index}"))}");
            Debug.Log($"pageStack={string.Join("->", pageStack.Select(each => $"{each.Display}/{each.Index}"))}");
#endif
            (IReadOnlyList<IAdvancedDropdownList> displayPage, int selectIndex) = GetPage(
                mainDropdownList,
                new Queue<AdvancedDropdownAttributeDrawer.SelectStack>(pageStack.SkipLast(1)),
                new Queue<AdvancedDropdownAttributeDrawer.SelectStack>(selectStack));

            scrollViewContainer.styleSheets.Add(hackSliderStyle);

            ScrollView scrollView = scrollViewContainer.Q<ScrollView>();

            VisualElement fadeOutOriChildren = new VisualElement();

            foreach (VisualElement scrollViewChildren in scrollView.Children().ToArray())
            {
                fadeOutOriChildren.Add(scrollViewChildren);
            }

            fadeOutOriChildren.AddToClassList("saintsfield-advanced-dropdown-fade-out-container");
            fadeOutOriChildren.AddToClassList("saintsfield-advanced-dropdown-anim");
            // Debug.Log($"Add GeoAnimOutLeftDestroy {fadeOutOriChildren}");
            if(animRightToLeft)
            {
                fadeOutOriChildren.RegisterCallback<AttachToPanelEvent>(AttackPanelAnimOutLeftDestroy);
            }
            else
            {
                fadeOutOriChildren.RegisterCallback<AttachToPanelEvent>(AttackPanelAnimOutRightDestroy);
            }
            scrollViewContainer.Add(fadeOutOriChildren);

            scrollView.Clear();

            VisualElement scrollContent = new VisualElement();
            // https://forum.unity.com/threads/how-to-refresh-scrollview-scrollbars-to-reflect-changed-content-width-and-height.1260920/
            scrollContent.RegisterCallback<TransitionEndEvent>(_ =>
            {
                scrollViewContainer.styleSheets.Remove(hackSliderStyle);

                Rect fakeOldRect = Rect.zero;
                Rect fakeNewRect = scrollView.layout;

                // ReSharper disable once ConvertToUsingDeclaration
                using (GeometryChangedEvent evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect)) {
                    evt.target = scrollView.contentContainer;
                    scrollView.contentContainer.SendEvent(evt);
                }
                // editorWindow.maxSize = editorWindow.minSize = new Vector2(_width, fakeNewRect.height);
            });
            scrollContent.AddToClassList("saintsfield-advanced-dropdown-anim");
            scrollContent.AddToClassList(animRightToLeft
                ? "saintsfield-advanced-dropdown-anim-right"
                : "saintsfield-advanced-dropdown-anim-left");

            // List<Image> allSelectImage = new List<Image>();

            foreach ((IAdvancedDropdownList dropdownItem, int index) in displayPage.WithIndex())
            {
                if (dropdownItem.isSeparator)
                {
                    VisualElement separator = separatorAsset.CloneTree();
                    scrollContent.Add(separator);
                    continue;
                }

                VisualElement elementItem = itemAsset.CloneTree();

                Button itemContainer =
                    elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

                Image selectImage = itemContainer.Q<Image>("item-checked-image");
                selectImage.image = dropdownItem.children.Count > 0
                    ? checkGroup
                    :check;
                // allSelectImage.Add(selectImage);

                itemContainer.Q<Label>("item-content").text = dropdownItem.displayName;

                if(!string.IsNullOrEmpty(dropdownItem.icon))
                {
                    itemContainer.Q<Image>("item-icon-image").image = Util.LoadResource<Texture2D>(dropdownItem.icon);
                }

                if(dropdownItem.children.Count > 0)
                {
                    itemContainer.Q<Image>("item-next-image").image = next;

                    // int nextSelectIndex = index;
                    // if (pageStack.Count <= selectStack.Count)  // within range
                    // {
                    //     nextSelectIndex = selectStack[pageStack.Count].Index;
                    // }

                    // ReSharper disable once UseIndexFromEndExpression
                    AdvancedDropdownAttributeDrawer.SelectStack curPage = pageStack[pageStack.Count - 1];

                    AdvancedDropdownAttributeDrawer.SelectStack[] nextPageStack = pageStack.SkipLast(1).Concat(new []
                    {
                        new AdvancedDropdownAttributeDrawer.SelectStack
                        {
                            Display = curPage.Display,
                            Index = index,
                        },
                        new AdvancedDropdownAttributeDrawer.SelectStack
                        {
                            Display = dropdownItem.displayName,
                            Index = -1,
                        },
                    }).ToArray();

                    itemContainer.clicked += () => goToStack(nextPageStack);
                }
                else
                {
                    if (dropdownItem.disabled)
                    {
                        itemContainer.SetEnabled(false);
                        itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-disabled");
                        itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                    }
                    else if(selectIndex == index)
                    {
                        itemContainer.pickingMode = PickingMode.Ignore;
                        itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                    }
                    else
                    {
                        itemContainer.clicked += () =>
                        {
                            string newDisplay = string.Join("/",
                                pageStack.Skip(1).Select(each => each.Display).Append(dropdownItem.displayName));
                            setValue(newDisplay, dropdownItem.value);
                            // allSelectImage.ForEach(each => each.visible = false);
                            // selectImage.visible = true;
                        };
                    }
                }

                bool isSelected = selectIndex == index;
                // Debug.Log($"isSelected={isSelected}, {index}");
                if (isSelected)
                {
                    selectImage.visible = true;
                    itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-selected");
                }

                scrollContent.Add(elementItem);
            }

            scrollContent.RegisterCallback<GeometryChangedEvent>(GeoAnimIntoView);

            scrollView.Add(scrollContent);
        }

        private static void GeoAnimIntoView(GeometryChangedEvent evt)
        {
            VisualElement targetItem = (VisualElement)evt.target;
            targetItem.UnregisterCallback<GeometryChangedEvent>(GeoAnimIntoView);
            // VisualElement targetItem = targetRoot.Q<VisualElement>(className: "saintsfield-advanced-dropdown-item");
            // Debug.Log($"attached: {targetRoot}");
            targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-right");
            targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-left");
            targetItem.AddToClassList("saintsfield-advanced-dropdown-anim-in-view");
        }

        private static void AttackPanelAnimOutLeftDestroy(AttachToPanelEvent evt)
        {
            GeoAnimOutDestroy(evt, "saintsfield-advanced-dropdown-anim-left");
        }

        private static void AttackPanelAnimOutRightDestroy(AttachToPanelEvent evt)
        {
            GeoAnimOutDestroy(evt, "saintsfield-advanced-dropdown-anim-right");
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void GeoAnimOutDestroy(AttachToPanelEvent evt, string className)
        {
            VisualElement targetItem = (VisualElement)evt.target;
            targetItem.UnregisterCallback<AttachToPanelEvent>(AttackPanelAnimOutLeftDestroy);
            targetItem.UnregisterCallback<AttachToPanelEvent>(AttackPanelAnimOutRightDestroy);

            targetItem.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                // Debug.Log("changed");
                targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-in-view");
                targetItem.AddToClassList(className);
                // targetItem.RegisterCallback<TransitionRunEvent>(Debug.Log);
                targetItem.RegisterCallback<TransitionEndEvent>(TransitionDestroy);
            });

            // targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-in-view");
            // targetItem.RegisterCallback<TransitionEndEvent>(TransitionDestroy);
            // targetItem.RegisterCallback<TransitionCancelEvent>(Debug.Log);
            // targetItem.RegisterCallback<TransitionStartEvent>(Debug.Log);
            // targetItem.RegisterCallback<TransitionRunEvent>(Debug.Log);
            // targetItem.AddToClassList(className);
        }

        private static void TransitionDestroy(TransitionEndEvent evt)
        {
            // Debug.Log($"TransitionDestroy={evt.target}");
            ((VisualElement)evt.target).RemoveFromHierarchy();
        }

        private static (IReadOnlyList<IAdvancedDropdownList> pageItems, int selectIndex) GetPage(
                IAdvancedDropdownList dropdownList,
                Queue<AdvancedDropdownAttributeDrawer.SelectStack> pageStack,
                Queue<AdvancedDropdownAttributeDrawer.SelectStack> selectStack
            )
        {
            if (pageStack.Count <= 0)
            {
                int selectIndex = -1;
                if(selectStack.Count > 0)
                {
                    selectIndex = selectStack.Dequeue().Index;
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                Debug.Log($"return page {dropdownList.displayName}");
#endif
                return (dropdownList.children, selectIndex);
            }

            AdvancedDropdownAttributeDrawer.SelectStack first = pageStack.Dequeue();
            int index = first.Index;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
            Debug.Log($"check page {dropdownList.displayName}[{index}]->{dropdownList.children[index].displayName}");
#endif

            if (selectStack.Count > 0)
            {
                AdvancedDropdownAttributeDrawer.SelectStack selectFirst = selectStack.Dequeue();
                if (selectFirst.Index != index)  // dis-match, not in the select chain
                {
                    selectStack.Clear();
                }
            }

            // ReSharper disable once TailRecursiveCall
            return GetPage(dropdownList.children[index], pageStack, selectStack);
        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
#endif
    }
#endif
    #endregion

    [CustomPropertyDrawer(typeof(AdvancedDropdownAttribute))]
    public class AdvancedDropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();

        ~AdvancedDropdownAttributeDrawer()
        {
            _iconCache.Clear();
        }

        #region Util

        public struct SelectStack : IEquatable<SelectStack>
        {
            // ReSharper disable InconsistentNaming
            public int Index;
            public string Display;
            // public object Value;
            // ReSharper enable InconsistentNaming
            public bool Equals(SelectStack other)
            {
                return Index == other.Index && Display == other.Display;
            }

            public override bool Equals(object obj)
            {
                return obj is SelectStack other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(Index, Display);
            }
        }

        public struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;

            // public FieldInfo FieldInfo;

            public string CurDisplay;
            public object CurValue;
            public IAdvancedDropdownList DropdownListValue;
            public IReadOnlyList<SelectStack> SelectStacks;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, AdvancedDropdownAttribute advancedDropdownAttribute, FieldInfo field, object parentObj)
        {
            string funcName = advancedDropdownAttribute.FuncName;

            string error;
            IAdvancedDropdownList dropdownListValue = null;
            if (funcName is null)
            {
                Type enumType = ReflectUtils.GetElementType(field.FieldType);
                if(enumType.IsEnum)
                {
                    Array enumValues = Enum.GetValues(enumType);
                    AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>();
                    foreach (object enumValue in enumValues)
                    {
                        enumDropdown.Add(ReflectUtils.GetRichLabelFromEnum(enumType, enumValue).value, enumValue);
                    }

                    error = "";
                    dropdownListValue = enumDropdown;
                }
                else
                {
                    error = $"{property.displayName}({enumType}) is not a enum";
                }
            }
            else
            {
                (string getOfError, IAdvancedDropdownList getOfDropdownListValue) =
                    Util.GetOf<IAdvancedDropdownList>(funcName, null, property, field, parentObj);
                error = getOfError;
                dropdownListValue = getOfDropdownListValue;
            }
            if(dropdownListValue == null || error != "")
            {
                return new MetaInfo
                {
                    Error = error == ""? $"dropdownList is null from `{funcName}` on target `{parentObj}`": error,
                    CurDisplay = "[Error]",
                    CurValue = null,
                    DropdownListValue = null,
                    SelectStacks = Array.Empty<SelectStack>(),
                };
            }

            #region Get Cur Value

            (string curError, int _, object curValue)  = Util.GetValue(property, field, parentObj);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
            Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
#endif
            if (curError != "")
            {
                return new MetaInfo
                {
                    Error = curError,
                    CurDisplay = "[Error]",
                    CurValue = null,
                    DropdownListValue = null,
                    SelectStacks = Array.Empty<SelectStack>(),
                };
            }
            if (curValue is IWrapProp wrapProp)
            {
                curValue = Util.GetWrapValue(wrapProp);
            }

            // process the unique options
            (string uniqueError, IAdvancedDropdownList dropdownListValueUnique) = GetUniqueList(dropdownListValue, advancedDropdownAttribute.EUnique, curValue, property, field, parentObj);

            if (uniqueError != "")
            {
                return new MetaInfo
                {
                    Error = curError,
                    CurDisplay = "[Error]",
                    CurValue = null,
                    DropdownListValue = null,
                    SelectStacks = Array.Empty<SelectStack>(),
                };
            }

            // string curDisplay = "";
            (IReadOnlyList<SelectStack> curSelected, string display) = GetSelected(curValue, Array.Empty<SelectStack>(), dropdownListValueUnique);
            #endregion

            return new MetaInfo
            {
                Error = "",
                // FieldInfo = field,
                CurDisplay = display,
                CurValue = curValue,
                DropdownListValue = dropdownListValueUnique,
                SelectStacks = curSelected,
            };
        }

        private static (string error, IAdvancedDropdownList dropdownList) GetUniqueList(IAdvancedDropdownList dropdownListValue, EUnique eUnique, object curValue, SerializedProperty property, FieldInfo info, object parent)
        {
            if(eUnique == EUnique.None)
            {
                return ("", dropdownListValue);
            }

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (arrayIndex == -1)
            {
                return ("", dropdownListValue);
            }

            (SerializedProperty arrProp, int _, string error) = Util.GetArrayProperty(property, info, parent);
            if (error != "")
            {
                return (error, null);
            }

            List<object> existsValues = new List<object>();

            foreach (SerializedProperty element in Enumerable.Range(0, arrProp.arraySize).Where(index => index != arrayIndex).Select(arrProp.GetArrayElementAtIndex))
            {
                (string otherError, int _, object otherValue) = Util.GetValue(element, info, parent);
                if (otherError != "")
                {
                    return (otherError, null);
                }

                if (otherValue is IWrapProp wrapProp)
                {
                    otherValue = Util.GetWrapValue(wrapProp);
                }

                existsValues.Add(otherValue);
            }

            // if (eUnique == EUnique.Remove)
            // {
            //     existsValues.Remove(curValue);
            // }

            return ("", ReWrapUniqueList(dropdownListValue, eUnique, existsValues, curValue));
        }

        private static AdvancedDropdownList<object> ReWrapUniqueList(IAdvancedDropdownList dropdownListValue, EUnique eUnique, List<object> existsValues, object curValue)
        {
            AdvancedDropdownList<object> dropdownList = new AdvancedDropdownList<object>(dropdownListValue.displayName, dropdownListValue.disabled, dropdownListValue.icon);
            IReadOnlyList<AdvancedDropdownList<object>> children = ReWrapUniqueChildren(dropdownListValue.children, eUnique, existsValues, curValue);
            dropdownList.SetChildren(children.ToList());
            return dropdownList;
        }

        private static IReadOnlyList<AdvancedDropdownList<object>> ReWrapUniqueChildren(IReadOnlyList<IAdvancedDropdownList> children, EUnique eUnique, IReadOnlyList<object> existsValues, object curValue)
        {
            List<AdvancedDropdownList<object>> newChildren = new List<AdvancedDropdownList<object>>();
            foreach (IAdvancedDropdownList originChild in children)
            {
                if (originChild.isSeparator)
                {
                    newChildren.Add(AdvancedDropdownList<object>.Separator());
                }
                else if (originChild.ChildCount() > 0)  // has sub child
                {
                    IReadOnlyList<AdvancedDropdownList<object>> subChildren = ReWrapUniqueChildren(originChild.children, eUnique, existsValues, curValue);
                    if (subChildren.Any(each => !each.isSeparator))
                    {
                        bool isDisabled = originChild.disabled ||
                                          subChildren.All(each => each.isSeparator || each.disabled);
                        AdvancedDropdownList<object> newChild = new AdvancedDropdownList<object>(originChild.displayName, isDisabled, originChild.icon);
                        newChild.SetChildren(subChildren.ToList());
                        newChildren.Add(newChild);
                    }
                }
                else
                {
                    object childValue = originChild.value;
                    bool exists = existsValues.Any(each => Util.GetIsEqual(each, childValue));
                    if (!exists)
                    {
                        newChildren.Add(new AdvancedDropdownList<object>(
                            originChild.displayName,
                            originChild.value,
                            originChild.disabled,
                            originChild.icon,
                            originChild.isSeparator));
                    }
                    else if (eUnique == EUnique.Disable)
                    {
                        newChildren.Add(new AdvancedDropdownList<object>(
                            originChild.displayName,
                            originChild.value,
                            true,
                            originChild.icon,
                            originChild.isSeparator));
                    }
                    else if (eUnique == EUnique.Remove)
                    {
                        if (Util.GetIsEqual(originChild.value, curValue))
                        {
                            newChildren.Add(new AdvancedDropdownList<object>(
                                originChild.displayName,
                                originChild.value,
                                true,
                                originChild.icon,
                                originChild.isSeparator));
                        }
                    }
                }
            }

            if (newChildren.All(each => each.isSeparator))
            {
                newChildren.Clear();
            }

            return newChildren;
        }

        private static string GetMetaStackDisplay(MetaInfo metaInfo)
        {
            return metaInfo.SelectStacks.Count == 0
                ? "-"
                : string.Join("/", metaInfo.SelectStacks.Skip(1).Select(each => each.Display).Append(metaInfo.CurDisplay));
        }

        private static (IReadOnlyList<SelectStack> stack, string display) GetSelected(object curValue, IReadOnlyList<SelectStack> curStacks, IAdvancedDropdownList dropdownPage)
        {
            foreach ((IAdvancedDropdownList item, int index) in dropdownPage.children.WithIndex())
            {
                if (item.isSeparator)
                {
                    continue;
                }

                if (item.children.Count > 0)  // it's a group
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                    Debug.Log($"GetSelected group {dropdownPage.displayName}");
#endif
                    (IReadOnlyList<SelectStack> subResult, string display) = GetSelected(curValue, curStacks.Append(new SelectStack
                    {
                        Display = dropdownPage.displayName,
                        Index = index,
                    }).ToArray(), item);
                    if (subResult.Count > 0)
                    {
                        return (subResult, display);
                    }

                    continue;
                }

                IEnumerable<SelectStack> thisLoopResult = curStacks.Append(new SelectStack
                {
                    Display = dropdownPage.displayName,
                    Index = index,
                });

                if (curValue is IWrapProp wrapProp)
                {
                    curValue = Util.GetWrapValue(wrapProp);
                }

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (Util.GetIsEqual(curValue, item.value))
                {
                    return (thisLoopResult.ToArray(), item.displayName);
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                Debug.Log($"Not Equal: {curValue} != {item.value}");
#endif
            }

            // Debug.Log($"GetSelected end in empty");
            // nothing selected
            return (Array.Empty<SelectStack>(), "");
        }



        private static IEnumerable<(string stackDisplay, string display, string icon, bool disabled, object value)> FlattenChild(string prefix, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList child in children)
            {
                if (child.Count > 0)
                {
                    // List<(string, object, List<object>, bool, string, bool)> grandChildren = child.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((string, string, string, bool, object) grandChild in FlattenChild(prefix, child.children))
                    {
                        yield return grandChild;
                    }
                }
                else
                {
                    yield return (Prefix(prefix, child.displayName), child.displayName, child.icon, child.disabled, child.value);
                }
            }
        }

        public static IEnumerable<(string stackDisplay, string display, string icon, bool disabled, object value)> Flatten(string prefix, IAdvancedDropdownList roots)
        {
            foreach (IAdvancedDropdownList root in roots)
            {
                if (root.Count > 0)
                {
                    // IAdvancedDropdownList children = root.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((string, string, string, bool, object) child in FlattenChild(Prefix(prefix, root.displayName), root.children))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return (Prefix(prefix, root.displayName), root.displayName, root.icon, root.disabled, root.value);
                }
            }
        }

        private static string Prefix(string prefix, string value) => string.IsNullOrEmpty(prefix)? value : $"{prefix}/{value}";
        #endregion

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            AdvancedDropdownAttribute advancedDropdownAttribute = (AdvancedDropdownAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, advancedDropdownAttribute, info, parent);
            _error = metaInfo.Error;

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string display = GetMetaStackDisplay(metaInfo);
            // Debug.Assert(false, "Here");
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                float minHeight = advancedDropdownAttribute.MinHeight;
                float itemHeight = advancedDropdownAttribute.ItemHeight > 0
                    ? advancedDropdownAttribute.ItemHeight
                    : EditorGUIUtility.singleLineHeight;
                float titleHeight = advancedDropdownAttribute.TitleHeight;
                Vector2 size;
                if (minHeight < 0)
                {
                    if(advancedDropdownAttribute.UseTotalItemCount)
                    {
                        float totalItemCount = GetValueItemCounts(metaInfo.DropdownListValue);
                        // Debug.Log(totalItemCount);
                        size = new Vector2(position.width, totalItemCount * itemHeight + titleHeight);
                    }
                    else
                    {
                        float maxChildCount = GetDropdownPageHeight(metaInfo.DropdownListValue, itemHeight, advancedDropdownAttribute.SepHeight).Max();
                        size = new Vector2(position.width, maxChildCount + titleHeight);
                    }
                }
                else
                {
                    size = new Vector2(position.width, minHeight);
                }

                // Vector2 size = new Vector2(position.width, maxChildCount * EditorGUIUtility.singleLineHeight + 31f);
                SaintsAdvancedDropdown dropdown = new SaintsAdvancedDropdown(
                    metaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();
                        onGUIPayload.SetValue(curItem);
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    },
                    GetIcon);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

        // protected override void ImGuiOnDispose()
        // {
        //     base.ImGuiOnDispose();
        //     foreach (Texture2D icon in _iconCache.Values)
        //     {
        //         UnityEngine.Object.DestroyImmediate(icon);
        //     }
        //     _iconCache.Clear();
        // }

        private static IEnumerable<float> GetDropdownPageHeight(IAdvancedDropdownList dropdownList, float itemHeight, float sepHeight)
        {
            if (dropdownList.ChildCount() == 0)
            {
                // Debug.Log($"yield 0");
                yield return 0;
                yield break;
            }

            // Debug.Log($"yield {dropdownList.children.Count}");
            yield return dropdownList.ChildCount() * itemHeight + dropdownList.SepCount() * sepHeight;
            foreach (IEnumerable<float> eachChildHeight in dropdownList.children.Select(child => GetDropdownPageHeight(child, itemHeight, sepHeight)))
            {
                foreach (int i in eachChildHeight)
                {
                    yield return i;
                }
            }
        }

        private static int GetValueItemCounts(IAdvancedDropdownList dropdownList)
        {
            if (dropdownList.isSeparator)
            {
                return 0;
            }

            if(dropdownList.ChildCount() == 0)
            {
                return 1;
            }

            int count = 0;
            foreach (IAdvancedDropdownList child in dropdownList.children)
            {
                count += GetValueItemCounts(child);
            }

            return count;

            // if(dropdownList.ChildCount() == 0)
            // {
            //     Debug.Log(1);
            //     yield return 1;
            //     yield break;
            // }
            //
            // // Debug.Log(dropdownList.ChildCount());
            // // yield return dropdownList.children.Count(each => each.ChildCount() == 0);
            // foreach (IAdvancedDropdownList eachChild in dropdownList.children)
            // {
            //     foreach (int subChildCount in GetChildCounts(eachChild))
            //     {
            //         if(subChildCount > 0)
            //         {
            //             Debug.Log(subChildCount);
            //             yield return subChildCount;
            //         }
            //     }
            // }
        }

        private Texture2D GetIcon(string icon)
        {
            if (_iconCache.TryGetValue(icon, out Texture2D result))
            {
                return result;
            }

            result = Util.LoadResource<Texture2D>(icon);
            if (result == null)
            {
                return null;
            }
            if (result.width == 1 && result.height == 1)
            {
                return null;
            }
            _iconCache[icon] = result;
            return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

#if UNITY_2021_3_OR_NEWER

        // private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            MetaInfo initMetaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButton(property);
            dropdownButton.userData = initMetaInfo.CurValue;
            dropdownButton.ButtonLabelElement.text = GetMetaStackDisplay(initMetaInfo);

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };

            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            VisualElement root = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            dropdownButton.ButtonElement.clicked += () =>
            {
                MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent);
                // Debug.Log(root.worldBound);
                // Debug.Log(Screen.height);
                // float maxHeight = Mathf.Max(400, Screen.height - root.worldBound.y - root.worldBound.height - 200);
                float maxHeight = Screen.height - root.worldBound.y - root.worldBound.height - 100;
                Rect worldBound = root.worldBound;
                // Debug.Log(worldBound);
                if (maxHeight < 100)
                {
                    // worldBound.x -= 400;
                    worldBound.y -= 300 + worldBound.height;
                    // Debug.Log(worldBound);
                    maxHeight = 300;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUiToolkit(
                    metaInfo,
                    root.worldBound.width,
                    maxHeight,
                    (newDisplay, curItem) =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();

                        dropdownButton.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property)).ButtonLabelElement.text = newDisplay;
                        dropdownButton.userData = curItem;
                        onValueChangedCallback(curItem);
                        // dropdownButton.buttonLabelElement.text = newDisplay;
                    }
                ));

                string curError = metaInfo.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                // ReSharper disable once InvertIf
                if (helpBox.text != curError)
                {
                    helpBox.text = curError;
                    helpBox.style.display = curError == ""? DisplayStyle.None : DisplayStyle.Flex;
                }
            };
        }

        // protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, Action<object> onValueChanged, FieldInfo info)
        // {
        //
        // }
#endif
    }
}
