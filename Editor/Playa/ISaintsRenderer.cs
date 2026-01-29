using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa
{
    public interface ISaintsRenderer
    {
#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        VisualElement CreateVisualElement(VisualElement inspectorRoot);
#endif
        bool InAnyHorizontalLayout { get; set; }
        bool InDirectHorizontalLayout { get; set; }

        void RenderIMGUI(float width);

        float GetHeightIMGUI(float width);

        void RenderPositionIMGUI(Rect position);

        void OnDestroy();

        void OnSearchField(string searchString);

        void SetSerializedProperty(SerializedProperty property);
    }
}
