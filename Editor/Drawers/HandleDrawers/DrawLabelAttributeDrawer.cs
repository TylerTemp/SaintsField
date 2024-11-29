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
#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private struct LabelInfoUIToolkit
        {
            public EColor EColor;
            public string Content;
            public bool IsCallback;

            public SerializedProperty Property;
            public FieldInfo Info;

            public Transform Transform;
            public GUIStyle GUIStyle;
        }

        private LabelInfoUIToolkit _labelInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            DrawLabelAttribute drawLabelAttributeUIToolkit = (DrawLabelAttribute)saintsAttribute;
            (string error, Transform trans) = GetTargetField(property, info, parent);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            GUIStyle guiStyle = GUI.skin.label;
            if(drawLabelAttributeUIToolkit.EColor != EColor.White)
            {
                guiStyle = new GUIStyle
                {
                    normal = {textColor = drawLabelAttributeUIToolkit.EColor.GetColor()},
                };
            }

            VisualElement child = new VisualElement
            {
                name = "draw-label-attribute-drawer",
            };
            container.Add(child);

            _labelInfoUIToolkit = new LabelInfoUIToolkit
            {
                EColor = drawLabelAttributeUIToolkit.EColor,
                Content = drawLabelAttributeUIToolkit.Content,
                IsCallback = drawLabelAttributeUIToolkit.IsCallback,
                Property = property,
                Info = info,
                Transform = trans,
                GUIStyle = guiStyle,
            };

            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUI);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUI);

            return null;
        }

        private void OnSceneGUI(SceneView obj)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (_labelInfoUIToolkit.Transform is null)
            {
                return;
            }

            Vector3 pos = _labelInfoUIToolkit.Transform.position;
            Handles.Label(pos, _labelInfoUIToolkit.Content, _labelInfoUIToolkit.GUIStyle);
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
