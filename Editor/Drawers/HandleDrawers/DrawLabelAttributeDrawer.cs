using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(DrawLabelAttribute))]
    public class DrawLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private class LabelInfoUIToolkit
        {
            public string Content;
            public bool IsCallback;
            public string ActualContent;
            public EColor EColor;

            public Transform Transform;
            public GUIStyle GUIStyle;

            public Action<SceneView> OnSceneGUIIMGUI;
        }

        private static (string error, Transform trans) GetTargetField(SerializedProperty property, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

                if (error == "" && propertyValue is IWrapProp wrapProp)
                {
                    object propWrapValue = Util.GetWrapValue(wrapProp);
                    switch (propWrapValue)
                    {
                        case null:
                            return ("", null);
                        case GameObject wrapGo:
                            return ("", wrapGo.transform);
                        case Component wrapComp:
                            return ("", wrapComp.transform);
                        default:
                            return ($"{propWrapValue} is not GameObject or Component", null);
                    }
                }

                return ($"{property.propertyType} is not supported", null);
            }
            if (property.objectReferenceValue is GameObject isGo)
            {
                return ("", isGo.transform);
            }
            if(property.objectReferenceValue is Component comp)
            {
                return ("", comp.transform);
                // go = ((Component) property.objectReferenceValue)?.gameObject;
            }

            return ($"{property.propertyType} is not GameObject or Component", null);
        }

        private static void OnSceneGUIInternal(SceneView _, LabelInfoUIToolkit labelInfo)
        {
            if (string.IsNullOrEmpty(labelInfo.ActualContent))
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

            Vector3 pos = labelInfo.Transform.position;
            Handles.Label(pos, labelInfo.ActualContent, labelInfo.GUIStyle);
        }

        #region IMGUI

        private static readonly Dictionary<string, LabelInfoUIToolkit> IdToMinMaxRange = new Dictionary<string, LabelInfoUIToolkit>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

#if UNITY_2019_3_OR_NEWER
        [InitializeOnEnterPlayMode]
        [InitializeOnLoadMethod]
#endif
        private static void ImGuiClearSharedData() => IdToMinMaxRange.Clear();

        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            // ReSharper disable once InvertIf
            if(IdToMinMaxRange.TryGetValue(_cacheKey, out LabelInfoUIToolkit labelInfo))
            {
                SceneView.duringSceneGui -= labelInfo.OnSceneGUIIMGUI;
                IdToMinMaxRange.Remove(_cacheKey);
            }
        }

        ~DrawLabelAttributeDrawer()
        {
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
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
            if (!IdToMinMaxRange.TryGetValue(_cacheKey, out LabelInfoUIToolkit labelInfo))
            {
                labelInfo = new LabelInfoUIToolkit
                {
                    OnSceneGUIIMGUI = OnSceneGUIIMGUI,
                };
                IdToMinMaxRange[_cacheKey] = labelInfo;
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
            }
            return position;
        }

        private void OnSceneGUIIMGUI(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _labelInfoUIToolkit);
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit
        private LabelInfoUIToolkit _labelInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
            (string error, Transform trans) = GetTargetField(property, info, parent);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            VisualElement child = new VisualElement
            {
                name = "draw-label-attribute-drawer",
            };

            _labelInfoUIToolkit = new LabelInfoUIToolkit
            {
                Content = drawLabelAttribute.Content,
                ActualContent = drawLabelAttribute.Content,
                IsCallback = drawLabelAttribute.IsCallback,
                EColor = drawLabelAttribute.EColor,
                Transform = trans,
            };

            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);

            return child;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (!_labelInfoUIToolkit.IsCallback)
            {
                return;
            }

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            (string error, object value) = Util.GetOf<object>(_labelInfoUIToolkit.Content, null, property, fieldInfo, parent);
            if (error != "")
            {
                return;
            }

            if (value is IWrapProp wrapProp)
            {
                value = Util.GetWrapValue(wrapProp);
            }

            _labelInfoUIToolkit.ActualContent = $"{value}";
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
