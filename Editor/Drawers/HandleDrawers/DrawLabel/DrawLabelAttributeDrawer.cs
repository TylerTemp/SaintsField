using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawLabel
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DrawLabelAttribute), true)]
    public partial class DrawLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private class LabelInfo
        {
            public DrawLabelAttribute DrawLabelAttribute;
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;

            public Vector3 Center;
            public string Content;
            public Color Color;
            public Util.TargetWorldPosInfo TargetWorldPosInfo;

            public GUIStyle GUIStyle;
        }

        private static void OnSceneGUIInternal(SceneView _, LabelInfo labelInfo)
        {
            UpdateLabelInfo(labelInfo);

            if (!string.IsNullOrEmpty(labelInfo.Error))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(labelInfo.Error);
#endif
                return;
            }

            if (string.IsNullOrEmpty(labelInfo.Content))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning("Empty content");
#endif
                return;
            }

            if(labelInfo.GUIStyle == null)
            {
                labelInfo.GUIStyle = new GUIStyle
                {
                    normal = { textColor = labelInfo.Color },
                };
            }
            else
            {
                labelInfo.GUIStyle.normal.textColor = labelInfo.Color;
            }

            Handles.Label(labelInfo.Center, labelInfo.Content, labelInfo.GUIStyle);
        }

        private static void UpdateLabelInfo(LabelInfo labelInfo)
        {
            string propertyPath;
            try
            {
                propertyPath = labelInfo.SerializedProperty.propertyPath;
            }
            catch (NullReferenceException)
            {
                labelInfo.Error = "Property disposed";
                return;
            }
            catch (ObjectDisposedException)
            {
                labelInfo.Error = "Property disposed";
                return;
            }

            if (labelInfo.DrawLabelAttribute.IsCallback)
            {
                (string error, object value) =
                    Util.GetOf<object>(labelInfo.DrawLabelAttribute.Content, null, labelInfo.SerializedProperty, labelInfo.MemberInfo, labelInfo.Parent);

                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    labelInfo.Error = error;
                    return;
                }

                if (value is IWrapProp wrapProp)
                {
                    value = Util.GetWrapValue(wrapProp);
                }

                labelInfo.Content = $"{value}";
            }
            else if (labelInfo.DrawLabelAttribute.Content is null)  // use field name
            {
                string fieldName = labelInfo.SerializedProperty.displayName;
                int propIndex = SerializedUtils.PropertyPathIndex(propertyPath);
                labelInfo.Content = propIndex <= 0
                    ? fieldName
                    : $"{fieldName}[{propIndex}]";
            }

            if (!string.IsNullOrEmpty(labelInfo.DrawLabelAttribute.ColorCallback))
            {
                (string error, Color result) = Util.GetOf(labelInfo.DrawLabelAttribute.ColorCallback, labelInfo.DrawLabelAttribute.Color, labelInfo.SerializedProperty, labelInfo.MemberInfo, labelInfo.Parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    labelInfo.Error = error;
                    return;
                }

                // wireDiscInfo.Error = "";
                labelInfo.Color = result;
            }

            if (!labelInfo.TargetWorldPosInfo.IsTransform)
            {
                labelInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(labelInfo.DrawLabelAttribute.Space, labelInfo.SerializedProperty, labelInfo.MemberInfo, labelInfo.Parent);
            }

            labelInfo.Center = labelInfo.TargetWorldPosInfo.IsTransform
                ? labelInfo.TargetWorldPosInfo.Transform.position
                : labelInfo.TargetWorldPosInfo.WorldPos;

            labelInfo.Error = "";
        }
    }
}
