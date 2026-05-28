using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
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

            string globalComponentIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();

#if SAINTSFIELD_DEBUG
            Debug.Log($"start to save {component} as {globalComponentIdString}");
#endif

            RuntimeSaver saver = RuntimeSaver.instance;

            // Here is a catch:
            // Object A as Id as IdA
            // Once Object A is destroyed, when adding a new component, Unity may re-use this IdA
            // Thus, let's compare the type name also
            string typeName = component.GetType().AssemblyQualifiedName;
            saver.pathSavers.RemoveAll(each => each.globalComponentIdString == globalComponentIdString && each.globalComponentTypeString == typeName);

            List<PathSaver> pathSavers = new List<PathSaver>();
            using(SerializedObject serializedObject = new SerializedObject(component))
            {

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
                    PathSaver arraySizeSaver = CreateBasePathSaver(component);
                    arraySizeSaver.propertyPath = property.propertyPath;
                    arraySizeSaver.propertyType = SaverPropertyType.ArraySize;
                    arraySizeSaver.intValue = property.arraySize;
                    yield return arraySizeSaver;
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

                PathSaver managedReferenceSaver = CreateBasePathSaver(component);
                managedReferenceSaver.propertyPath = property.propertyPath;
                managedReferenceSaver.propertyType = SaverPropertyType.ManagedReferenceFullTypename;
                managedReferenceSaver.stringValue = property.managedReferenceFullTypename;
                yield return managedReferenceSaver;
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

            PathSaver pathSaver = CreateBasePathSaver(component);
            pathSaver.propertyPath = property.propertyPath;
            pathSaver.propertyType = GetSaverPropertyType(property);

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
                    pathSaver.objectReferenceValueIsNull = property.objectReferenceValue == null;
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

        private static PathSaver CreateBasePathSaver(Component component)
        {
            // Debug.Log($"comp id={GlobalObjectId.GetGlobalObjectIdSlow(component).ToString()}");
            return new PathSaver
            {
                globalGameObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(component.gameObject).ToString(),
                gameObjectHierarchyPath = GetGameObjectHierarchyPath(component.gameObject),
                globalComponentIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString(),
                globalComponentTypeString = component.GetType().AssemblyQualifiedName,
                scenePath = component.gameObject.scene.path,
            };
        }

        private static string GetGameObjectHierarchyPath(GameObject gameObject)
        {
            List<string> pathParts = new List<string>();
            Transform current = gameObject.transform;
            while (current != null)
            {
                pathParts.Add(current.name);
                current = current.parent;
            }

            pathParts.Reverse();
            return string.Join("/", pathParts);
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

        public static bool RemoveComponent(Component component)
        {
            if (component == null)
            {
                return false;
            }

            string globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();

#if SAINTSFIELD_DEBUG
            Debug.Log($"start to save {component}");
#endif

            RuntimeSaver saver = RuntimeSaver.instance;

            int removedCount = saver.pathSavers.RemoveAll(each => each.globalComponentIdString == globalObjectIdString);
            return removedCount > 0;
        }

        public static void RestoreComponent(PathSaver pathSaver, Scene targetScene, IDictionary<string, Component> keyToNewComp)
        {
            if (!GlobalObjectId.TryParse(pathSaver.globalComponentIdString, out GlobalObjectId id))
            {
                Debug.LogWarning($"failed to parse {pathSaver.globalComponentIdString}");
                return;
            }

            Component target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as Component;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log($"Load component {target} from {id}");
#endif
            string newCompKey = $"{pathSaver.globalGameObjectIdString}_{pathSaver.globalComponentIdString}";

            if (target == null && keyToNewComp.TryGetValue(newCompKey, out Component cachedComp))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
                Debug.Log($"Use cached new component {cachedComp}");
#endif
                target = cachedComp;
            }

            // Prefab can not be located by GlobalObjectId, try search for it

            #region Prefab Search
            if(target == null)
            {
                string searchPath = pathSaver.gameObjectHierarchyPath;
                (bool found, GameObject foundGo) = FindInScene(targetScene, searchPath);
                if (found)
                {
                    GlobalObjectId foundGoId = GlobalObjectId.GetGlobalObjectIdSlow(foundGo);
                    (bool converted, GlobalObjectId foundUnpackId) = ConvertPrefabGidToUnpackedGid(foundGoId);
                    if (!converted)
                    {
                        Debug.LogWarning($"failed to convert gameObject at {targetScene.name}.{searchPath}");
                        return;
                    }

                    bool targetMatch = foundUnpackId.ToString() == pathSaver.globalGameObjectIdString;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
                    Debug.Log($"Found {targetMatch} GameObject, edit={foundGoId}, play={pathSaver.globalGameObjectIdString}");
#endif
                    if (!targetMatch)
                    {
                        Debug.LogWarning(
                            $"gameObject at {targetScene.name}.{searchPath} id mismatch(saved={pathSaver.globalGameObjectIdString}, get={foundGoId})");
                        return;
                    }

                    foreach (Component editComp in foundGo.GetComponents<Component>())
                    {
                        GlobalObjectId foundCompId = GlobalObjectId.GetGlobalObjectIdSlow(editComp);
                        (bool convertedComp, GlobalObjectId foundUnpackCompId) =
                            ConvertPrefabGidToUnpackedGid(foundCompId);
                        if (convertedComp && foundUnpackCompId.ToString() == pathSaver.globalComponentIdString)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
                            Debug.Log(
                                $"Found {editComp} Component for edit={foundCompId} play={pathSaver.globalComponentIdString}");
#endif
                            target = editComp;
                            break;
                        }
                    }

                    if (target == null) // not found, this is either wrong, or an add component
                    {
                        if (pathSaver.toDestroy)
                        {
                            Debug.LogWarning(
                                $"failed to remove component {pathSaver.globalComponentTypeString} on GameObject {foundGo}({pathSaver.globalGameObjectIdString}): target component {pathSaver.globalComponentIdString} not found on the target");
                            return;
                        }

                        // it's add component, and we just don't add it yet
                        // not possible it's already add: it's already been checked above
                        Debug.Assert(!keyToNewComp.ContainsKey(newCompKey));
                        (bool created, Component component) =
                            AddComponentToGo(foundGo, pathSaver.globalComponentTypeString);
                        if (!created)
                        {
                            return;
                        }

                        keyToNewComp[newCompKey] = component;
                        target = component;
                    }
                }
                else
                {
                    Debug.LogWarning($"not found path {searchPath} in {targetScene.name}, skip");
                    return;
                }
            }
            #endregion

            if (pathSaver.toDestroy)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
                Debug.Log($"Remove component {target}");
#endif
                if (target == null)
                {
                    Debug.LogWarning($"Failed to remove component: target is already null ({pathSaver.globalComponentIdString})");
                    return;
                }

                Undo.DestroyObjectImmediate(target);
                return;
            }

            if (target == null)
            {
                if (!GlobalObjectId.TryParse(pathSaver.globalGameObjectIdString, out GlobalObjectId gameObjectId))
                {
                    Debug.LogWarning($"failed to parse {pathSaver.globalGameObjectIdString} GameObject");
                    return;
                }

                GameObject targetGameObject =
                    GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gameObjectId) as GameObject;
                if (targetGameObject == null)
                {
                    Debug.LogWarning($"failed to find {pathSaver.globalGameObjectIdString}");
                    return;
                }

                (bool created, Component component) = AddComponentToGo(targetGameObject, pathSaver.globalComponentTypeString);
                if (!created)
                {
                    return;
                }
                keyToNewComp[newCompKey] = component;
                target = component;
            }

            if (string.IsNullOrEmpty(pathSaver.propertyPath))
            {
                Debug.LogWarning($"failed to restore {pathSaver.globalComponentIdString}/{pathSaver.propertyPath}");
                return;
            }

            using SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();

            SerializedProperty property = serializedObject.FindProperty(pathSaver.propertyPath);
            if (property == null)
            {
                Debug.LogWarning($"failed to find property {pathSaver.propertyPath} on {target}");
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log($"restoring {target.GetType().Name}.{pathSaver.propertyPath}=({pathSaver.propertyType})");
#endif

            switch (pathSaver.propertyType)
            {
                case SaverPropertyType.Generic:
                    // if (!property.isArray)
                    // {
                    //     property.boxedValue = pathSaver.boxedValue;
                    // }
                    return;
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
                    if (pathSaver.objectReferenceValueIsNull)
                    {
                        property.objectReferenceValue = null;
                    }
                    else
                    {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (pathSaver.objectReferenceValue != null)
                        {
                            property.objectReferenceValue = pathSaver.objectReferenceValue;
                        }
                        else
                        {

                            property.objectReferenceValue = EditorUtility.
#if UNITY_6000_3_OR_NEWER
                                    // ReSharper disable once RedundantCast
                                    EntityIdToObject((EntityId)
#else
                                    InstanceIDToObject(
#endif
                                        pathSaver.objectReferenceInstanceIDValue)
                                ;
                        }
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

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log($"restored {target.GetType().Name}.{pathSaver.propertyPath}=({pathSaver.propertyType})");
            if(property.propertyType == SerializedPropertyType.Generic && property.isArray)
            {
                Debug.Log($"{property.propertyPath}={property.arraySize}({pathSaver.intValue})");
            }
#endif

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(targetScene);
        }

        private static (bool created, Component result) AddComponentToGo(GameObject targetGameObject, string globalComponentTypeString)
        {
            Type componentType = Type.GetType(globalComponentTypeString);
            if (componentType == null)
            {
                Debug.LogWarning(
                    $"failed to get type {globalComponentTypeString} to add to gameObject {targetGameObject.name}");
                return (false, null);
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log($"Add new component {componentType} to {targetGameObject.name} GameObject");
#endif
            Component component = targetGameObject.AddComponent(componentType);
            EditorUtility.SetDirty(targetGameObject);
            return (true, component);
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

        public static void DestroyComponent(Component component)
        {
            string globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();

#if SAINTSFIELD_DEBUG
            Debug.Log($"start to record destroy {component} as {globalObjectIdString}");
#endif

            RuntimeSaver saver = RuntimeSaver.instance;

            string typeName = component.GetType().AssemblyQualifiedName;
            saver.pathSavers.RemoveAll(each => each.globalComponentIdString == globalObjectIdString && each.globalComponentTypeString == typeName);

            PathSaver pathSaver = CreateBasePathSaver(component);
            pathSaver.toDestroy = true;

            saver.pathSavers.Add(pathSaver);
            saver.SaveToDisk();
        }

        private static (bool found, GameObject result) FindInScene(Scene scene, string path)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return default;
            }

            string trimPath = path.Trim('/');

            string[] parts = trimPath.Split('/');
            Debug.Assert(parts.Length != 0, trimPath);

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != parts[0])
                {
                    continue;
                }

                Transform current = root.transform;

                for (int i = 1; i < parts.Length; i++)
                {
                    current = current.Find(parts[i]);
                    if (current == null)
                        return default;
                }

                return (true, current.gameObject);
            }

            return default;
        }

        /// <see href="https://uninomicon.com/globalobjectid">Prefabs have two GlobalObjectIds</see>
        private static (bool converted, GlobalObjectId result) ConvertPrefabGidToUnpackedGid(GlobalObjectId id)
        {
            ulong fileId = (id.targetObjectId ^ id.targetPrefabId) & 0x7fffffffffffffff;
            bool success = GlobalObjectId.TryParse(
                $"GlobalObjectId_V1-{id.identifierType}-{id.assetGUID}-{fileId}-0",
                out GlobalObjectId unpackedGid);
            // Assert.IsTrue(success);
            // return unpackedGid;
            return (success, unpackedGid);
        }
    }
}
