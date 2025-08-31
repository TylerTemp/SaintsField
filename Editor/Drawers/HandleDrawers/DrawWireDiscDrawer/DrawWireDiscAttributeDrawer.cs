using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawWireDiscDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DrawWireDiscAttribute), true)]
    public partial class DrawWireDiscAttributeDrawer: SaintsPropertyDrawer
    {
        private class WireDiscInfo
        {
            public DrawWireDiscAttribute DrawWireDiscAttribute;
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            public Util.TargetWorldPosInfo TargetWorldPosInfo;
            public Transform SpaceTransform;

            public float Radius;
            public Color Color;
            public Vector3 Center;
            public Vector3 Normal;

            public string Id;
        }

        private static void OnSceneGUIInternal(SceneView _, WireDiscInfo wireDiscInfo)
        {
            UpdateWireDiscInfo(wireDiscInfo);

            if (!string.IsNullOrEmpty(wireDiscInfo.TargetWorldPosInfo.Error) || !string.IsNullOrEmpty(wireDiscInfo.Error))
            {
                return;
            }

            // Handles.Label(pos, labelInfo.ActualContent, labelInfo.GUIStyle);
            // Debug.Log(pos);

            HandleVisibility.SetInView(
                wireDiscInfo.Id,
                wireDiscInfo.SerializedProperty.propertyPath,
                wireDiscInfo.SerializedProperty.serializedObject.targetObject.name,
                EditorGUIUtility.IconContent("CircleCollider2D Icon").image as Texture2D);

            if (HandleVisibility.IsHidden(wireDiscInfo.Id))
            {
                return;
            }

            // Handles.DrawWireDisc(pos, Vector3.up, wireDiscInfo.Radius);
            using(new HandleColorScoop(wireDiscInfo.Color))
            {
                // Debug.Log(wireDiscInfo.Radius);
                // Debug.Log(wireDiscInfo.SpaceTransform.name);
                // Debug.Log(wireDiscInfo.Center);
                Handles.DrawWireDisc(wireDiscInfo.Center, wireDiscInfo.Normal, wireDiscInfo.Radius);
            }
        }

        private static void UpdateWireDiscInfo(WireDiscInfo wireDiscInfo)
        {
            if (!SerializedUtils.IsOk(wireDiscInfo.SerializedProperty))
            {
                wireDiscInfo.Error = "SerializedProperty disposed";
                return;
            }

            Type rawType = wireDiscInfo.MemberInfo is FieldInfo fi
                ? fi.FieldType
                : ((PropertyInfo)wireDiscInfo.MemberInfo).PropertyType;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(wireDiscInfo.SerializedProperty)? ReflectUtils.GetElementType(rawType): rawType;
            bool filedIsNumber = fieldType == typeof(float) || fieldType == typeof(double) || fieldType == typeof(int) || fieldType == typeof(long) || fieldType == typeof(short);

            if(filedIsNumber)
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (wireDiscInfo.SerializedProperty.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        wireDiscInfo.Radius = wireDiscInfo.SerializedProperty.intValue;
                        break;
                    case SerializedPropertyType.Float:
                        wireDiscInfo.Radius = wireDiscInfo.SerializedProperty.floatValue;
                        break;
#if UNITY_2021_2_OR_NEWER
                    case SerializedPropertyType.Enum:
                        wireDiscInfo.Radius = wireDiscInfo.SerializedProperty.enumValueFlag;
                        break;
#endif
                    default:
                        throw new ArgumentOutOfRangeException(nameof(wireDiscInfo.SerializedProperty.propertyType), wireDiscInfo.SerializedProperty.propertyType, null);
                }

            }
            else if(!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.RadiusCallback))
            {
                (string error, float result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.RadiusCallback, wireDiscInfo.DrawWireDiscAttribute.Radius, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    wireDiscInfo.Error = error;
                    return;
                }

                wireDiscInfo.Radius = result;
            }

            if (!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.ColorCallback))
            {
                (string error, Color result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.ColorCallback, wireDiscInfo.DrawWireDiscAttribute.Color, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    wireDiscInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                wireDiscInfo.Color = result;
            }

            if (!wireDiscInfo.TargetWorldPosInfo.IsTransform)
            {
                if(filedIsNumber)
                {
                    UpdateWireDiscInfoSpaceTrans(wireDiscInfo);
                    if (wireDiscInfo.Error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(wireDiscInfo.Error);
#endif
                        return;
                    }

                    wireDiscInfo.TargetWorldPosInfo = new Util.TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = true,
                        Transform = wireDiscInfo.SpaceTransform,
                        WorldPos = wireDiscInfo.SpaceTransform.position,
                    };
                }
                else
                {
                    wireDiscInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(
                        wireDiscInfo.DrawWireDiscAttribute.Space, wireDiscInfo.SerializedProperty,
                        wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                    if (wireDiscInfo.TargetWorldPosInfo.Error != "")
                    {
                        // Debug.Log(wireDiscInfo.TargetWorldPosInfo.Error);
                        wireDiscInfo.Error = wireDiscInfo.TargetWorldPosInfo.Error;
#if SAINTSFIELD_DEBUG
                        Debug.LogError(wireDiscInfo.Error);
#endif
                        return;
                    }
                }
            }

            // Debug.Log(wireDiscInfo.TargetWorldPosInfo.IsTransform);
            Vector3 center = wireDiscInfo.TargetWorldPosInfo.IsTransform
                ? wireDiscInfo.TargetWorldPosInfo.Transform.position
                : wireDiscInfo.TargetWorldPosInfo.WorldPos;

            Vector3 positionOffset = wireDiscInfo.DrawWireDiscAttribute.PosOffset;
            if (!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.PosOffsetCallback))
            {
                (string error, Vector3 result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.PosOffsetCallback, positionOffset, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    wireDiscInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                positionOffset = result;
            }

            Vector3 scale = Vector3.one;
            if(wireDiscInfo.DrawWireDiscAttribute.Space != null && positionOffset.sqrMagnitude > Mathf.Epsilon)
            {
                if (wireDiscInfo.TargetWorldPosInfo.IsTransform)
                {
                    scale = HandleUtils.GetLocalToWorldScale(wireDiscInfo.TargetWorldPosInfo.Transform);
                }
                else
                {
                    UpdateWireDiscInfoSpaceTrans(wireDiscInfo);
                    if (wireDiscInfo.Error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(wireDiscInfo.Error);
#endif
                        return;
                    }
                    scale = HandleUtils.GetLocalToWorldScale(wireDiscInfo.SpaceTransform);
                }
            }

            wireDiscInfo.Center = center + Vector3.Scale(positionOffset, scale);

            Vector3 normal = wireDiscInfo.DrawWireDiscAttribute.Normal;
            if (!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.NormalCallback))
            {
                (string error, Vector3 result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.NormalCallback, wireDiscInfo.DrawWireDiscAttribute.Normal, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    wireDiscInfo.Error = error;
                    return;
                }

                normal = result;
            }

            Quaternion rotate = wireDiscInfo.DrawWireDiscAttribute.Rot;
            if(!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.RotCallback))
            {
                (string error, Quaternion result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.RotCallback, wireDiscInfo.DrawWireDiscAttribute.Rot, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    wireDiscInfo.Error = error;
                    return;
                }

                rotate = result;

                if (wireDiscInfo.DrawWireDiscAttribute.Space != null)
                {
                    UpdateWireDiscInfoSpaceTrans(wireDiscInfo);
                    rotate *= wireDiscInfo.SpaceTransform.rotation;
                }
            }

            wireDiscInfo.Normal = rotate * normal;

            // wireDiscInfo.Center = center;
            wireDiscInfo.Error = "";
        }



        private static WireDiscInfo CreateWireDiscInfo(DrawWireDiscAttribute drawWireDiscAttribute, SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            WireDiscInfo wireDiscInfo = new WireDiscInfo
            {
                Error = "",

                DrawWireDiscAttribute = drawWireDiscAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Radius = drawWireDiscAttribute.Radius,
                Color = drawWireDiscAttribute.Color,
                // TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(drawWireDiscAttribute.Space, serializedProperty, memberInfo, parent),

                Id = SerializedUtils.GetUniqueId(serializedProperty),
            };
            return wireDiscInfo;
        }

        private static void UpdateWireDiscInfoSpaceTrans(WireDiscInfo wireDiscInfo)
        {
            if (wireDiscInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(wireDiscInfo.TargetWorldPosInfo,
                wireDiscInfo.DrawWireDiscAttribute.Space, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo,
                wireDiscInfo.Parent);

            if (error != "")
            {
                wireDiscInfo.Error = error;
                return;
            }

            wireDiscInfo.SpaceTransform = trans;
        }
    }
}
