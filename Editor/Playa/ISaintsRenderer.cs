using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa
{
    public interface ISaintsRenderer
    {
        bool InDirectHorizontalLayout { get; set; }
        bool InAnyHorizontalLayout { get; set; }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateVisualElement();
#endif
        void RenderIMGUI(float width);

        float GetHeightIMGUI(float width);

        void RenderPositionIMGUI(Rect position);

        void OnDestroy();
    }
}
