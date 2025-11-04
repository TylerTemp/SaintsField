using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.BaseWrapTypeDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsWrapTypeDrawer
{
    public static class SaintsWrapUtils
    {
        public static VisualElement CreateCellElement(FieldInfo info, Type rawType, SerializedProperty serializedProperty, IReadOnlyList<Attribute> injectedAttributes, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent)
        {
            Attribute[] allCustomAttributes = ReflectCache.GetCustomAttributes<Attribute>(info);
            // Debug.Log($"{info.Name}: {string.Join<PropertyAttribute>(", ", allAttributes)}");

            // List<Attribute> allAttributes = new List<Attribute>();
            List<PropertyAttribute> allPropertyAttributes = new List<PropertyAttribute>();

            foreach (Attribute attr in injectedAttributes.Concat(allCustomAttributes))
            {
                // allAttributes.Add(attr);
                if (attr is PropertyAttribute propAttr)
                {
                    allPropertyAttributes.Add(propAttr);
                }

            }

            Type useDrawerType = null;
            Attribute useAttribute = null;

            // string wrapName = ReflectUtils.GetIWrapPropName(rawType);
            // Type wrapType = ReflectUtils.GetIWrapPropType(rawType, wrapName);
            // FieldInfo wrapInfo = (FieldInfo)ReflectUtils.GetProp(rawType, wrapName).fieldOrMethodInfo;
            Type wrapInstanceType = info.FieldType.GetGenericArguments()[0];
            Type underType = wrapInstanceType.GetGenericArguments()[0];

            bool needUseRef;
            bool targetIsArrayOrList = false;
            if (underType.IsArray)
            {
                targetIsArrayOrList = true;
                needUseRef = !RuntimeUtil.IsSubFieldUnitySerializable(underType.GetElementType());
            }
            else if(underType.IsGenericType && underType.GetGenericTypeDefinition() == typeof(List<>))
            {
                targetIsArrayOrList = true;
                needUseRef = !RuntimeUtil.IsSubFieldUnitySerializable(underType.GetGenericArguments()[0]);
            }
            else
            {
                needUseRef = injectedAttributes.Any(each => each is SerializeReference)
                             || !RuntimeUtil.IsSubFieldUnitySerializable(underType);
            }

// #if SAINTSFIELD_DEBUG
//             Debug.Log($"needUseRef={needUseRef}/{serializedProperty.propertyPath}");
// #endif

            if (!needUseRef)
            {
                if (underType.IsArray)
                {
                    Type arrayElement = underType.GetElementType();
                    Debug.Assert(arrayElement != null);
                    needUseRef = arrayElement.IsInterface || arrayElement.IsAbstract;
                }
                else if (underType.IsGenericType && underType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type listElement = underType.GetGenericArguments()[0];
                    needUseRef = listElement.IsInterface || listElement.IsAbstract;
                }
            }
            SerializedProperty wrapTypeProp = serializedProperty.FindPropertyRelative("wrapType");
            Debug.Assert(wrapTypeProp != null);
            string wrapName;
            if (needUseRef)
            {
                if (underType.IsArray)
                {
                    wrapName = "valueArray";
                    if (wrapTypeProp.intValue != (int)WrapType.Array)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.Log($"set wrap from {(WrapType)wrapTypeProp.intValue} to {WrapType.Array}");
#endif
                        wrapTypeProp.intValue = (int)WrapType.Array;
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
                else if (underType.IsGenericType && underType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    wrapName = "valueList";
                    if (wrapTypeProp.intValue != (int)WrapType.List)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.Log($"set wrap from {(WrapType)wrapTypeProp.intValue} to {WrapType.List}");
#endif
                        wrapTypeProp.intValue = (int)WrapType.List;
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    wrapName = "valueField";
                    if (wrapTypeProp.intValue != (int)WrapType.Field)
                    {

#if SAINTSFIELD_DEBUG
                        Debug.Log($"set wrap from {(WrapType)wrapTypeProp.intValue} to {WrapType.Field}");
#endif
                        wrapTypeProp.intValue = (int)WrapType.Field;
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                wrapName = "value";
                if (wrapTypeProp.intValue != (int)WrapType.T)
                {

#if SAINTSFIELD_DEBUG
                    Debug.Log($"set wrap from {wrapTypeProp.intValue} to {WrapType.T}");
#endif
                    wrapTypeProp.intValue = (int)WrapType.T;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            Type wrapType = ReflectUtils.GetIWrapPropType(rawType, wrapName);
            FieldInfo wrapInfo = (FieldInfo)ReflectUtils.GetProp(rawType, wrapName).fieldOrMethodInfo;

            SerializedProperty serializedBaseProperty = serializedProperty.FindPropertyRelative(wrapName) ??
                                                        SerializedUtils.FindPropertyByAutoPropertyName(
                                                            serializedProperty, wrapName);
            // bool isArray = serializedBaseProperty.propertyType == SerializedPropertyType.Generic
            //                && serializedBaseProperty.isArray;
            if(!needUseRef)
            {
                if (!targetIsArrayOrList)
                {
                    ISaintsAttribute saintsAttr = allPropertyAttributes
                        .OfType<ISaintsAttribute>()
                        .FirstOrDefault();

                    // Debug.Log(saintsAttr);

                    useAttribute = saintsAttr as Attribute;
                    if (saintsAttr != null)
                    {
                        useDrawerType = SaintsPropertyDrawer.GetFirstSaintsDrawerType(saintsAttr.GetType());
                    }
                    else
                    {
                        (Attribute attrOrNull, Type drawerType) =
                            SaintsPropertyDrawer.GetFallbackDrawerType(wrapInfo, serializedBaseProperty, allCustomAttributes);
                        // Debug.Log($"{FieldWithInfo.SerializedProperty.propertyPath}: {drawerType}");
                        useAttribute = attrOrNull;
                        useDrawerType = drawerType;

                        if (useDrawerType == null &&
                            serializedBaseProperty.propertyType == SerializedPropertyType.Generic)
                        {
                            PropertyAttribute prop = new SaintsRowAttribute(inline: true);
                            useAttribute = prop;
                            useDrawerType = typeof(SaintsRowAttributeDrawer);
                            allPropertyAttributes.Insert(0, prop);
                            // appendPropertyAttributes = allCustomAttributes.Prepend(prop).ToArray();
                        }
                    }
                }

                // Debug.Log($"useDrawerType={useDrawerType}, {serializedProperty.propertyPath}");
            }
            else
            {
                useDrawerType = typeof(BaseWrapDrawer);
            }

            // Debug.Log($"{info.Name}: {serializedBaseProperty.propertyPath}/{useDrawerType}");

            if (useDrawerType == null)
            {
                VisualElement r = UIToolkitUtils.CreateOrUpdateFieldRawFallback(
                    serializedBaseProperty,
                    allPropertyAttributes,
                    wrapType,
                    null,
                    wrapInfo,
                    true,
                    makeRenderer,
                    doTweenPlayRecorder,
                    null,
                    parent
                );
                VisualElement merged = UIToolkitCache.MergeWithDec(r, allPropertyAttributes);
                UIToolkitUtils.CheckOutOfScoopFoldout(merged, new HashSet<Toggle>());
                return merged;
            }

            // Nah... This didn't handle for mis-ordered case
            // // Above situation will handle all including SaintsRow for general class/struct/interface.
            // // At this point we only need to let Unity handle it
            // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // };
            // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // return (result, false);

            // Debug.Log($"{useAttribute}/{useDrawerType}: {serializedProperty.propertyPath}");

            PropertyDrawer propertyDrawer = useDrawerType == typeof(BaseWrapDrawer)
                ? SaintsPropertyDrawer.MakePropertyDrawer(useDrawerType, info, null, null)  // baseWrap itself will looking into the wrap
                : SaintsPropertyDrawer.MakePropertyDrawer(useDrawerType, wrapInfo, useAttribute, null);
            // Debug.Log(propertyDrawer);
            if (propertyDrawer is SaintsPropertyDrawer saintsPropertyDrawer)
            {
                saintsPropertyDrawer.InHorizontalLayout = true;
                saintsPropertyDrawer.AppendPropertyAttributes = allPropertyAttributes;
                // Debug.Log($"{needUseRef}{saintsPropertyDrawer is BaseWrapDrawer}/{saintsPropertyDrawer}");
                // Debug.Log($"{info.Name}: {serializedBaseProperty.propertyPath} -> {string.Join(", ", saintsPropertyDrawer.AppendPropertyAttributes)}");
            }

            MethodInfo uiToolkitMethod = useDrawerType.GetMethod("CreatePropertyGUI");

            // bool isSaintsDrawer = useDrawerType.IsSubclassOf(typeof(SaintsPropertyDrawer)) || useDrawerType == typeof(SaintsPropertyDrawer);

            bool useImGui = uiToolkitMethod == null ||
                            uiToolkitMethod.DeclaringType == typeof(PropertyDrawer);  // null: old Unity || did not override

            // Debug.Log($"{useDrawerType}/{uiToolkitMethod.DeclaringType}/{FieldWithInfo.SerializedProperty.propertyPath}");

            if (!useImGui)
            {
                // Debug.Log($"{propertyDrawer} draw {serializedProperty.propertyPath}");
                VisualElement r = propertyDrawer.CreatePropertyGUI(propertyDrawer is BaseWrapDrawer? serializedProperty: serializedBaseProperty);
                VisualElement merged = UIToolkitCache.MergeWithDec(r, allPropertyAttributes);
                UIToolkitUtils.CheckOutOfScoopFoldout(merged, new HashSet<Toggle>());
                return merged;
            }

            // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
            // We don't need to handle decorators either
            // Debug.Log(serializedProperty.propertyPath);
            // Debug.Log(info.FieldType);
            PropertyField result = new PropertyField(serializedBaseProperty, string.Empty)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            result.Bind(serializedBaseProperty.serializedObject);
            return result;
        }


        public static IReadOnlyList<Attribute> GetInjectedPropertyAttributes(FieldInfo info, Type expectedInjector)
        {
            List<Attribute> result = new List<Attribute>();

            foreach (Attribute propertyAttribute in ReflectCache.GetCustomAttributes<Attribute>(info))
            {
                if (propertyAttribute is not InjectAttributeBase injectBase ||
                    !expectedInjector.IsAssignableFrom(propertyAttribute.GetType()))
                {
                    continue;
                }

                Attribute injectedAttribute;
                try
                {
                    if(injectBase.Parameters.Length > 0)
                    {
                        // Debug.Log($"{injectBase.Decorator}: {string.Join(", ", injectBase.Parameters)}");
                        injectedAttribute =
                            Activator.CreateInstance(injectBase.Decorator, injectBase.Parameters) as Attribute;
                    }
                    else
                    {
                        // Debug.Log($"{injectBase.Decorator}");
                        injectedAttribute= Activator.CreateInstance(injectBase.Decorator, true) as Attribute;
                    }
                    // injectedAttribute = Activator.CreateInstance(injectBase.Decorator,
                    //     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, injectBase.Parameters,
                    //     null, null) as Attribute;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    continue;
                }

                if (injectedAttribute != null)
                {
                    result.Add(injectedAttribute);
                }
            }

            return result;
        }
    }
}
