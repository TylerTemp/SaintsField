using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.PositionHandle
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(PositionHandleAttribute), true)]
    public partial class PositionHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private static void SetValue(Vector3 newTargetPosition, string space, SerializedProperty property, MemberInfo info, object parent)
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
            if (space != null)  // world to self
            {
                (string error, Transform container) = GetContainingTransform(property);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
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
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;
            public string Space;

            public string Error;
            public Vector3 Center;
            public Util.TargetWorldPosInfo TargetWorldPosInfo;
        }

        private static void OnSceneGUIInternal(SceneView _, PositionHandleInfo positionHandleInfo)
        {
            UpdatePositionHandleInfo(positionHandleInfo);
            if (positionHandleInfo.Error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(positionHandleInfo.Error);
#endif
                return;
            }

            Vector3 worldPos = positionHandleInfo.Center;

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                // Debug.Log(worldPos);
                Vector3 newTargetPosition = Handles.PositionHandle(worldPos, Quaternion.identity);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    SetValue(newTargetPosition, positionHandleInfo.Space, positionHandleInfo.SerializedProperty, positionHandleInfo.MemberInfo, positionHandleInfo.Parent);
                    positionHandleInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void UpdatePositionHandleInfo(PositionHandleInfo positionHandleInfo)
        {
            try
            {
                string _ = positionHandleInfo.SerializedProperty.propertyPath;
            }
            catch (NullReferenceException)
            {
                positionHandleInfo.Error = "Property disposed";
                return;
            }
            catch (ObjectDisposedException)
            {
                positionHandleInfo.Error = "Property disposed";
                return;
            }

            if (positionHandleInfo.TargetWorldPosInfo.IsTransform)
            {
                positionHandleInfo.Center = positionHandleInfo.TargetWorldPosInfo.Transform.position;
                // Debug.Log(positionHandleInfo.Center);
                positionHandleInfo.Error = "";
                return;
            }

            positionHandleInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(positionHandleInfo.Space, positionHandleInfo.SerializedProperty, positionHandleInfo.MemberInfo, positionHandleInfo.Parent);
            if (positionHandleInfo.TargetWorldPosInfo.Error != "")
            {
                positionHandleInfo.Error = positionHandleInfo.TargetWorldPosInfo.Error;
#if SAINTSFIELD_DEBUG
                Debug.LogError(positionHandleInfo.Error);
#endif
                return;
            }

            positionHandleInfo.Center = positionHandleInfo.TargetWorldPosInfo.WorldPos;

            // Debug.Log(positionHandleInfo.Center);
            positionHandleInfo.Error = "";
        }
    }
}
