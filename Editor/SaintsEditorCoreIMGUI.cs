using System;
using System.Collections.Generic;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using UnityEditor;

namespace SaintsField.Editor
{
    public partial class SaintsEditorCore
    {
        private IReadOnlyList<ISaintsRenderer> _renderersIMGUI;

        public void OnEnableIMGUI()
        {
            if (!_saintsEditorIMGUI)
            {
                return;
            }

            // Debug.Log($"OnEnable");
            EnsureIMGUI();
        }

        private void EnsureIMGUI()
        {
            if (_renderersIMGUI == null)
            {
                try
                {
                    _renderersIMGUI =
                        SaintsEditor.Setup(Array.Empty<string>(), SerializedObject, GetMakeRender(), Targets);
                }
                catch (Exception)
                {
                    _renderersIMGUI = null; // just... let IMGUI renderer to deal with it...
                }
            }
        }

        public void OnDestroyIMGUI()
        {
            if (_renderersIMGUI != null)
            {
                foreach (ISaintsRenderer renderer in _renderersIMGUI)
                {
                    renderer.OnDestroy();
                }
            }
            _renderersIMGUI = null;
        }

        public void OnInspectorGUI()
        {
            _saintsEditorIMGUI = true;
            EnsureIMGUI();

            DrawHeaderGUI.HelperUpdate();

            // MonoScript monoScript = EditorShowMonoScript? GetMonoScript(target): null;
            if(_editorShowMonoScript)
            {
                MonoScript monoScript = SaintsEditor.GetMonoScript(Targets[0]);
                if (monoScript)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        try
                        {
                            EditorGUILayout.ObjectField("Script", monoScript, GetType(), false);
                        }
                        catch (NullReferenceException)
                        {
                            // ignored
                        }
                    }
                }
            }

            SerializedObject.Update();

            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            foreach (ISaintsRenderer renderer in _renderersIMGUI)
            {
                renderer.RenderIMGUI(UnityEngine.Screen.width);
            }

            if (changed.changed)
            {
                SerializedObject.ApplyModifiedProperties();
            }
        }
    }
}
