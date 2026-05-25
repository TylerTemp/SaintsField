using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.PrimitiveBoundsHandleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(PrimitiveBoundsHandleAttribute), true)]
    public partial class PrimitiveBoundsHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private class PrimitiveBoundsHandleInfo
        {
            public PrimitiveBoundsHandleAttribute PrimitiveBoundsHandleAttribute;
            public SerializedProperty SerializedProperty;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            public Transform SpaceTransform;

            public Color Color;
            public Vector3 PosOffset;
            public Matrix4x4 HandleMatrix;
            public BoxBoundsHandle BoxBoundsHandle;
            public bool Is2D;

            public string Id;
        }

        private static void OnSceneGUIInternal(SceneView _, PrimitiveBoundsHandleInfo handleInfo)
        {
            UpdatePrimitiveBoundsHandleInfo(handleInfo);

            if (!string.IsNullOrEmpty(handleInfo.Error))
            {
                return;
            }

            HandleVisibility.SetInView(
                handleInfo.Id,
                handleInfo.SerializedProperty.propertyPath,
                handleInfo.SerializedProperty.serializedObject.targetObject.name,
                EditorGUIUtility.IconContent(handleInfo.Is2D ? "BoxCollider2D Icon" : "BoxCollider Icon").image as Texture2D);

            if (HandleVisibility.IsHidden(handleInfo.Id))
            {
                return;
            }

            using (new HandleColorScoop(handleInfo.Color))
            using (new Handles.DrawingScope(handleInfo.HandleMatrix))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                handleInfo.BoxBoundsHandle.DrawHandle();
                if (changed.changed)
                {
                    UpdatePropertyValue(handleInfo);
                }
            }
        }

        private static void UpdatePropertyValue(PrimitiveBoundsHandleInfo handleInfo)
        {
            SerializedProperty serializedProperty = handleInfo.SerializedProperty;
            Vector3 center = handleInfo.BoxBoundsHandle.center - handleInfo.PosOffset;
            Vector3 size = handleInfo.BoxBoundsHandle.size;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Bounds:
                    serializedProperty.boundsValue = new Bounds(center, size);
                    break;
#if UNITY_2021_2_OR_NEWER
                case SerializedPropertyType.BoundsInt:
                {
                    Vector3Int sizeInt = new Vector3Int(
                        Mathf.Max(0, Mathf.RoundToInt(size.x)),
                        Mathf.Max(0, Mathf.RoundToInt(size.y)),
                        Mathf.Max(0, Mathf.RoundToInt(size.z)));
                    Vector3 min = center - size * 0.5f;
                    Vector3Int minInt = Vector3Int.RoundToInt(min);
                    serializedProperty.boundsIntValue = new BoundsInt(minInt, sizeInt);
                    break;
                }
                case SerializedPropertyType.Rect:
                    serializedProperty.rectValue = new Rect(
                        center.x - size.x * 0.5f,
                        center.y - size.y * 0.5f,
                        Mathf.Max(0f, size.x),
                        Mathf.Max(0f, size.y));
                    break;
                case SerializedPropertyType.RectInt:
                {
                    Vector2 size2 = new Vector2(size.x, size.y);
                    Vector2 center2 = new Vector2(center.x, center.y);
                    Vector2 min2 = center2 - size2 * 0.5f;
                    Vector2Int minInt2 = Vector2Int.RoundToInt(min2);
                    Vector2Int sizeInt2 = new Vector2Int(
                        Mathf.Max(0, Mathf.RoundToInt(size2.x)),
                        Mathf.Max(0, Mathf.RoundToInt(size2.y)));
                    serializedProperty.rectIntValue = new RectInt(minInt2, sizeInt2);
                    break;
                }
#endif
                default:
                    handleInfo.Error = $"Unsupported type {serializedProperty.propertyType}";
                    return;
            }

            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void UpdatePrimitiveBoundsHandleInfo(PrimitiveBoundsHandleInfo handleInfo)
        {
            if (!SerializedUtils.IsOk(handleInfo.SerializedProperty))
            {
                handleInfo.Error = "SerializedProperty disposed";
                return;
            }

            if (handleInfo.PrimitiveBoundsHandleAttribute.Space != null)
            {
                UpdatePrimitiveBoundsHandleInfoSpaceTrans(handleInfo);
                if (handleInfo.Error != string.Empty)
                {
                    return;
                }
            }

            handleInfo.Error = string.Empty;

            if (!string.IsNullOrEmpty(handleInfo.PrimitiveBoundsHandleAttribute.ColorCallback))
            {
                (string colorError, MemberInfo _, Color colorResult) = Util.GetOf(
                    handleInfo.PrimitiveBoundsHandleAttribute.ColorCallback,
                    handleInfo.PrimitiveBoundsHandleAttribute.Color,
                    handleInfo.SerializedProperty,
                    handleInfo.MemberInfo,
                    handleInfo.Parent,
                    null);
                if (colorError != "")
                {
                    handleInfo.Error = colorError;
                    return;
                }

                handleInfo.Color = colorResult;
            }

            Vector3 positionOffset = handleInfo.PrimitiveBoundsHandleAttribute.PosOffset;
            if (!string.IsNullOrEmpty(handleInfo.PrimitiveBoundsHandleAttribute.PosOffsetCallback))
            {
                (string posError, MemberInfo _, Vector3 posResult) = Util.GetOf(
                    handleInfo.PrimitiveBoundsHandleAttribute.PosOffsetCallback,
                    positionOffset,
                    handleInfo.SerializedProperty,
                    handleInfo.MemberInfo,
                    handleInfo.Parent,
                    null);
                if (posError != "")
                {
                    handleInfo.Error = posError;
                    return;
                }

                positionOffset = posResult;
            }

            Matrix4x4 matrix = Matrix4x4.identity;
            if (handleInfo.PrimitiveBoundsHandleAttribute.Space != null)
            {
                Vector3 scale = HandleUtils.GetLocalToWorldScale(handleInfo.SpaceTransform);
                matrix = Matrix4x4.TRS(handleInfo.SpaceTransform.position, handleInfo.SpaceTransform.rotation, scale);
            }

            handleInfo.HandleMatrix = matrix;
            handleInfo.PosOffset = positionOffset;

            if (!TryGetBoundsValue(handleInfo.SerializedProperty, out Vector3 center, out Vector3 size, out bool is2D, out string error))
            {
                handleInfo.Error = error;
                return;
            }

            handleInfo.Is2D = is2D;
            handleInfo.BoxBoundsHandle.axes = is2D
                ? PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y
                : PrimitiveBoundsHandle.Axes.All;
            handleInfo.BoxBoundsHandle.center = center + positionOffset;
            handleInfo.BoxBoundsHandle.size = size;
        }

        private static bool TryGetBoundsValue(SerializedProperty serializedProperty, out Vector3 center, out Vector3 size, out bool is2D, out string error)
        {
            center = default;
            size = default;
            is2D = false;
            error = "";

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Bounds:
                {
                    Bounds bounds = serializedProperty.boundsValue;
                    center = bounds.center;
                    size = bounds.size;
                    return true;
                }
#if UNITY_2021_2_OR_NEWER
                case SerializedPropertyType.BoundsInt:
                {
                    BoundsInt boundsInt = serializedProperty.boundsIntValue;
                    center = (Vector3)boundsInt.position + (Vector3)boundsInt.size * 0.5f;
                    size = (Vector3)boundsInt.size;
                    return true;
                }
                case SerializedPropertyType.Rect:
                {
                    Rect rect = serializedProperty.rectValue;
                    Vector2 rectCenter = rect.center;
                    Vector2 rectSize = rect.size;
                    center = new Vector3(rectCenter.x, rectCenter.y, 0f);
                    size = new Vector3(rectSize.x, rectSize.y, 0f);
                    is2D = true;
                    return true;
                }
                case SerializedPropertyType.RectInt:
                {
                    RectInt rectInt = serializedProperty.rectIntValue;
                    Vector2 rectIntCenter = (Vector2)rectInt.position + (Vector2)rectInt.size * 0.5f;
                    center = new Vector3(rectIntCenter.x, rectIntCenter.y, 0f);
                    size = new Vector3(rectInt.width, rectInt.height, 0f);
                    is2D = true;
                    return true;
                }
#endif
                default:
                    error = $"Unsupported type {serializedProperty.propertyType}";
                    return false;
            }
        }

        private static PrimitiveBoundsHandleInfo CreatePrimitiveBoundsHandleInfo(PrimitiveBoundsHandleAttribute primitiveBoundsHandleAttribute, SerializedProperty serializedProperty, int index, MemberInfo memberInfo, object parent)
        {
            return new PrimitiveBoundsHandleInfo
            {
                Error = "",
                PrimitiveBoundsHandleAttribute = primitiveBoundsHandleAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                Color = primitiveBoundsHandleAttribute.Color,
                BoxBoundsHandle = new BoxBoundsHandle(),
                Id = $"{SerializedUtils.GetUniqueId(serializedProperty)}_{index}",
            };
        }

        private static void UpdatePrimitiveBoundsHandleInfoSpaceTrans(PrimitiveBoundsHandleInfo handleInfo)
        {
            if (handleInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(
                handleInfo.PrimitiveBoundsHandleAttribute.Space,
                handleInfo.SerializedProperty,
                handleInfo.MemberInfo,
                handleInfo.Parent);

            if (error != "")
            {
                handleInfo.Error = error;
                return;
            }

            handleInfo.SpaceTransform = trans;
        }
    }
}
