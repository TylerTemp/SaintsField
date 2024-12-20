#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public class SaintsAdvancedDropdownUIToolkit : PopupWindowContent
    {
        private readonly float _width;
        private readonly AdvancedDropdownMetaInfo _metaInfo;
        private readonly Action<string, object> _setValue;

        // ReSharper disable once InconsistentNaming
        private readonly UnityEvent<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>> GoToStackEvent =
            new UnityEvent<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>>();

        private readonly bool _isFlat;
        private readonly float _maxHeight;
        private readonly bool _allowUnSelect;

        public SaintsAdvancedDropdownUIToolkit(AdvancedDropdownMetaInfo metaInfo, float width, float maxHeight, bool allowUnSelect, Action<string, object> setValue)
        {
            _width = width;
            _metaInfo = metaInfo;
            _setValue = setValue;
            _maxHeight = maxHeight;
            _allowUnSelect = allowUnSelect;

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
                    _metaInfo.CurValues,
                    _allowUnSelect,
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

                        // bool curSelect = _metaInfo.SelectStacks.Count > 0 && _metaInfo.CurValues.Any(each => Util.GetIsEqual(each, value)) ;
                        bool curSelect = _metaInfo.CurValues.Any(each => Util.GetIsEqual(each, value)) ;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                        Debug.Log($"curSelect={curSelect}, _metaInfo.SelectStacks.Count={_metaInfo.SelectStacks.Count}, _metaInfo.CurValue={_metaInfo.CurValues}, value={value}");
#endif

                        if(!string.IsNullOrEmpty(icon))
                        {
                            itemContainer.Q<Image>("item-icon-image").image = Util.LoadResource<Texture2D>(icon);
                        }

                        if (curSelect)
                        {
                            selectImage.visible = true;
                        }

                        if (disabled)
                        {
                            itemContainer.SetEnabled(false);
                            itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-disabled");
                            itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                        }
                        else  // not disabled (no not-enabled appearance)
                        {
                            if (curSelect && !_allowUnSelect)
                            {
                                itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                                Debug.Log($"cur selected: {value}");
#endif

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

        private static void SwapPage(
            IReadOnlyList<object> curValues,
            bool _allowUnSelect,
            IAdvancedDropdownList mainDropdownList,
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
                    : check;
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
                    else if(selectIndex == index && !_allowUnSelect)
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

                // bool isSelected = selectIndex == index;
                bool isSelected = curValues.Contains(dropdownItem.value);
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
}

#endif
