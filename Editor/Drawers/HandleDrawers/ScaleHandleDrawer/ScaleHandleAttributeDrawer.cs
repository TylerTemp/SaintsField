using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.ScaleHandleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ScaleHandleAttribute), true)]
    public partial class ScaleHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private class ScaleHandleInfo
        {
            public SerializedProperty SerializedProperty;
            public ScaleHandleAttribute ScaleHandleAttribute;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            public Transform SpaceTransform;
            public Vector3 Center;
            public Quaternion Rotation;
            public Vector3 Scale;

            public Action<object> OnValueChangedCallback;

            public string Id;
        }

        private static void SetValue(Vector3 newScale, ScaleHandleInfo info)
        {
            switch (info.SerializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                {
                    Vector2 value = new Vector2(newScale.x, newScale.y);
                    info.SerializedProperty.vector2Value = value;
                    info.OnValueChangedCallback.Invoke(value);
                    return;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2Int value = new Vector2Int((int)newScale.x, (int)newScale.y);
                    info.SerializedProperty.vector2IntValue = value;
                    info.OnValueChangedCallback.Invoke(value);
                    return;
                }
                case SerializedPropertyType.Vector3:
                    info.SerializedProperty.vector3Value = newScale;
                    info.OnValueChangedCallback.Invoke(newScale);
                    return;
                case SerializedPropertyType.Vector3Int:
                {
                    Vector3Int value = new Vector3Int((int)newScale.x, (int)newScale.y, (int)newScale.z);
                    info.SerializedProperty.vector3IntValue = value;
                    info.OnValueChangedCallback.Invoke(value);
                    return;
                }
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                    SetNumericValue(info.SerializedProperty, newScale.x, info.OnValueChangedCallback);
                    return;
                default:
                    Debug.LogError($"{info.SerializedProperty.propertyType} is not supported");
                    return;
            }
        }

        private static void SetNumericValue(SerializedProperty property, float newValue,
            Action<object> onValueChangedCallback)
        {
            switch (property.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    return;
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                {
                    uint typedValue = (uint)newValue;
                    property.uintValue = typedValue;
                    onValueChangedCallback.Invoke(typedValue);
                }
                    return;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                {
                    int typedValue = (int)newValue;
                    property.intValue = typedValue;
                    onValueChangedCallback.Invoke(typedValue);
                }
                    return;
                case SerializedPropertyNumericType.Int64:
                {
                    long typedValue = (long)newValue;
                    property.longValue = typedValue;
                    onValueChangedCallback.Invoke(typedValue);
                }
                    return;
                case SerializedPropertyNumericType.UInt64:
                {
                    ulong typedValue = (ulong)newValue;
                    property.ulongValue = typedValue;
                    onValueChangedCallback.Invoke(typedValue);
                }
                    return;
                case SerializedPropertyNumericType.Float:
                    property.floatValue = newValue;
                    onValueChangedCallback.Invoke(newValue);
                    return;
                case SerializedPropertyNumericType.Double:
                {
                    double typedValue = newValue;
                    property.doubleValue = typedValue;
                    onValueChangedCallback.Invoke(typedValue);
                }
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void OnSceneGUIInternal(SceneView _, ScaleHandleInfo scaleHandleInfo)
        {
            UpdateScaleHandleInfo(scaleHandleInfo);
            if (scaleHandleInfo.Error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(scaleHandleInfo.Error);
#endif
                return;
            }

            HandleVisibility.SetInView(
                scaleHandleInfo.Id,
                scaleHandleInfo.SerializedProperty.propertyPath,
                scaleHandleInfo.SerializedProperty.serializedObject.targetObject.name,
                EditorGUIUtility.IconContent("ScaleTool On").image as Texture2D);

            if (HandleVisibility.IsHidden(scaleHandleInfo.Id))
            {
                return;
            }

            Quaternion rotation = Tools.pivotRotation == PivotRotation.Local
                ? scaleHandleInfo.Rotation
                : Quaternion.identity;

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Vector3 newScale = Handles.ScaleHandle(
                    scaleHandleInfo.Scale,
                    scaleHandleInfo.Center,
                    rotation,
                    HandleUtility.GetHandleSize(scaleHandleInfo.Center));
                if (changed.changed)
                {
                    SetValue(newScale, scaleHandleInfo);
                    scaleHandleInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void UpdateScaleHandleInfo(ScaleHandleInfo scaleHandleInfo)
        {
            if (!SerializedUtils.IsOk(scaleHandleInfo.SerializedProperty))
            {
                scaleHandleInfo.Error = "Property disposed";
                return;
            }

            UpdateScaleHandleInfoSpaceTrans(scaleHandleInfo);
            if (scaleHandleInfo.Error != "")
            {
                return;
            }

            scaleHandleInfo.Error = "";
            scaleHandleInfo.Center = scaleHandleInfo.SpaceTransform.position;
            scaleHandleInfo.Rotation = scaleHandleInfo.ScaleHandleAttribute.Space == null
                ? Quaternion.identity
                : scaleHandleInfo.SpaceTransform.rotation;

            Vector3 positionOffset = scaleHandleInfo.ScaleHandleAttribute.PosOffset;
            if (!string.IsNullOrEmpty(scaleHandleInfo.ScaleHandleAttribute.PosOffsetCallback))
            {
                (string offsetError, MemberInfo _, Vector3 offsetResult) = Util.GetOf(
                    scaleHandleInfo.ScaleHandleAttribute.PosOffsetCallback,
                    positionOffset,
                    scaleHandleInfo.SerializedProperty,
                    scaleHandleInfo.MemberInfo,
                    scaleHandleInfo.Parent,
                    null);
                if (offsetError != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(offsetError);
#endif
                    scaleHandleInfo.Error = offsetError;
                    return;
                }

                positionOffset = offsetResult;
            }

            Vector3 scale = scaleHandleInfo.ScaleHandleAttribute.Space == null
                ? Vector3.one
                : HandleUtils.GetLocalToWorldScale(scaleHandleInfo.SpaceTransform);
            scaleHandleInfo.Center += Vector3.Scale(positionOffset, scale);

            switch (scaleHandleInfo.SerializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                {
                    Vector2 value = scaleHandleInfo.SerializedProperty.vector2Value;
                    scaleHandleInfo.Scale = new Vector3(value.x, value.y, 1f);
                    return;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2Int value = scaleHandleInfo.SerializedProperty.vector2IntValue;
                    scaleHandleInfo.Scale = new Vector3(value.x, value.y, 1f);
                    return;
                }
                case SerializedPropertyType.Vector3:
                    scaleHandleInfo.Scale = scaleHandleInfo.SerializedProperty.vector3Value;
                    return;
                case SerializedPropertyType.Vector3Int:
                {
                    Vector3Int value = scaleHandleInfo.SerializedProperty.vector3IntValue;
                    scaleHandleInfo.Scale = new Vector3(value.x, value.y, value.z);
                    return;
                }
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                {
                    float value = GetNumericValue(scaleHandleInfo.SerializedProperty);
                    scaleHandleInfo.Scale = new Vector3(value, value, value);
                    return;
                }
                default:
                    scaleHandleInfo.Error = $"{scaleHandleInfo.SerializedProperty.propertyType} is not supported";
                    return;
            }
        }

        private static float GetNumericValue(SerializedProperty property)
        {
            switch (property.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    return 0f;
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    return property.uintValue;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    return property.intValue;
                case SerializedPropertyNumericType.Int64:
                    return property.longValue;
                case SerializedPropertyNumericType.UInt64:
                    return property.ulongValue;
                case SerializedPropertyNumericType.Float:
                    return property.floatValue;
                case SerializedPropertyNumericType.Double:
                    return (float)property.doubleValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ScaleHandleInfo CreateScaleHandleInfo(ScaleHandleAttribute scaleHandleAttribute,
            SerializedProperty serializedProperty, int index, Action<object> onValueChangedCallback,
            MemberInfo memberInfo, object parent)
        {
            return new ScaleHandleInfo
            {
                Error = "",
                ScaleHandleAttribute = scaleHandleAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,
                OnValueChangedCallback = onValueChangedCallback,
                Id = $"{SerializedUtils.GetUniqueId(serializedProperty)}_{index}",
            };
        }

        private static void UpdateScaleHandleInfoSpaceTrans(ScaleHandleInfo scaleHandleInfo)
        {
            if (scaleHandleInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(
                scaleHandleInfo.ScaleHandleAttribute.Space,
                scaleHandleInfo.SerializedProperty,
                scaleHandleInfo.MemberInfo,
                scaleHandleInfo.Parent);

            if (error != "")
            {
                scaleHandleInfo.Error = error;
                return;
            }

            scaleHandleInfo.SpaceTransform = trans;
        }
    }
}
