using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

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
            throw new NotImplementedException();
        }
#endif

        public void Add(string groupPath, ISaintsRenderer renderer) => _renderers.Add(renderer);

        public void Render()
        {
            Debug.Log($"<--H-->");
            using(new GUILayout.HorizontalScope())
            {
                foreach (ISaintsRenderer renderer in _renderers)
                {
                    Debug.Log($"<r>{renderer}</r>");
                    renderer.Render();
                }
            }
            Debug.Log($"</--H-->");
        }
    }
}
