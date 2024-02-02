using System.Collections.Generic;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public class TabGroup: VerticalGroup
    {
        private string _curSelected;

        private readonly Dictionary<string, List<ISaintsRenderer>> _groupIdToRenderer =
            new Dictionary<string, List<ISaintsRenderer>>();

        private readonly List<string> _orderedKeys = new List<string>();  // no OrderedDict can use...

        public TabGroup(ELayout layoutInfoConfig) : base(layoutInfoConfig)
        {
        }

        public override void Add(string groupPath, ISaintsRenderer renderer)
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
        }

        public override void Render()
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                foreach (string orderedKey in _orderedKeys)
                {
                    if (GUILayout.Button(orderedKey))
                    {
                        _curSelected = orderedKey;
                    }
                }
            }

            base.Render();
        }

        protected override IEnumerable<ISaintsRenderer> GetRenderer() => _groupIdToRenderer[_curSelected];
    }
}
