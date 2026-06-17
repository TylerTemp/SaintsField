using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
// #if ODIN_INSPECTOR
//     [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
// #endif
    [CustomPropertyDrawer(typeof(SaintsObjInterface<>), true)]
    [CustomPropertyDrawer(typeof(SaintsInterface<,>), true)]
    [CustomPropertyDrawer(typeof(SaintsInterface<>), true)]
    public partial class SaintsInterfaceDrawer: SaintsPropertyDrawer
    {
        internal readonly struct InterfaceFieldInfo
        {
            public readonly string Error;
            public readonly int ArrayIndex;
            public readonly Type ValueType;
            public readonly Type InterfaceType;
            public readonly SerializedProperty ValueProp;
            public readonly SerializedProperty IsVRefProp;
            public readonly SerializedProperty VRefProp;
            public readonly MemberInfo VRefMemberInfo;

            public InterfaceFieldInfo(string error, int arrayIndex, Type valueType, Type interfaceType,
                SerializedProperty valueProp, SerializedProperty isVRefProp, SerializedProperty vRefProp,
                MemberInfo vRefMemberInfo)
            {
                Error = error;
                ArrayIndex = arrayIndex;
                ValueType = valueType;
                InterfaceType = interfaceType;
                ValueProp = valueProp;
                IsVRefProp = isVRefProp;
                VRefProp = vRefProp;
                VRefMemberInfo = vRefMemberInfo;
            }
        }

        private static readonly Dictionary<Type, IReadOnlyList<Type>> TypesImplementingInterfaceCache =
            new Dictionary<Type, IReadOnlyList<Type>>();

        public static (Type valueType, Type interfaceType) GetTypes(SerializedProperty property, FieldInfo info)
        {
            Type interfaceContainer = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

            foreach (Type thisType in RectUtils.GetGenBaseTypes(interfaceContainer))
            {
                if (thisType.IsGenericType && thisType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
                {
                    Type[] genericArguments = thisType.GetGenericArguments();
                    // Debug.Log($"from {thisType.Name} get types: {string.Join(",", genericArguments.Select(each => each.Name))}");
                    // Debug.Log();
                    return (genericArguments[0], genericArguments[1]);
                }
            }

            // throw new ArgumentException($"Failed to obtain generic arguments from {interfaceContainer}");
            return (null, null);
        }

        internal static InterfaceFieldInfo GetInterfaceFieldInfo(SerializedProperty property, FieldInfo info)
        {
            (string error, IWrapProp saintsInterfaceProp, int arrayIndex, object _) = GetSerName(property, info);
            if (error != "")
            {
                return new InterfaceFieldInfo(error, arrayIndex, null, null, null, null, null, null);
            }

            string wrapPropName = ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType());
            SerializedProperty valueProp = property.FindPropertyRelative(wrapPropName) ??
                                           SerializedUtils.FindPropertyByAutoPropertyName(property, wrapPropName);
            if (valueProp == null)
            {
                return new InterfaceFieldInfo($"{wrapPropName} not found in {property.propertyPath}", arrayIndex,
                    null, null, null, null, null, null);
            }

            SerializedProperty isVRefProp = property.FindPropertyRelative("IsVRef") ??
                                            SerializedUtils.FindPropertyByAutoPropertyName(property, "IsVRef");
            if (isVRefProp == null)
            {
                return new InterfaceFieldInfo($"IsVRef not found in {property.propertyPath}", arrayIndex, null,
                    null, valueProp, null, null, null);
            }

            SerializedProperty vRefProp = property.FindPropertyRelative("VRef") ??
                                          SerializedUtils.FindPropertyByAutoPropertyName(property, "VRef");
            if (vRefProp == null)
            {
                return new InterfaceFieldInfo($"VRef not found in {property.propertyPath}", arrayIndex, null, null,
                    valueProp, isVRefProp, null, null);
            }

            (Type valueType, Type interfaceType) = GetTypes(property, info);
            if (valueType == null || interfaceType == null)
            {
                return new InterfaceFieldInfo($"Failed to resolve interface types for {property.propertyPath}",
                    arrayIndex, valueType, interfaceType, valueProp, isVRefProp, vRefProp, null);
            }

            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
            MemberInfo vRefMemberInfo =
                (MemberInfo)fieldType.GetField("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) ??
                fieldType.GetProperty("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (vRefMemberInfo == null)
            {
                return new InterfaceFieldInfo($"VRef member not found in {fieldType}", arrayIndex, valueType,
                    interfaceType, valueProp, isVRefProp, vRefProp, null);
            }

            return new InterfaceFieldInfo("", arrayIndex, valueType, interfaceType, valueProp, isVRefProp, vRefProp,
                vRefMemberInfo);
        }

        internal static bool TryGetMatchedInterfaceValue(Object candidate, Type valueType, Type interfaceType,
            out Object matchedValue)
        {
            if (!candidate)
            {
                matchedValue = null;
                return true;
            }

            if (interfaceType.IsInstanceOfType(candidate))
            {
                matchedValue = candidate;
                return true;
            }

            (bool isMatch, Object result) = GetSerializedObject(candidate, valueType, interfaceType);
            matchedValue = result;
            return isMatch;
        }

        internal static bool SyncInterfaceModeSideEffectsWithoutApply(SerializedProperty valueProp,
            SerializedProperty vRefProp, bool isVRef)
        {
            bool changed = false;
            if (isVRef)
            {
                if (valueProp.objectReferenceValue != null)
                {
                    valueProp.objectReferenceValue = null;
                    changed = true;
                }
            }
            else if (vRefProp.managedReferenceValue != null)
            {
                vRefProp.managedReferenceValue = null;
                changed = true;
            }

            return changed;
        }

        internal static bool SyncInterfaceModeSideEffects(SerializedProperty valueProp, SerializedProperty vRefProp,
            bool isVRef)
        {
            bool changed = SyncInterfaceModeSideEffectsWithoutApply(valueProp, vRefProp, isVRef);
            if (changed)
            {
                valueProp.serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        internal static bool ShouldReferenceStartExpanded(IEnumerable<Attribute> allAttributes,
            SerializedProperty vRefProp) =>
            allAttributes.Any(each => each is DefaultExpandAttribute) || vRefProp.isExpanded;

        internal static IReadOnlyList<Type> GetTypesImplementingInterface(Type interfaceType)
        {
            if (TypesImplementingInterfaceCache.TryGetValue(interfaceType, out IReadOnlyList<Type> cachedTypes))
            {
                return cachedTypes;
            }

            List<Type> results = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Type> assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    assemblyTypes = e.Types.Where(each => each != null);
                }

                results.AddRange(assemblyTypes.Where(type =>
                    type != null
                    && !type.IsAbstract
                    && !type.ContainsGenericParameters
                    && !typeof(Object).IsAssignableFrom(type)
                    && (!type.IsClass || type.GetConstructor(Type.EmptyTypes) != null)
                    && interfaceType.IsAssignableFrom(type)));
            }

            return TypesImplementingInterfaceCache[interfaceType] = results;
        }

        private class FieldInterfaceSelectWindow : SaintsObjectPickerWindowIMGUI
        {
            private Type _fieldType;
            private Type _interfaceType;
            private Action<Object> _onSelected;

            public static void Open(Object curValue, Type originType, Type interfaceType, Action<Object> onSelected)
            {
                FieldInterfaceSelectWindow thisWindow = CreateInstance<FieldInterfaceSelectWindow>();
                thisWindow.titleContent = new GUIContent($"Select {originType.Name} with {interfaceType}");
                thisWindow._fieldType = originType;
                thisWindow._interfaceType = interfaceType;
                thisWindow._onSelected = onSelected;
                thisWindow.SetDefaultActive(curValue);
                thisWindow.ShowAuxWindow();
            }

            protected override bool AllowScene => true;

            protected override bool AllowAssets => true;

            private string _error = "";
            protected override string Error => _error;

            protected override IEnumerable<ItemInfo> FetchAllAssets()
            {
                IEnumerable<ItemInfo> r = base.FetchAllAssets();
                return SplitEachTarget(r, _interfaceType);
            }

            protected override IEnumerable<ItemInfo> FetchAllSceneObject()
            {
                IEnumerable<ItemInfo> r = base.FetchAllSceneObject();
                return SplitEachTarget(r, _interfaceType);
            }

            private static IEnumerable<ItemInfo> SplitEachTarget(IEnumerable<ItemInfo> itemInfos, Type interfaceType)
            {
                foreach (ItemInfo itemInfo in itemInfos)
                {
                    if (itemInfo.Object is GameObject go)
                    {
                        Component[] comps = go.GetComponents<Component>();

                        Dictionary<Type, List<Component>> typeToComponents = new Dictionary<Type, List<Component>>();

                        foreach (Component component in comps.Where(each => each != null))
                        {
                            Type compType = component.GetType();
                            if(interfaceType.IsInstanceOfType(component))
                            {
                                if (!typeToComponents.TryGetValue(compType, out List<Component> components))
                                {
                                    typeToComponents[compType] = components = new List<Component>();
                                }

                                components.Add(component);
                            }
                        }

                        foreach (List<Component> components in typeToComponents.Values)
                        {
                            foreach ((Component component, int index) in components.WithIndex())
                            {
                                yield return new ItemInfo
                                {
                                    failedCount = itemInfo.failedCount,
                                    GuiLabel = itemInfo.GuiLabel,
                                    Icon = itemInfo.Icon,
#if UNITY_6000_4_OR_NEWER
                                    InstanceID = component.GetEntityId(),
#else
                                    InstanceID = component.GetInstanceID(),
#endif
                                    Label = components.Count == 1 ? component.name : $"{component.name} [{index}]",
                                    Object = component,
                                    preview = itemInfo.preview,
                                };
                            }
                        }
                    }
                    else
                    {
                        yield return itemInfo;
                    }
                }
            }


            protected override bool IsEqual(ItemInfo itemInfo, Object target)
            {
                Object itemObject = itemInfo.Object;
                Debug.Assert(itemObject, itemObject);

                return itemObject == target;
            }

            protected override void OnSelect(ItemInfo itemInfo)
            {
                _error = "";
                Object obj = itemInfo.Object;

                if(!FetchFilter(itemInfo))
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError($"Selected object {obj} has no component {_fieldType}/{_interfaceType}");
#endif
                    _error = $"{itemInfo.Label} is invalid";
                    return;
                }
                _onSelected(obj);
            }

            protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            private bool FetchFilter(ItemInfo itemInfo)  // gameObject, Sprite, Texture2D, ...
            {
                return TryGetMatchedInterfaceValue(itemInfo.Object, _fieldType, _interfaceType, out _);
            }
        }

        public static (bool isMatch, Object result) GetSerializedObject(Object selectedObject, Type fieldType,
            Type interfaceType)
        {
            bool fieldTypeIsComponent = typeof(Component).IsAssignableFrom(fieldType);

            switch (selectedObject)
            {
                case GameObject go:
                {
                    // Debug.Log($"go={go}, fieldType={_fieldType}, interfaceType={_interfaceType}");
                    if (fieldTypeIsComponent)
                    {
                        Component compResult = go.GetComponents(fieldType)
                            .FirstOrDefault(interfaceType.IsInstanceOfType);
                        return compResult == null
                            ? (false, null)
                            : (true, compResult);
                    }

                    if (!fieldType.IsInstanceOfType(go))
                    {
                        return (false, null);
                    }

                    Component result = go.GetComponents(typeof(Component))
                        .FirstOrDefault(interfaceType.IsInstanceOfType);
                    return result == null
                        ? (false, null)
                        : (true, result);
                }
                case Component comp:
                {
                    if (fieldTypeIsComponent)
                    {
                        Component compResult = comp.GetComponents(fieldType)
                            .FirstOrDefault(interfaceType.IsInstanceOfType);
                        return compResult == null
                            ? (false, null)
                            : (true, compResult);
                    }

                    if (!fieldType.IsInstanceOfType(comp))
                    {
                        return (false, null);
                    }

                    Component result = comp.GetComponents(typeof(Component))
                        .FirstOrDefault(interfaceType.IsInstanceOfType);
                    return result == null
                        ? (false, null)
                        : (true, result);
                }
                case ScriptableObject so:
                    // Debug.Log(fieldType);
                    return (fieldType == typeof(ScriptableObject) || fieldType.IsSubclassOf(typeof(ScriptableObject)) || typeof(ScriptableObject).IsSubclassOf(fieldType))
                           && interfaceType.IsInstanceOfType(so)
                           ? (true, so)
                           : (false, null);
                default:
                    return new[]
                    {
                        fieldType,
                        interfaceType,
                    }.All(requiredType => requiredType.IsInstanceOfType(selectedObject))
                        ? (true, selectedObject)
                        : (false, null);

                    // Type itemType = itemInfo.Object.GetType();
                    // return checkTypes.All(requiredType => itemType.IsInstanceOfType(requiredType));

            }
        }

        private static (string error, IWrapProp propInfo, int index, object parent) GetSerName(SerializedProperty property, MemberInfo fieldInfo)
        {
            (SerializedUtils.FieldOrProp _, object parent) = SerializedUtils.GetFieldInfoAndDirectParent(property);

            (string error, int arrayIndex, object value) = Util.GetValue(property, fieldInfo, parent);

            if (error != "")
            {
                return (error, null, -1, null);
            }

            IWrapProp curValue = (IWrapProp) value;
            return ("", curValue, arrayIndex, parent);
        }

    }
}
