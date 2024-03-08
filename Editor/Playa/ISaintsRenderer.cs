using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa
{
    public interface ISaintsRenderer
    {
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateVisualElement();
#endif

        void Render();

        public float GetHeight();

        public void RenderPosition(Rect position);
    }
}
