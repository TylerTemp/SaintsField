#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor
{
    public partial class SaintsEditorWindow
    {
        public void CreateGUI()
        {
            RelinkRootUIToolkit();
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private void OnPlayModeStateRebindUIToolkit()
        {
            VisualElement root = rootVisualElement;
            if (root == null)
            {
                return;
            }

            RelinkRootUIToolkit();
        }

        private void RelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(this, typeof(SaintsEditorWindowSpecialEditor));
            InspectorElement element = new InspectorElement(editor);
            root.Add(element);

            element.schedule.Execute(OnEditorUpdateInternal).Every(1);
        }
    }
}
#endif
