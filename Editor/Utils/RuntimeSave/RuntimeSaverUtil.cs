using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils.RuntimeSave
{
    public static class RuntimeSaverUtil
    {
        public static void SaveComponent(Component component)
        {
            if (component == null)
            {
                return;
            }

#if SAINTSFIELD_DEBUG
            Debug.Log($"start to save {component}");
#endif

            RuntimeSaver saver = RuntimeSaver.instance;

            SerializedObject serializedObject = new SerializedObject(component);
            List<PathSaver> pathSavers = new List<PathSaver>();
            foreach (SerializedProperty serializedProperty in SerializedUtils.GetAllField(serializedObject))
            {
                IEnumerable<PathSaver> results = ToPathSaver(component, serializedProperty);
                foreach (PathSaver pathSaver in results)
                {
#if SAINTSFIELD_DEBUG
                    Debug.Log($"save {component.GetType().Name}.{pathSaver.propertyPath}={pathSaver.propertyType}");
#endif
                    pathSavers.Add(pathSaver);
                }


            }

            if (pathSavers.Count == 0)
            {
                return;
            }

            saver.pathSavers.AddRange(pathSavers);
            saver.SaveToDisk();
        }

        private static IEnumerable<PathSaver> ToPathSaver(Component component, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.FixedBufferSize)
            {
                yield break;
            }

            if (property.propertyType == SerializedPropertyType.Generic)
            {
                if (property.isArray)
                {
                    yield return new PathSaver
                    {
                        targetObject = component,
                        targetInstanceId = component.GetInstanceID(),
                        globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString(),
                        propertyPath = property.propertyPath,
                        propertyType = SaverPropertyType.ArraySize,
                        intValue = property.arraySize,
                    };
                    for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                    {
                        SerializedProperty arrayProperty = property.GetArrayElementAtIndex(arrayIndex);
                        // Debug.Log($"array {property.propertyPath}@{arrayIndex}={arrayProperty.propertyPath}");
                        foreach (PathSaver saver in ToPathSaver(component, arrayProperty))
                        {
                            yield return saver;
                        }
                    }
                }
                else
                {
                    foreach (SerializedProperty subProperty in SerializedUtils.GetPropertyChildren(property))
                    {
                        foreach (PathSaver saver in ToPathSaver(component, subProperty))
                        {
                            yield return saver;
                        }
                    }
                }

                yield break;
            }

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                // yield return new PathSaver
                // {
                //     targetObject = component,
                //     targetInstanceId = component.GetInstanceID(),
                //     globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString(),
                //     propertyPath = property.propertyPath,
                //     propertyType = SaverPropertyType.ManagedReferenceId,
                //     longValue = property.managedReferenceId,
                // };

                yield return new PathSaver
                {
                    targetObject = component,
                    targetInstanceId = component.GetInstanceID(),
                    propertyPath = property.propertyPath,
                    propertyType = SaverPropertyType.ManagedReferenceFullTypename,
                    stringValue =  property.managedReferenceFullTypename,
                };
                // yield return new PathSaver
                // {
                //     targetObject = component,
                //     targetInstanceId = component.GetInstanceID(),
                //     propertyPath = property.propertyPath,
                //     propertyType = SaverPropertyType.ManagedReferenceFieldTypename,
                //     stringValue =  property.managedReferenceFieldTypename,
                // };

                foreach (SerializedProperty subProperty in SerializedUtils.GetPropertyChildren(property))
                {
                    foreach (PathSaver saver in ToPathSaver(component, subProperty))
                    {
                        yield return saver;
                    }
                }
                yield break;
            }

            PathSaver pathSaver = new PathSaver
            {
                targetObject = component,
                targetInstanceId = component.GetInstanceID(),
                globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString(),
                propertyPath = property.propertyPath,
                propertyType = GetSaverPropertyType(property),
            };

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
#if UNITY_6000_0_OR_NEWER
                case SerializedPropertyType.RenderingLayerMask:
#endif
#if UNITY_6000_2_OR_NEWER
                case SerializedPropertyType.EntityId:
#endif
                    pathSaver = SaveNumericValues(property, pathSaver);
                    break;
                case SerializedPropertyType.Boolean:
                    pathSaver.boolValue = property.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    pathSaver = SaveFloatingPointValue(property, pathSaver);
                    break;
                case SerializedPropertyType.String:
                    pathSaver.stringValue = property.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    pathSaver.colorValue = property.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    pathSaver.objectReferenceValue = property.objectReferenceValue;
                    pathSaver.objectReferenceInstanceIDValue = property.objectReferenceInstanceIDValue;
                    break;
                case SerializedPropertyType.Enum:
                    pathSaver.enumValueIndex = property.enumValueIndex;
                    pathSaver.enumValueFlag = property.enumValueFlag;
                    break;
                case SerializedPropertyType.Vector2:
                    pathSaver.vector2Value = property.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    pathSaver.vector3Value = property.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    pathSaver.vector4Value = property.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    pathSaver.rectValue = property.rectValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    pathSaver.animationCurveValue = property.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    pathSaver.boundsValue = property.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    pathSaver.gradientValue = property.gradientValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    pathSaver.quaternionValue = property.quaternionValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    pathSaver.exposedReferenceValue = property.exposedReferenceValue;
                    break;
                case SerializedPropertyType.Vector2Int:
                    pathSaver.vector2IntValue = property.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    pathSaver.vector3IntValue = property.vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    pathSaver.rectIntValue = property.rectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    pathSaver.boundsIntValue = property.boundsIntValue;
                    break;

                    // pathSaver.managedReferenceId = property.managedReferenceId;
                    // pathSaver.managedReferenceFullTypename = property.managedReferenceFullTypename;
                    // pathSaver.managedReferenceFieldTypename = property.managedReferenceFieldTypename;
                    // pathSaver.managedReferenceValue = property.managedReferenceValue;
                    // break;
                case SerializedPropertyType.Hash128:
                    pathSaver.hash128Value = property.hash128Value;
                    break;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
                default:
                    yield break;
            }

            yield return pathSaver;
        }

        private static SaverPropertyType GetSaverPropertyType(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
#if UNITY_6000_2_OR_NEWER
                case SerializedPropertyType.EntityId:
#endif
                    return GetNumericSaverPropertyType(property);
                case SerializedPropertyType.Boolean:
                    return SaverPropertyType.Boolean;
                case SerializedPropertyType.Float:
                    return property.numericType == SerializedPropertyNumericType.Double
                        ? SaverPropertyType.Double
                        : SaverPropertyType.Float;
                case SerializedPropertyType.String:
                    return SaverPropertyType.String;
                case SerializedPropertyType.Color:
                    return SaverPropertyType.Color;
                case SerializedPropertyType.ObjectReference:
                    return SaverPropertyType.ObjectReference;
                case SerializedPropertyType.LayerMask:
                    return SaverPropertyType.Integer;
                case SerializedPropertyType.Enum:
                    return SaverPropertyType.Enum;
                case SerializedPropertyType.Vector2:
                    return SaverPropertyType.Vector2;
                case SerializedPropertyType.Vector3:
                    return SaverPropertyType.Vector3;
                case SerializedPropertyType.Vector4:
                    return SaverPropertyType.Vector4;
                case SerializedPropertyType.Rect:
                    return SaverPropertyType.Rect;
                case SerializedPropertyType.ArraySize:
                    return SaverPropertyType.ArraySize;
                case SerializedPropertyType.Character:
                    return SaverPropertyType.UInteger;
                case SerializedPropertyType.AnimationCurve:
                    return SaverPropertyType.AnimationCurve;
                case SerializedPropertyType.Bounds:
                    return SaverPropertyType.Bounds;
                case SerializedPropertyType.Gradient:
                    return SaverPropertyType.Gradient;
                case SerializedPropertyType.Quaternion:
                    return SaverPropertyType.Quaternion;
                case SerializedPropertyType.ExposedReference:
                    return SaverPropertyType.ExposedReference;
                case SerializedPropertyType.Vector2Int:
                    return SaverPropertyType.Vector2Int;
                case SerializedPropertyType.Vector3Int:
                    return SaverPropertyType.Vector3Int;
                case SerializedPropertyType.RectInt:
                    return SaverPropertyType.RectInt;
                case SerializedPropertyType.BoundsInt:
                    return SaverPropertyType.BoundsInt;
                // case SerializedPropertyType.ManagedReference:
                //     return SaverPropertyType.ManagedReference;
                case SerializedPropertyType.Hash128:
                    return SaverPropertyType.Hash128;
#if UNITY_6000_0_OR_NEWER
                case SerializedPropertyType.RenderingLayerMask:
                    return SaverPropertyType.UInteger;
#endif
                case SerializedPropertyType.Generic:
                default:
                    return SaverPropertyType.Generic;
            }
        }

        private static SaverPropertyType GetNumericSaverPropertyType(SerializedProperty property)
        {
            switch (property.numericType)
            {
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    return SaverPropertyType.Integer;
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    return SaverPropertyType.UInteger;
                case SerializedPropertyNumericType.Int64:
                    return SaverPropertyType.Long;
                case SerializedPropertyNumericType.UInt64:
                    return SaverPropertyType.ULong;
                default:
                    return SaverPropertyType.Integer;
            }
        }

        private static PathSaver SaveNumericValues(SerializedProperty property, PathSaver pathSaver)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                    pathSaver.intValue = property.arraySize;
                    return pathSaver;
                case SerializedPropertyType.LayerMask:
                    pathSaver.intValue = property.intValue;
                    return pathSaver;
#if UNITY_6000_0_OR_NEWER
                case SerializedPropertyType.RenderingLayerMask:
                    pathSaver.uintValue = property.uintValue;
                    return pathSaver;
#endif
                case SerializedPropertyType.Character:
                    pathSaver.uintValue = property.uintValue;
                    return pathSaver;
            }

            switch (property.numericType)
            {
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.Int32:
                    pathSaver.intValue = property.intValue;
                    break;
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                    pathSaver.uintValue = property.uintValue;
                    break;
                case SerializedPropertyNumericType.Int64:
                    pathSaver.longValue = property.longValue;
                    break;
                case SerializedPropertyNumericType.UInt64:
                    pathSaver.ulongValue = property.ulongValue;
                    break;
                default:
                    pathSaver.intValue = property.intValue;
                    break;
            }

            return pathSaver;
        }

        private static PathSaver SaveFloatingPointValue(SerializedProperty property, PathSaver pathSaver)
        {
            switch (property.numericType)
            {
                case SerializedPropertyNumericType.Double:
                    pathSaver.doubleValue = property.doubleValue;
                    break;
                default:
                    pathSaver.floatValue = property.floatValue;
                    break;
            }

            return pathSaver;
        }

        public static void RestoreComponent(PathSaver pathSaver)
        {
            // Try the direct Unity Object reference first. Unity remaps the
            // InstanceID across play-mode exit, so a serialized Object field on
            // a ScriptableSingleton survives play mode the same way
            // VInspectorClipboard's `sourceComponent` does.
            //
            // Fall back to InstanceID -> Object lookup, then finally the
            // GlobalObjectId saved at SaveComponent time. The GlobalObjectId
            // fallback only works reliably if the id was captured *outside*
            // play mode (identifierType == 1); ids captured during play mode
            // (identifierType == 2) are session-scoped and become unresolvable
            // once Edit mode is re-entered, which is why relying solely on
            // GlobalObjectId silently failed before.
            Object target = pathSaver.targetObject;

            if (target == null && pathSaver.targetInstanceId != 0)
            {
                target = EditorUtility.InstanceIDToObject(pathSaver.targetInstanceId);
            }

            if (target == null)
            {
                if (!GlobalObjectId.TryParse(pathSaver.globalObjectIdString, out GlobalObjectId id))
                {
                    Debug.LogWarning($"failed to parse {pathSaver.globalObjectIdString}");
                    return;
                }

                target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            }

            if (target == null)
            {
                Debug.LogWarning($"failed to find {pathSaver.globalObjectIdString}");
                return;
            }

            if (string.IsNullOrEmpty(pathSaver.propertyPath))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"failed to restore {pathSaver.globalObjectIdString}/{pathSaver.propertyPath}");
#endif
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();

            SerializedProperty property = serializedObject.FindProperty(pathSaver.propertyPath);
            if (property == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"failed to find property {pathSaver.propertyPath} on {target}");
#endif
                return;
            }

#if SAINTSFIELD_DEBUG
            Debug.Log($"restoring {target.GetType().Name}.{pathSaver.propertyPath}=({pathSaver.propertyType})");
#endif

            switch (pathSaver.propertyType)
            {
                case SaverPropertyType.Generic:
                    if (!property.isArray)
                    {
                        property.boxedValue = pathSaver.boxedValue;
                    }
                    break;
                case SaverPropertyType.Integer:
                    property.intValue = pathSaver.intValue;
                    break;
                case SaverPropertyType.UInteger:
                    property.uintValue = pathSaver.uintValue;
                    break;
                case SaverPropertyType.Long:
                    property.longValue = pathSaver.longValue;
                    break;
                case SaverPropertyType.ULong:
                    property.ulongValue = pathSaver.ulongValue;
                    break;
                case SaverPropertyType.Boolean:
                    property.boolValue = pathSaver.boolValue;
                    break;
                case SaverPropertyType.Float:
                    property.floatValue = pathSaver.floatValue;
                    break;
                case SaverPropertyType.Double:
                    property.doubleValue = pathSaver.doubleValue;
                    break;
                case SaverPropertyType.String:
                    property.stringValue = pathSaver.stringValue;
                    break;
                case SaverPropertyType.Color:
                    property.colorValue = pathSaver.colorValue;
                    break;
                case SaverPropertyType.ObjectReference:
                    if (pathSaver.objectReferenceValue != null)
                    {
                        property.objectReferenceValue = pathSaver.objectReferenceValue;
                    }
                    else
                    {
#if UNITY_6000_3_OR_NEWER
                        property.objectReferenceValue =
                            EditorUtility.EntityIdToObject((EntityId)pathSaver.objectReferenceInstanceIDValue);
#else
                        return;
#endif
                    }
                    break;
                case SaverPropertyType.Enum:
                    property.enumValueIndex = pathSaver.enumValueIndex;
                    property.enumValueFlag = pathSaver.enumValueFlag;
                    break;
                case SaverPropertyType.Vector2:
                    property.vector2Value = pathSaver.vector2Value;
                    break;
                case SaverPropertyType.Vector3:
                    property.vector3Value = pathSaver.vector3Value;
                    break;
                case SaverPropertyType.Vector4:
                    property.vector4Value = pathSaver.vector4Value;
                    break;
                case SaverPropertyType.Rect:
                    property.rectValue = pathSaver.rectValue;
                    break;
                case SaverPropertyType.ArraySize:
                    property.arraySize = pathSaver.intValue;
                    break;
                case SaverPropertyType.AnimationCurve:
                    property.animationCurveValue = pathSaver.animationCurveValue;
                    break;
                case SaverPropertyType.Bounds:
                    property.boundsValue = pathSaver.boundsValue;
                    break;
                case SaverPropertyType.Gradient:
                    property.gradientValue = pathSaver.gradientValue;
                    break;
                case SaverPropertyType.Quaternion:
                    property.quaternionValue = pathSaver.quaternionValue;
                    break;
                case SaverPropertyType.ExposedReference:
                    property.exposedReferenceValue = pathSaver.exposedReferenceValue;
                    break;
                case SaverPropertyType.Vector2Int:
                    property.vector2IntValue = pathSaver.vector2IntValue;
                    break;
                case SaverPropertyType.Vector3Int:
                    property.vector3IntValue = pathSaver.vector3IntValue;
                    break;
                case SaverPropertyType.RectInt:
                    property.rectIntValue = pathSaver.rectIntValue;
                    break;
                case SaverPropertyType.BoundsInt:
                    property.boundsIntValue = pathSaver.boundsIntValue;
                    break;
                case SaverPropertyType.ManagedReferenceFullTypename:
                {
                    (bool created, object value) = CreateManagedReferenceInstance(pathSaver.stringValue);
                    if (!created)
                    {
                        return;
                    }
                    property.boxedValue = value;
                }
                    break;
                case SaverPropertyType.Hash128:
                    property.hash128Value = pathSaver.hash128Value;
                    break;
            }

#if SAINTSFIELD_DEBUG
            Debug.Log($"restored {target.GetType().Name}.{pathSaver.propertyPath}=({pathSaver.propertyType})");
            if(property.propertyType == SerializedPropertyType.Generic && property.isArray)
            {
                Debug.Log($"{property.propertyPath}={property.arraySize}({pathSaver.intValue})");
            }
#endif

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static (bool craeted, object value) CreateManagedReferenceInstance(string fullTypename)
        {
            if (string.IsNullOrEmpty(fullTypename))
            {
                return (true, null);
            }

            // "AssemblyName Full.Type.Name"
            string[] parts = fullTypename.Split(' ');
            if (parts.Length < 2)
            {
                return (false, null);
            }

            string assemblyName = parts[0];
            string className = parts[1];

            Type type = Type.GetType($"{className}, {assemblyName}");
            if (type == null)
            {
                return (false, null);
            }

            try
            {
                // ConstructorInfo ctor = type.GetConstructor(
                //     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                //     null, Type.EmptyTypes, null);
                //
                // if (ctor != null)
                // {
                //     return (true, ctor.Invoke(null));
                // }

                // return (true, FormatterServices.GetUninitializedObject(type));
                return (true, System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create instance of {className}: {e.Message}");
                return (false, null);
            }
        }
    }
}
