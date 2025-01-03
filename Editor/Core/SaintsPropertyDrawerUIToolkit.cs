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
        protected static string NameLabelFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-label-field";
        public static string ClassLabelFieldUIToolkit = "saints-field--label-field";
        protected static string ClassFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-field";

        public const string ClassAllowDisable = "saints-field-allow-disable";
        protected static string UIToolkitFallbackName(SerializedProperty property) => $"saints-field--fallback-{property.propertyPath}";
        private static string UIToolkitOnChangedTrackerName(SerializedProperty property) =>
            $"saints-field-tracker--{property.propertyPath}";

        private static string NameSaintsPropertyDrawerRoot(SerializedProperty property) =>
            $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}__SaintsFieldRoot";

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

            (ISaintsAttribute[] iSaintsAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);
            Debug.Assert(iSaintsAttributes.Length > 0, property.propertyPath);

            // IReadOnlyList<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
            //     .Select((each, index) => new SaintsWithIndex
            //     {
            //         SaintsAttribute = each,
            //         Index = index,
            //     })
            //     .ToArray();
            IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers = iSaintsAttributes
                .WithIndex()
                .Select(each => new SaintsPropertyInfo
            {
                Drawer = GetOrCreateSaintsDrawerByAttr(each.value),
                Attribute = each.value,
                Index = each.index,
            }).ToArray();

            // SaintsPropertyInfo labelAttributeWithIndex = saintsPropertyDrawers.FirstOrDefault(each => each.Attribute.AttributeType == SaintsAttributeType.Label);
            SaintsPropertyInfo fieldAttributeWithIndex = saintsPropertyDrawers.FirstOrDefault(each => each.Attribute.AttributeType == SaintsAttributeType.Field);

            #region Above

            Dictionary<string, List<SaintsPropertyInfo>> groupedAboveDrawers =
                new Dictionary<string, List<SaintsPropertyInfo>>();
            foreach (SaintsPropertyInfo eachAttributeWithIndex in saintsPropertyDrawers)
            {
                if (!groupedAboveDrawers.TryGetValue(eachAttributeWithIndex.Attribute.GroupBy,
                        out List<SaintsPropertyInfo> currentGroup))
                {
                    groupedAboveDrawers[eachAttributeWithIndex.Attribute.GroupBy] = currentGroup = new List<SaintsPropertyInfo>();
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log("fallback field drawer");
#endif
                // _saintsFieldFallback.RegisterCallback<AttachToPanelEvent>(evt =>
                // {
                //     Debug.Log($"fallback field attached {property.propertyPath}: {evt.target}");
                // });
                VisualElement fallback = UnityFallbackUIToolkit(fieldInfo, property);
                fallback.AddToClassList(ClassFieldUIToolkit(property));
                fieldContainer.Add(fallback);
                containerElement.visible = false;
            }
            else
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"saints field drawer {fieldAttributeWithIndex.Drawer}");
#endif
                VisualElement fieldElement = fieldAttributeWithIndex.Drawer.CreateFieldUIToolKit(property,
                    fieldAttributeWithIndex.Attribute, containerElement, fieldInfo, parent);
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
                if(!groupedDrawers.TryGetValue(eachAttributeWithIndex.Attribute.GroupBy, out List<SaintsPropertyInfo> currentGroup))
                {
                    currentGroup = new List<SaintsPropertyInfo>();
                    groupedDrawers[eachAttributeWithIndex.Attribute.GroupBy] = currentGroup;
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
                    groupByContainer.Add(saintsPropertyInfo.Drawer.CreateBelowUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, fieldInfo, parent));
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
                OnAwakeUiToolKitInternal(property, containerElement, parent, saintsPropertyDrawers));

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Done property gui {property.propertyPath}/{this}");
#endif

            return rootElement;
        }
#endif

        private static VisualElement UnityFallbackUIToolkit(FieldInfo fieldInfo, SerializedProperty property)
        {
            // check if any property has drawer. If so, just use PropertyField
            // if not, check if it has custom drawer. if it exists, then try use that custom drawer
            if (hasOtherAttributeDrawer(fieldInfo))
            {
                return PropertyFieldFallbackUIToolkit(property);
            }

            Type foundDrawer = FindOtherPropertyDrawer(fieldInfo);

            if (foundDrawer == null)
            {
                return PropertyFieldFallbackUIToolkit(property);
            }

            MethodInfo uiToolkitMethod = foundDrawer.GetMethod("CreatePropertyGUI");
            // Debug.Assert(uiToolkitMethod != null, foundDrawer);
            // Debug.Log($"uiToolkitMethod: {uiToolkitMethod}");
            // if (uiToolkitMethod == null)
            // {
            //     return PropertyFieldFallbackUIToolkit(property);
            // }

            if(uiToolkitMethod == null || uiToolkitMethod.DeclaringType != foundDrawer)  // null: old Unity || did not override
            {
                PropertyDrawer imGuiDrawer = MakePropertyDrawer(foundDrawer, fieldInfo);
                MethodInfo imGuiGetPropertyHeightMethod = foundDrawer.GetMethod("GetPropertyHeight");
                MethodInfo imGuiOnGUIMethodInfo = foundDrawer.GetMethod("OnGUI");
                Debug.Assert(imGuiGetPropertyHeightMethod != null);
                Debug.Assert(imGuiOnGUIMethodInfo != null);

                IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);

                IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
                {
                    GUIContent label = imguiLabelHelper.NoLabel
                        ? GUIContent.none
                        : new GUIContent(imguiLabelHelper.RichLabel);

                    float height =
                        (float)imGuiGetPropertyHeightMethod.Invoke(imGuiDrawer, new object[] { property, label });
                    Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));

                    using(new ImGuiFoldoutStyleRichTextScoop())
                    using(new ImGuiLabelStyleRichTextScoop())
                    {
                        imGuiOnGUIMethodInfo.Invoke(imGuiDrawer, new object[] { rect, property, label });
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

            // Debug.Log("Yes");
            PropertyDrawer propertyDrawer = MakePropertyDrawer(foundDrawer, fieldInfo);
            if (propertyDrawer == null)
            {
                return PropertyFieldFallbackUIToolkit(property);
            }

            VisualElement result;
            try
            {
                result = propertyDrawer.CreatePropertyGUI(property);
            }
            catch (Exception)
            {
                return PropertyFieldFallbackUIToolkit(property);
            }
            if (result == null)
            {
                return PropertyFieldFallbackUIToolkit(property);
            }

            result.style.flexGrow = 1;
            result.AddToClassList(ClassAllowDisable);
            return result;

        }

        private static StyleSheet noDecoratorDrawer;

                private void OnAwakeUiToolKitInternal(SerializedProperty property, VisualElement containerElement,
            object parent, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers)
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"usingFallbackField {property.propertyPath}, parentRoots={parentRoots.Count}, {saintsPropertyDrawers.Count} ({NameSaintsPropertyDrawerRoot(property)})");
#endif
                if (parentRoots.Count != saintsPropertyDrawers.Count)
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
                OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers);
            }
            else
            {
                OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers);
            }
        }

        private static VisualElement BindWatchUIToolkit(SerializedProperty property, Action<object> onValueChangedCallback, bool isReference, PropertyField propertyField, FieldInfo fieldInfo, object parent)
        {
            // PropertyField fallbackField = propertyField.Q<PropertyField>(name: UIToolkitFallbackName(property));
            VisualElement trackerMain = propertyField.Q<VisualElement>(name: UIToolkitOnChangedTrackerName(property));
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
            object parent,  Action<object> onValueChangedCallback, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers)
        {

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
                saintsPropertyInfo.Drawer.OnAwakeUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, onValueChangedCallback, fieldInfo, parent);
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
            if (noDecoratorDrawer == null)
            {
                noDecoratorDrawer = Util.LoadResource<StyleSheet>("UIToolkit/NoDecoratorDrawer.uss");
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
            propertyField.styleSheets.Add(noDecoratorDrawer);
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
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
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
