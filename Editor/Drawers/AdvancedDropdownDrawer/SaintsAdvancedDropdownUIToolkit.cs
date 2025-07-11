#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static (Rect worldBound, float maxHeight) GetProperPos(Rect rootWorldBound)
        {
            int screenHeight = Screen.currentResolution.height;

            const float edgeHeight = 150;

            float maxHeight = screenHeight - rootWorldBound.yMax - edgeHeight;
            Rect worldBound = new Rect(rootWorldBound);
            // Debug.Log(worldBound);
            // ReSharper disable once InvertIf
            if (maxHeight < edgeHeight)
            {
                // worldBound.x -= 400;
                worldBound.y -= edgeHeight + worldBound.height;
                // Debug.Log(worldBound);
                maxHeight = Mathf.Max(edgeHeight, screenHeight - edgeHeight);
            }

            return (worldBound, maxHeight);
        }

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

        private static readonly KeyCode[] NormalInputKeys = Enumerable
            .Range((int)KeyCode.Space, KeyCode.KeypadEquals - KeyCode.Space)
            .Cast<KeyCode>()
            .ToArray();
        // private static readonly char[] PrintableAscii = {
        //     ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        //     '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
        //     '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
        //     'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_',
        //     '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
        //     'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', '|', '}', '~'
        // };

        private Texture2D _nextIcon;
        private Texture2D _checkGroupIcon;
        private Texture2D _checkIcon;
        private StyleSheet _hackSliderStyle;
        private VisualTreeAsset _itemAsset;
        private VisualElement _scrollViewContainer;
        private VisualTreeAsset _separatorAsset;

        private ToolbarSearchField _toolbarSearchField;

        // public VisualElement DebugCloneTree()
        // {
        //     return CloneTree();
        // }

        private VisualElement CloneTree()
        {
            StyleSheet ussStyle = Util.LoadResource<StyleSheet>("UIToolkit/SaintsAdvancedDropdown/Style.uss");
            _hackSliderStyle = Util.LoadResource<StyleSheet>("UIToolkit/SaintsAdvancedDropdown/HackSliderStyle.uss");

            VisualTreeAsset popUpAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/Popup.uxml");
            VisualElement root = popUpAsset.CloneTree();

            root.styleSheets.Add(ussStyle);

            _toolbarSearchField = root.Q<ToolbarSearchField>();
            TextField toolbarTextField = _toolbarSearchField.Q<TextField>();
            VisualElement searchField = toolbarTextField.Q("unity-text-input");

            // root.Q<Image>(name: "saintsfield-advanced-dropdown-search-clean-image").image = Util.LoadResource<Texture2D>("classic-close.png");

            _separatorAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/Separator.uxml");

            _itemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/ItemRow.uxml");

            _scrollViewContainer = root.Q<VisualElement>("saintsfield-advanced-dropdown-scroll-view-container");
            ScrollView scrollView = root.Q<ScrollView>();
            scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeoUpdateWindowSize);

            scrollView.focusable = true;
            scrollView.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if(_displayPage.Count > 0)
                {
                    if (evt.direction is NavigationMoveEvent.Direction.Up or NavigationMoveEvent.Direction.Down)
                    {
                        int offsetIndex = _displayKeyboardHighlight +
                                          (evt.direction == NavigationMoveEvent.Direction.Up ? -1 : 1);
                        int newIndex =
                            Util.PositiveMod(offsetIndex, _displayPage.Count);

                        // Debug.Log($"_displayKeyboardHighlight={_displayKeyboardHighlight}; {offsetIndex} % {_displayPage.Count}={newIndex}");
                        _displayKeyboardHighlight = newIndex;

                        foreach ((DisplayPageItem value, int index)  in _displayPage.WithIndex())
                        {
                            if (index == newIndex)
                            {
                                // Debug.Log($"add {index} class {KeyboardHoverClass}");
                                value.Item.Q<Button>().AddToClassList(KeyboardHoverClass);
                            }
                            else
                            {
                                value.Item.Q<Button>().RemoveFromClassList(KeyboardHoverClass);
                            }
                        }
                    }
                    else if (evt.direction == NavigationMoveEvent.Direction.Left)
                    {
                        _pagePreAction?.Invoke();
                    }
                    else if (evt.direction == NavigationMoveEvent.Direction.Right)
                    {
                        if(_displayKeyboardHighlight >= 0 && _displayKeyboardHighlight < _displayPage.Count)
                        {
                            DisplayPageItem displayPageItem = _displayPage[_displayKeyboardHighlight];
                            displayPageItem.OnNext?.Invoke();
                        }
                    }
                }
            }, TrickleDown.TrickleDown);

            scrollView.RegisterCallback<KeyUpEvent>(evt =>
            {
                // Debug.Log(evt.keyCode);
                if (evt.keyCode == KeyCode.Escape)
                {
                    // editorWindow.Close();
                    _toolbarSearchField.Focus();
                    return;
                }

                if (evt.keyCode == KeyCode.Return)
                {
                    if(_displayKeyboardHighlight >= 0 && _displayKeyboardHighlight < _displayPage.Count)
                    {
                        DisplayPageItem displayPageItem = _displayPage[_displayKeyboardHighlight];
                        displayPageItem.OnSelect?.Invoke();
                    }
                }
            }, TrickleDown.TrickleDown);

            scrollView.RegisterCallback<KeyUpEvent>(evt =>
            {
                // Debug.Log(evt.modifiers);
                // Debug.Log(Console.CapsLock);
                // Debug.Log(Input.GetKey(KeyCode.CapsLock));
                // Debug.Log(Array.IndexOf(NormalInputKeys, evt.keyCode) != -1);
                // ReSharper disable once InvertIf
                if (searchField.focusController?.focusedElement != searchField
                    && evt.modifiers == EventModifiers.None
                    && Array.IndexOf(NormalInputKeys, evt.keyCode) != -1)
                {
                    searchField.Focus();
                    // evt.caps

                    using (KeyDownEvent pooled = KeyDownEvent.GetPooled(evt.character, evt.keyCode, evt.modifiers))
                    {
                        pooled.target = searchField;
                        searchField.SendEvent(pooled);
                    }
                    using (KeyUpEvent pooled = KeyUpEvent.GetPooled(evt.character, evt.keyCode, evt.modifiers))
                    {
                        pooled.target = searchField;
                        searchField.SendEvent(pooled);
                    }
                    toolbarTextField.schedule.Execute(() =>
                        toolbarTextField.SelectRange(toolbarTextField.value.Length, toolbarTextField.value.Length));
                }
            }, TrickleDown.TrickleDown);

            // scrollView.contentContainer.style.borderBottomWidth = 1;
            // scrollView.contentContainer.style.borderBottomColor = Color.green;

            // Texture2D icon = RichTextDrawer.LoadTexture("eye.png");
            _nextIcon = Util.LoadResource<Texture2D>("arrow-next.png");
            _checkGroupIcon = Util.LoadResource<Texture2D>("arrow-right.png");
            _checkIcon = Util.LoadResource<Texture2D>("check.png");

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
                    _metaInfo.SelectStacks,
                    newStack,
                    animRightToLeft);

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
            IReadOnlyList<(IReadOnlyList<string> stackDisplay, string display, string icon, bool disabled, object value)> flattenOptions = AdvancedDropdownAttributeDrawer.Flatten(_metaInfo.DropdownListValue).ToArray();
            // Dictionary<string, VisualElement> stackDisplayToElement = new Dictionary<string, VisualElement>();
            // _toolbarSearchField.focusable = true;
#if UNITY_6000_0_OR_NEWER
            _toolbarSearchField.placeholderText = "Search";
#endif
            _toolbarSearchField.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                // ReSharper disable once InvertIf
                if(evt.direction is NavigationMoveEvent.Direction.Up or NavigationMoveEvent.Direction.Down)
                {
                    // Debug.Log(evt.direction);
                    scrollView.Focus();
                    // using (var pooledEvent = NavigationMoveEvent.GetPooled(evt.direction, evt.modifiers))
                    // {
                    //     pooledEvent.target = scrollView;
                    //     scrollView.SendEvent(pooledEvent);
                    // }
                    // scrollView.SendEvent(evt);
                }

                if (evt.direction == NavigationMoveEvent.Direction.Left &&
                    string.IsNullOrEmpty(_toolbarSearchField.value))
                {
                    _pagePreAction?.Invoke();
                }
            }, TrickleDown.TrickleDown);
            _toolbarSearchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if(evt.keyCode == KeyCode.Escape && string.IsNullOrEmpty(_toolbarSearchField.value))
                {
                    editorWindow.Close();
                }
            }, TrickleDown.TrickleDown);
            _toolbarSearchField.RegisterValueChangedCallback(evt =>
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

                _displayPage.Clear();
                _displayKeyboardHighlight = -1;
                _pagePreAction = () => _toolbarSearchField.Focus();

                toolbarBreadcrumbs.style.display = DisplayStyle.None;

                (IReadOnlyList<string> stackDisplay, string display, string icon, bool disabled, object value)[] matchedValueOptions = flattenOptions.Where(each =>
                {
                    // Debug.Log($"{string.Join("/", each.stackDisplay)}: {each.display}");
                    string lowerDisplay = each.display.ToLower();
                    return searchFragments.All(fragment => lowerDisplay.Contains(fragment));
                }).ToArray();

                // List<(string stackDisplay, string display, string icon, bool disabled, object value)> matchedOptions = new List<(string stackDisplay, string display, string icon, bool disabled, object value)>(matchedValueOptions);
                // matchedOptions.AddRange(matchedPathOptions);
                // var matchedOptions = matchedPathOptions;

                bool hasValueMatch = matchedValueOptions.Length > 0;

                bool anyHasIcon = false;
                List<Image> iconImages = new List<Image>(matchedValueOptions.Length);

                scrollView.Clear();
                foreach ((IReadOnlyList<string> stackDisplays, string display, string icon, bool disabled, object value) in matchedValueOptions)
                {
                    string stackDisplay = string.Join("/", stackDisplays.SkipLast(1));

                    TemplateContainer elementItem = _itemAsset.CloneTree();

                    Button itemContainer =
                        elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

                    Image selectImage = itemContainer.Q<Image>("item-checked-image");
                    selectImage.image = _checkIcon;

                    string labelText = stackDisplay.Length == 0
                        ? display
                        : $"{display} <color=#{ColorUtility.ToHtmlStringRGBA(EColor.Gray.GetColor())}>({stackDisplay})</color>";

                    itemContainer.Q<Label>("item-content").text = labelText;

                    // bool curSelect = _metaInfo.SelectStacks.Count > 0 && _metaInfo.CurValues.Any(each => Util.GetIsEqual(each, value)) ;
                    bool curSelect = _metaInfo.CurValues.Any(each => Util.GetIsEqual(each, value)) ;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                    Debug.Log($"curSelect={curSelect}, _metaInfo.SelectStacks.Count={_metaInfo.SelectStacks.Count}, _metaInfo.CurValue={_metaInfo.CurValues}, value={value}");
#endif

                    Image itemIconImage = itemContainer.Q<Image>("item-icon-image");
                    iconImages.Add(itemIconImage);
                    if(!string.IsNullOrEmpty(icon))
                    {
                        anyHasIcon = true;
                        itemIconImage.image = Util.LoadResource<Texture2D>(icon);
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
                            itemContainer.clicked += editorWindow.Close;
                            _displayPage.Add(new DisplayPageItem(itemContainer, null, editorWindow.Close));
                        }
                        else
                        {
                            Action onSelect = () =>
                            {
                                _setValue(stackDisplay, value);
                                editorWindow.Close();
                            };
                            itemContainer.clicked += onSelect;

                            _displayPage.Add(new DisplayPageItem(itemContainer, null, onSelect));
                        }
                    }

                    scrollView.Add(elementItem);
                }

                // path match
                SearchPathStack[] pathMatch = SearchPath(searchFragments, _metaInfo.DropdownListValue).ToArray();

                if(hasValueMatch && pathMatch.Length > 0)
                {
                    scrollView.Add(_separatorAsset.CloneTree());
                }

                foreach (SearchPathStack pathStack in pathMatch)
                {
                    // string stackDisplay = string.Join("/", pathStack.SelectStacks.Select(each => each.Display));
                    TemplateContainer elementItem = _itemAsset.CloneTree();

                    Button itemContainer =
                        elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

                    Image selectImage = itemContainer.Q<Image>("item-checked-image");
                    selectImage.image = _checkIcon;

                    AdvancedDropdownAttributeDrawer.SelectStack lastStack = pathStack.SelectStacks[^1];
                    AdvancedDropdownAttributeDrawer.SelectStack[] preStacks = pathStack.SelectStacks.Take(pathStack.SelectStacks.Count - 1).ToArray();

                    StringBuilder sb = new StringBuilder(lastStack.Display);
                    if (preStacks.Length > 0)
                    {
                        sb.Append($" <color=#{ColorUtility.ToHtmlStringRGBA(EColor.Gray.GetColor())}>(");
                        sb.Append(string.Join("/", preStacks.Select(each => each.Display)));
                        sb.Append(")</color>");
                    }

                    itemContainer.Q<Label>("item-content").text = sb.ToString();

                    Image itemIconImage = itemContainer.Q<Image>("item-icon-image");
                    iconImages.Add(itemIconImage);
                    if(!string.IsNullOrEmpty(pathStack.Target.icon))
                    {
                        anyHasIcon = true;
                        itemIconImage.image = Util.LoadResource<Texture2D>(pathStack.Target.icon);
                    }

                    // if (curSelect)
                    // {
                    //     selectImage.visible = true;
                    // }
                    itemContainer.Q<Image>("item-next-image").image = _nextIcon;

                    if (pathStack.Target.disabled)
                    {
                        itemContainer.SetEnabled(false);
                        itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-disabled");
                        itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                    }
                    else  // not disabled (no not-enabled appearance)
                    {
                        Action onNext = () =>
                        {
                            // Debug.Log($"go to {string.Join("/", pathStack.SelectStacks)}");
                            _toolbarSearchField.SetValueWithoutNotify("");
                            if (!_isFlat)
                            {
                                toolbarBreadcrumbs.style.display = DisplayStyle.Flex;
                            }

                            GoToStackEvent.Invoke(pathStack.SelectStacks.Append(
                                new AdvancedDropdownAttributeDrawer.SelectStack
                                {
                                    Index = 0,
                                    Display = pathStack.Target.children.First(each => !each.isSeparator).displayName,
                                }).ToArray());
                            // _setValue(stackDisplay, value);
                            // editorWindow.Close();
                        };
                        itemContainer.clicked += onNext;

                        _displayPage.Add(new DisplayPageItem(itemContainer, onNext, onNext));
                    }
                    scrollView.Add(elementItem);
                }


                if (!hasValueMatch && pathMatch.Length == 0)
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
                else if (!anyHasIcon)
                {
                    foreach (Image iconImage in iconImages)
                    {
                        iconImage.style.display = DisplayStyle.None;
                    }
                }
            });

            root.RegisterCallback<AttachToPanelEvent>(_ => searchField.Focus());
            // root.RegisterCallback<AttachToPanelEvent>(_ => scrollView.Focus());

// #if UNITY_2023_2_OR_NEWER
//             // don't lose focus
//             root.RegisterCallback<PointerDownEvent>(
//                 evt => root.focusController.IgnoreEvent(evt),
//                 TrickleDown.TrickleDown);
// #endif

            return root;
        }

        private struct SearchPathStack
        {
            public IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> SelectStacks;
            public IAdvancedDropdownList Target;

            public override string ToString()
            {
                return string.Join("/", SelectStacks.Select(each => $"[{each.Index}]:{each.Display}"));
            }
        }

        private static IEnumerable<SearchPathStack> SearchPath(IReadOnlyList<string> searchFragments, IAdvancedDropdownList dropdownList)
        {
            return SearchPathRec(searchFragments, dropdownList, Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), Array.Empty<string>());
        }

        private static IEnumerable<SearchPathStack> SearchPathRec(IReadOnlyList<string> searchFragments, IAdvancedDropdownList dropdownList, IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stackAccs, IReadOnlyList<string> parentDisplays)
        {
            if (dropdownList.ChildCount() == 0)
            {
                // Debug.Log($"Skip {dropdownList.displayName}");
                yield break;
            }

            foreach ((IAdvancedDropdownList child, int index) in dropdownList.children.WithIndex().Where(each => each.value.ChildCount() > 0))
            {
                AdvancedDropdownAttributeDrawer.SelectStack thisStack = new AdvancedDropdownAttributeDrawer.SelectStack
                {
                    Display = child.displayName,
                    Index = index,
                };
                AdvancedDropdownAttributeDrawer.SelectStack[] stackNew = stackAccs.Append(thisStack).ToArray();

                IReadOnlyList<string> accDisplays = parentDisplays.Append(child.displayName).ToArray();
                List<string> lowerDisplays = accDisplays.Select(each => each.ToLower()).ToList();
                if (searchFragments.All(fragment => lowerDisplays.Any(eachLow => eachLow.Contains(fragment))))
                {
                    yield return new SearchPathStack
                    {
                        SelectStacks = stackNew,
                        Target = child,
                    };
                }
                // else
                // {
                //     Debug.Log($"No match {dropdownList.displayName}: {string.Join(" ", lowerDisplays)} from {string.Join(" ", searchFragments)}");
                // }

                foreach (SearchPathStack searchPathStack in SearchPathRec(searchFragments, child, stackNew, accDisplays))
                {
                    yield return searchPathStack;
                }
            }
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

        private readonly struct DisplayPageItem
        {
            public readonly VisualElement Item;
            public readonly Action OnNext;
            public readonly Action OnSelect;

            public DisplayPageItem(VisualElement item, Action onNext, Action onSelect)
            {
                Item = item;

                OnNext = onNext;
                OnSelect = onSelect;
            }
        }

        private readonly List<DisplayPageItem> _displayPage = new List<DisplayPageItem>();
        // private List<int> _displayPageSelectIndices;
        private int _displayKeyboardHighlight = -1;

        public const string KeyboardHoverClass = "saintsfield-advanced-dropdown-item-keyboard-active";
        // private const Color KeyboardHoverHardColor = new Color(89 / 255f, 89 / 255f, 89 / 255f);

        private void OnPageCursorEnter(int index)
        {
            if (string.IsNullOrWhiteSpace(_toolbarSearchField.value))
            {
                return;
            }

            if(_displayKeyboardHighlight != index)
            {
                _displayKeyboardHighlight = index;
                foreach (DisplayPageItem displayPageItem in _displayPage)
                {
                    displayPageItem.Item.Q<Button>().RemoveFromClassList(KeyboardHoverClass);
                }
            }
        }

        private void OnPageCursorLeave(int index)
        {
            if (_displayKeyboardHighlight == index)
            {
                _displayKeyboardHighlight = -1;
            }
        }

        private Action _pagePreAction;

        private void SwapPage(
            IReadOnlyList<object> curValues,
            bool allowUnSelect,
            IAdvancedDropdownList mainDropdownList,
            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStack,
            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> pageStack,
            bool animRightToLeft)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
            Debug.Log($"selectStack={string.Join("->", selectStack.Select(each => $"{each.Display}/{each.Index}"))}");
            Debug.Log($"pageStack={string.Join("->", pageStack.Select(each => $"{each.Display}/{each.Index}"))}");
#endif
            (IReadOnlyList<IAdvancedDropdownList> displayPage, IReadOnlyList<int> selectIndices) = GetPage(
                mainDropdownList,
                curValues,
                new Queue<AdvancedDropdownAttributeDrawer.SelectStack>(pageStack.SkipLast(1)),
                new Queue<AdvancedDropdownAttributeDrawer.SelectStack>(selectStack));

            // _displayPage = new List<IAdvancedDropdownList>(displayPage);
            _displayPage.Clear();
            _displayKeyboardHighlight = -1;

            // Debug.Log(selectIndex);

            _scrollViewContainer.styleSheets.Add(_hackSliderStyle);

            ScrollView scrollView = _scrollViewContainer.Q<ScrollView>();

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
            _scrollViewContainer.Add(fadeOutOriChildren);

            scrollView.Clear();

            VisualElement scrollContent = new VisualElement();
            // https://forum.unity.com/threads/how-to-refresh-scrollview-scrollbars-to-reflect-changed-content-width-and-height.1260920/
            scrollContent.RegisterCallback<TransitionEndEvent>(_ =>
            {
                _scrollViewContainer.styleSheets.Remove(_hackSliderStyle);

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

            bool anyHasIcon = false;
            List<Image> iconImages = new List<Image>(displayPage.Count);

            foreach ((IAdvancedDropdownList dropdownItem, int index) in displayPage.WithIndex())
            {
                if (dropdownItem.isSeparator)
                {
                    VisualElement separator = _separatorAsset.CloneTree();
                    scrollContent.Add(separator);
                    continue;
                }

                VisualElement elementItem = _itemAsset.CloneTree();

                Button itemContainer =
                    elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

                Image selectImage = itemContainer.Q<Image>("item-checked-image");
                selectImage.image = dropdownItem.children.Count > 0
                    ? _checkGroupIcon
                    : _checkIcon;
                // allSelectImage.Add(selectImage);

                itemContainer.Q<Label>("item-content").text = dropdownItem.displayName;

                Image itemIconImage = itemContainer.Q<Image>("item-icon-image");
                iconImages.Add(itemIconImage);
                if(!string.IsNullOrEmpty(dropdownItem.icon))
                {
                    itemIconImage.image = Util.LoadResource<Texture2D>(dropdownItem.icon);
                    anyHasIcon = true;
                }
                List<AdvancedDropdownAttributeDrawer.SelectStack> prePageStack = pageStack.SkipLast(1).ToList();
                _pagePreAction = pageStack.Count <= 1
                    ? null
                    : () => GoToStackEvent.Invoke(prePageStack);

                if(dropdownItem.children.Count > 0)
                {
                    itemContainer.Q<Image>("item-next-image").image = _nextIcon;

                    AdvancedDropdownAttributeDrawer.SelectStack curPage = pageStack[^1];

                    List<AdvancedDropdownAttributeDrawer.SelectStack> nextPageStack = new List<AdvancedDropdownAttributeDrawer.SelectStack>(prePageStack);
                    nextPageStack.AddRange(new []
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
                    });

                    // AdvancedDropdownAttributeDrawer.SelectStack[] nextPageStack = pageStack.SkipLast(1).Concat().ToArray();

                    Action nextAction = () => GoToStackEvent.Invoke(nextPageStack);

                    itemContainer.clicked += nextAction;

                    _displayPage.Add(new DisplayPageItem(elementItem,
                        nextAction,
                        nextAction
                    ));

                    elementItem.RegisterCallback<PointerEnterEvent>(_ => OnPageCursorEnter(index));
                    elementItem.RegisterCallback<PointerLeaveEvent>(_ => OnPageCursorLeave(index));
                }
                else
                {
                    if (dropdownItem.disabled)
                    {
                        itemContainer.SetEnabled(false);
                        itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-disabled");
                        itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                    }
                    else if(selectIndices.Contains(index) && !allowUnSelect)
                    {
                        // itemContainer.pickingMode = PickingMode.Ignore;
                        itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                        elementItem.RegisterCallback<PointerEnterEvent>(_ => OnPageCursorEnter(index));
                        elementItem.RegisterCallback<PointerLeaveEvent>(_ => OnPageCursorLeave(index));
                        itemContainer.clicked += editorWindow.Close;
                        _displayPage.Add(new DisplayPageItem(elementItem,
                            null,
                            editorWindow.Close
                        ));
                    }
                    else
                    {
                        Action selectAction = () =>
                        {
                            string newDisplay = string.Join("/",
                                pageStack.Skip(1).Select(each => each.Display).Append(dropdownItem.displayName));
                            // setValue(newDisplay, dropdownItem.value);
                            _setValue(newDisplay, dropdownItem.value);
                            editorWindow.Close();
                        };

                        itemContainer.clicked += selectAction;
                        // elementItem.RegisterCallback<PointerEnterEvent>(_ => Debug.Log(dropdownItem.displayName));
                        elementItem.RegisterCallback<PointerEnterEvent>(_ => OnPageCursorEnter(index));
                        elementItem.RegisterCallback<PointerLeaveEvent>(_ => OnPageCursorLeave(index));

                        _displayPage.Add(new DisplayPageItem(elementItem,
                            null,
                            selectAction
                        ));
                    }
                }

                bool isSelected = selectIndices.Contains(index);
                // bool isSelected = curValues.Contains(dropdownItem.value);
                // Debug.Log($"isSelected={isSelected}, {index}");
                if (isSelected)
                {
                    selectImage.visible = true;
                    itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-selected");
                    _displayKeyboardHighlight = _displayPage.Count - 1;
                }

                scrollContent.Add(elementItem);
            }

            if (!anyHasIcon)
            {
                foreach (Image iconImage in iconImages)
                {
                    iconImage.style.display = DisplayStyle.None;
                }
            }

            scrollContent.RegisterCallback<GeometryChangedEvent>(GeoAnimIntoView);

            scrollView.Add(scrollContent);

            // Debug.Log($"init _displayKeyboardHighlight = {_displayKeyboardHighlight}, length={_displayPage.Count}");
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
        }

        private static void TransitionDestroy(TransitionEndEvent evt)
        {
            // Debug.Log($"TransitionDestroy={evt.target}");
            ((VisualElement)evt.target).RemoveFromHierarchy();
        }

        private static (IReadOnlyList<IAdvancedDropdownList> pageItems, IReadOnlyList<int> selectIndices) GetPage(
            IAdvancedDropdownList dropdownList,
            IReadOnlyList<object> curValues,
            Queue<AdvancedDropdownAttributeDrawer.SelectStack> pageStack,
            Queue<AdvancedDropdownAttributeDrawer.SelectStack> selectStack
        )
        {
            if (pageStack.Count <= 0)  // we are at the page we want
            {
                List<int> rootIndices = new List<int>();

                foreach ((IAdvancedDropdownList eachChild, int eachIndex) in dropdownList.children.WithIndex())
                {
                    if (eachChild.ChildCount() > 0)
                    {
                        bool hasSubSelect = CheckAnySelect(eachChild.children, curValues);
                        if (hasSubSelect)
                        {
                            rootIndices.Add(eachIndex);
                        }
                    }
                    else if (!eachChild.isSeparator && curValues.Contains(eachChild.value))
                    {
                        rootIndices.Add(eachIndex);
                    }
                }

                return (dropdownList.children, rootIndices);
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
            return GetPage(dropdownList.children[index], curValues, pageStack, selectStack);
        }

        private static bool CheckAnySelect(IReadOnlyList<IAdvancedDropdownList> eachChildChildren, IReadOnlyList<object> curValues)
        {
            foreach (IAdvancedDropdownList subChild in eachChildChildren)
            {
                if (subChild.isSeparator)
                {
                    continue;
                }

                if (subChild.ChildCount() > 0)
                {
                    if (CheckAnySelect(subChild.children, curValues))
                    {
                        return true;
                    }
                }

                else if (curValues.Contains(subChild.value))
                {
                    return true;
                }
            }

            return false;
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
