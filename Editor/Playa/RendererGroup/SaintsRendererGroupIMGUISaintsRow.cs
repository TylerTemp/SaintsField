using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public partial class SaintsRendererGroup
    {
        public float GetHeightIMGUI(float width)
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
                    contentHeight += renderer.GetHeightIMGUI(width);
                }
            }

            float marginTop = _config.MarginTop >= 0 ? _config.MarginTop : 2;
            float marginBottom = _config.MarginBottom >= 0 ? _config.MarginBottom : 2;

            return titleHeight + contentHeight + marginTop + marginBottom + 4;
        }

        public void RenderPositionIMGUI(Rect position)
        {
            float marginTop = _config.MarginTop >= 0 ? _config.MarginTop : 2;
            float marginBottom = _config.MarginBottom >= 0 ? _config.MarginBottom : 2;

            Rect marginedRect = new Rect(position)
            {
                y = position.y + marginTop + 2,
                height = position.height - marginTop - marginBottom - 4,
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

            if (_eLayout.HasFlag(ELayout.Background) || _eLayout.HasFlag(ELayout.Tab))
            {
                GUI.Box(marginedRect, GUIContent.none, EditorStyles.helpBox);
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
                    bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout) || _eLayout.HasFlag(ELayout.Collapse);
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
                        bool fancy = _eLayout.HasFlag(ELayout.TitleOut) && _eLayout.HasFlag(ELayout.Background);
                        if (fancy) // title clickable foldout
                        {
                            titleRect.height = EditorGUIUtility.singleLineHeight;

                            if (GUI.Button(titleRect, _groupPath.Split('/').Last(), GetFancyBoxLeftIconButtonStyle()))
                            {
                                _foldout = !_foldout;
                            }

                            Rect iconRect = new Rect(titleRect)
                            {
                                width = 16,
                            };

                            (Texture2D dropdownIcon, Texture2D dropdownRightIcon) = GetDropdownIcons();
                            Texture2D icon = _foldout ? dropdownIcon : dropdownRightIcon;
                            GUI.DrawTexture(iconRect, icon);

                            titleRect.y += titleRect.height;
                            titleUsedHeight += titleRect.height;
                        }
                        else
                        {
                            titleRect.height = EditorGUIUtility.singleLineHeight;
                            _foldout = EditorGUI.Foldout(titleRect, _foldout, _groupPath.Split('/').Last(), true,
                                new GUIStyle(EditorStyles.foldout)
                                {
                                    fontStyle = FontStyle.Bold,
                                });
                            titleRect.y += titleRect.height;
                            titleUsedHeight += titleRect.height;

                            if (_eLayout.HasFlag(ELayout.TitleOut) && _foldout)
                            {
                                titleRect.height = 1;
                                EditorGUI.DrawRect(titleRect, EColor.EditorSeparator.GetColor());
                                titleRect.y += titleRect.height;
                                titleUsedHeight += titleRect.height;
                            }
                        }
                    }

                    // foldout-tabs:
                    //  v      | x     | v   | x           | [f] tab
                    if(hasFoldout && !hasTitle && hasTab)
                    {
                        // using (new EditorGUILayout.HorizontalScope())
                        // {

                        titleRect.height = EditorGUIUtility.singleLineHeight;

                        List<GUIContent> tabsGuiContent = new List<GUIContent>
                        {
                            new GUIContent
                            {
                                image = _foldout
                                    ? GetDropdownIcons().dropdownIcon
                                    : GetDropdownIcons().dropdownRightIcon,
                            },
                        };
                        tabsGuiContent.AddRange(_orderedKeys.Select(each => new GUIContent(each)));

                        using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                        {
                            int curSelected = GUI.Toolbar(titleRect, _foldout ? (_curSelected + 1) : -1, tabsGuiContent.ToArray());
                            if (changed.changed)
                            {
                                if (curSelected == 0)
                                {
                                    _foldout = !_foldout;
                                }
                                else
                                {
                                    _curSelected = curSelected - 1;
                                    _foldout = true;
                                }
                            }
                        }

                        titleUsedHeight += titleRect.height;
                        titleRect.y += titleRect.height;

                        // titleRect.height = EditorGUIUtility.singleLineHeight;
                        // _foldout = EditorGUI.Foldout(titleRect, _foldout, GUIContent.none, true, _foldoutSmallStyle);
                        // titleRect.y += titleRect.height;
                        // titleUsedHeight += titleRect.height;
                        //
                        // using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                        // {
                        //     titleRect.height = EditorGUIUtility.singleLineHeight;
                        //     _curSelected = GUI.Toolbar(titleRect, _curSelected, _orderedKeys.ToArray());
                        //     titleRect.y += titleRect.height;
                        //     titleUsedHeight += titleRect.height;
                        //     if (changed.changed)
                        //     {
                        //         _foldout = true;
                        //     }
                        // }
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
                        float height = renderer.GetHeightIMGUI(position.width);
                        (Rect useRect, Rect leftRect) = RectUtils.SplitHeightRect(accRect, height);
                        Rect marginRect = new Rect(useRect)
                        {
                            x = useRect.x + 2,
                            width = useRect.width - 4,
                        };
                        renderer.RenderPositionIMGUI(marginRect);
                        accRect = leftRect;
                    }

                }
            }
        }
    }
}
