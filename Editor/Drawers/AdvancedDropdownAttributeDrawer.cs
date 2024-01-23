using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;
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
            AdvancedDropdownItem root = MakeUnityAdvancedDropdownItem(_dropdownListValue);

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

        private void MakeChildren(AdvancedDropdownItem parent, IEnumerable<IAdvancedDropdownList> children)
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
                    AdvancedDropdownItem item = MakeUnityAdvancedDropdownItem(childItem);
                    _itemToValue[item] = childItem.value;
                    // Debug.Log($"add {childItem.displayName} => {childItem.value}");
                    parent.AddChild(item);
                }
                else
                {
                    AdvancedDropdownItem subParent = MakeUnityAdvancedDropdownItem(childItem);
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
    public class SaintsAdvancedDropdownUiToolkit : PopupWindowContent
    {
        private readonly float _width;
        private readonly AdvancedDropdownAttributeDrawer.MetaInfo _metaInfo;
        private readonly Action<string, object> _setValue;

        // ReSharper disable once InconsistentNaming
        private readonly UnityEvent<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>> GoToStackEvent =
            new UnityEvent<IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack>>();

        public SaintsAdvancedDropdownUiToolkit(AdvancedDropdownAttributeDrawer.MetaInfo metaInfo, float width, Action<string, object> setValue)
        {
            _width = width;
            _metaInfo = metaInfo;
            _setValue = setValue;
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

        public VisualElement CloneTree()
        {
            StyleSheet ussStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/Style.uss");
            StyleSheet hackSliderStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/HackSliderStyle.uss");

            VisualTreeAsset popUpAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/Popup.uxml");
            VisualElement root = popUpAsset.CloneTree();

            // root.contentContainer.style.borderBottomWidth = 1;
            // root.contentContainer.style.borderBottomColor = Color.red;

            // var saintsRoot = root.Q<VisualElement>("saintsfield-advanced-dropdown-root");
            // saintsRoot.contentContainer.style.borderBottomWidth = 1;
            // saintsRoot.contentContainer.style.borderBottomColor = Color.red;

            root.styleSheets.Add(ussStyle);
            // root.styleSheets.Remove()

            // root.style.borderBottomWidth = 1;
            // root.style.borderBottomColor = Color.red;
            // root.userData = new DelayUpdateSize
            // {
            //     IsDelay = false,
            //     Height = 0,
            // };
            // root.RegisterCallback<GeometryChangedEvent>(GeoUpdateWindowSize);

            VisualTreeAsset separatorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/Separator.uxml");

            VisualTreeAsset itemAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/ItemRow.uxml");

            VisualElement scrollViewContainer = root.Q<VisualElement>("saintsfield-advanced-dropdown-scroll-view-container");
            ScrollView scrollView = root.Q<ScrollView>();
            scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeoUpdateWindowSize);

            // scrollView.contentContainer.style.borderBottomWidth = 1;
            // scrollView.contentContainer.style.borderBottomColor = Color.green;

            // Texture2D icon = RichTextDrawer.LoadTexture("eye.png");
            Texture2D next = RichTextDrawer.LoadTexture("arrow-next.png");
            Texture2D checkGroup = RichTextDrawer.LoadTexture("arrow-right.png");
            Texture2D check = RichTextDrawer.LoadTexture("check.png");

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStack = _metaInfo.SelectStacks;

            Debug.Log($"selectStack={string.Join("->", selectStack.Select(each => $"{each.Display}/{each.Index}"))}");
            ToolbarBreadcrumbs toolbarBreadcrumbs = root.Q<ToolbarBreadcrumbs>();

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
            GoToStackEvent.Invoke(_metaInfo.SelectStacks);

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
                    toolbarBreadcrumbs.style.display = DisplayStyle.Flex;
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

                        bool curSelect = _metaInfo.SelectStacks.Count > 0 && AdvancedDropdownAttributeDrawer.GetIsEqual(_metaInfo.CurValue, value);
                        Debug.Log($"curSelect={curSelect}, _metaInfo.SelectStacks.Count={_metaInfo.SelectStacks.Count}, _metaInfo.CurValue={_metaInfo.CurValue}, value={value}, _metaInfo.CurValue == value: {_metaInfo.CurValue == value}");

                        if(!string.IsNullOrEmpty(icon))
                        {
                            itemContainer.Q<Image>("item-icon-image").image = RichTextDrawer.LoadTexture(icon);
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
                            Debug.Log($"cur selected: {value}");
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

            editorWindow.maxSize = editorWindow.minSize = new Vector2(_width, height);
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
                Debug.Log($"push {stack.Display}: {string.Join("->", curStack.Select(each => $"{each.Display}/{each.Index}"))}");
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
            Debug.Log($"selectStack={string.Join("->", selectStack.Select(each => $"{each.Display}/{each.Index}"))}");
            Debug.Log($"pageStack={string.Join("->", pageStack.Select(each => $"{each.Display}/{each.Index}"))}");
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
                    itemContainer.Q<Image>("item-icon-image").image = RichTextDrawer.LoadTexture(dropdownItem.icon);
                }

                if(dropdownItem.children.Count > 0)
                {
                    itemContainer.Q<Image>("item-next-image").image = next;

                    // int nextSelectIndex = index;
                    // if (pageStack.Count <= selectStack.Count)  // within range
                    // {
                    //     nextSelectIndex = selectStack[pageStack.Count].Index;
                    // }

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

            scrollContent.RegisterCallback<GeometryChangedEvent>(GeoAnimIntoView);;

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

                Debug.Log($"return page {dropdownList.displayName}");
                return (dropdownList.children, selectIndex);
            }

            AdvancedDropdownAttributeDrawer.SelectStack first = pageStack.Dequeue();
            int index = first.Index;
            Debug.Log($"check page {dropdownList.displayName}[{index}]->{dropdownList.children[index].displayName}");

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

        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
    }
    #endregion

    [CustomPropertyDrawer(typeof(AdvancedDropdownAttribute))]
    public class AdvancedDropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();

        ~AdvancedDropdownAttributeDrawer()
        {
            foreach (Texture2D iconCacheValue in _iconCache.Values)
            {
                UnityEngine.Object.DestroyImmediate(iconCacheValue);
            }
        }

        #region Util

        public struct SelectStack
        {
            // ReSharper disable InconsistentNaming
            public int Index;
            public string Display;
            // public object Value;
            // ReSharper enable InconsistentNaming
        }

        public struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;

            public FieldInfo FieldInfo;

            public string CurDisplay;
            public object CurValue;
            public IAdvancedDropdownList DropdownListValue;
            public IReadOnlyList<SelectStack> SelectStacks;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, AdvancedDropdownAttribute advancedDropdownAttribute, object parentObj)
        {
            string funcName = advancedDropdownAttribute.FuncName;
            Debug.Assert(parentObj != null);
            Type parentType = parentObj.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(parentType, funcName);

            #region Get List Items
            IAdvancedDropdownList dropdownListValue;

            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                    return new MetaInfo
                    {
                        Error = $"not found `{funcName}` on target `{parentObj}`",
                    };
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
                        };
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
                        };
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));

                    try
                    {
                        dropdownListValue =
                            methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as IAdvancedDropdownList;
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return new MetaInfo
                        {
                            Error = e.InnerException.Message,
                        };
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return new MetaInfo
                        {
                            Error = e.Message,
                        };
                    }

                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`",
                        };
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            #endregion

            #region Get Cur Value
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            // Object target = property.serializedObject.targetObject;
            FieldInfo field = parentType.GetField(property.name, bindAttr);
            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            // string curDisplay = "";
            IReadOnlyList<SelectStack> curSelected = GetSelected(curValue, Array.Empty<SelectStack>(), dropdownListValue);
            #endregion

            return new MetaInfo
            {
                Error = "",
                FieldInfo = field,
                // CurDisplay = curDisplay,
                CurValue = curValue,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        private static IReadOnlyList<SelectStack> GetSelected(object curValue, IReadOnlyList<SelectStack> curStacks, IAdvancedDropdownList dropdownPage)
        {
            foreach ((IAdvancedDropdownList item, int index) in dropdownPage.children.WithIndex())
            {
                if (item.isSeparator)
                {
                    continue;
                }

                if (item.children.Count > 0)  // it's a group
                {
                    Debug.Log($"GetSelected group {dropdownPage.displayName}");
                    IReadOnlyList<SelectStack> subResult = GetSelected(curValue, curStacks.Append(new SelectStack
                    {
                        Display = dropdownPage.displayName,
                        Index = index,
                    }).ToArray(), item);
                    if (subResult.Count > 0)
                    {
                        return subResult;
                    }

                    continue;
                }

                IEnumerable<SelectStack> thisLoopResult = curStacks.Append(new SelectStack
                {
                    Display = dropdownPage.displayName,
                    Index = index,
                });

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (GetIsEqual(curValue, item.value))
                {
                    return thisLoopResult.ToArray();
                }
                // if (curValue == null && item.value == null)
                // {
                //     Debug.Log($"GetSelected null {item.displayName}/{index}");
                //     return thisLoopResult.ToArray();
                // }
                // if (curValue is UnityEngine.Object curValueObj
                //     && curValueObj == item.value as UnityEngine.Object)
                // {
                //     Debug.Log($"GetSelected {curValue} {item.displayName}/{index}");
                //     return thisLoopResult.ToArray();
                // }
                // if (item.value == null)
                // {
                //     Debug.Log($"GetSelected nothing null {item.displayName}/{index}");
                //     // nothing
                // }
                // else if (item.value.Equals(curValue))
                // {
                //     Debug.Log($"GetSelected {curValue} {item.displayName}/{index}");
                //     return thisLoopResult.ToArray();
                // }
            }

            // Debug.Log($"GetSelected end in empty");
            // nothing selected
            return Array.Empty<SelectStack>();
        }

        public static bool GetIsEqual(object curValue, object itemValue)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (curValue == null && itemValue == null)
            {
                Debug.Log($"GetSelected null");
                return true;
            }
            if (curValue is UnityEngine.Object curValueObj
                && itemValue is UnityEngine.Object itemValueObj
                && curValueObj == itemValueObj)
            {
                Debug.Log($"GetSelected Unity Object {curValue}");
                return true;
            }
            if (itemValue == null)
            {
                Debug.Log($"GetSelected nothing null");
                // nothing
                return false;
            }
            // ReSharper disable once InvertIf
            if (itemValue.Equals(curValue))
            {
                Debug.Log($"GetSelected equal {curValue}");
                return true;
            }

            return false;
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

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            AdvancedDropdownAttribute advancedDropdownAttribute = (AdvancedDropdownAttribute) saintsAttribute;

            // SaintsAdvancedDropdown dropdown = new SaintsAdvancedDropdown(new AdvancedDropdownState());
            // dropdown.Show(position);

            string funcName = advancedDropdownAttribute.FuncName;
            object parentObj = GetParentTarget(property);
            Debug.Assert(parentObj != null);
            Type parentType = parentObj.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(parentType, funcName);

            #region Get List Items
            IAdvancedDropdownList dropdownListValue;

            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error = $"not found `{funcName}` on target `{parentObj}`";
                    DefaultDrawer(position, property, label);
                }
                    return;
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));

                    _error = "";
                    // IEnumerable<AdvancedDropdownItem<object>> result;
                    try
                    {
                        dropdownListValue =
                            methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as IAdvancedDropdownList;
                        // Debug.Log(rawResult);
                        // Debug.Log(rawResult as IDropdownList);
                        // // Debug.Log(rawResult.GetType());
                        // // Debug.Log(rawResult.GetType().Name);
                        // // Debug.Log(typeof(rawResult));
                        //

                        // Debug.Log($"result: {dropdownListValue}");
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.Assert(e.InnerException != null);
                        _error = e.InnerException.Message;
                        Debug.LogException(e);
                        return;
                    }
                    catch (Exception e)
                    {
                        _error = e.Message;
                        Debug.LogException(e);
                        return;
                    }

                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            #endregion

            #region Get Cur Value
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            // Object target = property.serializedObject.targetObject;
            FieldInfo field = parentType.GetField(property.name, bindAttr);
            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            string curDisplay = "";
            Debug.Assert(dropdownListValue != null);
            foreach ((string stackDisplay, string display, string icon, bool disabled, object value) itemInfos in Flatten("", dropdownListValue))
            {
                string name = itemInfos.display;
                object itemValue = itemInfos.value;

                if (curValue == null && itemValue == null)
                {
                    curDisplay = name;
                    break;
                }
                if (curValue is UnityEngine.Object curValueObj
                    && curValueObj == itemValue as UnityEngine.Object)
                {
                    curDisplay = name;
                    break;
                }
                if (itemValue == null)
                {
                    // nothing
                }
                else if (itemValue.Equals(curValue))
                {
                    curDisplay = name;
                    break;
                }
            }
            #endregion

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(curDisplay), FocusType.Keyboard))
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
                        float totalItemCount = GetValueItemCounts(dropdownListValue);
                        // Debug.Log(totalItemCount);
                        size = new Vector2(position.width, totalItemCount * itemHeight + titleHeight);
                    }
                    else
                    {
                        float maxChildCount = GetDropdownPageHeight(dropdownListValue, itemHeight, advancedDropdownAttribute.SepHeight).Max();
                        size = new Vector2(position.width, maxChildCount + titleHeight);
                    }
                }
                else
                {
                    size = new Vector2(position.width, minHeight);
                }

                // Vector2 size = new Vector2(position.width, maxChildCount * EditorGUIUtility.singleLineHeight + 31f);
                SaintsAdvancedDropdown dropdown = new SaintsAdvancedDropdown(
                    dropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        Util.SetValue(property, curItem, parentObj, parentType, field);
                        SetValueChanged(property);
                    },
                    GetIcon);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

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

            result = RichTextDrawer.LoadTexture(icon);
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

#if UNITY_2021_3_OR_NEWER

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container,
            Label fakeLabel,
            object parent)
        {
            // VisualElement root = new VisualElement();
            //
            // VisualElement popContainer = new VisualElement
            // {
            //     name = "PopContainer",
            //     style =
            //     {
            //         borderLeftColor = Color.green,
            //         borderRightColor = Color.green,
            //         borderTopColor = Color.green,
            //         borderBottomColor = Color.green,
            //
            //         borderLeftWidth = 1,
            //         borderRightWidth = 1,
            //         borderTopWidth = 1,
            //         borderBottomWidth = 1,
            //     },
            // };
            //
            // MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, parent);
            //
            // SaintsAdvancedDropdownUiToolkit advancedDropdownUiToolkit = new SaintsAdvancedDropdownUiToolkit(metaInfo, default,
            //     curItem => Util.SetValue(property, curItem, parent, parent.GetType(), metaInfo.FieldInfo));
            //
            // popContainer.Add(advancedDropdownUiToolkit.CloneTree());
            //
            // root.Add(new Button(() =>
            // {
            //     popContainer.Clear();
            //
            //     MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, parent);
            //
            //     SaintsAdvancedDropdownUiToolkit advancedDropdownUiToolkit = new SaintsAdvancedDropdownUiToolkit(metaInfo, default,
            //         curItem => Util.SetValue(property, curItem, parent, parent.GetType(), metaInfo.FieldInfo));
            //
            //     popContainer.Add(advancedDropdownUiToolkit.CloneTree());
            //
            //     Debug.Log("Done");
            // })
            // {
            //     text = "Reload",
            // });
            //
            // root.Add(popContainer);
            //
            // return root;

            Button button = new Button
            {
                text = "Open",
                style =
                {
                    flexGrow = 1,
                },
            };

            button.clicked += () =>
            {
                MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, parent);
                UnityEditor.PopupWindow.Show(button.worldBound, new SaintsAdvancedDropdownUiToolkit(
                    metaInfo,
                    button.worldBound.width,
                    (newDisplay, curItem) =>
                    {
                        Util.SetValue(property, curItem, parent, parent.GetType(), metaInfo.FieldInfo);
                        button.text = newDisplay;
                    }
                ));
            };

            return button;
        }

        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent) => new HelpBox("Not supported for UI Toolkit", HelpBoxMessageType.Error);
#endif
    }
}
