using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.RadiusHandleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(RadiusHandleAttribute), true)]
    public partial class RadiusHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private class RadiusHandleInfo
        {
            public RadiusHandleAttribute RadiusHandleAttribute;
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            // public Util.TargetWorldPosInfo TargetWorldPosInfo;
            public Transform SpaceTransform;

            public float Radius;
            public Color Color;
            public Vector3 Center;
            public Quaternion Rotation;
            public float ScaleX;

            public string Id;
        }

        private static void OnSceneGUIInternal(SceneView _, RadiusHandleInfo radiusHandleInfo)
        {
            UpdateRadiusHandleInfo(radiusHandleInfo);

            if (!string.IsNullOrEmpty(radiusHandleInfo.Error))
            {
                return;
            }

            // Handles.Label(pos, labelInfo.ActualContent, labelInfo.GUIStyle);
            // Debug.Log(pos);

            HandleVisibility.SetInView(
                radiusHandleInfo.Id,
                radiusHandleInfo.SerializedProperty.propertyPath,
                radiusHandleInfo.SerializedProperty.serializedObject.targetObject.name,
                EditorGUIUtility.IconContent("SphereCollider Icon").image as Texture2D);

            if (HandleVisibility.IsHidden(radiusHandleInfo.Id))
            {
                return;
            }

            float useRadius = radiusHandleInfo.Radius * radiusHandleInfo.ScaleX;
            if (useRadius <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion rotation = Tools.pivotRotation == PivotRotation.Local
                ? radiusHandleInfo.Rotation
                : Quaternion.identity;

            // Handles.DrawWireDisc(pos, Vector3.up, wireDiscInfo.Radius);
            // radiusHandleInfo.Color = Color.red;
            // Debug.Log($"render with color {radiusHandleInfo.Color}");
            using(new HandleColorScoop(radiusHandleInfo.Color))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                // Debug.Log(wireDiscInfo.Radius);
                // Debug.Log(wireDiscInfo.SpaceTransform.name);
                // Debug.Log(wireDiscInfo.Center);
                float newRadius = Handles.RadiusHandle(rotation, radiusHandleInfo.Center,
                    useRadius);
                if (changed.changed)
                {
                    UpdatePropertyValue(radiusHandleInfo.SerializedProperty, newRadius / radiusHandleInfo.ScaleX);
                    // Debug.Log(newRadius * radiusHandleInfo.ScaleX);
                }
            }
        }

        private static void UpdatePropertyValue(SerializedProperty serializedProperty, float newRadius)
        {
            switch (serializedProperty.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    break;

                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    serializedProperty.uintValue = (uint)newRadius;
                    break;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    serializedProperty.intValue = (int)newRadius;
                    break;
                case SerializedPropertyNumericType.Int64:
                    serializedProperty.longValue = (long)newRadius;
                    break;
                case SerializedPropertyNumericType.UInt64:
                    serializedProperty.ulongValue = (ulong)newRadius;
                    break;
                case SerializedPropertyNumericType.Float:
                    serializedProperty.floatValue = newRadius;
                    break;
                case SerializedPropertyNumericType.Double:
                    serializedProperty.doubleValue = newRadius;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void UpdateRadiusHandleInfo(RadiusHandleInfo radiusHandleInfo)
        {
            if (!SerializedUtils.IsOk(radiusHandleInfo.SerializedProperty))
            {
                radiusHandleInfo.Error = "SerializedProperty disposed";
                return;
            }

            UpdateRadiusHandleInfoSpaceTrans(radiusHandleInfo);
            if (radiusHandleInfo.Error != string.Empty)
            {
                return;
            }

            radiusHandleInfo.Error = string.Empty;

            switch (radiusHandleInfo.SerializedProperty.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    radiusHandleInfo.Error = $"Unknown type {radiusHandleInfo.SerializedProperty.propertyType}";
                    break;

                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    radiusHandleInfo.Radius = radiusHandleInfo.SerializedProperty.uintValue;
                    break;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    radiusHandleInfo.Radius = radiusHandleInfo.SerializedProperty.intValue;
                    break;
                case SerializedPropertyNumericType.Int64:
                    radiusHandleInfo.Radius = radiusHandleInfo.SerializedProperty.longValue;
                    break;
                case SerializedPropertyNumericType.UInt64:
                    radiusHandleInfo.Radius = radiusHandleInfo.SerializedProperty.ulongValue;
                    break;
                case SerializedPropertyNumericType.Float:
                    radiusHandleInfo.Radius = radiusHandleInfo.SerializedProperty.floatValue;
                    break;
                case SerializedPropertyNumericType.Double:
                    radiusHandleInfo.Radius = (float)radiusHandleInfo.SerializedProperty.doubleValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(radiusHandleInfo.RadiusHandleAttribute.ColorCallback))
            {
                (string error, MemberInfo _, Color result) = Util.GetOf(radiusHandleInfo.RadiusHandleAttribute.ColorCallback, radiusHandleInfo.RadiusHandleAttribute.Color, radiusHandleInfo.SerializedProperty, radiusHandleInfo.MemberInfo, radiusHandleInfo.Parent, null);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    radiusHandleInfo.Error = error;
                    return;
                }

                radiusHandleInfo.Color = result;
            }

            // Debug.Log(radiusHandleInfo.Color);

            // Debug.Log(wireDiscInfo.TargetWorldPosInfo.IsTransform);
            Vector3 center = radiusHandleInfo.SpaceTransform.position;
            radiusHandleInfo.Rotation = radiusHandleInfo.SpaceTransform.rotation;

            Vector3 positionOffset = radiusHandleInfo.RadiusHandleAttribute.PosOffset;
            if (!string.IsNullOrEmpty(radiusHandleInfo.RadiusHandleAttribute.PosOffsetCallback))
            {
                (string error, MemberInfo _, Vector3 result) = Util.GetOf(radiusHandleInfo.RadiusHandleAttribute.PosOffsetCallback, positionOffset, radiusHandleInfo.SerializedProperty, radiusHandleInfo.MemberInfo, radiusHandleInfo.Parent, null);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    radiusHandleInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                positionOffset = result;
            }

            Vector3 scale;
            if (radiusHandleInfo.RadiusHandleAttribute.Space == null)
            {
                scale = Vector3.one;
                radiusHandleInfo.Rotation = Quaternion.identity;
            }
            else
            {
                scale = HandleUtils.GetLocalToWorldScale(radiusHandleInfo.SpaceTransform);
            }

            radiusHandleInfo.ScaleX = scale.x;
            radiusHandleInfo.Center = center + Vector3.Scale(positionOffset, scale);
        }

        private static RadiusHandleInfo CreateRadiusHandleInfo(RadiusHandleAttribute radiusHandleAttribute, SerializedProperty serializedProperty, int index, MemberInfo memberInfo, object parent)
        {
            return new RadiusHandleInfo
            {
                Error = "",

                RadiusHandleAttribute = radiusHandleAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Color = radiusHandleAttribute.Color,
                // TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(drawWireDiscAttribute.Space, serializedProperty, memberInfo, parent),

                Id = $"{SerializedUtils.GetUniqueId(serializedProperty)}_{index}",
            };
        }

        private static void UpdateRadiusHandleInfoSpaceTrans(RadiusHandleInfo radiusHandleInfo)
        {
            if (radiusHandleInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(
                radiusHandleInfo.RadiusHandleAttribute.Space, radiusHandleInfo.SerializedProperty, radiusHandleInfo.MemberInfo,
                radiusHandleInfo.Parent);

            if (error != "")
            {
                radiusHandleInfo.Error = error;
                return;
            }

            radiusHandleInfo.SpaceTransform = trans;
        }
    }
}
