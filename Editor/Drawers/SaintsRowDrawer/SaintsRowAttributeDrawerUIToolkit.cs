#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer;
using SaintsField.Editor.Drawers.GuidDrawer;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Playa;
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
        // private static string NameActualContainer(SerializedProperty property) => $"{property.propertyPath}__saints_row";

        public static VisualElement CreateElement(SerializedProperty property, string label, MemberInfo info, bool inHorizontalLayout, SaintsRowAttribute saintsRowAttribute, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent)
        {
            bool inline = saintsRowAttribute?.Inline ?? false;

            if (ForceInlineScoop.InlineCount > 0)
            {
                if (!inline)
                {
                    inline = true;
                }
                ForceInlineScoop.InlineCount--;
            }

            VisualElement root;

            if (inline)
            {
                // root = new EmptyPrefabOverrideElement(property)
                root = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                    // name = NameActualContainer(property),
                };
                root.Add(new Label
                {
                    style =
                    {
                        display = DisplayStyle.None,
                    },
                });
            }
            else
            {
                Foldout foldout = new FoldoutPrefabOverrideElement(property)
                {
                    text = label,
                    value = property.isExpanded,
                    style =
                    {
                        flexGrow = 1,
                    },
                    // name = NameActualContainer(property),
                    viewDataKey = property.propertyPath,
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    property.isExpanded = evt.newValue;
                });

                UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => {});

                root = foldout;
            }

            root.AddToClassList(ClassLabelFieldUIToolkit);

            root.AddToClassList(SaintsRowClass);
            root.AddToClassList(ClassAllowDisable);

            FillElement(root, label, property, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder, parent);

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
                        if (!SerializedUtils.IsOk(property))
                        {
                            return;
                        }
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

                            // VisualElement actualContainer = root.Q<VisualElement>(NameActualContainer(property));
                            root.Clear();
                            if (inline)
                            {
                                root.Add(new Label
                                {
                                    style =
                                    {
                                        display = DisplayStyle.None,
                                    },
                                });
                            }

                            SerializedProperty newProp = property.serializedObject.FindProperty(propPath);

                            FillElement(root, label, newProp, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder, parent);
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

        private static void FillElement(VisualElement root, string label, SerializedProperty property, MemberInfo info, bool inHorizontalLayout, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent)
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
                // Debug.Log($"{info.Name}/{property.propertyPath}/{saintsSerializedActual.Name}/{saintsSerializedActual.ElementType}");
                VisualElement renderSerializedActual = RenderSerializedActual(saintsSerializedActual, label, property, (FieldInfo)info, inHorizontalLayout, parent);
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

            // VisualElement actualContainer = root.Q<VisualElement>(NameActualContainer(property));

            root.Add(bodyElement);
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(doTweenPlayRecorder));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(doTweenPlayRecorder));
#endif
        }

        private static VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual,
            string label, SerializedProperty property, FieldInfo serInfo, bool inHorizontalLayout, object parent)
        {
            // Debug.Log(property.propertyPath);
            Attribute[] attributes = ReflectCache.GetCustomAttributes(serInfo);

            EnumToggleButtonsAttribute enumToggle = null;
            FlagsTreeDropdownAttribute flagsTreeDropdownAttribute = null;
            FlagsDropdownAttribute flagsDropdownAttribute = null;
            DateTimeAttribute dateTimeAttribute = null;
            TimeSpanAttribute timeSpanAttribute = null;
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
                    case DateTimeAttribute dt:
                        dateTimeAttribute = dt;
                        break;
                    case TimeSpanAttribute ts:
                        timeSpanAttribute = ts;
                        break;
                }
            }

            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
#endif
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                    Debug.Log($"saintsrow serInfo={serInfo.Name} attrs = {string.Join(", ", attributes.Select(a => a.GetType().Name))}");
#endif

                    if (enumToggle != null)
                    {
                        return EnumToggleButtonsAttributeDrawer.RenderSerializedActual(saintsSerializedActual, enumToggle, label, property, serInfo, parent);
                    }
                    return TreeDropdownAttributeDrawer.RenderSerializedActual(saintsSerializedActual, (ISaintsAttribute)flagsTreeDropdownAttribute ?? flagsDropdownAttribute, label, property, parent);
                    // return null;
                }
                case SaintsPropertyType.Interface:
                {
                    return SaintsInterfaceDrawer.RenderSerializedActual(saintsSerializedActual, label, property, attributes, inHorizontalLayout, serInfo, parent);
                }
                case SaintsPropertyType.DateTime:
                    return DateTimeAttributeDrawer.RenderSerializedActual(dateTimeAttribute, label, property, inHorizontalLayout);
                case SaintsPropertyType.TimeSpan:
                    return TimeSpanAttributeDrawer.RenderSerializedActual(timeSpanAttribute, label, property, attributes, inHorizontalLayout);
                case SaintsPropertyType.Guid:
                    return GuidAttributeDrawer.RenderSerializedActual(label, property, inHorizontalLayout);
                case SaintsPropertyType.Undefined:
                case SaintsPropertyType.ClassOrStruct:
                default:
                    return null;
            }
        }

        // private static Type GetElementType(Type rawType)
        // {
        //     if (rawType.IsArray)
        //     {
        //         return rawType.GetElementType();
        //     }
        //
        //     Type listType = SaintsEditorUtils.GetList(rawType);
        //     // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        //     // ReSharper disable once ConvertIfStatementToReturnStatement
        //     if (listType == null)
        //     {
        //         return rawType;
        //     }
        //
        //     return listType.GetGenericArguments()[0];
        // }
    }
}
#endif
