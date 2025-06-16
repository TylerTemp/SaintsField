using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using Unity.Netcode.Editor;
using UnityEditor;

namespace SaintsField.Editor.Playa.NetCode
{
    [CustomEditor(typeof(SaintsNetworkBehaviour), true)]
    public partial class SaintsNetworkBehaviourEditor : NetworkBehaviourEditor, IMakeRenderer
    // public partial class SaintsNetworkBehaviourEditor : SaintsEditor
    {
        private IReadOnlyList<ISaintsRenderer> _renderers;
        public IEnumerable<AbsRenderer> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(so, fieldWithInfo);
        }

        private FieldInfo _mInitializedField;
        private MethodInfo _initMethod;
        private FieldInfo _networkVariableNamesField;
        private MethodInfo _renderNetworkVariableMethod;
        private FieldInfo _networkVariableFields;
        private FieldInfo _mNetworkVariableFieldsField;

        private bool EnsureInit()
        {
            Type networkBehaviourEditorType = typeof(NetworkBehaviourEditor);

            _mInitializedField ??= networkBehaviourEditorType.GetField(
                "m_Initialized",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // ReSharper disable once PossibleNullReferenceException
            bool reflectedMInitialized = (bool)_mInitializedField.GetValue(this);

            // Debug.Log($"reflectedMInitialized={reflectedMInitialized}");

            if (!reflectedMInitialized)
            {
                serializedObject.Update();
                SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
                if (scriptProperty == null)
                {
                    return false;
                }

                MonoScript targetScript = scriptProperty.objectReferenceValue as MonoScript;
                // Init(targetScript);

                _initMethod ??= networkBehaviourEditorType.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);
                // ReSharper disable once PossibleNullReferenceException
                _initMethod.Invoke(this, new object[] { targetScript });
                // Debug.Log("invoke init done");
            }

            return true;
        }

        private IReadOnlyList<string> GetNetCodeVariableNames()
        {
            Type networkBehaviourEditorType = typeof(NetworkBehaviourEditor);

            if (!EnsureInit())
            {
                return Array.Empty<string>();
            }

            // Get the m_NetworkVariableNames field using reflection
            _networkVariableNamesField ??= networkBehaviourEditorType
                .GetField("m_NetworkVariableNames", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable once PossibleNullReferenceException
            List<string> networkVariableNames = (List<string>)_networkVariableNamesField.GetValue(this);

            // Get the RenderNetworkVariable method using reflection

            return networkVariableNames;
        }

        private IReadOnlyDictionary<string, FieldInfo> GetNetCodeVariableFields()
        {
            Type networkBehaviourEditorType = typeof(NetworkBehaviourEditor);

            if (!EnsureInit())
            {
                return new Dictionary<string, FieldInfo>();
            }

            // Get the m_NetworkVariableNames field using reflection
            _mNetworkVariableFieldsField ??= networkBehaviourEditorType
                .GetField("m_NetworkVariableFields", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable once PossibleNullReferenceException
            Dictionary<string, FieldInfo> networkVariableFields = (Dictionary<string, FieldInfo>)_mNetworkVariableFieldsField.GetValue(this);

            // Get the RenderNetworkVariable method using reflection

            return networkVariableFields;
        }
    }


#if SAINTSFIELD_SAINTS_NETWORK_BEHAVIOR_EDITOR_APPLY
    [CustomEditor(typeof(NetworkBehaviour), true)]
    public class ApplySaintsNetworkBehaviourEditor : SaintsNetworkBehaviourEditor{}
#endif
}
