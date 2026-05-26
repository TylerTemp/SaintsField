using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.RotationHandleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(RotationHandleAttribute), true)]
    public partial class RotationHandleAttributeDrawer: SaintsPropertyDrawer
    {
        private class RotationHandleInfo
        {
            public SerializedProperty SerializedProperty;
            public RotationHandleAttribute RotationHandleAttribute;
            public MemberInfo MemberInfo;
            public object Parent;

            public string Error;
            public Transform SpaceTransform;
            public Vector3 Center;
            public Quaternion Rotation;

            public Action<object> OnValueChangedCallback;

            public string Id;
        }

        private static void SetValue(Quaternion newWorldRotation, RotationHandleInfo info)
        {
            Quaternion newLocalRotation = WorldToLocalRotation(newWorldRotation, info);
            switch (info.SerializedProperty.propertyType)
            {
                case SerializedPropertyType.ObjectReference when info.SerializedProperty.objectReferenceValue is GameObject isGo:
                    Undo.RecordObject(isGo.transform, "Rotate");
                    isGo.transform.rotation = newWorldRotation;
                    return;
                case SerializedPropertyType.ObjectReference when info.SerializedProperty.objectReferenceValue is Component comp:
                    Undo.RecordObject(comp.transform, "Rotate");
                    comp.transform.rotation = newWorldRotation;
                    return;
                case SerializedPropertyType.ObjectReference:
                    Debug.LogError($"{info.SerializedProperty.objectReferenceValue} is not supported");
                    return;
                case SerializedPropertyType.Quaternion:
                    info.SerializedProperty.quaternionValue = newLocalRotation;
                    info.OnValueChangedCallback.Invoke(newLocalRotation);
                    return;
                case SerializedPropertyType.Vector3:
                {
                    Vector3 euler = newLocalRotation.eulerAngles;
                    info.SerializedProperty.vector3Value = euler;
                    info.OnValueChangedCallback.Invoke(euler);
                    return;
                }
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                {
                    float z = newLocalRotation.eulerAngles.z;
                    SetNumericValue(info.SerializedProperty, z, info.OnValueChangedCallback);
                    return;
                }
                default:
                    Debug.LogError($"{info.SerializedProperty.propertyType} is not supported");
                    return;
            }
        }

        private static void SetNumericValue(SerializedProperty property, float zRotation,
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
                    var newValue = (uint)zRotation;
                    property.uintValue = newValue;
                    onValueChangedCallback.Invoke(newValue);
                }
                    return;
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                {
                    int newValue = (int)zRotation;
                    property.intValue = newValue;
                    onValueChangedCallback.Invoke(newValue);
                }
                    return;
                case SerializedPropertyNumericType.Int64:
                {
                    long newValue = (long)zRotation;
                    property.longValue = newValue;
                    onValueChangedCallback.Invoke(newValue);
                }
                    return;
                case SerializedPropertyNumericType.UInt64:
                {
                    ulong newValue = (ulong)zRotation;
                    property.ulongValue = (ulong)zRotation;
                    onValueChangedCallback.Invoke(newValue);
                }
                    return;
                case SerializedPropertyNumericType.Float:
                {
                    property.floatValue = zRotation;
                    onValueChangedCallback.Invoke(zRotation);
                }
                    return;
                case SerializedPropertyNumericType.Double:
                {
                    property.doubleValue = zRotation;
                    onValueChangedCallback.Invoke(zRotation);
                }
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void OnSceneGUIInternal(SceneView _, RotationHandleInfo rotationHandleInfo)
        {
            UpdateRotationHandleInfo(rotationHandleInfo);
            if (rotationHandleInfo.Error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(rotationHandleInfo.Error);
#endif
                return;
            }

            HandleVisibility.SetInView(
                rotationHandleInfo.Id,
                rotationHandleInfo.SerializedProperty.propertyPath,
                rotationHandleInfo.SerializedProperty.serializedObject.targetObject.name,
                EditorGUIUtility.IconContent("RotateTool On").image as Texture2D);

            if (HandleVisibility.IsHidden(rotationHandleInfo.Id))
            {
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Quaternion newRotation = Handles.RotationHandle(rotationHandleInfo.Rotation, rotationHandleInfo.Center);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    SetValue(newRotation, rotationHandleInfo);
                    rotationHandleInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void UpdateRotationHandleInfo(RotationHandleInfo rotationHandleInfo)
        {
            if (!SerializedUtils.IsOk(rotationHandleInfo.SerializedProperty))
            {
                rotationHandleInfo.Error = "Property disposed";
                return;
            }

            UpdateRotationHandleInfoSpaceTrans(rotationHandleInfo);

            if (rotationHandleInfo.Error != "")
            {
                return;
            }

            bool centerSet = false;
            if (rotationHandleInfo.SerializedProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                switch (rotationHandleInfo.SerializedProperty.objectReferenceValue)
                {
                    case GameObject go:
                        centerSet = true;
                        rotationHandleInfo.Center = go.transform.position;
                        break;
                    case Component comp:
                        centerSet = true;
                        rotationHandleInfo.Center = comp.transform.position;
                        break;
                }
            }

            if (!centerSet)
            {
                rotationHandleInfo.Center = rotationHandleInfo.SpaceTransform.position;
            }

            Vector3 positionOffset = rotationHandleInfo.RotationHandleAttribute.PosOffset;
            if (!string.IsNullOrEmpty(rotationHandleInfo.RotationHandleAttribute.PosOffsetCallback))
            {
                (string offsetError, MemberInfo _, Vector3 offsetResult) = Util.GetOf(
                    rotationHandleInfo.RotationHandleAttribute.PosOffsetCallback,
                    positionOffset,
                    rotationHandleInfo.SerializedProperty,
                    rotationHandleInfo.MemberInfo,
                    rotationHandleInfo.Parent,
                    null);
                if (offsetError != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(offsetError);
#endif
                    rotationHandleInfo.Error = offsetError;
                    return;
                }

                positionOffset = offsetResult;
            }

            if (rotationHandleInfo.RotationHandleAttribute.Space != null)
            {
                Vector3 scale = HandleUtils.GetLocalToWorldScale(rotationHandleInfo.SpaceTransform);
                positionOffset = Vector3.Scale(positionOffset, scale);
            }

            rotationHandleInfo.Center += positionOffset;

            rotationHandleInfo.Rotation = GetCurrentRotation(rotationHandleInfo);
            rotationHandleInfo.Error = "";
        }

        private static Quaternion GetCurrentRotation(RotationHandleInfo info)
        {
            SerializedProperty property = info.SerializedProperty;
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    switch (property.objectReferenceValue)
                    {
                        case GameObject go:
                            return go.transform.rotation;
                        case Component comp:
                            return comp.transform.rotation;
                    }
                    break;
                case SerializedPropertyType.Quaternion:
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    return LocalToWorldRotation(property.quaternionValue, info);

                case SerializedPropertyType.Vector3:

                    Quaternion quaternion = Quaternion.Euler(property.vector3Value);
                    return LocalToWorldRotation(quaternion, info);
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                    return LocalToWorldRotation(Quaternion.Euler(0f, 0f, GetNumericValue(property)), info);
            }

            return Quaternion.identity;
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

        private static Quaternion LocalToWorldRotation(Quaternion localRotation, RotationHandleInfo info)
        {
            if (info.RotationHandleAttribute.Space == null)
            {
                return localRotation;
            }

            // Debug.Log($"show rotate as {info.SpaceTransform.rotation}({info.SpaceTransform.gameObject.name}) * {localRotation}");
            return info.SpaceTransform.rotation * localRotation;
        }

        private static Quaternion WorldToLocalRotation(Quaternion worldRotation, RotationHandleInfo info)
        {
            if (info.RotationHandleAttribute.Space == null)
            {
                return worldRotation;
            }

            // ReSharper disable once InvertIf
            if (info.SerializedProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                switch (info.SerializedProperty.objectReferenceValue)
                {
                    case GameObject:
                    case Component:
                        return worldRotation;
                }
            }

            return Quaternion.Inverse(info.SpaceTransform.rotation) * worldRotation;
        }

        private static RotationHandleInfo CreateRotationHandleInfo(RotationHandleAttribute rotationHandleAttribute, SerializedProperty serializedProperty, int index, Action<object> onValueChangedCallback, MemberInfo memberInfo, object parent)
        {
            return new RotationHandleInfo
            {
                Error = "",

                RotationHandleAttribute = rotationHandleAttribute,
                SerializedProperty = serializedProperty,
                MemberInfo = memberInfo,
                Parent = parent,

                OnValueChangedCallback = onValueChangedCallback,

                Id = $"{SerializedUtils.GetUniqueId(serializedProperty)}_{index}",
            };
        }

        private static void UpdateRotationHandleInfoSpaceTrans(RotationHandleInfo rotationHandleInfo)
        {
            if (rotationHandleInfo.SpaceTransform != null)
            {
                return;
            }

            (string error, Transform trans) = HandleUtils.GetSpaceTransform(
                rotationHandleInfo.RotationHandleAttribute.Space, rotationHandleInfo.SerializedProperty, rotationHandleInfo.MemberInfo,
                rotationHandleInfo.Parent);

            if (error != "")
            {
                rotationHandleInfo.Error = error;
                return;
            }

            rotationHandleInfo.SpaceTransform = trans;
        }
    }
}
