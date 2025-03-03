using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ParticlePlayDrawer
{
    public partial class ParticlePlayAttributeDrawer
    {
        private class InfoIMGUI
        {
            public double StartTime;
            public double PausedTime;
            public float RunTime;
            public ParticleSystem ParticleSystem;
            // public EditorApplication.CallbackFunction TickCallback;

            public PlayState Playing;

            public void Destroy()
            {
                if (ParticleSystem != null)
                {
                    try
                    {
                        ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
                StopTick();
            }

            public void StartTick()
            {
                StopTick();
                EditorApplication.update += Tick;
            }

            public void StopTick()
            {
                EditorApplication.update -= Tick;
            }

            private void Tick()
            {
                RunTime = (float)(EditorApplication.timeSinceStartup - StartTime);
            }
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();


        private Texture2D _playIcon;
        private Texture2D _pauseIcon;
        private Texture2D _resumeIcon;
        private Texture2D _stopIcon;
        private GUIStyle _iconButtonStyle;
        // private string _error = "";

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();

            // infoCache.TickCallback = UpdateTick;

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                infoCache.Destroy();
                InfoCacheIMGUI.Remove(key);
            });

            // EditorApplication.update += infoCache.TickCallback;
            return infoCache;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGuiPayload,
            FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);

            return SingleLineHeight * (cachedInfo.Playing == PlayState.None ? 1 : 2);
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            // ImGuiEnsureDispose(property.serializedObject.targetObject);
            ParticleSystem particleSystem = null;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (property.objectReferenceValue)
            {
                case GameObject go:
                    particleSystem = go.GetComponent<ParticleSystem>();
                    break;
                case ParticleSystem ps:
                    particleSystem = ps;
                    break;
                case Component compo:
                    particleSystem = compo.GetComponent<ParticleSystem>();
                    break;
            }

            cachedInfo.ParticleSystem = particleSystem;

            if (particleSystem == null)
            {
                // _error = "No ParticleSystem found.";
                // _error = "";
                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Button(position, "?");
                }

                return false;
            }

            if (_iconButtonStyle == null)
            {
                _playIcon = Util.LoadResource<Texture2D>("play.png");
                _pauseIcon = Util.LoadResource<Texture2D>("pause.png");
                _resumeIcon = Util.LoadResource<Texture2D>("resume.png");
                _stopIcon = Util.LoadResource<Texture2D>("stop.png");
                _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }

            bool particleNotEnabled = !particleSystem.gameObject.activeInHierarchy;
            bool needStop = particleNotEnabled && cachedInfo.Playing != PlayState.None;

            if (onGUIPayload.changed || needStop)
            {
                // Debug.Log($"Stop current");
                cachedInfo.StartTime = cachedInfo.PausedTime = 0;
                // EditorApplication.update -= cachedInfo.TickCallback;
                cachedInfo.StopTick();
                cachedInfo.Playing = PlayState.None;
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                SceneView.RepaintAll();
            }

            Rect playPauseRect = position;
            if (cachedInfo.Playing != PlayState.None)
            {
                Rect stopRect;
                (stopRect, playPauseRect) = RectUtils.SplitWidthRect(playPauseRect, SingleLineHeight);

                if (GUI.Button(stopRect, _stopIcon))
                {
                    cachedInfo.StartTime = 0;
                    // EditorApplication.update -= cachedInfo.TickCallback;
                    cachedInfo.StopTick();
                    cachedInfo.Playing = PlayState.None;
                    // ReSharper disable once Unity.NoNullPropagation
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    SceneView.RepaintAll();
                }
            }

            Texture2D buttonLabel;
            switch (cachedInfo.Playing)
            {
                case PlayState.None:
                    buttonLabel = _playIcon;
                    break;
                case PlayState.Playing:
                    buttonLabel = _pauseIcon;
                    break;
                case PlayState.Paused:
                    buttonLabel = _resumeIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cachedInfo.Playing), cachedInfo.Playing, null);
            }


            using(new EditorGUI.DisabledScope(particleNotEnabled))
            {
                if (GUI.Button(playPauseRect, buttonLabel))
                {
                    switch (cachedInfo.Playing)
                    {
                        case PlayState.None: // start to play
                            cachedInfo.StartTime = cachedInfo.PausedTime = EditorApplication.timeSinceStartup;
                            // EditorApplication.update += cachedInfo.TickCallback;
                            cachedInfo.StartTick();
                            cachedInfo.Playing = PlayState.Playing;
                            // Debug.Log($"None to {cachedInfo.Playing}");
                            break;
                        case PlayState.Playing: // pause
                            cachedInfo.PausedTime = EditorApplication.timeSinceStartup;
                            // EditorApplication.update -= cachedInfo.TickCallback;
                            cachedInfo.StopTick();
                            cachedInfo.Playing = PlayState.Paused;
                            break;
                        case PlayState.Paused: // resume
                            cachedInfo.StartTime += EditorApplication.timeSinceStartup - cachedInfo.PausedTime;
                            // EditorApplication.update += cachedInfo.TickCallback;
                            cachedInfo.StartTick();
                            cachedInfo.Playing = PlayState.Playing;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(cachedInfo.Playing), cachedInfo.Playing, null);
                    }
                }
            }

            // Debug.Log(cachedInfo.Playing);

            if (cachedInfo.Playing == PlayState.Playing)
            {
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem.Simulate(cachedInfo.RunTime, true);
                SceneView.RepaintAll();
                // EditorApplication.delayCall += HandleUtility.Repaint;
                // foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                //     if (item.serializedObject == property.serializedObject)
                //     {
                //         item.Repaint();
                //     }
            }

            return true;
        }

        // private static void Update(InfoIMGUI cachedInfo)
        // {
        //     cachedInfo.RunTime = (float)(EditorApplication.timeSinceStartup - cachedInfo.StartTime);
        // }

        // protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     int index,
        //     FieldInfo info,
        //     object parent)
        // {
        //     return _error != "";
        // }
        //
        // protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
        //     ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        // {
        //     return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        // }
        //
        // protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
        //     OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
        //     _error == ""
        //         ? position
        //         : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}
