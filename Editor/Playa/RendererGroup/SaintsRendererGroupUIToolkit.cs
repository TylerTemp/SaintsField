#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public partial class SaintsRendererGroup
    {

        public VisualElement CreateVisualElement()
        {
            const int radius = 3;

            Texture2D dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
            Texture2D dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");

            Dictionary<string, List<VisualElement>> fieldToVisualElement = new Dictionary<string, List<VisualElement>>();
            string curTab = null;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    borderTopLeftRadius = radius,
                    borderTopRightRadius = radius,
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius,
                },
            };

            ToolbarToggle foldoutToggle = null;

            VisualElement titleRow = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout) || _eLayout.HasFlag(ELayout.Collapse);
            bool hasTitle = _eLayout.HasFlag(ELayout.Title) || _eLayout.HasFlag(ELayout.TitleOut);
            bool hasTab = _eLayout.HasFlag(ELayout.Tab);

            Toolbar toolbar = new Toolbar
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 1,
                    marginRight = 0,
                },
            };

            ToolbarToggle[] toolbarToggles = _orderedKeys
                .Select(each => new ToolbarToggle
                {
                    text = each,
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        unityFontStyleAndWeight = FontStyle.Bold,
                    },
                })
                .ToArray();

            List<Action<string>> switchTabActions = new List<Action<string>>
            {
                tab =>
                {
                    foreach (ToolbarToggle toolbarToggle in toolbarToggles)
                    {
                        toolbarToggle.SetValueWithoutNotify(toolbarToggle.text == tab);
                    }

                    foreach((string groupPath, List<VisualElement> visualElements) in fieldToVisualElement)
                    {
                        bool display = tab == null || groupPath == tab;
                        visualElements.ForEach(visualElement => visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None);
                    }
                },
            };

            // ReSharper disable once ConvertToLocalFunction
            Action<string> switchTab = tab =>
            {
                foreach (Action<string> switchTabAction in switchTabActions)
                {
                    switchTabAction.Invoke(tab);
                }
            };

            foreach (ToolbarToggle toolbarToggle in toolbarToggles)
            {
                toolbarToggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        switchTab(curTab = toolbarToggle.text);
                    }
                    else
                    {
                        toolbarToggle.SetValueWithoutNotify(true);
                    }
                });
            }

            VisualElement body = new VisualElement
            {
                name = $"saints-editor-layout-{_groupPath}",
                style =
                {
                    flexGrow = 1,
                    flexDirection = _eLayout.HasFlag(ELayout.Horizontal)? FlexDirection.Row :FlexDirection.Column,
                },
            };
            if (_eLayout.HasFlag(ELayout.Background))
            {
                body.style.paddingRight = 4;
                body.style.paddingTop = 1;
                body.style.paddingBottom = 3;
            }

            // ReSharper disable once ConvertToLocalFunction
            Action<bool> foldoutAction = show =>
            {
                if (show)
                {
                    if (hasTitle)
                    {
                        toolbar.style.display = DisplayStyle.Flex;
                    }
                    switchTab(curTab);
                }
                else
                {
                    if (hasTitle)
                    {
                        toolbar.style.display = DisplayStyle.None;
                    }
                    foreach (List<VisualElement> visualElements in fieldToVisualElement.Values)
                    {
                        visualElements.ForEach(visualElement => visualElement.style.display = DisplayStyle.None);
                    }
                }
            };

            if (!hasFoldout && hasTitle)  // in this case, draw title above, alone
            {
                Label title = new Label(_groupPath.Split('/').Last())
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
                if (_eLayout.HasFlag(ELayout.TitleOut))
                {
                    if(_eLayout.HasFlag(ELayout.Background))
                    {
                        // boxed
                        title.style.backgroundColor = new Color(53f / 255, 53f / 255, 53f / 255, 1f);
                        title.style.borderBottomColor = EColor.MidnightAsh.GetColor();
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
                bool titleOut = _eLayout.HasFlag(ELayout.TitleOut);
                bool background = _eLayout.HasFlag(ELayout.Background);
                bool fancy = titleOut && background;
                if (fancy)  // title clickable foldout
                {
                    const float imageSize = 16;

                    Button title = new Button
                    {
                        text = _groupPath.Split('/').Last(),
                        style =
                        {
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
                            borderBottomColor = EColor.MidnightAsh.GetColor(),
                            // borderBottomWidth = 1f,
                            borderLeftWidth = 0,
                            borderRightWidth = 0,
                            borderTopWidth = 0,
                            borderBottomWidth = 0,

                            // alignItems = Align.FlexEnd,
                        },
                    };
                    Image foldoutImage = new Image
                    {
                        image = _foldout? dropdownIcon: dropdownRightIcon,
                        tintColor = Color.gray,
                        style =
                        {
                            width = imageSize,
                            height = imageSize,
                            marginLeft = -imageSize,
                        },
                    };

                    title.Add(foldoutImage);

                    body.style.display = _foldout? DisplayStyle.Flex : DisplayStyle.None;
                    title.clicked += () =>
                    {
                        // _foldout = !_foldout;
                        if (_foldout)  // need collapse
                        {
                            foreach (ToolbarToggle toolbarToggle in toolbarToggles)
                            {
                                toolbarToggle.SetValueWithoutNotify(false);
                            }
                        }
                        else
                        {
                            ToolbarToggle targetToolbar = toolbarToggles.FirstOrDefault(each => each.text == curTab);
                            if (targetToolbar != null)
                            {
                                targetToolbar.value = true;
                            }
                        }

                        _foldout = !_foldout;
                        body.style.display = _foldout? DisplayStyle.Flex : DisplayStyle.None;
                        foldoutImage.image = _foldout ? dropdownIcon : dropdownRightIcon;
                    };

                    switchTabActions.Add(_ =>
                    {
                        _foldout = true;
                        body.style.display = _foldout? DisplayStyle.Flex : DisplayStyle.None;
                        foldoutImage.image = _foldout ? dropdownIcon : dropdownRightIcon;
                    });
                    titleRow.Add(title);
                }
                else
                {
                    Foldout foldout = new Foldout
                    {
                        text = _groupPath.Split('/').Last(),
                        value = _foldout,
                    };
                    if (_eLayout.HasFlag(ELayout.TitleOut))
                    {
                        if (_eLayout.HasFlag(ELayout.Background))
                        {
                            foldout.style.backgroundColor = new Color(53f / 255, 53f / 255, 53f / 255, 1f);
                        }

                        Label foldoutLabel = foldout.Q<Label>(className: "unity-foldout__text");
                        foldoutLabel.style.flexGrow = 1;
                        if (_foldout)
                        {
                            foldoutLabel.style.borderBottomWidth = 1f;
                        }
                        foldoutLabel.style.borderBottomColor = EColor.EditorSeparator.GetColor();
                        foldout.RegisterValueChangedCallback(e => foldoutLabel.style.borderBottomWidth = e.newValue? 1f : 0f);
                    }

                    root = foldout;
                }
            }

            // foldout-tabs:
            if (hasFoldout && !hasTitle && hasTab)
            {
                // toolbar.Add(foldout);
                foldoutToggle = new ToolbarToggle
                {
                    value = true,
                    style =
                    {
                        paddingTop = 0,
                        paddingRight = 0,
                        paddingBottom = 0,
                        paddingLeft = 0,
                    },
                };
                Image foldoutImage = new Image
                {
                    image = dropdownIcon,
                    tintColor = Color.gray,
                };
                foldoutToggle.style.width = SaintsPropertyDrawer.SingleLineHeight;
                foldoutToggle.Add(foldoutImage);
                foldoutToggle.RegisterValueChangedCallback(evt =>
                {
                    _foldout = evt.newValue;
                    foldoutToggle.value = _foldout;
                    foldoutAction(_foldout);
                    foldoutImage.image = _foldout ? dropdownIcon : dropdownRightIcon;

                    if (!_foldout)
                    {
                        foreach (ToolbarToggle toolbarToggle in toolbarToggles)
                        {
                            toolbarToggle.SetValueWithoutNotify(false);
                        }
                    }
                });

                toolbar.Add(foldoutToggle);

                foreach (ToolbarToggle eachTab in toolbarToggles)
                {
                    toolbar.Add(eachTab);
                    eachTab.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue)
                        {
                            foldoutToggle.value = true;
                        }
                    });
                }

                titleRow.Add(toolbar);
            }

            // tabs
            if((!hasFoldout && hasTitle && hasTab) || (hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
            {
                // TODO: 2023_3+ these is a TabView can be used
                foreach (ToolbarToggle eachTab in toolbarToggles)
                {
                    toolbar.Add(eachTab);
                }
                titleRow.Add(toolbar);
            }

            foreach ((string groupPath, ISaintsRenderer renderer) in _renderers)
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                string groupId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                Debug.Log($"add item@{groupPath}->{groupId}: {renderer}");
#endif
                VisualElement fieldElement = renderer.CreateVisualElement();
                if(fieldElement != null)
                {
                    if (!fieldToVisualElement.TryGetValue(groupId, out List<VisualElement> visualElements))
                    {
                        fieldToVisualElement[groupId] = visualElements = new List<VisualElement>();
                    }

                    visualElements.Add(fieldElement);
                    body.Add(fieldElement);
                }
            }

            bool fancyBox = IsFancyBox(_eLayout);
            if(fancyBox)
            {
                root.style.backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f);
                root.style.borderTopWidth = 1;
                root.style.borderLeftWidth = 1;
                root.style.borderRightWidth = 1;
                root.style.borderBottomWidth = 1;
                root.style.borderLeftColor = EColor.MidnightAsh.GetColor();
                root.style.borderRightColor = EColor.MidnightAsh.GetColor();
                root.style.borderTopColor = EColor.MidnightAsh.GetColor();
                root.style.borderBottomColor = EColor.MidnightAsh.GetColor();
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

            if (_eLayout.HasFlag(ELayout.Tab))
            {
                // ReSharper disable once ConvertToLocalFunction
                EventCallback<AttachToPanelEvent> switchOnAttack = null;
                switchOnAttack = _ =>
                {
                    root.UnregisterCallback(switchOnAttack);
                    toolbarToggles[0].value = true;
                    if (foldoutToggle != null) foldoutToggle.value = _foldout;
                };
                root.RegisterCallback(switchOnAttack);
            }

            float marginTop = _config.MarginTop > 0 ? _config.MarginTop : 2;

            float marginBottom = _config.MarginBottom > 0 ? _config.MarginBottom : 0;

            root.style.marginTop = marginTop;
            root.style.marginBottom = marginBottom;

            // sub container has some space left
            if(_groupPath.Contains('/') && _eLayout.HasFlag(ELayout.Background) && _eLayout.HasFlag(ELayout.Title) && _eLayout.HasFlag(ELayout.TitleOut))
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
                root.RegisterCallback<AttachToPanelEvent>(_ => StartToCheckOutOfScoopFoldout(root));
            }

            if(_toggleCheckInfos.Count > 0)
            {
                root.schedule.Execute(() => LoopCheckTogglesUIToolkit(_toggleCheckInfos, root, body, _containerObject)).Every(150);
            }

            return root;
        }

        private static void LoopCheckTogglesUIToolkit(IReadOnlyList<ToggleCheckInfo> toggleCheckInfos, VisualElement root, VisualElement body, object target)
        {
            foreach (ToggleCheckInfo toggleCheckInfo in toggleCheckInfos)
            {
                SaintsEditorUtils.FillResult(toggleCheckInfo);
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

        private readonly HashSet<Toggle> _processedToggles = new HashSet<Toggle>();

        private void StartToCheckOutOfScoopFoldout(VisualElement root)
        {
            root.schedule
                .Execute(() => LoopCheckOutOfScoopFoldout(root, 0))
                .StartingIn(100);
        }

        private void LoopCheckOutOfScoopFoldout(VisualElement root, int timeout)
        {
            if (timeout >= 3000)
            {
                return;
            }

            foreach (VisualElement actualFieldContainer in root.Query<VisualElement>(className: AbsRenderer.ClassSaintsFieldPlayaContainer).ToList())
            {
                // Debug.Log(actualFieldContainer);
                List<Foldout> foldouts = actualFieldContainer.Query<Foldout>().ToList();
                if (foldouts.Count == 0)
                {
                    continue;
                }

                // Debug.Log(root.worldBound.x);

                foreach (Foldout foldout in foldouts)
                {
                    // this class name is not consistent in different UI Toolkit versions. So just remove it...
                    // Toggle toggle = actualFieldContainer.Q<Toggle>(className: "unity-foldout__toggle--inspector");
                    Toggle toggle = actualFieldContainer.Q<Toggle>();
                    if (toggle == null)
                    {
                        continue;
                    }

                    if (double.IsNaN(toggle.resolvedStyle.width))
                    {
                        continue;
                    }

                    if (!_processedToggles.Add(toggle))  // already processed
                    {
                        continue;
                    }

                    float distance = toggle.worldBound.x - root.worldBound.x;
                    if(distance < 0)
                    {
                        // Debug.Log($"process {toggle.worldBound.x} - {root.worldBound.x}: {distance}");
                        float marginLeft = -distance + 4;
                        // VisualElement saintsParent = UIToolkitUtils.FindParentClass(foldout, SaintsPropertyDrawer.ClassLabelFieldUIToolkit)
                        //     .FirstOrDefault();
                        // if(saintsParent == null)
                        // {
                        //     Debug.Log(foldout);
                        //     foldout.style.marginLeft = marginLeft;
                        // }
                        // else
                        // {
                        //     float ml = saintsParent.resolvedStyle.marginLeft;
                        //     float useValue = double.IsNaN(ml) ? marginLeft : marginLeft + ml;
                        //     saintsParent.style.marginLeft = useValue;
                        // }
                        VisualElement propertyParent = UIToolkitUtils.IterUpWithSelf(foldout).Skip(1).FirstOrDefault(each => each is PropertyField);
                        if (propertyParent != null)
                        {
                            propertyParent.style.marginLeft = marginLeft;
                        }
                    }
                }
            }

            root.schedule.Execute(() =>
            {
                LoopCheckOutOfScoopFoldout(root, timeout + 300);
            }).StartingIn(300);
        }
    }
}
#endif
