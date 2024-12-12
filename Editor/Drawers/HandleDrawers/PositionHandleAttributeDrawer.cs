using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(PositionHandleAttribute))]
    public class PositionHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private static void SetValue(Vector3 newTargetPosition, Space space, SerializedProperty property, FieldInfo info, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                {
                    (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

                    if (error == "" && propertyValue is IWrapProp wrapProp)
                    {
                        object propWrapValue = Util.GetWrapValue(wrapProp);
                        switch (propWrapValue)
                        {
                            case null:
                                Debug.LogError($"Target is null");
                                return;
                            case GameObject wrapGo:
                                Undo.RecordObject(wrapGo.transform, "Move Position");
                                wrapGo.transform.position = newTargetPosition;
                                return;
                            case Component wrapComp:
                                Undo.RecordObject(wrapComp.transform, "Move Position");
                                wrapComp.transform.position = newTargetPosition;
                                return;
                            default:
                                Debug.LogError($"{propWrapValue} is not GameObject or Component");
                                return;
                        }
                    }

                    Debug.LogError($"{property.propertyType} is not supported");
                    return;
                }
                case SerializedPropertyType.ObjectReference when property.objectReferenceValue is GameObject isGo:
                    Undo.RecordObject(isGo.transform, "Move Position");
                    isGo.transform.position = newTargetPosition;
                    return;
                case SerializedPropertyType.ObjectReference when property.objectReferenceValue is Component comp:
                    Undo.RecordObject(comp.transform, "Move Position");
                    comp.transform.position = newTargetPosition;
                    return;
                case SerializedPropertyType.ObjectReference:
                    Debug.LogError($"{property.objectReferenceValue} is not supported");
                    return;
            }

            Vector3 rawValue = newTargetPosition;
            if (space == Space.Self)  // world to self
            {
                (string error, Transform container) = GetContainingTransform(property);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                rawValue = container.InverseTransformPoint(newTargetPosition);
            }

            if(property.propertyType == SerializedPropertyType.Vector3)
            {
                property.vector3Value = rawValue;
            }
            else if(property.propertyType == SerializedPropertyType.Vector2)
            {
                property.vector2Value = rawValue;
            }
        }

        private static (string error, Transform container) GetContainingTransform(SerializedProperty property)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (property.serializedObject.targetObject)
            {
                case GameObject go:
                    return ("", go.transform);
                case Component comp:
                    return ("", comp.transform);
                default:
                    return ($"Target is not GameObject or Component", null);
            }
        }

        private struct PositionHandleInfo
        {
            public SerializedProperty Property;
            public FieldInfo Info;
            public object Parent;
            public Space Space;
        }

        ~PositionHandleAttributeDrawer()
        {
            // SceneView.duringSceneGui -= OnSceneGUIIMGUI;
#if UNITY_2021_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
#endif
        }

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit
        private static string NamePositionHandle(SerializedProperty property) => $"{property.propertyPath}_PositionHandle";

        private PositionHandleInfo _positionHandleInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _positionHandleInfoUIToolkit = new PositionHandleInfo
            {
                Property = property,
                Info = info,
                Parent = parent,
                Space = ((PositionHandleAttribute)saintsAttribute).Space,
            };

            VisualElement child = new VisualElement
            {
                name = NamePositionHandle(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        // protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     int index,
        //     VisualElement container, Action<object> onValueChanged, FieldInfo info)
        // {
        //
        //     object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
        //
        //     (string error, object value) = Util.GetOf<object>(_labelInfoUIToolkit.Content, null, property, fieldInfo, parent);
        //     if (error != "")
        //     {
        //         return;
        //     }
        //
        //     if (value is IWrapProp wrapProp)
        //     {
        //         value = Util.GetWrapValue(wrapProp);
        //     }
        //
        //     _labelInfoUIToolkit.ActualContent = $"{value}";
        // }

        // private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            try
            {
                string _ = _positionHandleInfoUIToolkit.Property.propertyPath;
            }
            catch (NullReferenceException)
            {
                Debug.LogWarning("Property disposed, removing SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
                return;
            }
            catch (ObjectDisposedException)
            {
                Debug.LogWarning("Property disposed, removing SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
                return;
            }

            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetTargetWorldPosInfo(_positionHandleInfoUIToolkit.Space, _positionHandleInfoUIToolkit.Property, _positionHandleInfoUIToolkit.Info, _positionHandleInfoUIToolkit.Parent);
            if(targetWorldPosInfo.Error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(targetWorldPosInfo.Error);
#endif
                return;
            }

            // Debug.Log(worldPos);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Vector3 newTargetPosition = Handles.PositionHandle(targetWorldPosInfo.IsTransform? targetWorldPosInfo.Transform.position: targetWorldPosInfo.WorldPos, Quaternion.identity);
                if (changed.changed)
                {
                    SetValue(newTargetPosition, _positionHandleInfoUIToolkit.Space, _positionHandleInfoUIToolkit.Property, _positionHandleInfoUIToolkit.Info, _positionHandleInfoUIToolkit.Parent);
                    _positionHandleInfoUIToolkit.Property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        #endregion

#endif
    }
}
