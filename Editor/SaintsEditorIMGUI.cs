using System;
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {
        public override bool RequiresConstantRepaint() => true;

        public virtual void OnEnable()
        {
            if (!_saintsEditorIMGUI)
            {
                return;
            }

            // Debug.Log($"OnEnable");
            try
            {
                _renderers = Setup(Array.Empty<string>(), serializedObject, this, target);
            }
            catch (Exception)
            {
                _renderers = null;  // just... let IMGUI renderer to deal with it...
            }
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            AliveInstances.Add(this);
#endif
        }

        public virtual void OnDestroy()
        {
            if (_renderers != null)
            {
                foreach (ISaintsRenderer renderer in _renderers)
                {
                    renderer.OnDestroy();
                }
            }
            _renderers = null;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            RemoveInstance(this);
#endif
        }

        public override void OnInspectorGUI()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_renderers == null)
            {
                _renderers = Setup(Array.Empty<string>(), serializedObject, this, target);
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
