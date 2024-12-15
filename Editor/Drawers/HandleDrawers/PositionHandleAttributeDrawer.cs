using System;
using System.Collections.Generic;
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
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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

        private class PositionHandleInfo
        {
            public SerializedProperty Property;
            public FieldInfo Info;
            public object Parent;
            public Space Space;

            public Util.TargetWorldPosInfo TargetWorldPosInfo;
        }

        private static bool OnSceneGUIInternal(SceneView _, PositionHandleInfo positionHandleInfo)
        {
            Vector3 worldPos;
            if (positionHandleInfo.TargetWorldPosInfo.IsTransform)
            {
                Transform trans = positionHandleInfo.TargetWorldPosInfo.Transform;
                if (trans == null)
                {
                    return false;
                }
                worldPos = trans.position;
            }
            else
            {
                worldPos = positionHandleInfo.TargetWorldPosInfo.WorldPos;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Vector3 newTargetPosition = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (changed.changed)
                {
                    SetValue(newTargetPosition, positionHandleInfo.Space, positionHandleInfo.Property, positionHandleInfo.Info, positionHandleInfo.Parent);
                    positionHandleInfo.Property.serializedObject.ApplyModifiedProperties();
                }
            }

            return true;
        }

        ~PositionHandleAttributeDrawer()
        {
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
#if UNITY_2021_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
#endif
        }

        #region IMGUI

        private readonly Dictionary<string, PositionHandleInfo> _idToInfoImGui = new Dictionary<string, PositionHandleInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private string _cacheKey;

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
            _idToInfoImGui.Remove(_cacheKey);
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
            if (!_idToInfoImGui.TryGetValue(_cacheKey, out PositionHandleInfo positionHandleInfo))
            {
                PositionHandleAttribute positionHandleAttribute = (PositionHandleAttribute)saintsAttribute;

                Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleAttribute.Space, property, info, parent);
                if (targetWorldPosInfo.Error != "")
                {
                    Debug.LogError(targetWorldPosInfo.Error);
                    return position;
                }

                positionHandleInfo = new PositionHandleInfo
                {
                    Property = property,
                    Info = info,
                    Parent = parent,
                    Space = positionHandleAttribute.Space,
                    TargetWorldPosInfo = targetWorldPosInfo,
                };
                _idToInfoImGui[_cacheKey] = positionHandleInfo;
                ImGuiEnsureDispose(property.serializedObject.targetObject);
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            positionHandleInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleInfo.Space, property, info, parent);
            return position;
        }

        private void OnSceneGUIIMGUI(SceneView sceneView)
        {
            if (_idToInfoImGui.TryGetValue(_cacheKey, out PositionHandleInfo positionHandleInfo))
            {
                if (!OnSceneGUIInternal(sceneView, positionHandleInfo))
                {
                    Debug.LogWarning($"Target disposed, remove SceneGUI");
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                }
            }
        }

        #endregion


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
            PositionHandleAttribute positionHandleAttribute = (PositionHandleAttribute)saintsAttribute;
            _positionHandleInfoUIToolkit = new PositionHandleInfo
            {
                Property = property,
                Info = info,
                Parent = parent,
                Space = positionHandleAttribute.Space,

                TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleAttribute.Space, property, info, parent),
            };

            VisualElement child = new VisualElement
            {
                name = NamePositionHandle(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (_positionHandleInfoUIToolkit.TargetWorldPosInfo.Error != "")
            {
                return;
            }

            if (_positionHandleInfoUIToolkit.TargetWorldPosInfo.IsTransform)
            {
                return;
            }

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            if (parent == null)
            {
                return;
            }

            _positionHandleInfoUIToolkit.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(_positionHandleInfoUIToolkit.Space, property, info, parent);
        }

        // private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            if (_positionHandleInfoUIToolkit.TargetWorldPosInfo.Error != "")
            {
                return;
            }

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

            if(!OnSceneGUIInternal(sceneView, _positionHandleInfoUIToolkit)) {
                Debug.LogWarning("Target disposed, removing SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
            }
        }
        #endregion

#endif
    }
}
