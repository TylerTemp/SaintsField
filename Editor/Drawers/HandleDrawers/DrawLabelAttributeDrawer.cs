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
#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private class LabelInfoUIToolkit
        {
            public string Content;
            public bool IsCallback;
            public string ActualContent;
            public EColor EColor;

            public SerializedProperty Property;
            public FieldInfo Info;

            public Transform Transform;
            // public GUIStyle GUIStyle;
        }

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
                Property = property,
                Info = info,
                Transform = trans,
            };

            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUI);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUI);

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
            // Debug.Log($"Updated: {_labelInfoUIToolkit.ActualContent}");
            // SceneView.duringSceneGui -= OnSceneGUI;
        }

        private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUI(SceneView obj)
        {
            if (string.IsNullOrEmpty(_labelInfoUIToolkit.ActualContent))
            {
                return;
            }

            if(_guiStyleUIToolkit == null)
            {
                if (_labelInfoUIToolkit.EColor == EColor.White)
                {
                    _guiStyleUIToolkit = GUI.skin.label;
                }
                else
                {
                    _guiStyleUIToolkit = new GUIStyle
                    {
                        normal = { textColor = _labelInfoUIToolkit.EColor.GetColor() },
                    };
                }
            }

            Vector3 pos = _labelInfoUIToolkit.Transform.position;
            Handles.Label(pos, _labelInfoUIToolkit.ActualContent, _guiStyleUIToolkit);

            // Handles.color = Color.magenta;
            // Handles.RadiusHandle(Quaternion.identity, _labelInfoUIToolkit.Transform.position, 0.1f);
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

        #endregion

#endif
    }
}
