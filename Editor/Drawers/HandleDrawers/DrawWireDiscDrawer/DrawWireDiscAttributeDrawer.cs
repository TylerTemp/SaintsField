using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawWireDiscDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
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
        }

        private static void OnSceneGUIInternal(SceneView _, WireDiscInfo wireDiscInfo)
        {
            UpdateWireDiscInfo(wireDiscInfo);

            if (!string.IsNullOrEmpty(wireDiscInfo.TargetWorldPosInfo.Error))
            {
                return;
            }

            // Handles.Label(pos, labelInfo.ActualContent, labelInfo.GUIStyle);
            // Debug.Log(pos);

            // Handles.DrawWireDisc(pos, Vector3.up, wireDiscInfo.Radius);
            using(new HandleColorScoop(wireDiscInfo.Color))
            {
                Handles.DrawWireDisc(wireDiscInfo.Center, wireDiscInfo.Normal, wireDiscInfo.Radius);
            }
        }

        private static void UpdateWireDiscInfo(WireDiscInfo wireDiscInfo)
        {
            if(!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.RadiusCallback))
            {
                (string error, float result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.RadiusCallback, wireDiscInfo.DrawWireDiscAttribute.Radius, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
                    wireDiscInfo.Error = error;
                    return;
                }

                wireDiscInfo.Error = "";
                wireDiscInfo.Radius = result;
            }

            if (wireDiscInfo.DrawWireDiscAttribute.ColorIsCallback)
            {
                (string error, Color result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.ColorCallback, wireDiscInfo.DrawWireDiscAttribute.Color, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
                    wireDiscInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                wireDiscInfo.Color = result;
            }

            if (!wireDiscInfo.TargetWorldPosInfo.IsTransform)
            {
                wireDiscInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(wireDiscInfo.DrawWireDiscAttribute.Space, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
            }

            Vector3 center = wireDiscInfo.TargetWorldPosInfo.IsTransform
                ? wireDiscInfo.TargetWorldPosInfo.Transform.position
                : wireDiscInfo.TargetWorldPosInfo.WorldPos;

            Vector3 positionOffset = wireDiscInfo.DrawWireDiscAttribute.PosOffset;
            if (!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.PosOffsetCallback))
            {
                (string error, Vector3 result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.PosOffsetCallback, positionOffset, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
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
                    scale = GetLocalToWorldScale(wireDiscInfo.TargetWorldPosInfo.Transform);
                }
                else
                {
                    UpdateWireDiscInfoSpaceTrans(wireDiscInfo);
                    if (wireDiscInfo.Error != "")
                    {
                        return;
                    }
                    scale = GetLocalToWorldScale(wireDiscInfo.SpaceTransform);
                }
            }

            wireDiscInfo.Center = center + Vector3.Scale(positionOffset, scale);

            Vector3 normal = wireDiscInfo.DrawWireDiscAttribute.Normal;
            if (!string.IsNullOrEmpty(wireDiscInfo.DrawWireDiscAttribute.NormalCallback))
            {
                (string error, Vector3 result) = Util.GetOf(wireDiscInfo.DrawWireDiscAttribute.NormalCallback, wireDiscInfo.DrawWireDiscAttribute.Normal, wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
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

        private static Vector3 GetLocalToWorldScale(Transform transform)
        {
            Vector3 worldScale = transform.localScale;
            Transform parent = transform.parent;

            while (parent != null)
            {
                worldScale = Vector3.Scale(worldScale,parent.localScale);
                parent = parent.parent;
            }

            return worldScale;
        }

        private static WireDiscInfo CreateWireDiscInfo(DrawWireDiscAttribute drawWireDiscAttribute, SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            WireDiscInfo wireDiscInfo = new WireDiscInfo
            {
                DrawWireDiscAttribute = drawWireDiscAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Radius = drawWireDiscAttribute.Radius,
                Color = drawWireDiscAttribute.Color,
                TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(drawWireDiscAttribute.Space, serializedProperty, memberInfo, parent),
            };
            return wireDiscInfo;
        }

        private static void UpdateWireDiscInfoSpaceTrans(WireDiscInfo wireDiscInfo)
        {
            if (wireDiscInfo.SpaceTransform != null)
            {
                return;
            }

            if (wireDiscInfo.TargetWorldPosInfo.Transform != null)
            {
                wireDiscInfo.SpaceTransform = wireDiscInfo.TargetWorldPosInfo.Transform;
                return;
            }

            string parentSpace = wireDiscInfo.DrawWireDiscAttribute.Space;

            Debug.Assert(parentSpace != null);

            Transform spaceTrans;

            Object spaceTarget;
            if (parentSpace == "this")
            {
                spaceTarget = wireDiscInfo.SerializedProperty.serializedObject.targetObject;
            }
            else
            {
                (string error, Object result) = Util.GetOf<Object>(parentSpace, null,
                    wireDiscInfo.SerializedProperty, wireDiscInfo.MemberInfo, wireDiscInfo.Parent);
                if (error != "")
                {
                    wireDiscInfo.Error = error;
                    return;
                }
                spaceTarget = result;
            }

            switch (spaceTarget)
            {
                case GameObject go:
                    spaceTrans = go.transform;
                    break;
                case Component comp:
                    spaceTrans = comp.transform;
                    break;
                default:
                    wireDiscInfo.Error = $"Space {parentSpace} is not a GameObject or Component";
                    return;
            }

            wireDiscInfo.SpaceTransform = spaceTrans;
        }
    }
}
