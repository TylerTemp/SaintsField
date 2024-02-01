using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.RendererGroup
{
    public class VerticalGroup: ISaintsRendererGroup
    {
        private readonly List<ISaintsRenderer> _renderers = new List<ISaintsRenderer>();

        public VerticalGroup(ELayout layoutInfoConfig)
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
            using(new GUILayout.VerticalScope())
            {
                foreach (ISaintsRenderer renderer in _renderers)
                {
                    renderer.Render();
                }
            }
        }
    }
}
