using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa
{
    public interface ISaintsRenderer
    {
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateVisualElement();
#endif

        void Render();

        float GetHeightIMGUI(float width);

        void RenderPosition(Rect position);

        void OnDestroy();
    }
}
