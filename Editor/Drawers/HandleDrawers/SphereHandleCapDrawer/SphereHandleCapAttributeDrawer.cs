using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.SphereHandleCapDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SphereHandleCapAttribute), true)]
    public partial class SphereHandleCapAttributeDrawer: SaintsPropertyDrawer
    {
        private class SphereInfo
        {
            public SphereHandleCapAttribute SphereHandleCapAttribute;
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

        private static SphereInfo CreateSphereInfo(SphereHandleCapAttribute sphereHandleCapAttribute, SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            return new SphereInfo
            {
                Error = "",

                SphereHandleCapAttribute = sphereHandleCapAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Radius = sphereHandleCapAttribute.Radius,
                Color = sphereHandleCapAttribute.Color,
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

            // using(new GiamosColorScoop(sphereInfo.Color))
            // {
            //     Gizmos.DrawSphere(sphereInfo.Center, sphereInfo.Radius);
            // }
            // Handles.Draw
            using(new HandleColorScoop(sphereInfo.Color))
            {
                Handles.SphereHandleCap(
                    0,
                    sphereInfo.Center,
                    Quaternion.identity,
                    sphereInfo.Radius,
                    EventType.Repaint
                );
            }
        }

        private static void UpdateSphereInfo(SphereInfo sphereInfo)
        {
            Type rawType = sphereInfo.MemberInfo is FieldInfo fi
                ? fi.FieldType
                : ((PropertyInfo)sphereInfo.MemberInfo).PropertyType;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(sphereInfo.SerializedProperty)? ReflectUtils.GetElementType(rawType): rawType;
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
            else if(!string.IsNullOrEmpty(sphereInfo.SphereHandleCapAttribute.RadiusCallback))
            {
                (string error, float result) = Util.GetOf(sphereInfo.SphereHandleCapAttribute.RadiusCallback, sphereInfo.SphereHandleCapAttribute.Radius, sphereInfo.SerializedProperty, sphereInfo.MemberInfo, sphereInfo.Parent);
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

            if (!string.IsNullOrEmpty(sphereInfo.SphereHandleCapAttribute.ColorCallback))
            {
                (string error, Color result) = Util.GetOf(sphereInfo.SphereHandleCapAttribute.ColorCallback, sphereInfo.SphereHandleCapAttribute.Color, sphereInfo.SerializedProperty, sphereInfo.MemberInfo, sphereInfo.Parent);
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
                        sphereInfo.SphereHandleCapAttribute.Space, sphereInfo.SerializedProperty,
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

            Vector3 positionOffset = sphereInfo.SphereHandleCapAttribute.PosOffset;
            if (!string.IsNullOrEmpty(sphereInfo.SphereHandleCapAttribute.PosOffsetCallback))
            {
                (string error, Vector3 result) = Util.GetOf(sphereInfo.SphereHandleCapAttribute.PosOffsetCallback, positionOffset, sphereInfo.SerializedProperty, sphereInfo.MemberInfo, sphereInfo.Parent);
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
            if(sphereInfo.SphereHandleCapAttribute.Space != null && positionOffset.sqrMagnitude > Mathf.Epsilon)
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
                sphereInfo.SphereHandleCapAttribute.Space, sphereInfo.SerializedProperty, sphereInfo.MemberInfo,
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
