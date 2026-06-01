using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.SliderHandleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SliderHandleAttribute), true)]
    public partial class SliderHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private class SliderHandleInfo
        {
            public SliderHandleAttribute SliderHandleAttribute;
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            public Transform SpaceTransform;

            public float HandleValue;
            public Color Color;
            public Vector3 StartPoint;
            public Vector3 DirectionWorld;
            public Vector3 DirectionLocal;
            public float Size;
            public float Snap;

            public string Id;
        }



        private static void OnSceneGUIInternal(SceneView _, SliderHandleInfo sliderHandleInfo)
        {
            UpdateSliderHandleInfo(sliderHandleInfo);
            if (!string.IsNullOrEmpty(sliderHandleInfo.Error))
            {
                return;
            }

            HandleVisibility.SetInView(
                sliderHandleInfo.Id,
                sliderHandleInfo.SerializedProperty.propertyPath,
                sliderHandleInfo.SerializedProperty.serializedObject.targetObject.name,
                EditorGUIUtility.IconContent("MoveTool On").image as Texture2D);

            if (HandleVisibility.IsHidden(sliderHandleInfo.Id))
            {
                return;
            }



            Vector3 startPoint = sliderHandleInfo.StartPoint;
            Vector3 directionWorld = sliderHandleInfo.DirectionWorld;
            Vector3 sliderDirection = sliderHandleInfo.HandleValue < 0f
                ? -directionWorld
                : directionWorld;
            Vector3 endPoint = startPoint + sliderHandleInfo.HandleValue * directionWorld;

            float useSize = sliderHandleInfo.Size <= 0f
                ? HandleUtility.GetHandleSize(endPoint)
                : sliderHandleInfo.Size;
            float useSnap = sliderHandleInfo.Snap > 0f
                ? sliderHandleInfo.Snap
                : 0f;

            using (new HandleColorScoop(sliderHandleInfo.Color))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                // float dottedScreenSize = Mathf.Clamp(useSize * 0.2f, 2f, 8f);
                Vector3 newEndPoint = Handles.Slider(
                    endPoint,
                    sliderDirection,
                    useSize,
                    (id, position, rotation, size, type) =>
                    {
                        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                        switch (type)
                        {
                            case EventType.MouseMove:
                            case EventType.Layout:
                                HandleUtility.AddControl(id, HandleUtility.DistanceToLine(startPoint, endPoint));
                                break;
                            case EventType.Repaint:
                                Handles.DrawLine(startPoint, endPoint);
                                break;
                        }

                        Util.ConeTipHandleCap(id, position, rotation, size, type);
                    },
                    useSnap);
                if (changed.changed)
                {
                    Vector3 deltaDirection = newEndPoint - startPoint;
                    Vector3 axisDirection;
                    if (sliderHandleInfo.SliderHandleAttribute.Space != null)
                    {
                        deltaDirection = sliderHandleInfo.SpaceTransform.InverseTransformDirection(deltaDirection);
                        axisDirection = sliderHandleInfo.DirectionLocal;
                    }
                    else
                    {
                        axisDirection = directionWorld;
                    }

                    float newValue = Vector3.Dot(deltaDirection, axisDirection);
                    UpdatePropertyValue(sliderHandleInfo.SerializedProperty, newValue);
                }
            }
        }

        private static void UpdatePropertyValue(SerializedProperty serializedProperty, float newValue)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    serializedProperty.vector2Value = new Vector2(newValue, newValue);
                    break;
                case SerializedPropertyType.Vector2Int:
                {
                    int castValue = (int)newValue;
                    serializedProperty.vector2IntValue = new Vector2Int(castValue, castValue);
                    break;
                }
                case SerializedPropertyType.Vector3:
                    serializedProperty.vector3Value = new Vector3(newValue, newValue, newValue);
                    break;
                case SerializedPropertyType.Vector3Int:
                {
                    int castValue = (int)newValue;
                    serializedProperty.vector3IntValue = new Vector3Int(castValue, castValue, castValue);
                    break;
                }
                case SerializedPropertyType.Vector4:
                    serializedProperty.vector4Value = new Vector4(newValue, newValue, newValue, newValue);
                    break;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                    UpdateNumericPropertyValue(serializedProperty, newValue);
                    break;
                default:
                    return;
            }

            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void UpdateNumericPropertyValue(SerializedProperty serializedProperty, float newValue)
        {
            switch (serializedProperty.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    break;
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    serializedProperty.uintValue = (uint)newValue;
                    break;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    serializedProperty.intValue = (int)newValue;
                    break;
                case SerializedPropertyNumericType.Int64:
                    serializedProperty.longValue = (long)newValue;
                    break;
                case SerializedPropertyNumericType.UInt64:
                    serializedProperty.ulongValue = (ulong)newValue;
                    break;
                case SerializedPropertyNumericType.Float:
                    serializedProperty.floatValue = newValue;
                    break;
                case SerializedPropertyNumericType.Double:
                    serializedProperty.doubleValue = newValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void UpdateSliderHandleInfo(SliderHandleInfo sliderHandleInfo)
        {
            if (!SerializedUtils.IsOk(sliderHandleInfo.SerializedProperty))
            {
                sliderHandleInfo.Error = "SerializedProperty disposed";
                return;
            }

            UpdateSliderHandleInfoSpaceTrans(sliderHandleInfo);
            if (!string.IsNullOrEmpty(sliderHandleInfo.Error))
            {
                return;
            }

            sliderHandleInfo.Error = string.Empty;
            if (!TryGetPropertyValue(sliderHandleInfo.SerializedProperty, out float handleValue))
            {
                sliderHandleInfo.Error = $"{sliderHandleInfo.SerializedProperty.propertyType} is not supported";
                return;
            }
            sliderHandleInfo.HandleValue = handleValue;

            if (!string.IsNullOrEmpty(sliderHandleInfo.SliderHandleAttribute.ColorCallback))
            {
                (string colorError, MemberInfo _, Color colorResult) = Util.GetOf(
                    sliderHandleInfo.SliderHandleAttribute.ColorCallback,
                    sliderHandleInfo.SliderHandleAttribute.Color,
                    sliderHandleInfo.SerializedProperty,
                    sliderHandleInfo.MemberInfo,
                    sliderHandleInfo.Parent,
                    null);
                if (colorError != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(colorError);
#endif
                    sliderHandleInfo.Error = colorError;
                    return;
                }

                sliderHandleInfo.Color = colorResult;
            }

            Vector3 positionOffset = sliderHandleInfo.SliderHandleAttribute.PosOffset;
            if (!string.IsNullOrEmpty(sliderHandleInfo.SliderHandleAttribute.PosOffsetCallback))
            {
                (string offsetError, MemberInfo _, Vector3 offsetResult) = Util.GetOf(
                    sliderHandleInfo.SliderHandleAttribute.PosOffsetCallback,
                    positionOffset,
                    sliderHandleInfo.SerializedProperty,
                    sliderHandleInfo.MemberInfo,
                    sliderHandleInfo.Parent,
                    null);
                if (offsetError != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(offsetError);
#endif
                    sliderHandleInfo.Error = offsetError;
                    return;
                }

                positionOffset = offsetResult;
            }

            Vector3 scale = sliderHandleInfo.SliderHandleAttribute.Space == null
                ? Vector3.one
                : HandleUtils.GetLocalToWorldScale(sliderHandleInfo.SpaceTransform);
            sliderHandleInfo.StartPoint = sliderHandleInfo.SpaceTransform.position + Vector3.Scale(positionOffset, scale);

            Vector3 direction = sliderHandleInfo.SliderHandleAttribute.Direction;
            if (!string.IsNullOrEmpty(sliderHandleInfo.SliderHandleAttribute.DirectionCallback))
            {
                (string directionError, MemberInfo _, Vector3 directionResult) = Util.GetOf(
                    sliderHandleInfo.SliderHandleAttribute.DirectionCallback,
                    direction,
                    sliderHandleInfo.SerializedProperty,
                    sliderHandleInfo.MemberInfo,
                    sliderHandleInfo.Parent,
                    null);
                if (directionError != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(directionError);
#endif
                    sliderHandleInfo.Error = directionError;
                    return;
                }

                direction = directionResult;
            }

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                sliderHandleInfo.Error = "Direction cannot be zero";
                return;
            }

            sliderHandleInfo.DirectionLocal = direction.normalized;

            if (sliderHandleInfo.SliderHandleAttribute.Space != null)
            {
                direction = sliderHandleInfo.SpaceTransform.TransformDirection(direction);
            }

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                sliderHandleInfo.Error = "Direction cannot be zero";
                return;
            }

            sliderHandleInfo.DirectionWorld = direction.normalized;
            sliderHandleInfo.Size = sliderHandleInfo.SliderHandleAttribute.Size;
            sliderHandleInfo.Snap = sliderHandleInfo.SliderHandleAttribute.Snap;
        }

        private static bool TryGetPropertyValue(SerializedProperty serializedProperty, out float value)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    value = serializedProperty.vector2Value.x;
                    return true;
                case SerializedPropertyType.Vector2Int:
                    value = serializedProperty.vector2IntValue.x;
                    return true;
                case SerializedPropertyType.Vector3:
                    value = serializedProperty.vector3Value.x;
                    return true;
                case SerializedPropertyType.Vector3Int:
                    value = serializedProperty.vector3IntValue.x;
                    return true;
                case SerializedPropertyType.Vector4:
                    value = serializedProperty.vector4Value.x;
                    return true;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                    return TryGetNumericPropertyValue(serializedProperty, out value);
                default:
                    value = 0f;
                    return false;
            }
        }

        private static bool TryGetNumericPropertyValue(SerializedProperty serializedProperty, out float value)
        {
            switch (serializedProperty.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    value = 0f;
                    return false;
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    value = serializedProperty.uintValue;
                    return true;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    value = serializedProperty.intValue;
                    return true;
                case SerializedPropertyNumericType.Int64:
                    value = serializedProperty.longValue;
                    return true;
                case SerializedPropertyNumericType.UInt64:
                    value = serializedProperty.ulongValue;
                    return true;
                case SerializedPropertyNumericType.Float:
                    value = serializedProperty.floatValue;
                    return true;
                case SerializedPropertyNumericType.Double:
                    value = (float)serializedProperty.doubleValue;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static SliderHandleInfo CreateSliderHandleInfo(SliderHandleAttribute sliderHandleAttribute,
            SerializedProperty serializedProperty, int index, MemberInfo memberInfo, object parent)
        {
            return new SliderHandleInfo
            {
                Error = "",
                SliderHandleAttribute = sliderHandleAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Color = sliderHandleAttribute.Color,
                Id = $"{SerializedUtils.GetUniqueId(serializedProperty)}_{index}",
            };
        }

        private static void UpdateSliderHandleInfoSpaceTrans(SliderHandleInfo sliderHandleInfo)
        {
            if (sliderHandleInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(
                sliderHandleInfo.SliderHandleAttribute.Space,
                sliderHandleInfo.SerializedProperty,
                sliderHandleInfo.MemberInfo,
                sliderHandleInfo.Parent);
            if (error != "")
            {
                sliderHandleInfo.Error = error;
                return;
            }

            sliderHandleInfo.SpaceTransform = trans;
        }
    }
}
