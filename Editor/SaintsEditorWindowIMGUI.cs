using System;
using SaintsField.Editor.Playa.SaintsEditorWindowUtils;
using UnityEditor;

namespace SaintsField.Editor
{
    public partial class SaintsEditorWindow
    {
        [NonSerialized] private SaintsEditorWindowSpecialEditor _saintsEditorWindowSpecialEditor;

        public void OnGUI () {
#if !(UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE)
            if(_saintsEditorWindowSpecialEditor == null)
            {
                // Debug.Log("Create Editor for IMGUI");
                _saintsEditorWindowSpecialEditor = (SaintsEditorWindowSpecialEditor)UnityEditor.Editor.CreateEditor(EditorGetTargetInternal(),
                    EditorDrawerType);
                _saintsEditorWindowSpecialEditor.EditorShowMonoScript = EditorShowMonoScript;
            }
            _saintsEditorWindowSpecialEditor.OnInspectorGUI();
            EditorOnUpdateInternal();
            EditorApplication.delayCall += Repaint;
#endif
        }

        private void EditorCleanUpIMGUI()
        {
            EditorApplication.delayCall -= Repaint;
            if (_saintsEditorWindowSpecialEditor != null)
            {
                DestroyImmediate(_saintsEditorWindowSpecialEditor);
                _saintsEditorWindowSpecialEditor = null;
            }
        }
    }
}
