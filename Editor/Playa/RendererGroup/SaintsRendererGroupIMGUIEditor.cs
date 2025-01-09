using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public partial class SaintsRendererGroup
    {
        public void RenderIMGUI()
        {
            bool show = true;
            bool disable = false;
            if(_toggleCheckInfos.Count > 0)
            {
                foreach (ToggleCheckInfo toggleCheckInfo in _toggleCheckInfos)
                {
                    SaintsEditorUtils.FillResult(toggleCheckInfo);
                }

                (bool show, bool disable) r = SaintsEditorUtils.GetToggleResult(_toggleCheckInfos);
                show = r.show;
                disable = r.disable;
            }

            if (!show)
            {
                return;
            }

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

            float marginTop = _config.MarginTop >= 0 ? _config.MarginTop : 2;
            try
            {
                EditorGUILayout.GetControlRect(false, marginTop);
            }
            catch (ArgumentException)
            {
                return;
            }

            GUIStyle fullBoxStyle = (_eLayout.HasFlag(ELayout.Background) || _eLayout.HasFlag(ELayout.Tab))
                ? new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(EditorStyles.helpBox.padding.left, EditorStyles.helpBox.padding.right + 4, EditorStyles.helpBox.padding.top, EditorStyles.helpBox.padding.bottom),
                }
                : GUIStyle.none;
            IDisposable disposable = _eLayout.HasFlag(ELayout.Horizontal)
                // ReSharper disable once RedundantCast
                ? (IDisposable)new EditorGUILayout.HorizontalScope(fullBoxStyle)
                : new EditorGUILayout.VerticalScope(fullBoxStyle);

            using (new EditorGUI.DisabledScope(disable))
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
                        bool fancy = _eLayout.HasFlag(ELayout.TitleOut) && _eLayout.HasFlag(ELayout.Background);
                        if (fancy) // title clickable foldout
                        {
                            // var oriColor = GUI.color;
                            // GUI.color = Color.clear;
                            if (GUILayout.Button(_groupPath.Split('/').Last(), GetFancyBoxLeftIconButtonStyle(), GUILayout.ExpandWidth(true)))
                            {
                                _foldout = !_foldout;
                            }

                            Rect iconRect = GUILayoutUtility.GetLastRect();
                            iconRect.width = 16;

                            (Texture2D dropdownIcon, Texture2D dropdownRightIcon) = GetDropdownIcons();
                            Texture2D icon = _foldout ? dropdownIcon : dropdownRightIcon;
                            GUI.DrawTexture(iconRect, icon);

                            // GUI.color = oriColor;
                            // titleRect.height = EditorGUIUtility.singleLineHeight;
                        }
                        else
                        {
                            _foldout = EditorGUILayout.Foldout(_foldout, _groupPath.Split('/').Last(), true,
                                new GUIStyle(EditorStyles.foldout)
                                {
                                    fontStyle = FontStyle.Bold,
                                });
                            if (_eLayout.HasFlag(ELayout.TitleOut) && _foldout)
                            {
                                Rect lineSep = EditorGUILayout.GetControlRect(false, 1);
                                EditorGUI.DrawRect(lineSep, EColor.EditorSeparator.GetColor());
                            }
                        }
                    }

                    // foldout-tabs:
                    //  v      | x     | v   | x           | [f] tab
                    if(hasFoldout && !hasTitle && hasTab)
                    {
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

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                            {
                                int curSelected = GUILayout.Toolbar(_foldout ? (_curSelected + 1) : -1, tabsGuiContent.ToArray(), GUILayout.MaxHeight(18));
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
                    using(new SaintsRendererGroupIMGUINeedIndentFixScoop(NeedIndentCheck(_eLayout)))
                    {
                        foreach (ISaintsRenderer renderer in GetRenderer())
                        {
                            renderer.RenderIMGUI();
                        }
                    }

                }
            }

            float marginBottom = _config.MarginBottom >= 0 ? _config.MarginBottom : 2;
            EditorGUILayout.GetControlRect(false, marginBottom);
        }

    }
}
