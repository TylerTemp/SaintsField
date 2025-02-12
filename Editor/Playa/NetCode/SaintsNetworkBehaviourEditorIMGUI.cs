using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.NetCode
{
    public partial class SaintsNetworkBehaviourEditor
    {
        public virtual void OnEnable()
        {
            // This can be null and throw an exception when running test runner in the editor
            if (target == null)
            {
                return;
            }
            // When we first add a NetworkBehaviour this editor will be enabled
            // so we go ahead and check for an already existing NetworkObject here
            // ReSharper disable once PossibleNullReferenceException
            CheckForNetworkObject((target as NetworkBehaviour).gameObject);

            try
            {
                _renderers = SaintsEditor.Setup(IMGUIGetNetCodeVariableNames(), serializedObject, this, target);
            }
            catch (Exception)
            {
                _renderers = null;  // just... let IMGUI renderer to deal with it...
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            RenderNetCodeIMGUI();

            MonoScript monoScript = SaintsEditor.GetMonoScript(target);
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

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_renderers == null)
            {
                _renderers = SaintsEditor.Setup(IMGUIGetNetCodeVariableNames(), serializedObject, this, target);
            }
            foreach (ISaintsRenderer renderer in _renderers)
            {
                renderer.RenderIMGUI(Screen.width);
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }

        private ICollection<string> IMGUIGetNetCodeVariableNames() => GetNetCodeVariableFields().Values
            .Where(each => each != null).Select(each => each.Name).ToArray();

        private void RenderNetCodeIMGUI()
        {
            Type networkBehaviourEditorType = typeof(NetworkBehaviourEditor);

            if (!EnsureInit())
            {
                return;
            }

            // Get the RenderNetworkVariable method using reflection
            _renderNetworkVariableMethod = networkBehaviourEditorType
                .GetMethod("RenderNetworkVariable", BindingFlags.NonPublic | BindingFlags.Instance);

            IReadOnlyList<string> networkVariableNames = GetNetCodeVariableNames();

            for (int i = 0; i < networkVariableNames.Count; i++)
            {
                // Call the RenderNetworkVariable method using reflection
                // ReSharper disable once PossibleNullReferenceException
                _renderNetworkVariableMethod.Invoke(this, new object[] { i });
            }
        }
    }
}
