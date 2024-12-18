using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.PositionHandle
{
    [CustomPropertyDrawer(typeof(PositionHandleAttribute))]
    public partial class PositionHandleAttributeDrawer: SaintsPropertyDrawer
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
            // SceneView.duringSceneGui -= OnSceneGUIIMGUI;
#if UNITY_2021_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
#endif
        }
    }
}
