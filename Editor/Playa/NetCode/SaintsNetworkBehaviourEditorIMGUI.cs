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
        private void OnEnableIMGUI()
        {
            if (!_saintsEditorIMGUI)
            {
                return;
            }

            _coreEditor ??= new SaintsEditorCore(this, true, this);

            _coreEditor.OnEnableIMGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                RenderNetCodeIMGUI();
                if (changed.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            _coreEditor ??= new SaintsEditorCore(this, true, this);
            _coreEditor.OnInspectorGUI();
        }

        // private ICollection<string> IMGUIGetNetCodeVariableNames() => GetNetCodeVariableFields().Values
        //     .Where(each => each != null).Select(each => each.Name).ToArray();

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
