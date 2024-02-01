using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public class HorizontalGroup: ISaintsRendererGroup
    {
        private readonly List<ISaintsRenderer> _renderers = new List<ISaintsRenderer>();

        public HorizontalGroup(ELayout layoutInfoConfig)
        {
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateVisualElement()
        {

        }
#endif

        public void Add(ISaintsRenderer renderer) => _renderers.Add(renderer);

        public void Render()
        {
            Debug.Log($"Now render layout {this}");
            using(new GUILayout.HorizontalScope())
            {
                foreach (ISaintsRenderer renderer in _renderers)
                {
                    renderer.Render();
                }
            }
        }
    }
}
