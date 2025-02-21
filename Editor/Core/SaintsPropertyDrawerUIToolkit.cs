#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Core
{
    public partial class SaintsPropertyDrawer
    {
        protected virtual void OnDisposeUIToolkit()
        {

        }

        protected static string NameLabelFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-label-field";
        public static string ClassLabelFieldUIToolkit = "saints-field--label-field";

        public static string ClassNoRichLabelUpdate = "saints-field-no-rich-label-update";

        protected static string ClassFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-field";

        public const string ClassAllowDisable = "saints-field-allow-disable";
        protected static string UIToolkitFallbackName(SerializedProperty property) => $"saints-field--fallback-{property.propertyPath}";
        private static string UIToolkitOnChangedTrackerName(SerializedProperty property) =>
            $"saints-field-tracker--{property.propertyPath}";

        private static string NameSaintsPropertyDrawerRoot(SerializedProperty property) =>
            $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}__SaintsFieldRoot";

        protected virtual bool UseCreateFieldUIToolKit => false;

#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Create property gui {property.propertyPath}/{property.displayName}/{this}");
#endif

            VisualElement containerElement = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                },
                name = $"{property.propertyPath}__SaintsFieldContainer",
            };

            (PropertyAttribute[] allAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);

            ISaintsAttribute[] iSaintsAttributes = allAttributes.OfType<ISaintsAttribute>().ToArray();
            // Debug.Assert(iSaintsAttributes.Length > 0, property.propertyPath);

            // IReadOnlyList<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
            //     .Select((each, index) => new SaintsWithIndex
            //     {
            //         SaintsAttribute = each,
            //         Index = index,
            //     })
            //     .ToArray();
            List<SaintsPropertyInfo> saintsPropertyDrawers = iSaintsAttributes
                .WithIndex()
                .Select(each => new SaintsPropertyInfo
                {
                    Drawer = GetOrCreateSaintsDrawerByAttr(each.value),
                    Attribute = each.value,
                    Index = each.index,
                })
                .ToList();

            // for type drawer that is in SaintsField, use this to draw with a fake property attribute
            SaintsPropertyInfo fieldAttributeWithIndex = saintsPropertyDrawers.FirstOrDefault(each => each.Attribute.AttributeType == SaintsAttributeType.Field);
            if(fieldAttributeWithIndex.Attribute == null)
            {
                fieldAttributeWithIndex = CheckSaintsPropertyInfoInject(property, allAttributes, fieldInfo, saintsPropertyDrawers.Count);
                if (fieldAttributeWithIndex.Drawer != null)
                {
                    saintsPropertyDrawers.Add(fieldAttributeWithIndex);
                }
                else if(UseCreateFieldUIToolKit)
                {
                    saintsPropertyDrawers.Add(new SaintsPropertyInfo
                    {
                        Drawer = this,
                        Attribute = null,
                        Index = -1,
                    });
                }
            }

            #region Above

            Dictionary<string, List<SaintsPropertyInfo>> groupedAboveDrawers =
                new Dictionary<string, List<SaintsPropertyInfo>>();
            foreach (SaintsPropertyInfo eachAttributeWithIndex in saintsPropertyDrawers)
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

            foreach (SaintsPropertyInfo eachAttributeWithIndex in saintsPropertyDrawers)
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

            if (fieldIsFallback)
            {
                if (UseCreateFieldUIToolKit)
                {
                    VisualElement fieldElement = CreateFieldUIToolKit(property,
                        null, allAttributes, containerElement, fieldInfo, parent);
                    fieldElement.style.flexGrow = 1;
                    fieldElement.AddToClassList(ClassFieldUIToolkit(property));
                    fieldContainer.Add(fieldElement);
                    // fieldContainer.userData = null;
                }
                else
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                    Debug.Log("fallback field drawer");
#endif
                    VisualElement fallback = UnityFallbackUIToolkit(fieldInfo, property, containerElement,
                        allAttributes, saintsPropertyDrawers, parent);
                    fallback.AddToClassList(ClassFieldUIToolkit(property));
                    fieldContainer.Add(fallback);
                    containerElement.visible = false;
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
            }

            containerElement.Add(fieldContainer);

            #endregion

            #region post field

            foreach (SaintsPropertyInfo eachAttributeWithIndex in saintsPropertyDrawers)
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

            foreach (SaintsPropertyInfo eachAttributeWithIndex in saintsPropertyDrawers)
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
            foreach (SaintsPropertyInfo eachAttributeWithIndex in saintsPropertyDrawers)
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
                        saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, fieldInfo, parent);
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
            rootElement.AddToClassList(NameSaintsPropertyDrawerRoot(property));
            rootElement.Add(containerElement);

            rootElement.schedule.Execute(() =>
                OnAwakeUiToolKitInternal(property, containerElement, parent, saintsPropertyDrawers, allAttributes));

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Done property gui {property.propertyPath}/{this}");
#endif

            return rootElement;
        }
#endif

        private static readonly List<Func<SerializedProperty, FieldInfo,IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)>> _saintsPropertyInfoInjects = new List<Func<SerializedProperty, FieldInfo, IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)>>();

        private static SaintsPropertyInfo CheckSaintsPropertyInfoInject(SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, int length)
        {
            foreach (Func<SerializedProperty, FieldInfo,IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)> func in _saintsPropertyInfoInjects)
            {
                (ISaintsAttribute fakeAttribute, Type drawerType) = func(property, info, allAttributes);
                if (drawerType != null)
                {
                    return new SaintsPropertyInfo
                    {
                        Drawer = (SaintsPropertyDrawer)MakePropertyDrawer(drawerType, info, (PropertyAttribute)fakeAttribute),
                        Attribute = fakeAttribute,
                        Index = length,
                    };
                }
            }

            return default;
        }

        protected static void AddSaintsPropertyInfoInject(Func<SerializedProperty, FieldInfo, IReadOnlyList<PropertyAttribute>, (ISaintsAttribute fakeAttribute, Type drawerType)> func)
        {
            _saintsPropertyInfoInjects.Add(func);
        }

        private static VisualElement UnityFallbackUIToolkit(FieldInfo info, SerializedProperty property, VisualElement containerElement, IReadOnlyList<PropertyAttribute> allAttributes, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, object parent)
        {
            // check if any property has drawer. If so, just use PropertyField
            // if not, check if it has custom drawer. if it exists, then try use that custom drawer
            (Attribute _, Type attributeDrawerType) = GetOtherAttributeDrawerType(info);
            if (attributeDrawerType != null)
            {
                return PropertyFieldFallbackUIToolkit(property);
            }

            Type foundDrawer = FindTypeDrawer(info);
            // Debug.LogWarning(foundDrawer);

            if (foundDrawer == null)
            {
                return PropertyFieldFallbackUIToolkit(property);
            }

            PropertyDrawer typeDrawer = MakePropertyDrawer(foundDrawer, info, null);
            VisualElement element = DrawUsingDrawerInstance(foundDrawer, typeDrawer, property, info, allAttributes, saintsPropertyDrawers, containerElement, parent);
            // return element ?? PropertyFieldFallbackUIToolkit(property);
            return element ?? PropertyFieldFallbackUIToolkit(property);
        }

        private static VisualElement DrawUsingDrawerInstance(Type drawerType, PropertyDrawer drawerInstance, SerializedProperty property, FieldInfo info, IReadOnlyList<PropertyAttribute> allAttributes, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, VisualElement containerElement, object parent)
        {
            Debug.Assert(drawerType != null);
            if (drawerInstance == null)
            {
                return null;
            }

            MethodInfo uiToolkitMethod = drawerType.GetMethod("CreatePropertyGUI");

            if(uiToolkitMethod == null || uiToolkitMethod.DeclaringType != drawerType)  // null: old Unity || did not override
            {
                PropertyDrawer imGuiDrawer = drawerInstance;
                MethodInfo imGuiGetPropertyHeightMethod = drawerType.GetMethod("GetPropertyHeight");
                MethodInfo imGuiOnGUIMethodInfo = drawerType.GetMethod("OnGUI");
                Debug.Assert(imGuiGetPropertyHeightMethod != null);
                Debug.Assert(imGuiOnGUIMethodInfo != null);

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
                };

                IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);

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
                        float height =
                            (float)imGuiGetPropertyHeightMethod.Invoke(imGuiDrawer, new object[] { property, label });
                        Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
                        imGuiOnGUIMethodInfo.Invoke(imGuiDrawer, new object[] { rect, property, label });

                        // Debug.Log(changed.changed);

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
            IReadOnlyList<PropertyAttribute> allAttributes)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"On Awake {property.propertyPath}: {string.Join(",", saintsPropertyDrawers.Select(each => each.Attribute.GetType().Name))}");
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
            };

            PropertyField fallbackField = containerElement.Q<PropertyField>(name: UIToolkitFallbackName(property));
            // Debug.Log($"check has fallback {property.propertyPath}: {fallbackField}");

            if(fallbackField != null)
            {
                // containerElement.visible = true;

                List<VisualElement> parentRoots = UIToolkitUtils.FindParentClass(containerElement, NameSaintsPropertyDrawerRoot(property)).ToList();
                // Debug.Log($"usingFallbackField {property.propertyPath}, parentRoots={parentRoots.Count}, {saintsPropertyDrawers.Count} ({NameSaintsPropertyDrawerRoot(property)})");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"usingFallbackField {property.propertyPath}, parentRoots={parentRoots.Count}, {saintsPropertyDrawers.Count} ({NameSaintsPropertyDrawerRoot(property)})");
#endif

                int saintsPropCount = 0;
                foreach (PropertyAttribute propertyAttribute in allAttributes)
                {
                    bool isSaintsProperty = propertyAttribute is ISaintsAttribute;
                    // Debug.Log($"{propertyAttribute}: {propertyAttribute is ISaintsAttribute}");
                    if (PropertyIsDecoratorDrawer(propertyAttribute))
                    {
                        continue;
                    }
                    if (isSaintsProperty)
                    {
                        saintsPropCount++;
                    }
                    else
                    {
                        break;
                    }
                }

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

                if (parentRoots.Count != saintsPropCount)
                    // if (parentRoots.Count != saintsPropertyDrawers.Count)
                {
                    return;
                }
                // Debug.Log(PropertyAttributeToPropertyDrawers[]);

                // Debug.Log(fieldInfo.FieldType);
                // Debug.Log(string.Join(",", PropertyAttributeToPropertyDrawers.Keys));

                // ReSharper disable once UseIndexFromEndExpression
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
                topRoot.Add(containerElement);

                // thisPropField.Bind(property.serializedObject);
                // fallbackField.Unbind();
                fallbackField.BindProperty(property);
                bool isReference = false;
#if UNITY_2021_3_OR_NEWER
                // HashSet<string> trackedSubPropertyNames = new HashSet<string>();
                isReference = property.propertyType == SerializedPropertyType.ManagedReference;
#endif

                bool watch = !property.isArray ||
                             (property.isArray && !SaintsFieldConfigUtil.DisableOnValueChangedWatchArrayFieldUIToolkit());
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

                    VisualElement trackerMain = BindWatchUIToolkit(property, onValueChangedCallback, isReference,
                        fallbackField, fieldInfo, parent);
                    if (isReference || property.propertyType == SerializedPropertyType.Generic)
                    {
                        TrackPropertyManagedUIToolkit(onValueChangedCallback, property,
                            property, fieldInfo, trackerMain, parent);
                    }
                }
                else  // this does not work on some unity version, e.g. 2022.3.14f1, for serialized class
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

                OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers, allAttributes);
            }
            else
            {
                OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers, allAttributes);
            }
        }

        private static StyleSheet _noDecoratorDrawer;

        private static VisualElement BindWatchUIToolkit(SerializedProperty property, Action<object> onValueChangedCallback, bool isReference, PropertyField propertyField, FieldInfo fieldInfo, object parent)
        {
            VisualElement trackerMain = propertyField.Q<VisualElement>(name: UIToolkitOnChangedTrackerName(property));
            // ReSharper disable once UseNullPropagation
            if (trackerMain != null)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ON_VALUE_CHANGED
                Debug.Log($"remove old tracker main: {trackerMain}");
#endif
                trackerMain.RemoveFromHierarchy();
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
            OnUpdateUiToolKitInternal(property, containerElement, saintsPropertyDrawers, onValueChangedCallback, fieldInfo);
        }

        private static void OnUpdateUiToolKitInternal(SerializedProperty property, VisualElement container,
            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, Action<object> onValueChangedCallback,
            FieldInfo info
        )
        {
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

            foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            {
                saintsPropertyInfo.Drawer.OnUpdateUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, container, onValueChangedCallback, info);
            }

            container.parent.schedule.Execute(() => OnUpdateUiToolKitInternal(property, container, saintsPropertyDrawers, onValueChangedCallback, info)).StartingIn(SaintsFieldConfig.UpdateLoopDefaultMs);
        }

        protected static PropertyField PropertyFieldFallbackUIToolkit(SerializedProperty property)
        {
            if (_noDecoratorDrawer == null)
            {
                _noDecoratorDrawer = Util.LoadResource<StyleSheet>("UIToolkit/NoDecoratorDrawer.uss");
            }

            // PropertyField propertyField = new PropertyField(property, new string(' ', property.displayName.Length))
            PropertyField propertyField = new PropertyField(property)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = UIToolkitFallbackName(property),
            };

            // propertyField.AddToClassList(SaintsFieldFallbackClass);
            propertyField.AddToClassList(ClassAllowDisable);
            propertyField.styleSheets.Add(_noDecoratorDrawer);
            // propertyField.AddToClassList("unity-base-field__aligned");
            // propertyField.RegisterValueChangeCallback(Debug.Log);
            return propertyField;
        }

                protected static void OnLabelStateChangedUIToolkit(SerializedProperty property, VisualElement container,
            string toLabel, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried,
            RichTextDrawer richTextDrawer)
        {
            VisualElement saintsLabelField = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
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
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
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
