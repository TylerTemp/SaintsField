#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers;
using SaintsField.Editor.Drawers.FullWidthRichLabelDrawer;
using SaintsField.Editor.Drawers.RichLabelDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// #if SAINTSFIELD_OBVIOUS_SOAP
// using Obvious.Soap;
// #endif

namespace SaintsField.Editor.Core
{
    public partial class SaintsPropertyDrawer: IDOTweenPlayRecorder
    {
        protected virtual void OnDisposeUIToolkit()
        {

        }

        protected static string NameLabelFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-label-field";
        public static string ClassLabelFieldUIToolkit = "saints-field--label-field";

        public static string ClassNoRichLabelUpdate = "saints-field-no-rich-label-update";
        private static string NameSaintsPropertyDrawerOverrideLabel = "saints-property-drawer-override-label";

        protected static string ClassFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-field";

        public const string ClassAllowDisable = "saints-field-allow-disable";
        public static string UIToolkitFallbackName(SerializedProperty property) => $"saints-field--fallback-{property.propertyPath}";
        private static string UIToolkitOnChangedTrackerName(SerializedProperty property) =>
            $"saints-field-tracker--{property.propertyPath}";

        private static string NameSaintsPropertyDrawerRoot(SerializedProperty property) =>
            $"{SerializedUtils.GetUniqueId(property)}--saints-field--root";

        private static string NameSaintsPropertyDrawerContainer(SerializedProperty property) =>
            $"{SerializedUtils.GetUniqueId(property)}--saints-field--container";

        protected virtual bool UseCreateFieldUIToolKit => false;
        public bool SaintsSubRenderer = false;

        // public IReadOnlyList<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)> AppendSaintsAttributeDrawer;
        public IReadOnlyList<PropertyAttribute> AppendPropertyAttributes = null;
        public IReadOnlyList<PropertyAttribute> OverridePropertyAttributes = null;

        protected List<SaintsPropertyInfo> SaintsPropertyDrawers;

#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Create property gui {property.propertyPath}/{this}/{GetHashCode()}");
#endif
            if (!SerializedUtils.IsOk(property))
            {
                return new VisualElement();
            }

            // IMGUIContainer Fallback
            // if (SubDrawCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideDrawCount) &&
            //     insideDrawCount > 0)
            // {
            //     // Debug.Log($"Sub Draw GetPropertyHeight/{this}");
            //     // return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
            //     return new VisualElement();
            // }

            VisualElement containerElement = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                },
                name = NameSaintsPropertyDrawerContainer(property),
            };

            (PropertyAttribute[] allAttributesRaw, object parent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);
            PropertyAttribute[] allAttributes;
            if (SaintsSubRenderer)
            {
                allAttributes = attribute == null? Array.Empty<PropertyAttribute>(): new []{attribute};
            }
            else
            {
                if (OverridePropertyAttributes != null)
                {
                    allAttributes = OverridePropertyAttributes.ToArray();
                }
                else
                {
                    allAttributes = AppendPropertyAttributes == null
                        ? allAttributesRaw
                        : allAttributesRaw.Concat(AppendPropertyAttributes).ToArray();
                }
            }

            ISaintsAttribute[] iSaintsAttributes = allAttributes.OfType<ISaintsAttribute>().ToArray();
            // Debug.Assert(iSaintsAttributes.Length > 0, property.propertyPath);

            // IReadOnlyList<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
            //     .Select((each, index) => new SaintsWithIndex
            //     {
            //         SaintsAttribute = each,
            //         Index = index,
            //     })
            //     .ToArray();
            SaintsPropertyDrawers = iSaintsAttributes
                // .WithIndex()
                .Select((value, index) => new SaintsPropertyInfo
                {
                    Drawer = GetOrCreateSaintsDrawerByAttr(value),
                    Attribute = value,
                    Index = index,
                })
                .ToList();

            // PropertyField with empty label. This value will not be updated by Unity even call PropertyField.label = something, which has no actual effect in unity's drawer either
            if (string.IsNullOrEmpty(GetPreferredLabel(property)))
            {
                SaintsPropertyDrawers.RemoveAll(each => each.Attribute is RichLabelAttribute rl && string.IsNullOrEmpty(rl.RichTextXml));

                NoLabelAttribute noLabelAttribute = new NoLabelAttribute();

                bool found = false;
                for (int richLabelIndex = 0; richLabelIndex < SaintsPropertyDrawers.Count; richLabelIndex++)
                {
                    SaintsPropertyDrawer eachDrawer = SaintsPropertyDrawers[richLabelIndex].Drawer;
                    // ReSharper disable once InvertIf
                    if (eachDrawer is RichLabelAttributeDrawer)
                    {
                        found = true;
                        SaintsPropertyDrawers[richLabelIndex] = new SaintsPropertyInfo
                        {
                            Drawer = GetOrCreateSaintsDrawerByAttr(noLabelAttribute),
                            Attribute = noLabelAttribute,
                            Index = richLabelIndex,
                        };
                        break;
                    }
                }

                if (!found)
                {
                    SaintsPropertyDrawers.Add(new SaintsPropertyInfo
                    {
                        Drawer = GetOrCreateSaintsDrawerByAttr(noLabelAttribute),
                        Attribute = noLabelAttribute,
                        Index = SaintsPropertyDrawers.Count,
                    });
                }
            }

            // if in horizental layout, and not SaintsRow
            // 1. we need to swap RichLabel to AboveRichLabel, if it has content
            // 2. otherwise (NoLabel), we no longer added a `NoLabel` to it

            bool needAboveProcessor = InHorizontalLayout;
            if (needAboveProcessor)
            {
                if (GetType() == typeof(SaintsRowAttributeDrawer))
                {
                    needAboveProcessor = false;
                }
                else if (SaintsPropertyDrawers.Any(each => each.Drawer is SaintsRowAttributeDrawer
                                                           || each.Attribute is NoLabelAttribute
                                                           || (each.Attribute is RichLabelAttribute rl &&
                                                               string.IsNullOrEmpty(rl.RichTextXml))))
                {
                    needAboveProcessor = false;
                }
                else if (property.propertyType == SerializedPropertyType.Boolean && SaintsPropertyDrawers.All(each => each.Attribute.AttributeType != SaintsAttributeType.Field) && SaintsPropertyDrawers.All(each => each.Drawer is not LeftToggleAttributeDrawer))
                {
                    needAboveProcessor = false;
                    LeftToggleAttribute leftToggleAttribute =
                        new LeftToggleAttribute();

                    LeftToggleAttributeDrawer leftToggleAttributeDrawer =
                        (LeftToggleAttributeDrawer)
                        GetOrCreateSaintsDrawerByAttr(leftToggleAttribute);
                    // fullWidthRichLabelAttributeDrawer.IsSaintsPropertyDrawerOverrideLabel = true;
                    SaintsPropertyDrawers.Add(new SaintsPropertyInfo
                    {
                        Drawer = leftToggleAttributeDrawer,
                        Attribute = leftToggleAttribute,
                        Index = SaintsPropertyDrawers.Count,
                    });
                }
            }

            // Debug.Log($"needAboveProcessor={needAboveProcessor}; prop={property.propertyPath}; attrs={string.Join<PropertyAttribute>(",", allAttributes)}");

            if(needAboveProcessor)
            {
                bool alreadyHasRichLabel = false;
                bool alreadyHasNoLabel = false;
                foreach ((SaintsPropertyInfo saintsPropertyInfo, int index) in SaintsPropertyDrawers.WithIndex())
                {
                    if (saintsPropertyInfo.Attribute is RichLabelAttribute richLabel)
                    {
                        if (string.IsNullOrEmpty(richLabel.RichTextXml))
                        {
                            alreadyHasNoLabel = true;
                        }
                        else
                        {
                            alreadyHasRichLabel = true;
                            AboveRichLabelAttribute aboveRichLabelAttribute =
                                new AboveRichLabelAttribute(richLabel.RichTextXml, richLabel.IsCallback);

                            FullWidthRichLabelAttributeDrawer fullWidthRichLabelAttributeDrawer =
                                (FullWidthRichLabelAttributeDrawer)
                                GetOrCreateSaintsDrawerByAttr(aboveRichLabelAttribute);
                            // fullWidthRichLabelAttributeDrawer.IsSaintsPropertyDrawerOverrideLabel = true;
                            SaintsPropertyDrawers[index] = new SaintsPropertyInfo
                            {
                                Drawer = fullWidthRichLabelAttributeDrawer,
                                Attribute = aboveRichLabelAttribute,
                                Index = saintsPropertyInfo.Index,
                            };
                        }
                        break;
                    }
                }

                if(!alreadyHasNoLabel)
                {
                    // Debug.Log($"add no label: {property.propertyPath}");
                    NoLabelAttribute noLabelAttribute = new NoLabelAttribute();
                    SaintsPropertyDrawers.Add(new SaintsPropertyInfo
                    {
                        Drawer = GetOrCreateSaintsDrawerByAttr(noLabelAttribute),
                        Attribute = noLabelAttribute,
                        Index = SaintsPropertyDrawers.Count,
                    });
                }

                if (!alreadyHasRichLabel)
                {
                    AboveRichLabelAttribute aboveRichLabelAttribute =
                        // ReSharper disable once RedundantArgumentDefaultValue
                        new AboveRichLabelAttribute("<label />");

                    FullWidthRichLabelAttributeDrawer fullWidthRichLabelAttributeDrawer =
                        (FullWidthRichLabelAttributeDrawer)
                        GetOrCreateSaintsDrawerByAttr(aboveRichLabelAttribute);
                    // fullWidthRichLabelAttributeDrawer.IsSaintsPropertyDrawerOverrideLabel = true;
                    SaintsPropertyDrawers.Add(new SaintsPropertyInfo
                    {
                        Drawer = fullWidthRichLabelAttributeDrawer,
                        Attribute = aboveRichLabelAttribute,
                        Index = SaintsPropertyDrawers.Count,
                    });
                }
            }

            // if (AppendSaintsAttributeDrawer != null)
            // {
            //     foreach ((ISaintsAttribute appendAttr, SaintsPropertyDrawer appendDrawer) in AppendSaintsAttributeDrawer)
            //     {
            //         saintsPropertyDrawers.Add(new SaintsPropertyInfo
            //         {
            //             Drawer = appendDrawer,
            //             Attribute = appendAttr,
            //             Index = saintsPropertyDrawers.Count,
            //         });
            //     }
            // }



            // for type drawer that is in SaintsField, use this to draw with a fake property attribute
            SaintsPropertyInfo fieldAttributeWithIndex = SaintsPropertyDrawers.FirstOrDefault(each => each.Attribute.AttributeType == SaintsAttributeType.Field);
            if(fieldAttributeWithIndex.Attribute == null)
            {
                // fieldAttributeWithIndex = CheckSaintsPropertyInfoInject(property, allAttributes, fieldInfo, saintsPropertyDrawers.Count);
                // if (fieldAttributeWithIndex.Drawer != null)
                // {
                //     saintsPropertyDrawers.Add(fieldAttributeWithIndex);
                // }
                // else if(UseCreateFieldUIToolKit)
                // {
                //     saintsPropertyDrawers.Add(new SaintsPropertyInfo
                //     {
                //         Drawer = this,
                //         Attribute = null,
                //         Index = -1,
                //     });
                // }
                if(UseCreateFieldUIToolKit)
                {
                    SaintsPropertyDrawers.Add(new SaintsPropertyInfo
                    {
                        Drawer = this,
                        Attribute = null,
                        Index = -1,
                    });
                }
            }

            #region Above

            // if (InHorizentalLayout)
            // {
            //     Label saintsPropertyDrawerOverrideLabel = new Label(GetPreferredLabel(property));
            //     saintsPropertyDrawerOverrideLabel.AddToClassList(NameSaintsPropertyDrawerOverrideLabel);
            //     containerElement.Add(saintsPropertyDrawerOverrideLabel);
            // }

            Dictionary<string, List<SaintsPropertyInfo>> groupedAboveDrawers =
                new Dictionary<string, List<SaintsPropertyInfo>>();
            foreach (SaintsPropertyInfo eachAttributeWithIndex in SaintsPropertyDrawers)
            {
                string groupBy = eachAttributeWithIndex.Attribute?.GroupBy ?? "";
                if (!groupedAboveDrawers.TryGetValue(groupBy,
                        out List<SaintsPropertyInfo> currentGroup))
                {
                    groupedAboveDrawers[groupBy] = currentGroup = new List<SaintsPropertyInfo>();
                }

                currentGroup.Add(eachAttributeWithIndex);
            }

            Dictionary<string, VisualElement> aboveGroupByVisualElement = new Dictionary<string, VisualElement>();

            // ReSharper disable once UseDeconstruction
            foreach (KeyValuePair<string, List<SaintsPropertyInfo>> drawerInfoKv in groupedAboveDrawers)
            {
                string groupBy = drawerInfoKv.Key;

                VisualElement groupByContainer;
                if(groupBy == "")
                {
                    groupByContainer = new VisualElement();
                    containerElement.Add(groupByContainer);
                }
                else
                {
                    if(!aboveGroupByVisualElement.TryGetValue(groupBy, out groupByContainer))
                    {
                        aboveGroupByVisualElement[groupBy] = groupByContainer = new VisualElement();
                        groupByContainer.style.flexDirection = FlexDirection.Row;
                        containerElement.Add(groupByContainer);
                    }
                }

                foreach (SaintsPropertyInfo saintsPropertyInfo in drawerInfoKv.Value)
                {
                    groupByContainer.Add(saintsPropertyInfo.Drawer.CreateAboveUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, fieldInfo, parent));
                }
                // Debug.Log($"aboveUsedHeight={aboveUsedHeight}");
            }

            #endregion

            // labelRect.height = EditorGUIUtility.singleLineHeight;

            VisualElement labelFieldContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            containerElement.Add(labelFieldContainer);

            VisualElement overlayLabelContainer = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    left = LabelLeftSpace,
                    top = 0,
                    height = EditorGUIUtility.singleLineHeight,
                    width = LabelBaseWidth,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.NoWrap,
                    alignItems = Align.Center, // vertical
                    overflow = Overflow.Hidden,
                },
                pickingMode = PickingMode.Ignore,
            };
            // #region label info
            //
            // // if (labelAttributeWithIndex.SaintsAttribute != null)
            // // {
            // //     _saintsLabelDrawer = GetOrCreateSaintsDrawer(labelAttributeWithIndex);
            // // }
            // // else
            // // {
            // //     _saintsLabelDrawer = null;
            // // }
            //
            // #endregion

            #region label/field
            VisualElement fieldContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameLabelFieldUIToolkit(property),
                userData = null,
            };
            fieldContainer.AddToClassList(ClassLabelFieldUIToolkit);

            #region Pre Overlay

            foreach (SaintsPropertyInfo eachAttributeWithIndex in SaintsPropertyDrawers)
            {
                SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.Drawer;

                VisualElement element =
                    drawerInstance.CreatePreOverlayUIKit(property, eachAttributeWithIndex.Attribute, eachAttributeWithIndex.Index, containerElement, parent);
                // ReSharper disable once InvertIf
                if (element != null)
                {
                    fieldContainer.Add(element);
                }
            }

            #endregion

            bool fieldIsFallback = fieldAttributeWithIndex.Attribute == null;
            bool onChangeManuallyWatch;

            if (fieldIsFallback)
            {
                string fallbackClass = ClassFieldUIToolkit(property);
                if (UseCreateFieldUIToolKit)
                {
                    VisualElement fieldElement = CreateFieldUIToolKit(property,
                        null, allAttributes, containerElement, fieldInfo, parent);
                    fieldElement.style.flexGrow = 1;
                    fieldElement.AddToClassList(fallbackClass);
                    fieldContainer.Add(fieldElement);
                    // fieldContainer.userData = null;
                    onChangeManuallyWatch = false;
                }
                else
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                    Debug.Log("fallback field drawer");
#endif
                    VisualElement fallback = UnityFallbackUIToolkit(fieldInfo, property, allAttributes, containerElement, GetPreferredLabel(property), SaintsPropertyDrawers, parent);
                    fallback.AddToClassList(fallbackClass);
                    fallback.AddToClassList(ClassAllowDisable);
                    fieldContainer.Add(fallback);
                    // containerElement.visible = false;
                    onChangeManuallyWatch = true;
                }
            }
            else
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"saints field drawer {fieldAttributeWithIndex.Drawer}");
#endif
                VisualElement fieldElement = fieldAttributeWithIndex.Drawer.CreateFieldUIToolKit(property,
                    fieldAttributeWithIndex.Attribute, allAttributes, containerElement, fieldInfo, parent);
                // fieldElement.style.flexShrink = 1;
                fieldElement.style.flexGrow = 1;
                fieldElement.AddToClassList(ClassFieldUIToolkit(property));
                // fieldElement.RegisterValueChangeCallback(_ => SetValueChanged(property, true));

                fieldContainer.Add(fieldElement);
                fieldContainer.userData = fieldAttributeWithIndex;

                onChangeManuallyWatch = false;
            }

            containerElement.Add(fieldContainer);

            #endregion

            #region post field

            foreach (SaintsPropertyInfo eachAttributeWithIndex in SaintsPropertyDrawers)
            {
                VisualElement postFieldElement = eachAttributeWithIndex.Drawer.CreatePostFieldUIToolkit(property, eachAttributeWithIndex.Attribute, eachAttributeWithIndex.Index, containerElement, fieldInfo, parent);
                if (postFieldElement != null)
                {
                    postFieldElement.style.flexShrink = 0;
                    fieldContainer.Add(postFieldElement);
                }
            }

            #endregion

            #region Post Overlay

            foreach (SaintsPropertyInfo eachAttributeWithIndex in SaintsPropertyDrawers)
            {
                SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.Drawer;

                VisualElement element =
                    drawerInstance.CreatePostOverlayUIKit(property, eachAttributeWithIndex.Attribute, eachAttributeWithIndex.Index, containerElement, parent);
                // ReSharper disable once InvertIf
                if (element != null)
                {
                    fieldContainer.Add(element);
                }
            }

            #endregion

            containerElement.Add(overlayLabelContainer);

            #region below

            Dictionary<string, List<SaintsPropertyInfo>> groupedDrawers =
                new Dictionary<string, List<SaintsPropertyInfo>>();
            foreach (SaintsPropertyInfo eachAttributeWithIndex in SaintsPropertyDrawers)
            {
                string groupBy = eachAttributeWithIndex.Attribute?.GroupBy ?? "";
                if(!groupedDrawers.TryGetValue(groupBy, out List<SaintsPropertyInfo> currentGroup))
                {
                    currentGroup = new List<SaintsPropertyInfo>();
                    groupedDrawers[groupBy] = currentGroup;
                }
                currentGroup.Add(eachAttributeWithIndex);
            }

            Dictionary<string, VisualElement> belowGroupByVisualElement = new Dictionary<string, VisualElement>();

            foreach ((KeyValuePair<string, List<SaintsPropertyInfo>> groupedDrawerInfo, int index) in groupedDrawers.WithIndex())
            {
                string groupBy = groupedDrawerInfo.Key;
                List<SaintsPropertyInfo> drawerInfo = groupedDrawerInfo.Value;

                VisualElement groupByContainer;
                if (groupBy == "")
                {
                    groupByContainer = new VisualElement
                    {
                        style =
                        {
                            width = Length.Percent(100),
                        },
                        name = $"{property.propertyPath}__SaintsFieldBelow_{index}",
                    };
                    containerElement.Add(groupByContainer);
                }
                else
                {
                    if(!belowGroupByVisualElement.TryGetValue(groupBy, out groupByContainer))
                    {
                        belowGroupByVisualElement[groupBy] = groupByContainer = new VisualElement
                        {
                            style =
                            {
                                width = Length.Percent(100),
                            },
                            name = $"{property.propertyPath}__SaintsFieldBelow_{index}_{groupBy}",
                        };
                        groupByContainer.style.flexDirection = FlexDirection.Row;
                        containerElement.Add(groupByContainer);
                    }
                }

                foreach (SaintsPropertyInfo saintsPropertyInfo in drawerInfo)
                {
                    // belowRect = drawerInstance.DrawBelow(belowRect, property, bugFixCopyLabel, eachAttribute);
                    VisualElement creatBelow = saintsPropertyInfo.Drawer.CreateBelowUIToolkit(property,
                        saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, allAttributes, containerElement, fieldInfo, parent);
                    if(creatBelow != null)
                    {
                        groupByContainer.Add(creatBelow);
                    }
                }

            }
            #endregion

            VisualElement rootElement = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                },
                name = NameSaintsPropertyDrawerRoot(property),
                // userData = this,
            };
            // rootElement.AddToClassList(NameSaintsPropertyDrawerRoot(property));
            rootElement.Add(containerElement);

            rootElement.schedule.Execute(() =>
                OnAwakeUiToolKitInternal(property, containerElement, parent, SaintsPropertyDrawers, allAttributes, onChangeManuallyWatch));

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Done property gui {property.propertyPath}/{this}");
#endif

            return rootElement;
        }
#endif

        // private static readonly List<Func<SerializedProperty, FieldInfo,IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)>> _saintsPropertyInfoInjects = new List<Func<SerializedProperty, FieldInfo, IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)>>();

        // private static SaintsPropertyInfo CheckSaintsPropertyInfoInject(SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, int length)
        // {
        //     foreach (Func<SerializedProperty, FieldInfo,IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)> func in _saintsPropertyInfoInjects)
        //     {
        //         (ISaintsAttribute fakeAttribute, Type drawerType) = func(property, info, allAttributes);
        //         if (drawerType != null)
        //         {
        //             return new SaintsPropertyInfo
        //             {
        //                 Drawer = (SaintsPropertyDrawer)MakePropertyDrawer(drawerType, info, (PropertyAttribute)fakeAttribute),
        //                 Attribute = fakeAttribute,
        //                 Index = length,
        //             };
        //         }
        //     }
        //
        //     return default;
        // }

        // protected static void AddSaintsPropertyInfoInject(Func<SerializedProperty, FieldInfo, IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)> func)
        // {
        //     _saintsPropertyInfoInjects.Add(func);
        // }

#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        protected VisualElement UnityFallbackUIToolkit(FieldInfo info, SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement containerElement, string passedPreferredLabel, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, object parent)
        {
            (Attribute attrOrNull, Type drawerType) = GetFallbackDrawerType(info, property, allAttributes);

            // Debug.Log($"{GetType().Name}: attrOrNull={attrOrNull}; drawerType={drawerType}; allAttribute={string.Join(", ", allAttributes)}");

            if (drawerType == null)
            {
                // return PropertyFieldFallbackUIToolkit(property);

                Type rawType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                    ? ReflectUtils.GetElementType(info.FieldType)
                    : info.FieldType;
                return UIToolkitUtils.CreateOrUpdateFieldRawFallback(property, allAttributes, rawType, passedPreferredLabel,
                    info, InHorizontalLayout, this, this, null, parent);
            }

            // return PropertyFieldFallbackUIToolkit(property);

            PropertyDrawer typeDrawer = MakePropertyDrawer(drawerType, info, attrOrNull, passedPreferredLabel);
            if (typeDrawer is SaintsPropertyDrawer spd)
            {
                // Debug.Log($"{GetType().Name}: fall to SaintsPropertyDrawer={spd}; allAttribute={string.Join(", ", allAttributes)}");
                spd.InHorizontalLayout = InHorizontalLayout;
                spd.SaintsSubRenderer = true;
            }

            VisualElement element = DrawUsingDrawerInstance(passedPreferredLabel, drawerType, typeDrawer, property, info,
                saintsPropertyDrawers, containerElement);
            // ReSharper disable once InvertIf
            if (element != null)
            {
                UIToolkitUtils.PropertyDrawerElementDirtyFix(property, typeDrawer, element);
            }

            return element;
        }
#endif

        private static VisualElement DrawUsingDrawerInstance(string passedLabel, Type drawerType, PropertyDrawer drawerInstance, SerializedProperty property, FieldInfo info, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, VisualElement containerElement)
        {
            Debug.Assert(drawerType != null);
            if (drawerInstance == null)
            {
                return null;
            }

            MethodInfo uiToolkitMethod = drawerType.GetMethod("CreatePropertyGUI");

            if(uiToolkitMethod == null || uiToolkitMethod.DeclaringType == typeof(PropertyDrawer))  // null: old Unity || did not override
            {
#if UNITY_6000_0_OR_NEWER
                return PropertyFieldFallbackUIToolkit(property, passedLabel);
#else

                Action<object> onValueChangedCallback = null;
                onValueChangedCallback = value =>
                {
                    object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    if (newFetchParent == null)
                    {
                        Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                        return;
                    }

                    foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
                    {
                        saintsPropertyInfo.Drawer.OnValueChanged(
                            property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement,
                            info, newFetchParent,
                            onValueChangedCallback,
                            value);
                    }

                    SaintsEditorApplicationChanged.OnAnyEvent.Invoke();
                };

                // This breaks: AYellowPaper SerializedDictionary
//
//                 IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);
//
//                 return new IMGUIContainer(() =>
//                 {
//                     using(new ImGuiFoldoutStyleRichTextScoop())
//                     using(new ImGuiLabelStyleRichTextScoop())
//                     using(new InsideSaintsFieldScoop(SubDrawCounter, InsideSaintsFieldScoop.MakeKey(property)))
//                     using(new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
//                     {
//                         GUIContent label = imguiLabelHelper.NoLabel
//                             ? GUIContent.none
//                             : new GUIContent(imguiLabelHelper.RichLabel);
//                         // Debug.Log(imguiLabelHelper);
//
//                         using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
//                         {
//                             property.serializedObject.Update();
//                             EditorGUILayout.PropertyField(property, label);
//                             // ReSharper disable once InvertIf
//                             if (changed.changed)
//                             {
//                                 // Debug.Log("changed");
//                                 property.serializedObject.ApplyModifiedProperties();
//
//                                 object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
//                                 if (newFetchParent == null)
//                                 {
//                                     Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
//                                     return;
//                                 }
//
//                                 (string error, int _, object value) = Util.GetValue(property, info, newFetchParent);
//                                 if (error == "")
//                                 {
//                                     onValueChangedCallback(value);
//                                 }
//                             }
//                         }
//                     }
//                 })
//                 {
//                     style =
//                     {
//                         flexGrow = 1,
//                     },
//                     userData = imguiLabelHelper,
//                 };
// #endif
                // PropertyDrawer imGuiDrawer = drawerInstance;
                //
                // using(new InsideSaintsFieldScoop(SubDrawCounter, InsideSaintsFieldScoop.MakeKey(property)))
                // using(new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
                // {
                //     // Debug.Log($"Fall {property.propertyPath}");
                //     // EditorGUILayout.PropertyField(property, label, true);
                //     // Debug.Log($"Fall Done {property.propertyPath}");
                //     // GUIContent label = new GUIContent(property.displayName);
                //     PropertyField prop = new PropertyField(property)
                //     {
                //         style =
                //         {
                //             flexGrow = 1,
                //         },
                //     };
                //     prop.styleSheets.Add(GetNoDecoratorUss());
                //     UIToolkitUtils.SetPropertyFieldDrawNestingLevel1(prop);
                //     // FieldInfo fieldInfo = typeof(PropertyField).GetField("m_DrawNestingLevel", BindingFlags.NonPublic | BindingFlags.Instance);
                //     // if (fieldInfo != null)
                //     // {
                //     //     fieldInfo.SetValue(prop, 1);
                //     // }
                //     return prop;
                // }


                // This works fine with: AYellowPaper.SerializedDictionary, Wwise.Event, I2Language.LocalizedString
                IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(passedLabel);

                IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
                {
                    property.serializedObject.Update();

                    GUIContent label = imguiLabelHelper.NoLabel
                        ? GUIContent.none
                        : new GUIContent(imguiLabelHelper.RichLabel);

                    using(new ImGuiFoldoutStyleRichTextScoop())
                    using(new ImGuiLabelStyleRichTextScoop())
                    using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                    {
                        // // This weird way is from Unity's PropertyDrawer
                        // // The other ways which commented below does not work:
                        // // they either not work for I2Language, or Wwise.Event
                        // Rect position;
                        // using(new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
                        // {
                        //     Debug.Log($"== Get Height from {imGuiDrawer}");
                        //     position = new Rect
                        //     {
                        //         height = imGuiDrawer.GetPropertyHeight(property, label),
                        //         width = imGuiContainer.resolvedStyle.width,
                        //     };
                        // }
                        // Debug.Log($"== Done Height from {imGuiDrawer}: {position.height}");
                        // using(new InsideSaintsFieldScoop(SubDrawCounter, InsideSaintsFieldScoop.MakeKey(property)))
                        // {
                        //     imGuiDrawer.OnGUI(position, property, label);
                        // }
                        // // ReSharper disable once PossibleNullReferenceException
                        // // ReSharper disable once AccessToModifiedClosure
                        // imGuiContainer.style.height = position.height;

                        using(new InsideSaintsFieldScoop(SubDrawCounter, InsideSaintsFieldScoop.MakeKey(property)))
                        using(new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
                        {
                            // Debug.Log($"Fall {property.propertyPath}");
                            // This works with Wwise.Bank/Event in list; not work with AYellowPaper.SerializedDictionary
                            // EditorGUILayout.PropertyField(property, label, true);
                            // Debug.Log($"Fall Done {property.propertyPath}");

                            // This not work with Wwise.Bank/Event in list; But for other situation it works just fine
                            float height = drawerInstance.GetPropertyHeight(property, label);
                            Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
                            drawerInstance.OnGUI(rect, property, label);
                        }

                        // using(new InsideSaintsFieldScoop(SubDrawCounter, InsideSaintsFieldScoop.MakeKey(property)))
                        // using(new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
                        // {
                        //     float height = drawerInstance.GetPropertyHeight(property, label);
                        //     // Debug.Log($"container height={height}");
                        //     Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
                        //     drawerInstance.OnGUI(rect, property, label);
                        // }
                        //
                        // // Debug.Log(changed.changed);
                        //
                        // ReSharper disable once InvertIf
                        if (changed.changed)
                        {
                            property.serializedObject.ApplyModifiedProperties();

                            object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                            if (newFetchParent == null)
                            {
                                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                                return;
                            }

                            (string error, int _, object value) = Util.GetValue(property, info, newFetchParent);
                            if (error == "")
                            {
                                onValueChangedCallback(value);
                            }
                        }
                    }
                })
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 0,
                    },
                    userData = imguiLabelHelper,
                };
                imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);

                return imGuiContainer;
#endif
            }

            VisualElement attrCreateReturnElement = drawerInstance.CreatePropertyGUI(property);
            if (attrCreateReturnElement == null)
            {
                return null;
            }
            attrCreateReturnElement.style.flexGrow = 1;
            return attrCreateReturnElement;
        }

        private void OnAwakeUiToolKitInternal(SerializedProperty property, VisualElement containerElement,
            object parent, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers,
            IReadOnlyList<PropertyAttribute> allAttributes, bool manuallyWatch)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"On Awake {property.propertyPath}: {string.Join(",", saintsPropertyDrawers.Select(each => each.Attribute?.GetType().Name))}");
#endif
            try
            {
                string _ = property.propertyPath;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }

            // ReSharper disable once ConvertToLocalFunction
            Action<object> onValueChangedCallback = null;
            onValueChangedCallback = obj =>
            {
                object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (newFetchParent == null)
                {
                    Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    return;
                }

                foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
                {
                    saintsPropertyInfo.Drawer.OnValueChanged(
                        property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement,
                        fieldInfo, newFetchParent,
                        onValueChangedCallback,
                        obj);
                }
                SaintsEditorApplicationChanged.OnAnyEvent.Invoke();
            };

            PropertyField fallbackField = containerElement.Q<PropertyField>(name: UIToolkitFallbackName(property));
            // Debug.Log($"check has fallback {property.propertyPath}: {fallbackField}");

            if (manuallyWatch)
            {
                                // ReSharper disable once ConvertToConstant.Local
                bool isReference = property.propertyType == SerializedPropertyType.ManagedReference || property.propertyType == SerializedPropertyType.Generic;

// #if UNITY_2021_3_OR_NEWER
//                         property.propertyType == SerializedPropertyType.ManagedReference
// #else
//                         // HashSet<string> trackedSubPropertyNames = new HashSet<string>();
//                         false
// #endif
                    // ;

                // Issue 97
                // bool watch = !property.isArray ||
                //              (property.isArray && !SaintsFieldConfigUtil.DisableOnValueChangedWatchArrayFieldUIToolkit());
                bool watch = isReference;
                if (watch && SerializedUtils.IsArrayOrDirectlyInsideArray(property))
                {
                    watch = !SaintsFieldConfigUtil.DisableOnValueChangedWatchArrayFieldUIToolkit();
                }

                // Debug.Log($"watch={watch}/fallbackField={fallbackField}/{isReference}");

                if(watch)
                {
                    // see:
                    // https://issuetracker.unity3d.com/issues/visualelements-that-use-trackpropertyvalue-keep-tracking-properties-when-they-are-removed
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                    Debug.Log($"watch {property.propertyPath}");
#endif
                    // foreach (VisualElement oldTracker in containerElement.Query<VisualElement>(name: UIToolkitOnChangedTrackerName(property)).ToList())
                    // {
                    //     oldTracker.RemoveFromHierarchy();
                    // }
                    // VisualElement trackerContainer = new VisualElement
                    // {
                    //     // name = UIToolkitOnChangedTrackerName(property),
                    // };
                    // fallbackField.Add(trackerContainer);

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    VisualElement trackerMain = BindWatchUIToolkit(property, onValueChangedCallback, isReference,
                        containerElement, fieldInfo, parent);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (isReference || property.propertyType == SerializedPropertyType.Generic)
                    {
                        TrackPropertyManagedUIToolkit(onValueChangedCallback, property,
                            property, fieldInfo, trackerMain, parent);
                    }
                }
                  // this does not work on some unity version, e.g. 2022.3.14f1, for serialized class
                else if (fallbackField != null)
                {
                    fallbackField.RegisterValueChangeCallback(evt =>
                    {
                        SerializedProperty prop = evt.changedProperty;
                        if(SerializedProperty.EqualContents(prop, property))
                        {
                            object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                            if (noCacheParent == null)
                            {
                                Debug.LogWarning($"Property disposed unexpectedly, skip onChange callback.");
                                return;
                            }
                            (string error, int _, object curValue) = Util.GetValue(property, fieldInfo, noCacheParent);
                            if (error == "")
                            {
                                onValueChangedCallback(curValue);
                            }
                        }
                    });
                }
                else
                {
                    containerElement.TrackPropertyValue(property, _ =>
                    {
                        if (!SerializedUtils.IsOk(property))
                        {
                            return;
                        }

                        object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        if (noCacheParent == null)
                        {
                            Debug.LogWarning($"Property disposed unexpectedly, skip onChange callback.");
                            return;
                        }
                        (string error, int _, object curValue) = Util.GetValue(property, fieldInfo, noCacheParent);
                        if (error == "")
                        {
                            onValueChangedCallback(curValue);
                        }
                    });
                }
            }

            if(fallbackField != null)
            {
                // containerElement.visible = true;
                string c = NameSaintsPropertyDrawerContainer(property);
                VisualElement deepestContainer = containerElement.Query<VisualElement>(name: c).Last();

                List<VisualElement> parentRoots = UIToolkitUtils.FindParentName(deepestContainer, NameSaintsPropertyDrawerRoot(property)).ToList();
                // Debug.Log($"usingFallbackField {property.propertyPath}, parentRoots={parentRoots.Count}, {saintsPropertyDrawers.Count} ({NameSaintsPropertyDrawerRoot(property)})");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"usingFallbackField {property.propertyPath}, parentRoots={parentRoots.Count}, {saintsPropertyDrawers.Count} ({NameSaintsPropertyDrawerRoot(property)})");
#endif

                // int saintsPropCount = 0;
                // foreach (PropertyAttribute propertyAttribute in allAttributes)
                // {
                //     bool isSaintsProperty = propertyAttribute is ISaintsAttribute;
                //     // Debug.Log($"{propertyAttribute}: {propertyAttribute is ISaintsAttribute}");
                //     if (PropertyIsDecoratorDrawer(propertyAttribute))
                //     {
                //         continue;
                //     }
                //     if (isSaintsProperty)
                //     {
                //         saintsPropCount++;
                //     }
                //     else
                //     {
                //         break;
                //     }
                // }

                // Debug.Log($"parentRoots.Count={parentRoots.Count}, saintsPropCount={saintsPropCount}");

                // int saintsPropCount = allAttributes.TakeWhile(each =>
                //     each is ISaintsAttribute
                //     // || PropertyIsDecoratorDrawer(each)
                //     ).Count();
                // Debug.Log(saintsPropCount);
                // Debug.Log(saintsPropertyDrawers.Count);
                // Debug.Log(string.Join(",", allAttributes));
                // Debug.Log(string.Join(",", allAttributes.TakeWhile(each =>
                //     each is ISaintsAttribute
                //     // || PropertyIsDecoratorDrawer(each)
                // )));
                // Debug.Log(string.Join(",", saintsPropertyDrawers.Select(each => each.Attribute)));

                // Debug.Log(parentRoots.Count);

                // fallback will draw all (saintsPropCount), then this drawer itself will draw one.
                // int nestedCount = saintsPropCount + 1;

                // if (saintsPropCount != 0 && parentRoots.Count != nestedCount)
                // // if (parentRoots.Count < saintsPropCount)
                //     // if (parentRoots.Count != saintsPropertyDrawers.Count)
                // {
                //     return;
                // }
                // Debug.Log(PropertyAttributeToPropertyDrawers[]);

                // Debug.Log(fieldInfo.FieldType);
                // Debug.Log(string.Join(",", PropertyAttributeToPropertyDrawers.Keys));

                // ReSharper disable once UseIndexFromEndExpression
                if(parentRoots.Count > 0)
                {
                    VisualElement topRoot = parentRoots[parentRoots.Count - 1];

                    // PropertyField thisPropField = containerElement.Q<PropertyField>(className: SaintsFieldFallbackClass);

                    // var container = thisPropField.Query<VisualElement>(className: "unity-decorator-drawers-container").ToList();
                    // Debug.Log($"container={container.Count}");

                    // thisPropField.styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/UnityLabelTransparent.uss"));

//                 // really... this delay is not predictable
//                 containerElement.schedule.Execute(() =>
//                 {
//                     // var container = thisPropField.Query<VisualElement>(className: "unity-decorator-drawers-container").ToList();
//                     // Debug.Log($"container={container.Count}");
//                     // fallbackField.Query<VisualElement>(className: "unity-decorator-drawers-container").ForEach(each => each.RemoveFromHierarchy());
// // #if !SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
// //                     Label label = fallbackField.Q<Label>(className: "unity-label");
// //                     if (label != null)
// //                     {
// //                         UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
// //                     }
// // #endif
//
//
//                 });

                    topRoot.Clear();
                    topRoot.Add(deepestContainer);
                }

                // thisPropField.Bind(property.serializedObject);
                // fallbackField.Unbind();
                fallbackField.BindProperty(property);

                // Debug.Log($"Ready for {property.propertyPath} in fallback style");
                deepestContainer.schedule.Execute(() => {
                    if(deepestContainer.parent != null)
                    {
                        OnAwakeReady(property, deepestContainer, parent, onValueChangedCallback, saintsPropertyDrawers,
                            allAttributes);
                    }
                });
            }
            else
            {
                OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers, allAttributes);
            }
        }

        private static StyleSheet _noDecoratorDrawer;

        private static VisualElement BindWatchUIToolkit(SerializedProperty property, Action<object> onValueChangedCallback, bool isReference, VisualElement propertyField, FieldInfo fieldInfo, object parent)
        {
            string trackMainName = UIToolkitOnChangedTrackerName(property);
            // Debug.Log(trackMainName);
            VisualElement trackerMain = propertyField.Q<VisualElement>(name: trackMainName);
            // ReSharper disable once UseNullPropagation
            if (trackerMain != null)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                Debug.Log($"remove old tracker main: {trackerMain}");
#endif
                trackerMain.RemoveFromHierarchy();
                UIToolkitUtils.Unbind(trackerMain);
            }
            // if (trackerMain == null)
            {
                trackerMain = new VisualElement
                {
                    name = UIToolkitOnChangedTrackerName(property),
                };
                propertyField.Add(trackerMain);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                Debug.Log($"add main tracker {property.propertyPath}/{trackerMain}");
#endif
                trackerMain.TrackPropertyValue(property, prop =>
                {
                    object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(prop).parent;
                    if (noCacheParent == null)
                    {
                        Debug.LogWarning("Property disposed unexpectedly, skip onChange callback.");
                        return;
                    }

                    (string error, int _, object curValue) = Util.GetValue(property, fieldInfo, noCacheParent);
                    if (error == "")
                    {
                        onValueChangedCallback(curValue);
                    }

                    if (isReference)
                    {
                        // reference changing will destroy the old one, and create a new one (weird... what's wrong with you Unity...)
                        // so we need to rebind the watch
                        BindWatchUIToolkit(property, onValueChangedCallback, true, propertyField, fieldInfo,
                            parent);
                        // TrackPropertyManagedUIToolkit(onValueChangedCallback, property,
                        //     property, fieldInfo, trackerMain,
                        //     parent);
                    }
                });
            }
            // else
            // {
            //     Debug.Log($"exists main tracker {trackerMain}");
            // }

            if (isReference)
            {
                TrackPropertyManagedUIToolkit(onValueChangedCallback, property,
                    property, fieldInfo, trackerMain,
                    parent);
            }

            return trackerMain;
//             bool hasTracker = trackerContainer != null;
//             if (!hasTracker)
//             {
//                 trackerContainer = new VisualElement
//                 {
//                     name = UIToolkitOnChangedTrackerName(property),
//                 };
//                 fallbackField.Add(trackerContainer);
//             }
//
//             Debug.Log($"hasTracker={hasTracker}");
//             // VisualElement
//
//
//             trackerContainer.TrackPropertyValue(property, prop =>
//             {
//                 object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(prop).parent;
//                 if (noCacheParent == null)
//                 {
//                     Debug.LogWarning("Property disposed unexpectedly, skip onChange callback.");
//                     return;
//                 }
//
//                 (string error, int _, object curValue) = Util.GetValue(property, fieldInfo, noCacheParent);
//                 if (error == "")
//                 {
//                     onValueChangedCallback(curValue);
//                 }
//
// // #if UNITY_2021_3_OR_NEWER
//                 if (isReference && !hasTracker)
//                 {
//                     // reference changing will destroy the old one, and create a new one (weird... what's wrong with you Unity...)
//                     // so we need to rebind the watch
//                     VisualElement newTrackerContainer = BindWatchUIToolkit(property, onValueChangedCallback, true, containerElement, fieldInfo,
//                         parent);
//                     TrackPropertyManagedUIToolkit(onValueChangedCallback, prop,
//                         prop, fieldInfo, newTrackerContainer,
//                         noCacheParent);
//                 }
// // #endif
//             });
//
//             return trackerContainer;

        }

        private static void TrackPropertyManagedUIToolkit(Action<object> onValueChangedCallback, SerializedProperty watchSubProperty, SerializedProperty getValueProperty, MemberInfo memberInfo, VisualElement tracker, object newFetchParent)
        {
#if UNITY_2021_3_OR_NEWER
            foreach ((string _, SerializedProperty subProperty) in SaintsRowAttributeDrawer.GetSerializableFieldInfo(watchSubProperty))
            {
                int propertyIndex = SerializedUtils.PropertyPathIndex(getValueProperty.propertyPath);
                VisualElement subTracker = tracker.Q<VisualElement>(name: UIToolkitOnChangedTrackerName(subProperty));
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                Debug.Log($"Try add sub track: {subProperty.propertyPath}; real value prop = {getValueProperty.propertyPath}, index={propertyIndex}/{subTracker}");
#endif
                if (subTracker != null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                    Debug.Log($"Remove old sub track: {subProperty.propertyPath} {subTracker}");
#endif
                    // continue;
                    subTracker.RemoveFromHierarchy();
                    UIToolkitUtils.Unbind(subTracker);
                }

                subTracker = new VisualElement
                {
                    name = UIToolkitOnChangedTrackerName(subProperty),
                };
                tracker.Add(subTracker);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                Debug.Log($"Add new sub track: {subProperty.propertyPath} {subTracker}");
#endif
                subTracker.TrackPropertyValue(subProperty,
                    _ =>
                    {
                        // object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(p).parent;
                        // this won't work as `getValueProperty` will be disposed, giving propertyPath = ""
                        // (string subError, int _, object subValue) = Util.GetValue(getValueProperty, memberInfo, newFetchParent);
                        (string subError, int _, object subValue) = Util.GetValueAtIndex(propertyIndex, memberInfo, newFetchParent);
                        // Debug.Log($"propertyIndex={propertyIndex}, newValue={subValue}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                        if (subError != "")
                        {
                            Debug.LogError(subError);
                        }
#endif

                        if (subError == "")
                        {
                            // ReSharper disable once RedundantAssignment
                            onValueChangedCallback(subValue);
                        }
                    });
            }
#endif
        }

        private void OnAwakeReady(SerializedProperty property, VisualElement containerElement,
            object parent,  Action<object> onValueChangedCallback, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, IReadOnlyList<PropertyAttribute> allAttributes)
        {

            // Debug.Log("OnAwakeReady");
            // Action<object> onValueChangedCallback = obj =>
            // {
            //     foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            //     {
            //         saintsPropertyInfo.Drawer.OnValueChanged(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, fieldInfo, parent, obj);
            //     }
            // };

            containerElement.visible = true;

            containerElement.userData = this;

// #if !SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
//             Label label = containerElement.Q<PropertyField>(name: UIToolkitFallbackName(property))?.Q<Label>(className: "unity-label");
//             if (label != null)
//             {
//                 // UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
//                 label.schedule.Execute(() => UIToolkitUtils.FixLabelWidthUIToolkit(label));
//             }
// #endif

            // try
            // {
            //     string _ = property.propertyPath;
            // }
            // catch (ObjectDisposedException)
            // {
            //     return;
            // }

            foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            {
                saintsPropertyInfo.Drawer.OnAwakeUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, allAttributes, containerElement, onValueChangedCallback, fieldInfo, parent);
            }

            // foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            // {
            //     saintsPropertyInfo.Drawer.OnStartUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, onValueChangedCallback, fieldInfo, parent);
            // }

            // containerElement.schedule.Execute(() => OnUpdateUiToolKitInternal(property, containerElement, parent, saintsPropertyDrawers));
            OnUpdateUiToolKitInternal(property, allAttributes, containerElement, saintsPropertyDrawers, onValueChangedCallback, fieldInfo);
        }

        private static void OnUpdateUiToolKitInternal(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers,
            Action<object> onValueChangedCallback,
            FieldInfo info
        )
        {
            if (!SerializedUtils.IsOk(property))
            {
                return;
            }
            // try
            // {
            //     string _ = property.propertyPath;
            // }
            // catch (ObjectDisposedException)
            // {
            //     return;
            // }
            // catch (NullReferenceException)
            // {
            //     return;
            // }

            if (container.parent == null)
            {
                return;
            }

            foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            {
                saintsPropertyInfo.Drawer.OnUpdateUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, allAttributes, container, onValueChangedCallback, info);
            }

            // Debug.Log($"container={container}/parent={container.parent}");

            container.parent.schedule.Execute(() => OnUpdateUiToolKitInternal(property, allAttributes, container, saintsPropertyDrawers, onValueChangedCallback, info)).StartingIn(SaintsFieldConfig.UpdateLoopDefaultMs);
        }

        private static StyleSheet GetNoDecoratorUss()
        {
            if (!_noDecoratorDrawer)
            {
                _noDecoratorDrawer = Util.LoadResource<StyleSheet>(UIToolkitUtils.NoDecoratorDrawerUssFile);
            }

            return _noDecoratorDrawer;
        }

        protected static PropertyField PropertyFieldFallbackUIToolkit(SerializedProperty property, string label)
        {
            // PropertyField propertyField = new PropertyField(property, new string(' ', property.displayName.Length))
            PropertyField propertyField = new PropertyField(property, label)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = UIToolkitFallbackName(property),
            };

            // propertyField.AddToClassList(SaintsFieldFallbackClass);
            propertyField.AddToClassList(ClassAllowDisable);
            propertyField.styleSheets.Add(GetNoDecoratorUss());
            propertyField.BindProperty(property);
            propertyField.Bind(property.serializedObject);
            // propertyField.AddToClassList("unity-base-field__aligned");
            // propertyField.RegisterValueChangeCallback(Debug.Log);
            return propertyField;
        }

        protected static void OnLabelStateChangedUIToolkit(SerializedProperty property, VisualElement container,
            string toLabel, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried,
            RichTextDrawer richTextDrawer)
        {
            VisualElement saintsLabelField = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));

            IMGUIContainer imGuiContainer = saintsLabelField.Q<IMGUIContainer>();

            // ReSharper disable once MergeIntoPattern
            // ReSharper disable once MergeSequentialChecks
            if(imGuiContainer?.userData is IMGUILabelHelper imguiLabelHelper)
            {
                // Debug.Log($"imguiLabelHelper={imguiLabelHelper}");\
                imguiLabelHelper.NoLabel = string.IsNullOrEmpty(toLabel);
                imguiLabelHelper.RichLabel = toLabel;
                return;
            }

            Label saintsPropertyDrawerOverrideLabel = container.Q<Label>(classes: NameSaintsPropertyDrawerOverrideLabel);
            if (saintsPropertyDrawerOverrideLabel != null)
            {
                UIToolkitUtils.SetLabel(saintsPropertyDrawerOverrideLabel, richTextChunks, richTextDrawer);
                return;
            }



            object saintsLabelFieldDrawerData = saintsLabelField.userData;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RICH_LABEL
            Debug.Log($"OnLabelStateChangedUIToolkit: {saintsLabelFieldDrawerData}");
#endif

            if (saintsLabelFieldDrawerData != null)
            {
                // Debug.Log(saintsLabelFieldDrawerData);
                SaintsPropertyInfo drawerInfo = (SaintsPropertyInfo) saintsLabelFieldDrawerData;
                // string newLabel = toLabel == null ? null : new string(' ', property.displayName.Length);
                // string newLabel = toLabel == null ? null : property.displayName;

                drawerInfo.Drawer.ChangeFieldLabelToUIToolkit(property, drawerInfo.Attribute, drawerInfo.Index,
                    container, toLabel, richTextChunks, tried, richTextDrawer);
                // Debug.Log($"{drawerInfo.Drawer}/{toLabel}");
            }
            else
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RICH_LABEL
                Debug.Log($"3rd party drawer, label need fallback");
#endif
                // TODO: allow disabling this function, as it might break some custom drawer
                VisualElement actualFieldCanHasLabel = saintsLabelField.Q<VisualElement>(className: ClassFieldUIToolkit(property));

                if (actualFieldCanHasLabel != null)
                {
                    UIToolkitUtils.ChangeLabelLoop(actualFieldCanHasLabel, richTextChunks, richTextDrawer);
                }
            }
            // Debug.Log(mainDrawer._saintsFieldFallback);
            // Debug.Log(mainDrawer._saintsFieldDrawer);
            // ChangeFieldLabelTo(toLabel);
        }

        protected virtual void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            // Debug.Log($"tried: {tried}, label={labelOrNull}, chunk.length={richTextChunks.Count}");
            // foreach (RichTextDrawer.RichTextChunk richTextChunk in richTextChunks)
            // {
            //     Debug.Log(richTextChunk);
            // }
            if (tried)
            {
                return;
            }

            VisualElement saintsField = container.Q<VisualElement>(className: ClassFieldUIToolkit(property));
            if (saintsField != null)
            {
                UIToolkitUtils.ChangeLabelLoop(saintsField,
                    richTextChunks,
                    richTextDrawer);
            }
        }

        #region Callbacks

        protected virtual VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            return null;
        }

        protected virtual VisualElement CreatePreOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            return null;
        }

        protected virtual VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            return null;
        }

        protected virtual VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            throw new NotImplementedException();
        }

        protected virtual VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return null;
        }

        protected virtual VisualElement DrawPreLabelUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return null;
        }

        protected virtual VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return null;
        }

        protected virtual void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
        }

        // protected virtual void OnStartUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     int index,
        //     VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        // {
        // }

        protected virtual void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
        }

        protected virtual void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent,
            Action<object> onValueChangedCallback,
            object newValue)
        {
        }

        #endregion
    }
}
#endif
