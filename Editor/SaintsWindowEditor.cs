using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor
{
    public class SaintsWindowEditor<T>: EditorWindow where T: ScriptableObject
    {
        public static void DebugOpenWindow<TW>(bool utility = false, string title = null, bool focus = true) where TW: ScriptableObject
        {
            EditorWindow window = GetWindow(typeof(SaintsWindowEditor<TW>), utility, title, focus);
            window.Show();
        }

        private T _dataSource;

        public void CreateGUI()
        {
            if(_dataSource == null)
            {
                _dataSource = CreateInstance<T>();
            }
            RelinkRoot();

            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private void OnPlayModeStateChange(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.EnteredEditMode &&
                stateChange != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            _dataSource = CreateInstance<T>();
            // RefreshData();

            VisualElement root = rootVisualElement;
            if (root == null)
            {
                return;
            }

            RelinkRoot();
        }

        private void RelinkRoot()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(_dataSource, typeof(SaintsEditor));
            root.Add(new InspectorElement(editor));
        }
    }
}
