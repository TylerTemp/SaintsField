using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Playa.RendererGroup
{
    public class SaintsRendererGroup: ISaintsRendererGroup
    {
        public class Config
        {
            public ELayout eLayout;
            public bool isDOTween;
            public float marginTop;
            public float marginBottom;
        }

        private int _curSelected;

        private readonly Dictionary<string, List<ISaintsRenderer>> _groupIdToRenderer =
            new Dictionary<string, List<ISaintsRenderer>>();
        private readonly List<(string groupPath, ISaintsRenderer renderer)> _renderers =
            new List<(string groupPath, ISaintsRenderer renderer)>();

        private readonly List<string> _orderedKeys = new List<string>();  // no OrderedDict can use...

        private readonly string _groupPath;
        private readonly ELayout _eLayout;
        private readonly Config _config;

        private GUIStyle _foldoutSmallStyle;
        private GUIStyle _titleLabelStyle;

        private bool _foldout;

        public SaintsRendererGroup(string groupPath, Config config)
        {
            _groupPath = groupPath;
            _config = config;
            _eLayout = config.eLayout;
            _foldout = !config.eLayout.HasFlag(ELayout.Collapse);
        }

        public void Add(string groupPath, ISaintsRenderer renderer)
        {
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string lastId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            // if(_curSelected == null)
            // {
            //     _curSelected = lastId;
            // }

            if(!_groupIdToRenderer.TryGetValue(lastId, out List<ISaintsRenderer> renderers))
            {
                _groupIdToRenderer[lastId] = renderers = new List<ISaintsRenderer>();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                Debug.Log($"Add Key: {lastId} of {groupPath}");
#endif
                _orderedKeys.Add(lastId);
            }

            renderers.Add(renderer);
            _renderers.Add((groupPath, renderer));
        }

        public void OnDestroy()
        {
            foreach ((string _, ISaintsRenderer renderer) in _renderers)
            {
                renderer.OnDestroy();
            }
        }

        #region IMGUI


        public void Render()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_foldoutSmallStyle == null) {
                _foldoutSmallStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fixedWidth = 5,
                };
            }

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_titleLabelStyle == null)
            {
                _titleLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    // alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };
            }

            float marginTop = _config.marginTop >= 0 ? _config.marginTop : 2;
            EditorGUILayout.GetControlRect(false, marginTop);

            GUIStyle fullBoxStyle = _eLayout.HasFlag(ELayout.Background)
                ? GUI.skin.box
                : GUIStyle.none;
            IDisposable disposable = _eLayout.HasFlag(ELayout.Horizontal)
                // ReSharper disable once RedundantCast
                ? (IDisposable)new EditorGUILayout.HorizontalScope(fullBoxStyle)
                : new EditorGUILayout.VerticalScope(fullBoxStyle);

            using (disposable)
            {
                #region Title

                using (new EditorGUILayout.VerticalScope())
                {
                    bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout) || _eLayout.HasFlag(ELayout.Collapse);
                    bool hasTitle = _eLayout.HasFlag(ELayout.Title) || _eLayout.HasFlag(ELayout.TitleOut);
                    bool hasTab = _eLayout.HasFlag(ELayout.Tab);

                    // this looks better:
                    // foldout | title | tab | style:title | style: tab
                    // --------|-------|-----|-------------|------------
                    //  v      | v     | v   | [f] title   | tab
                    //  v      | v     | x   | [f] title   | x
                    //  v      | x     | v   | x           | [f] tab
                    //  v      | x     | x   | [f] title   | x
                    //  x      | v     | v   | title       | tab
                    //  x      | v     | v   | title       | tab
                    //  x      | v     | x   | title       | x
                    //  x      | x     | v   | x           | tab
                    //  x      | x     | x   | x           | x

                    // line-title:
                    //  x      | v     | v   | title       | tab
                    //  x      | v     | x   | title       | x
                    if (!hasFoldout && hasTitle)  // in this case, draw title above, alone
                    {
                        EditorGUILayout.LabelField(_groupPath.Split('/').Last(), _titleLabelStyle);
                        if(_eLayout.HasFlag(ELayout.TitleOut))
                        {
                            Rect lineSep = EditorGUILayout.GetControlRect(false, 1);
                            EditorGUI.DrawRect(lineSep, EColor.EditorSeparator.GetColor());
                        }
                        // EditorGUILayout.LabelField(_groupPath.Split('/').Last(), _centerLabelStyle);
                        // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }

                    // foldout-title:
                    //  v      | v     | v   | [f] title   | tab
                    //  v      | v     | x   | [f] title   | x
                    //  v      | x     | x   | [f] title   | x
                    if (hasFoldout && (
                            (hasTitle && hasTab)
                            || hasTitle
                            || !hasTab))
                    {
                        _foldout = EditorGUILayout.Foldout(_foldout, _groupPath.Split('/').Last(), true, new GUIStyle(EditorStyles.foldout){
                            fontStyle = FontStyle.Bold,
                        });
                        if(_eLayout.HasFlag(ELayout.TitleOut) && _foldout)
                        {
                            Rect lineSep = EditorGUILayout.GetControlRect(false, 1);
                            EditorGUI.DrawRect(lineSep, EColor.EditorSeparator.GetColor());
                        }
                    }

                    // foldout-tabs:
                    //  v      | x     | v   | x           | [f] tab
                    if(hasFoldout && !hasTitle && hasTab)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            _foldout = EditorGUILayout.Foldout(_foldout, GUIContent.none, true, _foldoutSmallStyle);
                            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                            {
                                var curSelected = GUILayout.Toolbar(_foldout ? _curSelected : -1, _orderedKeys.ToArray());
                                if (changed.changed)
                                {
                                    _foldout = true;
                                }

                                if (curSelected != -1)
                                {
                                    _curSelected = curSelected;
                                }
                            }
                        }
                    }

                    // tabs
                    //  x      | v     | v   | title       | tab
                    //  v      | v     | v   | [f] title   | tab
                    //  x      | v     | v   | title       | tab
                    //  x      | x     | v   | x           | tab
                    if((!hasFoldout && hasTitle && hasTab) || (hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
                    {
                        // Debug.Log($"Draw tabs, hasFoldout={hasFoldout}, _foldout={_foldout}");
                        if((hasFoldout && _foldout) || !hasFoldout)
                        {
                            // Debug.Log($"Draw tabs, all = {string.Join(",", _orderedKeys)}");
                            _curSelected = GUILayout.Toolbar(_curSelected, _orderedKeys.ToArray());
                        }
                    }
                }

                #endregion

                if(_foldout)
                {
                    foreach (ISaintsRenderer renderer in GetRenderer())
                    {
                        renderer.Render();
                    }

                }
            }

            float marginBottom = _config.marginBottom >= 0 ? _config.marginBottom : 2;
            EditorGUILayout.GetControlRect(false, marginBottom);
        }

        private IEnumerable<ISaintsRenderer> GetRenderer()
        {
            return _eLayout.HasFlag(ELayout.Tab)
                ? _groupIdToRenderer[_orderedKeys[_curSelected]]
                : _renderers.Select(each => each.renderer);
        }

        public float GetHeight()
        {
            float titleHeight = 0f;

            bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout);
            bool hasTitle = _eLayout.HasFlag(ELayout.Title);
            bool hasTab = _eLayout.HasFlag(ELayout.Tab);

            if (!hasFoldout && hasTitle)  // in this case, draw title above, alone
            {
                titleHeight += EditorGUIUtility.singleLineHeight;
                // EditorGUILayout.LabelField(_groupPath.Split('/').Last(), _titleLabelStyle);
                if(_eLayout.HasFlag(ELayout.TitleOut))
                {
                    titleHeight += 1;
                    // Rect lineSep = EditorGUILayout.GetControlRect(false, 1);
                    // EditorGUI.DrawRect(lineSep, EColor.EditorSeparator.GetColor());
                }
            }

            if (hasFoldout && (
                    (hasTitle && hasTab)
                    || hasTitle
                    || !hasTab))
            {
                titleHeight += EditorGUIUtility.singleLineHeight;
                // _foldout = EditorGUILayout.Foldout(_foldout, _groupPath.Split('/').Last(), true, new GUIStyle(EditorStyles.foldout){
                //     fontStyle = FontStyle.Bold,
                // });
                if(_eLayout.HasFlag(ELayout.TitleOut) && _foldout)
                {
                    titleHeight += 1f;
                    // Rect lineSep = EditorGUILayout.GetControlRect(false, 1);
                    // EditorGUI.DrawRect(lineSep, EColor.EditorSeparator.GetColor());
                }
            }

            // foldout-tabs:
            //  v      | x     | v   | x           | [f] tab
            if(hasFoldout && !hasTitle && hasTab)
            {
                titleHeight += EditorGUIUtility.singleLineHeight;
                // using (new EditorGUILayout.HorizontalScope())
                // {
                //     _foldout = EditorGUILayout.Foldout(_foldout, GUIContent.none, true, _foldoutSmallStyle);
                //     using(var changed = new EditorGUI.ChangeCheckScope())
                //     {
                //         _curSelected = GUILayout.Toolbar(_curSelected, _orderedKeys.ToArray());
                //         if (changed.changed)
                //         {
                //             _foldout = true;
                //         }
                //     }
                // }
            }

            // tabs
            //  x      | v     | v   | title       | tab
            //  v      | v     | v   | [f] title   | tab
            //  x      | v     | v   | title       | tab
            //  x      | x     | v   | x           | tab
            if((!hasFoldout && hasTitle && hasTab) || (hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
            {
                // Debug.Log($"Draw tabs, hasFoldout={hasFoldout}, _foldout={_foldout}");
                if((hasFoldout && _foldout) || !hasFoldout)
                {
                    titleHeight += EditorGUIUtility.singleLineHeight;
                    // _curSelected = GUILayout.Toolbar(_curSelected, _orderedKeys.ToArray());
                }
            }

            float contentHeight = 0f;
            if(_foldout)
            {
                foreach (ISaintsRenderer renderer in GetRenderer())
                {
                    contentHeight += renderer.GetHeight();
                }
            }

            float marginTop = _config.marginTop >= 0 ? _config.marginTop : 2;
            float marginBottom = _config.marginBottom >= 0 ? _config.marginBottom : 2;

            return titleHeight + contentHeight + marginTop + marginBottom;
        }

        public void RenderPosition(Rect position)
        {
            float marginTop = _config.marginTop >= 0 ? _config.marginTop : 2;
            float marginBottom = _config.marginBottom >= 0 ? _config.marginBottom : 2;

            Rect marginedRect = new Rect(position)
            {
                y = position.y + marginTop,
                height = position.height - marginTop - marginBottom,
            };

            Debug.Assert(!_eLayout.HasFlag(ELayout.Horizontal), $"Horizontal is not supported for IMGUI in SaintsEditorAttribute mode");

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_foldoutSmallStyle == null) {
                _foldoutSmallStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fixedWidth = 5,
                };
            }

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_titleLabelStyle == null)
            {
                _titleLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    // alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };
            }

            if (_eLayout.HasFlag(ELayout.Background))
            {
                GUI.Box(marginedRect, GUIContent.none);
            }

            // GUIStyle fullBoxStyle = _eLayout.HasFlag(ELayout.Background)
            //     ? GUI.skin.box
            //     : GUIStyle.none;
            // IDisposable disposable = _eLayout.HasFlag(ELayout.Horizontal)
            //     // ReSharper disable once RedundantCast
            //     ? (IDisposable)new EditorGUILayout.HorizontalScope(fullBoxStyle)
            //     : new EditorGUILayout.VerticalScope(fullBoxStyle);

            // using (disposable)
            // Rect bodyRect = new Rect(position);
            float titleUsedHeight = 0f;

            {
                #region Title

                // using (new EditorGUILayout.VerticalScope())
                {
                    bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout);
                    bool hasTitle = _eLayout.HasFlag(ELayout.Title);
                    bool hasTab = _eLayout.HasFlag(ELayout.Tab);

                    Rect titleRect = new Rect(marginedRect)
                    {
                        height = 0,
                    };

                    // this looks better:
                    // foldout | title | tab | style:title | style: tab
                    // --------|-------|-----|-------------|------------
                    //  v      | v     | v   | [f] title   | tab
                    //  v      | v     | x   | [f] title   | x
                    //  v      | x     | v   | x           | [f] tab
                    //  v      | x     | x   | [f] title   | x
                    //  x      | v     | v   | title       | tab
                    //  x      | v     | v   | title       | tab
                    //  x      | v     | x   | title       | x
                    //  x      | x     | v   | x           | tab
                    //  x      | x     | x   | x           | x

                    // line-title:
                    //  x      | v     | v   | title       | tab
                    //  x      | v     | x   | title       | x
                    if (!hasFoldout && hasTitle)  // in this case, draw title above, alone
                    {
                        titleRect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(titleRect, _groupPath.Split('/').Last(), _titleLabelStyle);
                        titleRect.y += titleRect.height;
                        titleUsedHeight += titleRect.height;

                        if(_eLayout.HasFlag(ELayout.TitleOut))
                        {
                            titleRect.height = 1;
                            EditorGUI.DrawRect(titleRect, EColor.EditorSeparator.GetColor());
                            titleRect.y += titleRect.height;
                            titleUsedHeight += titleRect.height;
                        }
                        // EditorGUILayout.LabelField(_groupPath.Split('/').Last(), _centerLabelStyle);
                        // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }

                    // foldout-title:
                    //  v      | v     | v   | [f] title   | tab
                    //  v      | v     | x   | [f] title   | x
                    //  v      | x     | x   | [f] title   | x
                    if (hasFoldout && (
                            (hasTitle && hasTab)
                            || hasTitle
                            || !hasTab))
                    {
                        titleRect.height = EditorGUIUtility.singleLineHeight;
                        _foldout = EditorGUI.Foldout(titleRect, _foldout, _groupPath.Split('/').Last(), true, new GUIStyle(EditorStyles.foldout){
                            fontStyle = FontStyle.Bold,
                        });
                        titleRect.y += titleRect.height;
                        titleUsedHeight += titleRect.height;

                        if(_eLayout.HasFlag(ELayout.TitleOut) && _foldout)
                        {
                            titleRect.height = 1;
                            EditorGUI.DrawRect(titleRect, EColor.EditorSeparator.GetColor());
                            titleRect.y += titleRect.height;
                            titleUsedHeight += titleRect.height;
                        }
                    }

                    // foldout-tabs:
                    //  v      | x     | v   | x           | [f] tab
                    if(hasFoldout && !hasTitle && hasTab)
                    {
                        // using (new EditorGUILayout.HorizontalScope())
                        {
                            titleRect.height = EditorGUIUtility.singleLineHeight;
                            _foldout = EditorGUI.Foldout(titleRect, _foldout, GUIContent.none, true, _foldoutSmallStyle);
                            titleRect.y += titleRect.height;
                            titleUsedHeight += titleRect.height;

                            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                            {
                                titleRect.height = EditorGUIUtility.singleLineHeight;
                                _curSelected = GUI.Toolbar(titleRect, _curSelected, _orderedKeys.ToArray());
                                titleRect.y += titleRect.height;
                                titleUsedHeight += titleRect.height;
                                if (changed.changed)
                                {
                                    _foldout = true;
                                }
                            }
                        }
                    }

                    // tabs
                    //  x      | v     | v   | title       | tab
                    //  v      | v     | v   | [f] title   | tab
                    //  x      | v     | v   | title       | tab
                    //  x      | x     | v   | x           | tab
                    if((!hasFoldout && hasTitle && hasTab) || (hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
                    {
                        // Debug.Log($"Draw tabs, hasFoldout={hasFoldout}, _foldout={_foldout}");
                        if((hasFoldout && _foldout) || !hasFoldout)
                        {
                            // Debug.Log($"Draw tabs, all = {string.Join(",", _orderedKeys)}");
                            titleRect.height = EditorGUIUtility.singleLineHeight;
                            _curSelected = GUI.Toolbar(titleRect, _curSelected, _orderedKeys.ToArray());
                            titleRect.y += titleRect.height;
                            titleUsedHeight += titleRect.height;
                        }
                    }
                }

                #endregion

                if(_foldout)
                {
                    // Rect bodyRect = new Rect(marginedRect)
                    // {
                    //     y = marginedRect.y + titleUsedHeight,
                    //     height = marginedRect.height - titleUsedHeight,
                    // };
                    Rect bodyRect = RectUtils.SplitHeightRect(marginedRect, titleUsedHeight).leftRect;

                    Rect accRect = bodyRect;
                    foreach (ISaintsRenderer renderer in GetRenderer())
                    {
                        float height = renderer.GetHeight();
                        (Rect useRect, Rect leftRect) = RectUtils.SplitHeightRect(accRect, height);
                        renderer.RenderPosition(useRect);
                        accRect = leftRect;
                    }

                }
            }
        }

        #endregion

        #region UIToolkit

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

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
                            paddingLeft = 5,
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

                            alignItems = Align.FlexEnd,
                        },
                    };
                    Image foldoutImage = new Image
                    {
                        image = _foldout? dropdownIcon: dropdownRightIcon,
                        style =
                        {
                            width = 16,
                            height = 16,
                        },
                    };
                    foldoutImage.transform.scale = new Vector3(-1, 1, 1);

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
                Debug.Log($"add item@{groupPath}: {renderer}");
#endif
                VisualElement fieldElement = renderer.CreateVisualElement();
                // if(_eLayout.HasFlag(ELayout.Background))
                // {
                //     fieldElement.style.marginRight = 4;
                // }

                if(!fieldToVisualElement.TryGetValue(groupId, out List<VisualElement> visualElements))
                {
                    fieldToVisualElement[groupId] = visualElements = new List<VisualElement>();
                }
                visualElements.Add(fieldElement);
                body.Add(fieldElement);
            }

            if(_eLayout.HasFlag(ELayout.Background))
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

            float marginTop = _config.marginTop > 0 ? _config.marginTop : 2;
            float marginBottom = _config.marginBottom > 0 ? _config.marginBottom : 0;

            root.style.marginTop = marginTop;
            root.style.marginBottom = marginBottom;

            // root.RegisterCallback<DetachFromPanelEvent>(_ =>
            // {
            //     UnityEngine.Object.DestroyImmediate(dropdownIcon);
            //     UnityEngine.Object.DestroyImmediate(dropdownRightIcon);
            // });

            return root;
        }

#endif
        #endregion

    }
}
