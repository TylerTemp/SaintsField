using System;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {
        public override bool RequiresConstantRepaint() => true;

        private void OnEnableIMGUI()
        {
            if (!_saintsEditorIMGUI)
            {
                return;
            }

            // Debug.Log($"OnEnable");
            try
            {
                _renderers = Setup(Array.Empty<string>(), serializedObject, this, targets);
            }
            catch (Exception)
            {
                _renderers = null;  // just... let IMGUI renderer to deal with it...
            }
        }

        private void OnDestroyIMGUI()
        {
            if (_renderers != null)
            {
                foreach (ISaintsRenderer renderer in _renderers)
                {
                    renderer.OnDestroy();
                }
            }
            _renderers = null;
        }

        public override void OnInspectorGUI()
        {
            DrawHeaderGUI.HelperUpdate();

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_renderers == null)
            {
                _renderers = Setup(Array.Empty<string>(), serializedObject, this, targets);
            }
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            AliveInstances.Add(this);
#endif

            // MonoScript monoScript = EditorShowMonoScript? GetMonoScript(target): null;
            if(EditorShowMonoScript)
            {
                MonoScript monoScript = GetMonoScript(target);
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

            serializedObject.Update();

            foreach (ISaintsRenderer renderer in _renderers)
            {
                renderer.RenderIMGUI(Screen.width);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
