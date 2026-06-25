using System;
using System.Collections.Generic;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.PlayaFullWidthRichLabelFakeRenderer;
using SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor
{
    public partial class SaintsEditorCore
    {
        private IReadOnlyList<ISaintsRenderer> _topRenderersIMGUI;
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
                    _topRenderersIMGUI = MakeTopCompRenderersIMGUI();
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
            if (_topRenderersIMGUI != null)
            {
                foreach (ISaintsRenderer renderer in _topRenderersIMGUI)
                {
                    renderer.OnDestroy();
                }
            }
            _topRenderersIMGUI = null;

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

            foreach (ISaintsRenderer renderer in _topRenderersIMGUI ?? Array.Empty<ISaintsRenderer>())
            {
                renderer.RenderIMGUI(UnityEngine.Screen.width);
            }

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

        private IReadOnlyList<ISaintsRenderer> MakeTopCompRenderersIMGUI()
        {
            if (Targets.Length == 0 || Targets[0] == null)
            {
                return Array.Empty<ISaintsRenderer>();
            }

            Type objectType = Targets[0].GetType();
            List<ISaintsRenderer> compTopRenderers = new List<ISaintsRenderer>();
            foreach (Attribute attr in ReflectCache.GetCustomAttributes<Attribute>(objectType))
            {
                switch (attr)
                {
                    case CompInfoBoxAttribute compInfoBoxAttribute:
                    {
                        InfoBoxAttribute infoBoxAttribute = new InfoBoxAttribute(
                            compInfoBoxAttribute.Content,
                            compInfoBoxAttribute.MessageType,
                            compInfoBoxAttribute.ShowCallback,
                            compInfoBoxAttribute.IsCallback);
                        PlayaInfoBoxRenderer drawer = new PlayaInfoBoxRenderer(
                            SerializedObject,
                            MakeTopCompFieldInfo(objectType, infoBoxAttribute, 0),
                            infoBoxAttribute);
                        compTopRenderers.Add(drawer);
                    }
                        break;
                    case CompTextAttribute compTextAttribute:
                    {
                        AboveTextAttribute aboveTextAttribute = new AboveTextAttribute(
                            BuildAboveTextContent(compTextAttribute.Content, compTextAttribute.IsCallback),
                            compTextAttribute.PaddingLeft,
                            compTextAttribute.PaddingRight);
                        PlayaFullWidthRichLabelRenderer drawer = new PlayaFullWidthRichLabelRenderer(
                            SerializedObject,
                            MakeTopCompFieldInfo(objectType, aboveTextAttribute, 0),
                            aboveTextAttribute);
                        compTopRenderers.Add(drawer);
                    }
                        break;
                }
            }

            return compTopRenderers;
        }
    }
}
