#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.RendererGroup.TabGroup;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public partial class SaintsRendererGroup
    {
        private HelpBox _helpBox;
        private Dictionary<string, List<VisualElement>> _fieldToVisualElement;

        public static readonly Color FancyBgColor = new Color(0.2509804f, 0.2509804f, 0.2509804f);
        private static readonly Color FancyBorderColor = EColor.MidnightAsh.GetColor();

        // private static Texture2D dropdownIcon;
        // private static Texture2D dropdownRightIcon;

        private RichTextDrawer _richTextDrawer;

        public VisualElement CreateVisualElement(VisualElement inspectorRoot)
        {
            _richTextDrawer ??= new RichTextDrawer();

            const int radius = 3;

            _dropdownIcon ??= Util.LoadResource<Texture2D>("classic-dropdown.png");
            _dropdownRightIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-right.png");

            _fieldToVisualElement = new Dictionary<string, List<VisualElement>>();

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    borderTopLeftRadius = radius,
                    borderTopRightRadius = radius,
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius,
                    width = new StyleLength(Length.Percent(100)),
                },
            };

            VisualElement titleRow = new VisualElement
            {
                style =
                {
                    flexGrow =  InAnyHorizontalLayout? 0: 1,
                },
                name = $"saints-field-group--title--{_groupPath}",
            };

            bool hasFoldout = _eLayout.HasFlagFast(ELayout.Foldout) || _eLayout.HasFlagFast(ELayout.Collapse);
            bool hasTitle = _eLayout.HasFlagFast(ELayout.Title) || _eLayout.HasFlagFast(ELayout.TitleOut);
            bool hasTab = _eLayout.HasFlagFast(ELayout.Tab);

            // Toolbar toolbar = new Toolbar
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         marginLeft = 1,
            //         marginRight = 0,
            //     },
            // };

            // List<Action<string>> switchTabActions = new List<Action<string>>
            // {
            //     tab =>
            //     {
            //         foreach (ToolbarToggle toolbarToggle in toolbarToggles)
            //         {
            //             toolbarToggle.SetValueWithoutNotify(toolbarToggle.text == tab);
            //         }
            //
            //         foreach((string groupPath, List<VisualElement> visualElements) in fieldToVisualElement)
            //         {
            //             bool display = tab == null || groupPath == tab;
            //             visualElements.ForEach(visualElement => visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None);
            //         }
            //     },
            // };

            // ReSharper disable once ConvertToLocalFunction
            // Action<string> switchTab = tab =>
            // {
            //     foreach (Action<string> switchTabAction in switchTabActions)
            //     {
            //         switchTabAction.Invoke(tab);
            //     }
            // };

            // foreach (ToolbarToggle toolbarToggle in toolbarToggles)
            // {
            //     toolbarToggle.RegisterValueChangedCallback(evt =>
            //     {
            //         if (evt.newValue)
            //         {
            //             switchTab(curTab = toolbarToggle.text);
            //         }
            //         else
            //         {
            //             toolbarToggle.SetValueWithoutNotify(true);
            //         }
            //     });
            // }

            bool inHorizontal = _eLayout.HasFlagFast(ELayout.Horizontal);
            VisualElement body = new VisualElement
            {
                name = $"saints-field-group--body--{_groupPath}",
                style =
                {
                    flexGrow = 1,
                    flexDirection = inHorizontal? FlexDirection.Row :FlexDirection.Column,
                },
            };
            if (_eLayout.HasFlagFast(ELayout.Background))
            {
                body.style.paddingRight = 4;
                body.style.paddingTop = 1;
                body.style.paddingBottom = 3;
            }

            // // ReSharper disable once ConvertToLocalFunction
            // Action<bool> foldoutAction = show =>
            // {
            //     if (show)
            //     {
            //         if (hasTitle)
            //         {
            //             toolbar.style.display = DisplayStyle.Flex;
            //         }
            //         switchTab(curTab);
            //     }
            //     else
            //     {
            //         if (hasTitle)
            //         {
            //             toolbar.style.display = DisplayStyle.None;
            //         }
            //         foreach (List<VisualElement> visualElements in fieldToVisualElement.Values)
            //         {
            //             visualElements.ForEach(visualElement => visualElement.style.display = DisplayStyle.None);
            //         }
            //     }
            // };

            if (!hasFoldout && hasTitle)  // in this case, draw title above, alone
            {
                VisualElement title = new VisualElement
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        paddingLeft = 5,
                        paddingTop = 2,
                        paddingBottom = 2,
                        borderTopLeftRadius = radius,
                        borderTopRightRadius = radius,
                    },
                };

                // work around UI Toolkit CJK issue
                UIToolkitUtils.SetLabelChildren(title, RichTextDrawer.ParseRichXmlWithProvider(_groupPath.Last() + "  ", new RichTextDrawer.EmptyRichTextTagProvider()), _richTextDrawer);

                title.RemoveFromClassList("unity-label");
                if (_eLayout.HasFlagFast(ELayout.TitleOut))
                {
                    if(_eLayout.HasFlagFast(ELayout.Background))
                    {
                        // boxed
                        title.style.backgroundColor = new Color(53f / 255, 53f / 255, 53f / 255, 1f);
                        title.style.borderBottomColor = FancyBgColor;
                    }
                    else
                    {
                        // separator
                        title.style.borderBottomColor = EColor.EditorSeparator.GetColor();
                        title.style.paddingBottom = 1;
                        title.style.marginBottom = 2;
                    }

                    title.style.borderBottomWidth = 1f;
                }
                titleRow.Add(title);
            }

            // foldout-title:
            if (hasFoldout && (
                    (hasTitle && hasTab)
                    || hasTitle
                    || !hasTab))
            {
                bool titleOut = _eLayout.HasFlagFast(ELayout.TitleOut);
                bool background = _eLayout.HasFlagFast(ELayout.Background);
                bool fancy = titleOut && background;
                if (fancy)  // title clickable foldout
                {
                    const float imageSize = 16;

                    Button title = new Button
                    {
                        // text = _groupPath.Split('/').Last(),
                        style =
                        {
                            flexDirection = FlexDirection.Row,

                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginTop = 0,
                            marginLeft = 0,
                            marginRight = 0,
                            unityTextAlign = TextAnchor.MiddleLeft,
                            paddingLeft = imageSize,
                            paddingTop = 2,
                            paddingBottom = 2,
                            // borderTopLeftRadius = radius,
                            // borderTopRightRadius = radius,
                            borderBottomLeftRadius = 0,
                            borderBottomRightRadius = 0,
                            backgroundColor = new Color(53f / 255, 53f / 255, 53f / 255, 1f),
                            borderBottomColor = FancyBorderColor,
                            // borderBottomWidth = 1f,
                            borderLeftWidth = 0,
                            borderRightWidth = 0,
                            borderTopWidth = 0,
                            borderBottomWidth = 0,

                            // alignItems = Align.FlexEnd,
                        },
                    };
                    titleRow.Add(title);

                    Image foldoutImage = new Image
                    {
                        image = _foldout? _dropdownIcon: _dropdownRightIcon,
                        tintColor = Color.gray,
                        style =
                        {
                            width = imageSize,
                            height = imageSize,
                            marginLeft = -imageSize,
                        },
                    };
                    title.Add(foldoutImage);

                    VisualElement titleLabel = new VisualElement();
                    title.Add(titleLabel);
                    // work around UI Toolkit CJK issue
                    UIToolkitUtils.SetLabelChildren(titleLabel, RichTextDrawer.ParseRichXmlWithProvider(_groupPath.Last() + "  ", new RichTextDrawer.EmptyRichTextTagProvider()), _richTextDrawer);

                    body.style.display = _foldout? DisplayStyle.Flex : DisplayStyle.None;
                    title.clicked += () =>
                    {
                        _foldout = !_foldout;
                        body.style.display = _foldout? DisplayStyle.Flex : DisplayStyle.None;
                        foldoutImage.image = _foldout ? _dropdownIcon : _dropdownRightIcon;
                        // tabToolbar.style.display = _foldout? DisplayStyle.Flex : DisplayStyle.None;
                    };
                }
                else
                {
                    Foldout foldout = new Foldout
                    {
                        text = " ",
                        // text = _groupPath.Split('/').Last(),
                        // text = _groupPath.Last(),
                        value = _foldout,
                    };
                    Label foldoutLabel = foldout.Q<Toggle>().Q<Label>();
                    UIToolkitUtils.SetLabel(foldoutLabel, RichTextDrawer.ParseRichXmlWithProvider(_groupPath.Last(), new RichTextDrawer.EmptyRichTextTagProvider()), _richTextDrawer);
                    if (_eLayout.HasFlagFast(ELayout.TitleOut))
                    {
                        if (_eLayout.HasFlagFast(ELayout.Background))
                        {
                            foldout.style.backgroundColor = new Color(53f / 255, 53f / 255, 53f / 255, 1f);
                        }

                        // Label foldoutLabel = foldout.Q<Label>(className: "unity-foldout__text");
                        foldoutLabel.style.flexGrow = 1;
                        if (_foldout)
                        {
                            foldoutLabel.style.borderBottomWidth = 1f;
                        }
                        foldoutLabel.style.borderBottomColor = EColor.EditorSeparator.GetColor();
                        foldout.RegisterValueChangedCallback(e =>
                        {
                            foldoutLabel.style.borderBottomWidth = e.newValue ? 1f : 0f;
                            if (e.newValue)
                            {
                                // Debug.Log("CheckOutOfScoopFoldout");
                                UIToolkitUtils.CheckOutOfScoopFoldout(foldout, new HashSet<Toggle>());
                            }
                        });
                    }

                    root = foldout;
                }
            }

            // foldout-tabs:
            if (hasTab)
            {
                if (hasFoldout)
                {
                    body.Add(SetupTabToolbar(false));
                }
                else
                {
                    titleRow.Add(SetupTabToolbar(true));
                }
            }
            // if (hasFoldout && !hasTitle && hasTab)
            // {
            //     titleRow.Add(SetupTabToolbar(true));
            // }
            //
            // // tabs
            // // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // if((!hasFoldout && hasTitle && hasTab) || (hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
            // {
            //     titleRow.Add(SetupTabToolbar(!hasFoldout));
            // }

            foreach ((string groupPath, ISaintsRenderer renderer) in _renderers)
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                // string groupId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);
                string groupId = RuntimeUtil.SeparatePath(groupPath).LastOrDefault() ?? "";
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                Debug.Log($"add item@{groupPath}->{groupId}: {renderer}");
#endif

                // bool inHorizontal = EnumFlagsUtil.HasFlag(_eLayout, ELayout.Horizontal);
                renderer.InAnyHorizontalLayout = InAnyHorizontalLayout || inHorizontal;
                renderer.InDirectHorizontalLayout = inHorizontal;

                VisualElement fieldElement = renderer.CreateVisualElement(inspectorRoot);
                // ReSharper disable once InvertIf
                if(fieldElement != null)
                {
                    if (!_fieldToVisualElement.TryGetValue(groupId, out List<VisualElement> visualElements))
                    {
                        _fieldToVisualElement[groupId] = visualElements = new List<VisualElement>();
                    }

                    if (inHorizontal)
                    {
                        fieldElement.style.flexBasis = 0;
                    }
                    visualElements.Add(fieldElement);
                    body.Add(fieldElement);

                    _onSearchFieldUIToolkit.AddListener(renderer.OnSearchField);
                    fieldElement.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(renderer.OnSearchField));
                }
            }

            bool fancyBox = IsFancyBox(_eLayout);
            if(fancyBox)
            {
                root.style.backgroundColor = FancyBgColor;
                root.style.borderTopWidth = 1;
                root.style.borderLeftWidth = 1;
                root.style.borderRightWidth = 1;
                root.style.borderBottomWidth = 1;
                root.style.borderLeftColor = FancyBorderColor;
                root.style.borderRightColor = FancyBorderColor;
                root.style.borderTopColor = FancyBorderColor;
                root.style.borderBottomColor = FancyBorderColor;
                root.style.paddingRight = 2;
            }

            root.Add(titleRow);
            root.Add(body);


            // UIToolkitUtils.WaitUntilThenDo(body, () =>
            // {
            //     var l = body.Query<Label>().ToList();
            //     if (l.Count <= 1)
            //     {
            //         Debug.Log($"not found");
            //         return (false, null);
            //     }
            //
            //     return (true, l);
            // }, labels =>
            // {
            //     Debug.Log($"fix labels {labels.Count}");
            //     labels.ForEach(UIToolkitUtils.FixLabelWidthUIToolkit);
            // }, 200);

            // if (_eLayout.HasFlagFast(ELayout.Tab))
            // {
            //     // ReSharper disable once ConvertToLocalFunction
            //     EventCallback<AttachToPanelEvent> switchOnAttach = null;
            //     switchOnAttach = _ =>
            //     {
            //         root.UnregisterCallback(switchOnAttach);
            //         toolbarToggles[0].value = true;
            //         if (foldoutToggle != null)
            //         {
            //             foldoutToggle.value = _foldout;
            //         }
            //     };
            //     root.RegisterCallback(switchOnAttach);
            // }

            float marginTop = _config.MarginTop > 0 ? _config.MarginTop : 2;

            float marginBottom = _config.MarginBottom > 0 ? _config.MarginBottom : 0;

            root.style.marginTop = marginTop;
            root.style.marginBottom = marginBottom;

            // sub container has some space left
            // if(_groupPath.Contains('/') && _eLayout.HasFlagFast(ELayout.Background) && _eLayout.HasFlagFast(ELayout.Title) && _eLayout.HasFlagFast(ELayout.TitleOut))
            if(_groupPath.Count >= 2 && _eLayout.HasFlagFast(ELayout.Background) && _eLayout.HasFlagFast(ELayout.Title) && _eLayout.HasFlagFast(ELayout.TitleOut))
            {
                root.style.marginLeft = 4;
            }

            // root.RegisterCallback<DetachFromPanelEvent>(_ =>
            // {
            //     UnityEngine.Object.DestroyImmediate(dropdownIcon);
            //     UnityEngine.Object.DestroyImmediate(dropdownRightIcon);
            // });

            if(NeedIndentCheck(_eLayout))
            {
                // Debug.Log($"Check indent: {_eLayout}");
                root.RegisterCallback<AttachToPanelEvent>(_ => UIToolkitUtils.LoopCheckOutOfScoopFoldout(root));
            }

            if(_toggleCheckInfos.Count > 0)
            {
                root.schedule.Execute(() => LoopCheckTogglesUIToolkit(_toggleCheckInfos, root, body)).Every(150);
            }

            root.name = $"saints-field-group--{_groupPath}";

            root.Add(_helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            });

            return root;

            TabToolbar SetupTabToolbar(bool hasExpand)
            {
                TabToolbar tabToolbar = new TabToolbar(_orderedKeys, hasExpand);

                tabToolbar.OnValueChangedAddListener(SetDisplayGroups);
                tabToolbar.SetValueWithoutNotification(_orderedKeys[0]);
                UIToolkitUtils.OnAttachToPanelOnce(tabToolbar, _ =>
                {
                    SetDisplayGroups(_orderedKeys[0]);
                });
                return tabToolbar;
            }
        }

        private void SetDisplayGroups(string tab)
        {
            // Debug.Log($"cur tab = {tab}");
            foreach((string groupPath, List<VisualElement> visualElements) in _fieldToVisualElement)
            {
                // Debug.Log($"groupPath={groupPath}");
                bool display = tab == null || groupPath == tab;
                visualElements.ForEach(visualElement => visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None);
            }
        }

        private void LoopCheckTogglesUIToolkit(List<ToggleCheckInfo> notFilledToggleCheckInfos, VisualElement root, VisualElement body)
        {
            List<ToggleCheckInfo> toggleCheckInfos = notFilledToggleCheckInfos
                .Select(v => SaintsEditorUtils.FillResult(v, null))
                .ToList();
            string error = string.Join("\n", toggleCheckInfos.SelectMany(each => each.Errors));

            UIToolkitUtils.SetHelpBox(_helpBox, error);
            if (error != "")
            {
                return;
            }

            (bool show, bool disable) = SaintsEditorUtils.GetToggleResult(toggleCheckInfos);

            bool currentDisabled = !body.enabledSelf;
            if (currentDisabled != disable)
            {
                body.SetEnabled(!disable);
            }

            bool currentVisible = root.style.display != DisplayStyle.None;
            if (currentVisible != show)
            {
                root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        // private readonly HashSet<Toggle> _processedToggles = new HashSet<Toggle>();


    }
}
#endif
