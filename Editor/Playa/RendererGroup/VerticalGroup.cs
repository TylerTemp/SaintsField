using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEditor;
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
            throw new NotImplementedException();
        }
#endif

        public virtual void Add(string groupPath, ISaintsRenderer renderer) => _renderers.Add(renderer);

        public virtual void Render()
        {
            Debug.Log($"Now render layout {this}");

            using(new EditorGUILayout.VerticalScope())
            {
                foreach (ISaintsRenderer renderer in GetRenderer())
                {
                    renderer.Render();
                }
            }
        }

        protected virtual IEnumerable<ISaintsRenderer> GetRenderer() => _renderers;
    }
}
