using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(DrawLabelAttribute))]
    public class DrawLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private class LabelInfo
        {
            public string Content;
            public bool IsCallback;
            public string ActualContent;
            public EColor EColor;

            public Util.TargetWorldPosInfo TargetWorldPosInfo;

            public GUIStyle GUIStyle;

            // ReSharper disable once InconsistentNaming
            public Action<SceneView> OnSceneGUIIMGUI;
        }



        private static void OnSceneGUIInternal(SceneView _, LabelInfo labelInfo)
        {
            if (string.IsNullOrEmpty(labelInfo.ActualContent))
            {
                return;
            }

            if (!string.IsNullOrEmpty(labelInfo.TargetWorldPosInfo.Error))
            {
                return;
            }

            if(labelInfo.GUIStyle == null)
            {
                if (labelInfo.EColor == EColor.White)
                {
                    labelInfo.GUIStyle = GUI.skin.label;
                }
                else
                {
                    labelInfo.GUIStyle = new GUIStyle
                    {
                        normal = { textColor = labelInfo.EColor.GetColor() },
                    };
                }
            }

            Vector3 pos = labelInfo.TargetWorldPosInfo.IsTransform
                ? labelInfo.TargetWorldPosInfo.Transform.position
                : labelInfo.TargetWorldPosInfo.WorldPos;
            Handles.Label(pos, labelInfo.ActualContent, labelInfo.GUIStyle);
        }
        ~DrawLabelAttributeDrawer()
        {
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
#if UNITY_2021_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
#endif
        }

        #region IMGUI

        private static readonly Dictionary<string, LabelInfo> IdToLabelInfo = new Dictionary<string, LabelInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

#if UNITY_2019_3_OR_NEWER
        [InitializeOnEnterPlayMode]
#endif
        [InitializeOnLoadMethod]
        private static void ImGuiClearSharedData()
        {
            foreach (LabelInfo labelInfo in IdToLabelInfo.Values)
            {
                SceneView.duringSceneGui -= labelInfo.OnSceneGUIIMGUI;
            }
            IdToLabelInfo.Clear();
        }

        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            // ReSharper disable once InvertIf
            if(IdToLabelInfo.TryGetValue(_cacheKey, out LabelInfo labelInfo))
            {
                SceneView.duringSceneGui -= labelInfo.OnSceneGUIIMGUI;
                IdToLabelInfo.Remove(_cacheKey);
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            _cacheKey = GetKey(property);
            // ReSharper disable once InvertIf
            if (!IdToLabelInfo.TryGetValue(_cacheKey, out LabelInfo labelInfo))
            {
                DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;

                Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
                if (targetWorldPosInfo.Error != "")
                {
                    Debug.LogError(targetWorldPosInfo.Error);
                    return position;
                }

                labelInfo = new LabelInfo
                {
                    Content = drawLabelAttribute.Content,
                    ActualContent = drawLabelAttribute.Content,
                    IsCallback = drawLabelAttribute.IsCallback,
                    EColor = drawLabelAttribute.EColor,
                    TargetWorldPosInfo = targetWorldPosInfo,
                    GUIStyle = drawLabelAttribute.EColor == EColor.White
                        ? GUI.skin.label
                        : new GUIStyle
                        {
                            normal = { textColor = drawLabelAttribute.EColor.GetColor() },
                        },
                    OnSceneGUIIMGUI = OnSceneGUIIMGUI,
                };
                IdToLabelInfo[_cacheKey] = labelInfo;
                ImGuiEnsureDispose(property.serializedObject.targetObject);
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            if (!labelInfo.IsCallback)
            {
                return position;
            }

            (string valueError, object value) = Util.GetOf<object>(labelInfo.Content, null, property, fieldInfo, parent);
            if (valueError != "")
            {
                Debug.LogError(valueError);
                return position;
            }

            if (value is IWrapProp wrapProp)
            {
                value = Util.GetWrapValue(wrapProp);
            }

            labelInfo.ActualContent = $"{value}";
            return position;
        }

        private void OnSceneGUIIMGUI(SceneView sceneView)
        {
            if (IdToLabelInfo.TryGetValue(_cacheKey, out LabelInfo labelInfo))
            {
                OnSceneGUIInternal(sceneView, labelInfo);
            }
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit
        private static string NameDrawLabel(SerializedProperty property) => $"{property.propertyPath}_DrawLabel";

        private LabelInfo _labelInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
            if (targetWorldPosInfo.Error != "")
            {
                return new HelpBox(targetWorldPosInfo.Error, HelpBoxMessageType.Error);
            }

            _labelInfoUIToolkit = new LabelInfo
            {
                Content = drawLabelAttribute.Content,
                ActualContent = drawLabelAttribute.Content,
                IsCallback = drawLabelAttribute.IsCallback,
                EColor = drawLabelAttribute.EColor,
                TargetWorldPosInfo = targetWorldPosInfo,
            };

            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement child = new VisualElement
            {
                name = NameDrawLabel(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (_labelInfoUIToolkit.IsCallback)
            {
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

                (string error, object value) =
                    Util.GetOf<object>(_labelInfoUIToolkit.Content, null, property, info, parent);

                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    return;
                }

                if (value is IWrapProp wrapProp)
                {
                    value = Util.GetWrapValue(wrapProp);
                }

                _labelInfoUIToolkit.ActualContent = $"{value}";
            }

            if (!_labelInfoUIToolkit.TargetWorldPosInfo.IsTransform)
            {
                DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if(parent != null)
                {
                    _labelInfoUIToolkit.TargetWorldPosInfo = Util.GetTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
                }
            }
        }

        private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _labelInfoUIToolkit);
        }
        #endregion

#endif
    }
}
