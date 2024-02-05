using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public class SaintsRendererGroup: ISaintsRendererGroup
    {
        private int _curSelected;

        private readonly Dictionary<string, List<ISaintsRenderer>> _groupIdToRenderer =
            new Dictionary<string, List<ISaintsRenderer>>();
        private readonly List<(string groupPath, ISaintsRenderer renderer)> _renderers =
            new List<(string groupPath, ISaintsRenderer renderer)>();

        private readonly List<string> _orderedKeys = new List<string>();  // no OrderedDict can use...

        private readonly string _groupPath;
        private readonly ELayout _eLayout;

        private GUIStyle _foldoutSmallStyle;
        private GUIStyle _titleLabelStyle;

        private bool _foldout = true;

        public SaintsRendererGroup(string groupPath, ELayout eLayout)
        {
            _groupPath = groupPath;
            _eLayout = eLayout;
        }

        public void Add(string groupPath, ISaintsRenderer renderer)
        {
            string lastId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            // if(_curSelected == null)
            // {
            //     _curSelected = lastId;
            // }

            if(!_groupIdToRenderer.TryGetValue(lastId, out List<ISaintsRenderer> renderers))
            {
                _groupIdToRenderer[lastId] = renderers = new List<ISaintsRenderer>();
                Debug.Log($"Add Key: {lastId} of {groupPath}");
                _orderedKeys.Add(lastId);
            }

            renderers.Add(renderer);
            _renderers.Add((groupPath, renderer));
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

            if(_titleLabelStyle == null)
            {
                _titleLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    // alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };
            }

            GUIStyle fullBoxStyle = _eLayout.HasFlag(ELayout.Background)
                ? GUI.skin.box
                : GUIStyle.none;
            IDisposable disposable = _eLayout.HasFlag(ELayout.Horizontal)
                ? new EditorGUILayout.HorizontalScope(fullBoxStyle)
                : new EditorGUILayout.VerticalScope(fullBoxStyle);

            using (disposable)
            {
                #region Title

                using (new EditorGUILayout.VerticalScope())
                {
                    bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout);
                    bool hasTitle = _eLayout.HasFlag(ELayout.Title);
                    bool hasTab = _eLayout.HasFlag(ELayout.Tab);

                    // this looks better:
                    // foldout | title | tab | style:title | style: tab
                    // --------|-------|-----|-------------|------------
                    //  v      | v     | v   | [f] title   | tab
                    //  v      | v     | x   | [f] title   | x
                    //  v      | x     | v   | x           | [f] tab
                    //  v      | x     | x   | [f] title   | x
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
                        if(_eLayout.HasFlag(ELayout.TitleOutstanding))
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
                        if(_eLayout.HasFlag(ELayout.TitleOutstanding) && _foldout)
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
                            _curSelected = GUILayout.Toolbar(_curSelected, _orderedKeys.ToArray());
                        }
                    }

                    // tabs
                    //  v      | v     | v   | [f] title   | tab
                    //  x      | v     | v   | title       | tab
                    //  x      | x     | v   | x           | tab
                    if((hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
                    {
                        if(hasFoldout && _foldout)
                        {
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
        }

        private IEnumerable<ISaintsRenderer> GetRenderer()
        {
            return _eLayout.HasFlag(ELayout.Tab)
                ? _groupIdToRenderer[_orderedKeys[_curSelected]]
                : _renderers.Select(each => each.renderer);
        }

        #endregion

        #region UIToolkit

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        public VisualElement CreateVisualElement()
        {
            Texture2D dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
            Texture2D dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");

            Dictionary<string, VisualElement> fieldToVisualElement = new Dictionary<string, VisualElement>();
            string curTab = null;

            VisualElement titleRow = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout);
            bool hasTitle = _eLayout.HasFlag(ELayout.Title);
            bool hasTab = _eLayout.HasFlag(ELayout.Tab);

            Toolbar toolbar = new Toolbar
            {
                style =
                {
                    flexGrow = 1,
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

            Action<string> switchTab = tab =>
            {
                foreach (ToolbarToggle toolbarToggle in toolbarToggles)
                {
                    toolbarToggle.SetValueWithoutNotify(toolbarToggle.text == tab);
                }

                foreach((string groupPath, VisualElement visualElement) in fieldToVisualElement)
                {
                    bool display = tab == null || groupPath == tab;
                    visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
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

            Action<bool> foldoutAction = show =>
            {
                if (show)
                {
                    if(hasTitle)
                    {
                        toolbar.style.display = DisplayStyle.Flex;
                    }
                    switchTab(curTab);
                }
                else
                {
                    if(hasTitle)
                    {
                        toolbar.style.display = DisplayStyle.None;
                    }
                    foreach (VisualElement visualElement in fieldToVisualElement.Values)
                    {
                        visualElement.style.display = DisplayStyle.None;
                    }
                }
            };

            if (!hasFoldout && hasTitle)  // in this case, draw title above, alone
            {
                Label title = new Label(_groupPath.Split('/').Last());
                if(_eLayout.HasFlag(ELayout.TitleOutstanding))
                {
                    title.style.backgroundColor = EColor.Gray.GetColor();
                }
                titleRow.Add(title);
            }

            // foldout-title:
            if (hasFoldout && (
                    (hasTitle && hasTab)
                    || hasTitle
                    || !hasTab))
            {
                Foldout foldout = new Foldout
                {
                    text = _groupPath.Split('/').Last(),
                };
                if(_eLayout.HasFlag(ELayout.TitleOutstanding))
                {
                    foldout.style.backgroundColor = new Color(53f/255, 53f/255, 53f/255, 1f);
                    foldout.style.borderTopLeftRadius = foldout.style.borderTopRightRadius = 3;
                }
                foldout.RegisterValueChangedCallback(evt => foldoutAction(evt.newValue));

                titleRow.Add(foldout);
            }

            // foldout-tabs:
            if(hasFoldout && !hasTitle && hasTab)
            {
                // toolbar.Add(foldout);
                ToolbarToggle foldoutToggle = new ToolbarToggle
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
                    foldoutAction(evt.newValue);
                    foldoutImage.image = evt.newValue ? dropdownIcon : dropdownRightIcon;
                });

                toolbar.Add(foldoutToggle);

                foreach (ToolbarToggle eachTab in toolbarToggles)
                {
                    toolbar.Add(eachTab);
                }

                titleRow.Add(toolbar);
            }

            // tabs
            if((hasFoldout && hasTitle && hasTab) || (!hasFoldout && hasTitle && hasTab) | (!hasFoldout && !hasTitle && hasTab))
            {
                // TODO: 2023_3+ these is a TabView can be used

                foreach (ToolbarToggle eachTab in toolbarToggles)
                {
                    toolbar.Add(eachTab);
                }
                titleRow.Add(toolbar);
            }

            VisualElement body = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = _eLayout.HasFlag(ELayout.Horizontal)? FlexDirection.Row :FlexDirection.Column,
                },
            };

            foreach ((string groupPath, ISaintsRenderer renderer) in _renderers)
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                string groupId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);
                VisualElement fieldElement = fieldToVisualElement[groupId] = renderer.CreateVisualElement();
                body.Add(fieldElement);
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            if(_eLayout.HasFlag(ELayout.Background))
            {
                root.style.backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f);
            }

            root.Add(titleRow);
            root.Add(body);

            if (_eLayout.HasFlag(ELayout.Tab))
            {
                // ReSharper disable once ConvertToLocalFunction
                EventCallback<AttachToPanelEvent> switchOnAttack = null;
                switchOnAttack = evt =>
                {
                    root.UnregisterCallback(switchOnAttack);
                    toolbarToggles[0].value = true;
                };
                root.RegisterCallback(switchOnAttack);
            }

            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                UnityEngine.Object.DestroyImmediate(dropdownIcon);
                UnityEngine.Object.DestroyImmediate(dropdownRightIcon);
            });

            return root;
        }

#endif
        #endregion

    }
}
