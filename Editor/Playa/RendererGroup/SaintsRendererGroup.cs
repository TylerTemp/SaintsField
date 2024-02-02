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
        private string _curSelected;

        private readonly Dictionary<string, List<ISaintsRenderer>> _groupIdToRenderer =
            new Dictionary<string, List<ISaintsRenderer>>();
        private readonly List<(string groupPath, ISaintsRenderer renderer)> _renderers =
            new List<(string groupPath, ISaintsRenderer renderer)>();

        private readonly List<string> _orderedKeys = new List<string>();  // no OrderedDict can use...

        private readonly string _groupPath;
        private readonly ELayout _eLayout;

        private bool _foldout = true;

        public SaintsRendererGroup(string groupPath, ELayout eLayout)
        {
            _groupPath = groupPath;
            _eLayout = eLayout;
        }

        public void Add(string groupPath, ISaintsRenderer renderer)
        {
            string lastId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);

            if(_curSelected == null)
            {
                _curSelected = lastId;
            }

            if(!_groupIdToRenderer.TryGetValue(lastId, out List<ISaintsRenderer> renderers))
            {
                _groupIdToRenderer[lastId] = renderers = new List<ISaintsRenderer>();
                _orderedKeys.Add(lastId);
            }

            renderers.Add(renderer);
            _renderers.Add((groupPath, renderer));
        }

        public void Render()
        {
            bool hasFoldout = _eLayout.HasFlag(ELayout.Foldout);
            bool hasTitle = _eLayout.HasFlag(ELayout.Title);
            bool hasTab = _eLayout.HasFlag(ELayout.Tab);

            bool drawTitleWithFoldout = hasTitle;

            if (hasFoldout && hasTitle)  // in this case, draw title above, alone
            {
                EditorGUILayout.LabelField(_groupPath.Split('/').Last());
                drawTitleWithFoldout = false;
            }

            using(new EditorGUILayout.HorizontalScope())
            {
                if (hasFoldout)
                {
                    GUIStyle style = new GUIStyle(EditorStyles.foldout)
                    {
                        fixedWidth = 5,
                    };
                    _foldout = EditorGUILayout.Foldout(_foldout, GUIContent.none, style);
                }

                if (drawTitleWithFoldout)
                {
                    EditorGUILayout.LabelField(_groupPath.Split('/').Last());
                    Debug.Assert(!hasTab);
                }
                else
                {
                    foreach (string orderedKey in _orderedKeys)
                    {
                        if (GUILayout.Button(orderedKey))
                        {
                            _curSelected = orderedKey;
                        }
                    }
                }
            }


            if(_foldout)
            {
                IDisposable disposable = _eLayout.HasFlag(ELayout.Horizontal)
                    ? new EditorGUILayout.HorizontalScope()
                    : new EditorGUILayout.VerticalScope();
                using (disposable)
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
                ? _groupIdToRenderer[_curSelected]
                : _renderers.Select(each => each.renderer);
        }
    }
}
