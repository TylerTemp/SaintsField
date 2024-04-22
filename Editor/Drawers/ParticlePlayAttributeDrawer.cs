using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ParticlePlayAttribute))]
    public class ParticlePlayAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private double _previousTime;
        private float _runTime;
        private bool _playing;

        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            return SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            ParticleSystem particleSystem = default;
            switch (property.objectReferenceValue)
            {
                case GameObject go:
                    particleSystem = go.GetComponent<ParticleSystem>();
                    break;
                case Component compo:
                    particleSystem = compo.GetComponent<ParticleSystem>();
                    break;
            }

            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                _playing = GUI.Toggle(position, _playing, _playing? "■": "▶", GUI.skin.button);
                if(changed.changed)
                {
                    if (_playing)
                    {
                        // ReSharper disable once Unity.NoNullPropagation
                        if(particleSystem?.useAutoRandomSeed ?? false)
                        {
                            particleSystem.randomSeed = (uint)Random.Range(0, int.MaxValue);
                        }

                        _previousTime = EditorApplication.timeSinceStartup;
                        EditorApplication.update += Update;
                        _playing = true;
                    }
                    else
                    {
                        _previousTime = 0;
                        EditorApplication.update -= Update;
                        _playing = false;
                        // ReSharper disable once Unity.NoNullPropagation
                        particleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        SceneView.RepaintAll();
                    }
                }
            }
            if(_playing)
            {
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem?.Simulate(_runTime, true);
                SceneView.RepaintAll();
            }

            return true;
        }

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            _runTime = (float)(EditorApplication.timeSinceStartup - _previousTime);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion
    }
}
