#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.SaintsEditorWindowUtils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor
{
    public partial class SaintsEditorWindow
    {
        // [NonSerialized] private UnityEngine.Object _editorTargetUIToolkit;
        public void CreateGUI()
        {
            EditorRelinkRootUIToolkit();
            EditorApplication.playModeStateChanged += EditorOnPlayModeStateChangeUIToolkit;
            // _editorChangeTargetEvent.AddListener(EditorChangeTargetUIToolkit);
        }

        // private void EditorChangeTargetUIToolkit(UnityEngine.Object newTarget)
        // {
        //     _editorTargetUIToolkit = newTarget;
        //     EditorRelinkRootUIToolkit();
        // }

        private void EditorOnPlayModeStateChangeUIToolkit(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.EnteredEditMode &&
                stateChange != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            VisualElement root = rootVisualElement;
            if (root == null)
            {
                return;
            }

            // _editorTargetUIToolkit = EditorGetInitTarget(_editorTargetUIToolkit == null? this: _editorTargetUIToolkit);

            EditorRelinkRootUIToolkit();
        }

        private void EditorCleanUpUIToolkit()
        {
            EditorApplication.playModeStateChanged -= EditorOnPlayModeStateChangeUIToolkit;
        }

        private void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            // UnityEngine.Object target = _editorTargetUIToolkit == null ? this : _editorTargetUIToolkit;
            // UnityEngine.Object target = _editorTargetUIToolkit == null ? this : _editorTargetUIToolkit;
            // Debug.Log(target);
            SaintsEditorWindowSpecialEditor editor = (SaintsEditorWindowSpecialEditor)UnityEditor.Editor.CreateEditor(EditorGetTargetInternal(), EditorDrawerType);
            editor.EditorShowMonoScript = EditorShowMonoScript;
            InspectorElement element = new InspectorElement(editor);

            ScrollView sv = new ScrollView();
            sv.Add(element);
            root.Add(sv);

            element.schedule.Execute(EditorOnUpdateInternal).Every(1);
        }
    }
}
#endif
