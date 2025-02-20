using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawSphereDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DrawSphereAttribute), true)]
    public partial class DrawSphereAttributeDrawer: SaintsPropertyDrawer
    {
        private class SphereInfo
        {
            public DrawSphereAttribute DrawSphereAttribute;
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            public Util.TargetWorldPosInfo TargetWorldPosInfo;
            public Transform SpaceTransform;

            public float Radius;
            public Color Color;
            public Vector3 Center;
        }

        private static SphereInfo CreateSphereInfo(DrawSphereAttribute drawSphereAttribute, SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            return new SphereInfo
            {
                Error = "",

                DrawSphereAttribute = drawSphereAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Radius = drawSphereAttribute.Radius,
                Color = drawSphereAttribute.Color,
                // TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(drawWireDiscAttribute.Space, serializedProperty, memberInfo, parent),
            };
        }

        private static void OnSceneGUIInternal(SceneView _, SphereInfo sphereInfo)
        {
            UpdateSphereInfo(sphereInfo);

            if (!string.IsNullOrEmpty(sphereInfo.TargetWorldPosInfo.Error))
            {
                return;
            }

            using(new GiamosColorScoop(sphereInfo.Color))
            {
                Gizmos.DrawSphere(sphereInfo.Center, sphereInfo.Radius);
            }
        }

        private static void UpdateSphereInfo(SphereInfo sphereInfo)
        {
            Type fieldType = ReflectUtils.GetElementType(sphereInfo.MemberInfo is FieldInfo fi
                ? fi.FieldType
                : ((PropertyInfo)sphereInfo.MemberInfo).PropertyType);
            bool filedIsNumber = fieldType == typeof(float) || fieldType == typeof(double) || fieldType == typeof(int) || fieldType == typeof(long) || fieldType == typeof(short);

            if(filedIsNumber)
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (sphereInfo.SerializedProperty.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        sphereInfo.Radius = sphereInfo.SerializedProperty.intValue;
                        break;
                    case SerializedPropertyType.Float:
                        sphereInfo.Radius = sphereInfo.SerializedProperty.floatValue;
                        break;
#if UNITY_2021_2_OR_NEWER
                    case SerializedPropertyType.Enum:
                        sphereInfo.Radius = sphereInfo.SerializedProperty.enumValueFlag;
                        break;
#endif
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sphereInfo.SerializedProperty.propertyType), sphereInfo.SerializedProperty.propertyType, null);
                }

            }
            else if(!string.IsNullOrEmpty(sphereInfo.DrawSphereAttribute.RadiusCallback))
            {
                (string error, float result) = Util.GetOf(sphereInfo.DrawSphereAttribute.RadiusCallback, sphereInfo.DrawSphereAttribute.Radius, sphereInfo.SerializedProperty, sphereInfo.MemberInfo, sphereInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    sphereInfo.Error = error;
                    return;
                }

                sphereInfo.Radius = result;
            }

            if (!string.IsNullOrEmpty(sphereInfo.DrawSphereAttribute.ColorCallback))
            {
                (string error, Color result) = Util.GetOf(sphereInfo.DrawSphereAttribute.ColorCallback, sphereInfo.DrawSphereAttribute.Color, sphereInfo.SerializedProperty, sphereInfo.MemberInfo, sphereInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    sphereInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                sphereInfo.Color = result;
            }

            if (!sphereInfo.TargetWorldPosInfo.IsTransform)
            {
                if(filedIsNumber)
                {
                    UpdateSphereInfoSpaceTrans(sphereInfo);
                    if (sphereInfo.Error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(sphereInfo.Error);
#endif
                        return;
                    }

                    sphereInfo.TargetWorldPosInfo = new Util.TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = true,
                        Transform = sphereInfo.SpaceTransform,
                        WorldPos = sphereInfo.SpaceTransform.position,
                    };
                }
                else
                {
                    sphereInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(
                        sphereInfo.DrawSphereAttribute.Space, sphereInfo.SerializedProperty,
                        sphereInfo.MemberInfo, sphereInfo.Parent);
                    if (sphereInfo.TargetWorldPosInfo.Error != "")
                    {
                        // Debug.Log(wireDiscInfo.TargetWorldPosInfo.Error);
                        sphereInfo.Error = sphereInfo.TargetWorldPosInfo.Error;
#if SAINTSFIELD_DEBUG
                        Debug.LogError(sphereInfo.Error);
#endif
                        return;
                    }
                }
            }

            // Debug.Log(wireDiscInfo.TargetWorldPosInfo.IsTransform);
            Vector3 center = sphereInfo.TargetWorldPosInfo.IsTransform
                ? sphereInfo.TargetWorldPosInfo.Transform.position
                : sphereInfo.TargetWorldPosInfo.WorldPos;

            Vector3 positionOffset = sphereInfo.DrawSphereAttribute.PosOffset;
            if (!string.IsNullOrEmpty(sphereInfo.DrawSphereAttribute.PosOffsetCallback))
            {
                (string error, Vector3 result) = Util.GetOf(sphereInfo.DrawSphereAttribute.PosOffsetCallback, positionOffset, sphereInfo.SerializedProperty, sphereInfo.MemberInfo, sphereInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    sphereInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                positionOffset = result;
            }

            Vector3 scale = Vector3.one;
            if(sphereInfo.DrawSphereAttribute.Space != null && positionOffset.sqrMagnitude > Mathf.Epsilon)
            {
                if (sphereInfo.TargetWorldPosInfo.IsTransform)
                {
                    scale = HandleUtils.GetLocalToWorldScale(sphereInfo.TargetWorldPosInfo.Transform);
                }
                else
                {
                    UpdateSphereInfoSpaceTrans(sphereInfo);
                    if (sphereInfo.Error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(sphereInfo.Error);
#endif
                        return;
                    }
                    scale = HandleUtils.GetLocalToWorldScale(sphereInfo.SpaceTransform);
                }
            }

            sphereInfo.Center = center + Vector3.Scale(positionOffset, scale);
            sphereInfo.Error = "";
        }

        private static void UpdateSphereInfoSpaceTrans(SphereInfo sphereInfo)
        {
            if (sphereInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(sphereInfo.TargetWorldPosInfo,
                sphereInfo.DrawSphereAttribute.Space, sphereInfo.SerializedProperty, sphereInfo.MemberInfo,
                sphereInfo.Parent);

            if (error != "")
            {
                sphereInfo.Error = error;
                return;
            }

            sphereInfo.SpaceTransform = trans;
        }
    }
}
