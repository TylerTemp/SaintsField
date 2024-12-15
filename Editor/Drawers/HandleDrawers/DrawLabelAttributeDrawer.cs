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
            public Space Space;

            public string Content;
            public bool IsCallback;
            public string ActualContent;
            public EColor EColor;

            public Util.TargetWorldPosInfo TargetWorldPosInfo;

            public GUIStyle GUIStyle;
        }



        private static void OnSceneGUIInternal(SceneView _, LabelInfo labelInfo)
        {
            // ReSharper disable once ReplaceWithStringIsNullOrEmpty
            // ReSharper disable once MergeIntoLogicalPattern
            if (labelInfo.ActualContent == null || labelInfo.ActualContent == "")
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

        private readonly Dictionary<string, LabelInfo> IdToLabelInfo = new Dictionary<string, LabelInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            // ReSharper disable once InvertIf
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
            IdToLabelInfo.Remove(_cacheKey);

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

                Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
                if (targetWorldPosInfo.Error != "")
                {
                    Debug.LogError(targetWorldPosInfo.Error);
                    return position;
                }

                labelInfo = new LabelInfo
                {
                    Space = drawLabelAttribute.Space,
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
                };
                IdToLabelInfo[_cacheKey] = labelInfo;
                ImGuiEnsureDispose(property.serializedObject.targetObject);
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
                return position;
            }

            if (!labelInfo.TargetWorldPosInfo.IsTransform)
            {
                labelInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(labelInfo.Space, property, info, parent);
            }

            if (!labelInfo.IsCallback)
            {
                return position;
            }

            (string valueError, object value) = Util.GetOf<object>(labelInfo.Content, null, property, info, parent);
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
            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
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
                    _labelInfoUIToolkit.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
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
