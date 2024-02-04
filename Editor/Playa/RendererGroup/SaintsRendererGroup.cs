using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

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
    }
}
