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

        private Texture2D _playIcon;
        private Texture2D _stopIcon;
        private GUIStyle _iconButtonStyle;

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
            ParticleSystem particleSystem = null;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (property.objectReferenceValue)
            {
                case GameObject go:
                    particleSystem = go.GetComponent<ParticleSystem>();
                    break;
                case Component compo:
                    particleSystem = compo.GetComponent<ParticleSystem>();
                    break;
            }

            if (particleSystem == null)
            {
                _error = "No ParticleSystem found";
                using(new EditorGUI.DisabledScope(true))
                {
                    GUI.Button(position, "?");
                }
                return false;
            }

            if(_iconButtonStyle == null)
            {
                _playIcon = Util.LoadResource<Texture2D>("play.png");
                _stopIcon = Util.LoadResource<Texture2D>("stop.png");
                _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }

            if (onGUIPayload.changed)
            {
                _previousTime = 0;
                EditorApplication.update -= Update;
                _playing = false;
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                SceneView.RepaintAll();
            }

            if (GUI.Button(position, _playing ? _stopIcon : _playIcon, _iconButtonStyle))
            {
                _playing = !_playing;
                if (_playing)
                {
                    SceneView.RepaintAll();

                    // ReSharper disable once Unity.NoNullPropagation
                    if(particleSystem.useAutoRandomSeed)
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
