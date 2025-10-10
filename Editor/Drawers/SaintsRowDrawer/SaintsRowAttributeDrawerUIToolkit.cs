#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsRowDrawer
{
    public partial class SaintsRowAttributeDrawer
    {
        public class ForceInlineScoop : IDisposable
        {
            // ReSharper disable once NotAccessedField.Global
            public static int InlineCount;

            public ForceInlineScoop(int inlineCount)
            {
                InlineCount = inlineCount;
            }

            public void Dispose()
            {
                InlineCount = 0;
            }
        }

        protected override bool UseCreateFieldUIToolKit => true;

        public const string SaintsRowClass = "saints-field--saintsrow";
        private static string NameActualContainer(SerializedProperty property) => $"${property.propertyPath}__saints_row";

        public static VisualElement CreateElement(SerializedProperty property, string label, MemberInfo info, bool inHorizontalLayout, SaintsRowAttribute saintsRowAttribute, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent)
        {
            bool inline = saintsRowAttribute?.Inline ?? false;

            if (!inline && ForceInlineScoop.InlineCount > 0)
            {
                inline = true;
            }

            ForceInlineScoop.InlineCount--;

            VisualElement root;

            if (inline)
            {
                root = new EmptyPrefabOverrideElement(property)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                    name = NameActualContainer(property),
                };
            }
            else
            {
                VisualElement foldoutWrapper = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };

                Foldout foldout = new Foldout
                {
                    text = label,
                    value = property.isExpanded,
                    style =
                    {
                        flexGrow = 1,
                    },
                    name = NameActualContainer(property),
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    property.isExpanded = evt.newValue;
                });

                foldoutWrapper.Add(foldout);
                foldoutWrapper.Add(new EmptyPrefabOverrideElement(property)
                {
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        bottom = 0,
                        left = 0,
                        right = 0,
                        height = 18,
                    },
                    pickingMode = PickingMode.Ignore,
                });

                // foldout.Add(new EmptyPrefabOverrideElement(property)
                // {
                //     style =
                //     {
                //         position = Position.Absolute,
                //         top = -20,
                //         bottom = 0,
                //         left = -15,
                //         right = 0,
                //         height = 18,
                //     },
                //     pickingMode = PickingMode.Ignore,
                // });

                // Toggle toggle = foldout.Q<Toggle>();
                // if (toggle != null)
                // {
                //     toggle.Add(
                //         new EmptyPrefabOverrideElement(property)
                //         {
                //             style =
                //             {
                //                 position = Position.Absolute,
                //                 top = 0,
                //                 bottom = 0,
                //                 left = 0,
                //                 right = 0,
                //             },
                //             pickingMode = PickingMode.Ignore,
                //         });
                //     // Label toggleLabel = toggle.Q<Label>();
                //     // if (toggleLabel != null)
                //     // {
                //     //     // EmptyPrefabOverrideElement emptyPrefabOverrideElement =
                //     //     //     new EmptyPrefabOverrideElement(property)
                //     //     //     {
                //     //     //         style =
                //     //     //         {
                //     //     //             position = Position.Absolute,
                //     //     //             top = 0,
                //     //     //             bottom = 0,
                //     //     //             left = 0,
                //     //     //             right = 0,
                //     //     //         },
                //     //     //         pickingMode = PickingMode.Ignore,
                //     //     //     };
                //     //     // toggleLabel.Add(emptyPrefabOverrideElement);
                //     // }
                // }

                UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => {});

                // root = foldout;
                root = foldoutWrapper;
            }

            root.AddToClassList(SaintsRowClass);
            root.AddToClassList(ClassAllowDisable);

            FillElement(root, label, property, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder);

            // ReSharper disable once InvertIf
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                string propPath = property.propertyPath;
                root.userData = property.managedReferenceId;
                root.schedule.Execute(() =>
                    {
                        long curId = (long) root.userData;
                        // ReSharper disable once InvertIf
                        long newId;
                        try
                        {
                            newId = property.managedReferenceId;
                        }
                        catch (InvalidOperationException)
                        {
                            return;
                        }
                        // ReSharper disable once InvertIf
                        if (curId != newId)
                        {
                            // Debug.Log($"{property.propertyPath} Changed {curId} -> {property.managedReferenceId}/{property.managedReferenceFieldTypename}");
                            root.userData = property.managedReferenceId;

                            VisualElement actualContainer = root.Q<VisualElement>(NameActualContainer(property));
                            actualContainer.Clear();

                            SerializedProperty newProp = property.serializedObject.FindProperty(propPath);

                            FillElement(root, label, newProp, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder);
                        }
                        // Debug.Log(property.managedReferenceId);
                    })
                    .Every(100);
            }
            return root;
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsRowAttribute saintsRowAttribute = saintsAttribute as SaintsRowAttribute;

            VisualElement ele = CreateElement(property, property.displayName, info, InHorizontalLayout, saintsRowAttribute, this, this, parent);
            //
            // if (InHorizentalLayout)
            // {
            //     ele.style.marginLeft = IndentWidth;
            // }

            return ele;
        }

        // private static Type GetMemberType(MemberInfo member)
        // {
        //     return member.MemberType == MemberTypes.Property
        //         ? ((PropertyInfo) member).PropertyType
        //         : ((FieldInfo) member).FieldType;
        // }

        private static void FillElement(VisualElement root, string label, SerializedProperty property, MemberInfo info, bool inHorizontalLayout, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder)
        {
            // Debug.Log(info.Name);
            // Debug.Log(info.DeclaringType);
            // Debug.Log(info.ReflectedType);

            SaintsSerializedActualAttribute saintsSerializedActual = ReflectCache.GetCustomAttributes<SaintsSerializedActualAttribute>(info).FirstOrDefault();
            // Debug.Log($"{saintsSerializedActual?.Path}/{saintsSerializedActual?.PathType}");
            if (saintsSerializedActual != null)
            {
                if (label.EndsWith("__Saints Serialized__"))
                {
                    label = label[..^"__Saints Serialized__".Length];
                }
                VisualElement renderSerializedActual = RenderSerializedActual(saintsSerializedActual, label, property, info, saintsSerializedActual.ElementType, SerializedUtils.GetFieldInfoAndDirectParent(property).parent);
                root.Add(renderSerializedActual);
                return;
            }

            // Debug.Log($"{property.propertyPath}: {inHorizontalLayout}");
            object value = null;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                value = property.managedReferenceValue;
                if (value == null)
                {
                    return;
                }
            }
            else
            {
                string error = "";
                object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                // Debug.Log($"parentValue={parentValue}/{property.propertyPath}");
                if (parentValue == null)
                {
                    error = $"Parent of {property.propertyPath} not found";
                }
                else
                {
                    (string getValueError, int _, object getValue) = Util.GetValue(property, info, parentValue);
                    if (getValueError != "")
                    {
                        error = getValueError;
                    }
                    else
                    {
                        value = getValue;
                    }
                }

                if (error != "")
                {
                    root.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                Debug.Assert(value != null);
            }

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            IReadOnlyList<ISaintsRenderer> renderer =
                SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, makeRenderer, new []{value});

             VisualElement bodyElement = new VisualElement();

             Type objectType = value.GetType();
             IPlayaClassAttribute[] playaClassAttributes = ReflectCache.GetCustomAttributes<IPlayaClassAttribute>(objectType);

             foreach (ISaintsRenderer saintsRenderer in SaintsEditor.GetClassStructRenderer(objectType, playaClassAttributes, property.serializedObject, new[]{value}))
             {
                 VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                 if (rendererElement != null)
                 {
                     bodyElement.Add(rendererElement);
                 }
             }

            // this... fixed by adding Bind()... wtf...
            foreach (ISaintsRenderer saintsRenderer in renderer)
            {
                saintsRenderer.InAnyHorizontalLayout = inHorizontalLayout;
                saintsRenderer.SetSerializedProperty(property);
                VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                if (rendererElement != null)
                {
                    // Debug.Log($"add: {saintsRenderer}");
                    bodyElement.Add(rendererElement);
                }
            }

            VisualElement actualContainer = root.Q<VisualElement>(NameActualContainer(property));

            actualContainer.Add(bodyElement);
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(doTweenPlayRecorder));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(doTweenPlayRecorder));
#endif
        }

        private static VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual,
            string label, SerializedProperty property, MemberInfo serInfo, Type targetType, object parent)
        {
            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
#endif
                {
                    // Attribute[] attributes = PointedTargetAttributes(saintsSerializedActual.Name, property.serializedObject.targetObject.GetType());
                    Attribute[] attributes = ReflectCache.GetCustomAttributes(serInfo);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                    Debug.Log($"saintsrow serInfo={serInfo.Name} attrs = {string.Join(", ", attributes.Select(a => a.GetType().Name))}");
#endif
                    EnumToggleButtonsAttribute enumToggle = null;
                    FlagsTreeDropdownAttribute flagsTreeDropdownAttribute = null;
                    FlagsDropdownAttribute flagsDropdownAttribute = null;
                    foreach (Attribute attribute in attributes)
                    {
                        switch (attribute)
                        {
                            case EnumToggleButtonsAttribute et:
                                enumToggle = et;
                                break;
                            case FlagsTreeDropdownAttribute ftd:
                                flagsTreeDropdownAttribute = ftd;
                                break;
                            case FlagsDropdownAttribute fd:
                                flagsDropdownAttribute = fd;
                                break;
                        }
                    }
                    if (enumToggle != null)
                    {
                        return EnumToggleButtonsAttributeDrawer.RenderSerializedActual(enumToggle, label, property, serInfo, targetType, parent);
                    }
                    return TreeDropdownAttributeDrawer.RenderSerializedActual((ISaintsAttribute)flagsTreeDropdownAttribute ?? flagsDropdownAttribute, label, property, targetType);
                    // return null;
                }
                case SaintsPropertyType.Undefined:
                default:
                    return null;
            }
        }

        // private static Attribute[] PointedTargetAttributes(IReadOnlyList<SaintsSerializedPath> paths, Type serObjType)
        // private static Attribute[] PointedTargetAttributes(string name, Type containerType)
        // {
        //     MemberInfo[] targetMemberInfos = containerType.GetMember(
        //         name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
        //     // ReSharper disable once InvertIf
        //     if (targetMemberInfos.Length == 0)
        //     {
        //         Debug.LogWarning($"failed to find {name} on type {containerType}");
        //         return Array.Empty<Attribute>();
        //     }
        //
        //     return ReflectCache.GetCustomAttributes(targetMemberInfos[0]);
        //
        //     // Type accType = serObjType;
        //     // MemberInfo targetMemberInfo = null;
        //     // foreach (SaintsSerializedPath path in paths)
        //     // {
        //     //     bool pathIsProperty = path.IsProperty;
        //     //     if (pathIsProperty)
        //     //     {
        //     //         PropertyInfo propertyInfo = accType.GetProperty(
        //     //             path.Name,
        //     //             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        //     //         if (propertyInfo == null)
        //     //         {
        //     //             Debug.LogWarning($"Failed to get attributes of {path.Name} under {serObjType}, chain: {string.Join("->", paths.Select(p => p.Name))}");
        //     //             return Array.Empty<Attribute>();
        //     //         }
        //     //
        //     //         targetMemberInfo = propertyInfo;
        //     //         accType = GetElementType(propertyInfo.PropertyType);
        //     //     }
        //     //     else
        //     //     {
        //     //         FieldInfo fieldInfo = accType.GetField(
        //     //             path.Name,
        //     //             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        //     //         if (fieldInfo == null)
        //     //         {
        //     //             Debug.LogWarning($"Failed to get attributes of {path.Name} under {serObjType}, chain: {string.Join("->", paths.Select(p => p.Name))}");
        //     //             return Array.Empty<Attribute>();
        //     //         }
        //     //
        //     //         targetMemberInfo = fieldInfo;
        //     //         accType = GetElementType(fieldInfo.FieldType);
        //     //     }
        //     //     // MemberInfo[] members = accType.GetMember(
        //     //     //     path.Name,
        //     //     //     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        //     //     // Type memberElementType = null;
        //     //     // foreach (MemberInfo memberInfo in members)
        //     //     // {
        //     //     //     switch (memberInfo.MemberType)
        //     //     //     {
        //     //     //         case MemberTypes.Field:
        //     //     //     }
        //     //     // }
        //     // }
        //     //
        //     // // ReSharper disable once InvertIf
        //     // if (targetMemberInfo == null)
        //     // {
        //     //     Debug.LogWarning($"Failed to get attributes of {serObjType}, chain: {string.Join("->", paths.Select(p => p.Name))}");
        //     //     return Array.Empty<Attribute>();
        //     // }
        //
        //     // return ReflectCache.GetCustomAttributes(targetMemberInfo);
        // }

        private static Type GetElementType(Type rawType)
        {
            if (rawType.IsArray)
            {
                return rawType.GetElementType();
            }

            Type listType = SaintsEditorUtils.GetList(rawType);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (listType == null)
            {
                return rawType;
            }

            return listType.GetGenericArguments()[0];
        }
    }
}
#endif
