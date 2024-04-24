using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Core
{
    // above
    // pre, label, field, post
    // below-
    public abstract class SaintsPropertyDrawer: PropertyDrawer
    {
        public const int LabelLeftSpace = 4;
        public const int LabelBaseWidth = 120;
        public const int IndentWidth = 15;
        public const float SingleLineHeight = 20f;
        // public const string EmptyRectLabel = "                ";

        // public static bool IsSubDrawer = false;
        private static readonly Dictionary<InsideSaintsFieldScoop.PropertyKey, int> SubDrawCounter = new Dictionary<InsideSaintsFieldScoop.PropertyKey, int>();
        private static readonly Dictionary<InsideSaintsFieldScoop.PropertyKey, int> SubGetHeightCounter = new Dictionary<InsideSaintsFieldScoop.PropertyKey, int>();

        private static readonly Dictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>> PropertyAttributeToPropertyDrawers =
            new Dictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>>();
#if UNITY_2022_1_OR_NEWER
        private static IReadOnlyDictionary<Type, IReadOnlyList<Type>> PropertyAttributeToDecoratorDrawers =
            new Dictionary<Type, IReadOnlyList<Type>>();
#endif

        // private class SharedInfo
        // {
        //     public bool Changed;
        //     // public object ParentTarget;
        // }

        // private static readonly Dictionary<string, SharedInfo> PropertyPathToShared = new Dictionary<string, SharedInfo>();

        // private IReadOnlyList<ISaintsAttribute> _allSaintsAttributes;
        // private SaintsPropertyDrawer _labelDrawer;
        // private SaintsPropertyDrawer _fieldDrawer;

        // ReSharper disable once InconsistentNaming
        protected readonly string FieldControlName;

        private struct SaintsWithIndex
        {
            // ReSharper disable InconsistentNaming
            public ISaintsAttribute SaintsAttribute;
            // ReSharper disable once NotAccessedField.Local
            public int Index;
            // ReSharper enable InconsistentNaming
        }

        private readonly Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _cachedDrawer = new Dictionary<SaintsWithIndex, SaintsPropertyDrawer>();
        // private readonly Dictionary<Type, PropertyDrawer> _cachedOtherDrawer = new Dictionary<Type, PropertyDrawer>();
        // private readonly HashSet<Type> _usedDrawerTypes = new HashSet<Type>();
        // private readonly Dictionary<ISaintsAttribute, >
        // private struct UsedAttributeInfo
        // {
        //     public Type DrawerType;
        //     public ISaintsAttribute Attribute;
        // }

        // private readonly List<UsedAttributeInfo> _usedAttributes = new List<UsedAttributeInfo>();
        // private readonly Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _usedAttributes = new Dictionary<SaintsWithIndex, SaintsPropertyDrawer>();

        // private static readonly FieldDrawerConfigAttribute DefaultFieldDrawerConfigAttribute =
        //     new FieldDrawerConfigAttribute(FieldDrawerConfigAttribute.FieldDrawType.Inline, 0);

        // private string _cachedPropPath;

        protected static readonly Dictionary<object, object> inMemoryStorage = new Dictionary<object, object>();
#if UNITY_2022_1_OR_NEWER
        private static Assembly UnityEditorAssemble;
#endif

        // ReSharper disable once PublicConstructorInAbstractClass
        public SaintsPropertyDrawer()
        {
            // Selection.selectionChanged += OnSelectionChanged;
            // Debug.Log($"OnSaintsCreate Start: {SepTitleAttributeDrawer.drawCounter}/{fieldInfo}");
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//             Debug.Log($"new SaintsPropertyDrawer {this}");
// #endif
            // if (IsSubDrawer)
            // {
            //     return;
            // }

            FieldControlName = Guid.NewGuid().ToString();

            // _usedAttributes.Clear();

            // _propertyAttributeToDrawers.Clear();

            // ReSharper disable once InvertIf
            if(PropertyAttributeToPropertyDrawers.Count == 0)
            {
                Dictionary<Type, HashSet<Type>> attrToDrawers = new Dictionary<Type, HashSet<Type>>();
#if UNITY_2022_1_OR_NEWER
                Dictionary<Type, List<Type>> attrToDecoratorDrawers =
                    new Dictionary<Type, List<Type>>();
#endif

                foreach (Assembly asb in AppDomain.CurrentDomain.GetAssemblies())
                {
#if UNITY_2022_1_OR_NEWER
                    if (asb.GetName().Name == "UnityEditor")
                    {
                        UnityEditorAssemble = asb;
                    }
#endif

                    Type[] allTypes = asb.GetTypes();

                    List<Type> allSubPropertyDrawers = allTypes
                        // .Where(type => type.IsSubclassOf(typeof(SaintsPropertyDrawer)))
                        .Where(type => type.IsSubclassOf(typeof(PropertyDrawer)))
                        .ToList();

                    foreach (Type eachPropertyDrawer in allSubPropertyDrawers)
                    {
                        foreach (Type attr in eachPropertyDrawer.GetCustomAttributes<CustomPropertyDrawer>(true)
                                     .Select(instance => typeof(CustomPropertyDrawer)
                                         .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                                         ?.GetValue(instance))
                                     .Where(each => each != null))
                        {
                            if (!attrToDrawers.TryGetValue(attr, out HashSet<Type> attrList))
                            {
                                attrToDrawers[attr] = attrList = new HashSet<Type>();
                            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CORE_DRAWER_INIT
                            Debug.Log($"Found drawer: {attr} -> {saintsSubDrawer}");
#endif

                            attrList.Add(eachPropertyDrawer);
                        }
                    }

#if UNITY_2022_1_OR_NEWER
                    List<Type> allSubDecoratorDrawers = allTypes
                        .Where(type => type.IsSubclassOf(typeof(DecoratorDrawer)))
                        .ToList();

                    foreach (Type eachDecoratorDrawer in allSubDecoratorDrawers)
                    {
                        foreach (Type attr in eachDecoratorDrawer.GetCustomAttributes<CustomPropertyDrawer>(true)
                                     .Select(instance => typeof(CustomPropertyDrawer)
                                         .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                                         ?.GetValue(instance))
                                     .Where(each => each != null))
                        {
                            if (!attrToDecoratorDrawers.TryGetValue(attr, out List<Type> attrList))
                            {
                                attrToDecoratorDrawers[attr] = attrList = new List<Type>();
                            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CORE_DRAWER_INIT
                            Debug.Log($"Found dec drawer: {attr} -> {saintsSubDrawer}");
#endif

                            if(!attrList.Contains(eachDecoratorDrawer))
                            {
                                attrList.Add(eachDecoratorDrawer);
                            }
                        }
                    }

#endif

                }

                foreach (KeyValuePair<Type, HashSet<Type>> kv in attrToDrawers)
                {
                    PropertyAttributeToPropertyDrawers[kv.Key] = kv.Value
                        .Select(each => (each.IsSubclassOf(typeof(SaintsPropertyDrawer)), each))
                        .ToArray();
// #if EXT_INSPECTOR_LOG
//                     Debug.Log($"attr {kv.Key} has drawer(s) {string.Join(",", kv.Value)}");
// #endif
                }

#if UNITY_2022_1_OR_NEWER
                PropertyAttributeToDecoratorDrawers = attrToDecoratorDrawers.ToDictionary(each => each.Key, each => (IReadOnlyList<Type>)each.Value);
#endif
            }
        }

        // ~SaintsPropertyDrawer()
        // {
        //     Debug.Log($"[{this}] Stop listening changed");
        //     Selection.selectionChanged -= OnSelectionChanged;
        // }
        //
        // private void OnSelectionChanged()
        // {
        //     Debug.Log($"selection changed: {string.Join(", ", Selection.objects.Select(each => each.ToString()))}");
        // }

        // ~SaintsPropertyDrawer()
        // {
        //     PropertyAttributeToDrawers.Clear();
        // }

        #region IMGUI GC Issue

        private UnityEngine.Object _imGuiObject;

        protected virtual void ImGuiOnDispose()
        {
            Selection.selectionChanged -= ImGuiCheckChanged;
            _imGuiObject = null;
        }

        private void ImGuiCheckChanged()
        {
            // no longer selected
            if(Array.IndexOf(Selection.objects, _imGuiObject) == -1)
            {
                ImGuiOnDispose();
            }
        }

        protected void ImGuiEnsureDispose(UnityEngine.Object serTarget)
        {
            if (_imGuiObject == serTarget)
            {
                return;
            }

            ImGuiOnDispose();
            _imGuiObject = serTarget;
            Selection.selectionChanged += ImGuiCheckChanged;
        }

        #endregion

        private float _labelFieldBasicHeight = EditorGUIUtility.singleLineHeight;

        protected virtual bool GetAndVisibility(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return true;
        }

        private bool GetVisibility(SerializedProperty property, IEnumerable<SaintsWithIndex> saintsAttributeWithIndexes, object parent)
        {
            List<bool> showAndResults = new List<bool>();
            foreach (SaintsWithIndex saintsAttributeWithIndex in saintsAttributeWithIndexes)
            {
                if(saintsAttributeWithIndex.SaintsAttribute is IImGuiVisibilityAttribute visibilityAttribute)
                {
                    SaintsPropertyDrawer drawer = GetOrCreateSaintsDrawer(saintsAttributeWithIndex);
                    showAndResults.Add(drawer.GetAndVisibility(property, saintsAttributeWithIndex.SaintsAttribute, fieldInfo, parent));
                }
            }
            // Debug.Log($"visibility={string.Join(", ", showAndResults)}");

            return showAndResults.Count == 0 || showAndResults.Any(each => each);
        }

        #region GetPropertyHeight
        private float _filedWidthCache = -1;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // if (IsSubDrawer)
            // {
            //     return EditorGUI.GetPropertyHeight(property, label);
            // }
            // Debug.Log($"GetPropertyHeight/{this}");

            if (SubDrawCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideDrawCount) && insideDrawCount > 0)
            {
                // Debug.Log($"Sub Draw GetPropertyHeight/{this}");
                // return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
                return GetPropertyHeightFallback(property, label);
            }

            if (SubGetHeightCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideGetHeightCount) && insideGetHeightCount > 0)
            {
                // Debug.Log($"Sub GetHeight GetPropertyHeight/{this}");
                // return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
                return GetPropertyHeightFallback(property, label);
            }

            (PropertyAttribute[] allAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);
            // (ISaintsAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);
            SaintsWithIndex[] saintsAttributeWithIndexes = allAttributes
                .OfType<ISaintsAttribute>()
                // .Where(each => !(each is VisibilityAttribute))
                .Select((each, index) => new SaintsWithIndex
                {
                    SaintsAttribute = each,
                    Index = index,
                })
                .ToArray();

            if (!GetVisibility(
                    property,
                    saintsAttributeWithIndexes,
                    parent
                ))
            {
                // Debug.Log($"height 0");
                return 0f;
            }
            // Debug.Log("height continue");

            // if (_usedAttributes.Count == 0)
            // {
            //     foreach ((SaintsWithIndex each, SaintsPropertyDrawer drawer) in attributes
            //                  .Select((each, index) => new SaintsWithIndex
            //                  {
            //                      SaintsAttribute = each,
            //                      Index = index,
            //                  })
            //                  .Where(each => !(each.SaintsAttribute is VisibilityAttribute))
            //                  .Select(each => (each, GetOrCreateSaintsDrawer(each))))
            //     {
            //         _usedAttributes[each] = drawer;
            //     }
            // }
            Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _usedAttributes = saintsAttributeWithIndexes
                .ToDictionary(each => each, GetOrCreateSaintsDrawer);

            // float defaultHeight = base.GetPropertyHeight(property, label);
            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer)[] filedOrLabel = _usedAttributes
                .Where(each => each.Key.SaintsAttribute.AttributeType == SaintsAttributeType.Field || each.Key.SaintsAttribute.AttributeType == SaintsAttributeType.Label)
                .Select(each => (IsaintsAttribute: each.Key.SaintsAttribute, each.Value))
                .ToArray();

            // foreach ((ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer) in filedOrLabel)
            // {
            //     Debug.Log($"GetHeight found {iSaintsAttribute} {iSaintsAttribute.AttributeType} {drawer}");
            // }

            // SaintsPropertyDrawer[] usedDrawerInfos = _usedDrawerTypes.Select(each => _cachedDrawer[each]).ToArray();
            // SaintsPropertyDrawer[] fieldInfos = usedDrawerInfos.Where(each => each.AttributeType is SaintsAttributeType.Field or SaintsAttributeType.Label).ToArray();

            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer) labelFound = filedOrLabel.FirstOrDefault(each => each.iSaintsAttribute.AttributeType == SaintsAttributeType.Label);
            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer) fieldFound = filedOrLabel.FirstOrDefault(each => each.iSaintsAttribute.AttributeType == SaintsAttributeType.Field);

            // Debug.Log($"labelFound.iSaintsAttribute={labelFound.iSaintsAttribute}");
            bool hasSaintsLabel = labelFound.iSaintsAttribute != null;
            // Debug.Log($"hasSaintsLabel={hasSaintsLabel}");

            bool saintsDrawNoLabel = hasSaintsLabel &&
                                     !labelFound.drawer.WillDrawLabel(property, labelFound.iSaintsAttribute, fieldInfo, parent);

            bool hasSaintsField = fieldFound.iSaintsAttribute != null;

            bool disabledLabelField = label.text == "" || saintsDrawNoLabel;
            // Debug.Log(disabledLabelField);

            float labelBasicHeight = saintsDrawNoLabel? 0f: EditorGUIUtility.singleLineHeight;
            float fieldBasicHeight = hasSaintsField
                ? fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute, fieldInfo, !disabledLabelField, parent)
                // : EditorGUIUtility.singleLineHeight;
                // : EditorGUI.GetPropertyHeight(property, label, true);
                : GetPropertyHeightFallback(property, label);

            // Debug.Log($"hasSaintsField={hasSaintsField}, labelBasicHeight={labelBasicHeight}, fieldBasicHeight={fieldBasicHeight}");
            _labelFieldBasicHeight = Mathf.Max(labelBasicHeight, fieldBasicHeight);

            float aboveHeight = 0;
            float belowHeight = 0;

            float fullWidth = _filedWidthCache <= 0
                ? EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15
                : _filedWidthCache;
            // Nah, Unity will give `EditorGUIUtility.currentViewWidth=0` on first render...
            // Let Drawer decide what to do then...
            // float fullWidth = 100;
            // Debug.Log($"fullWidth={fullWidth}, _filedWidthCache={_filedWidthCache}; EditorGUIUtility.currentViewWidth={EditorGUIUtility.currentViewWidth}, EditorGUI.indentLevel={EditorGUI.indentLevel}");

            foreach (IGrouping<string, KeyValuePair<SaintsWithIndex, SaintsPropertyDrawer>> grouped in _usedAttributes.ToLookup(each => each.Key.SaintsAttribute.GroupBy))
            {
                float eachWidth = grouped.Key == ""
                    ? fullWidth
                    : fullWidth / grouped.Count();

                IEnumerable<float> aboveHeights = grouped
                    .Select(each => each.Value.GetAboveExtraHeight(property, label, eachWidth, each.Key.SaintsAttribute, fieldInfo, parent))
                    .Where(each => each > 0)
                    .DefaultIfEmpty(0);
                IEnumerable<float> belowHeights = grouped
                    .Select(each => each.Value.GetBelowExtraHeight(property, label, eachWidth, each.Key.SaintsAttribute, fieldInfo, parent))
                    .Where(each => each > 0)
                    .DefaultIfEmpty(0);

                if (grouped.Key == "")
                {
                    aboveHeight += aboveHeights.Sum();
                    belowHeight += belowHeights.Sum();
                }
                else
                {
                    aboveHeight += aboveHeights.Max();
                    belowHeight += belowHeights.Max();
                }
                // Debug.Log($"belowHeight={belowHeight}");
            }

            // Debug.Log($"aboveHeight={aboveHeight}");

            // Debug.Log($"_labelFieldBasicHeight={_labelFieldBasicHeight}");
            // Debug.Log($"Done GetPropertyHeight/{this}");

            return _labelFieldBasicHeight + aboveHeight + belowHeight;
        }

        private static float GetPropertyHeightFallback(SerializedProperty property, GUIContent label)
        {
            // TODO: check, if it has dec, this value might be wrong
            using (new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
        }

        protected virtual float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return 0;
        }

        // protected virtual float GetLabelHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     return 0;
        // }

        protected virtual float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return 0;
        }

        protected virtual float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return 0;
        }
        #endregion

        // private float _aboveUsedHeight;

//         private void UsedAttributesTryAdd(SaintsWithIndex key, SaintsPropertyDrawer value)
//         {
// #if UNITY_2021_3_OR_NEWER
//             // _usedAttributes.TryAdd(key, value);
// #else
//             // if (!_usedAttributes.TryGetValue(key, out SaintsPropertyDrawer _))
//             // {
//             //     _usedAttributes[key] = value;
//             // }
// #endif
//         }

        // protected static void SetValueChanged(SerializedProperty property, bool changed=true)
        // {
        //     // Debug.LogWarning($"set {property.propertyPath}=true");
        //     if(!PropertyPathToShared.TryGetValue(property.propertyPath, out SharedInfo sharedInfo))
        //     {
        //         PropertyPathToShared[property.propertyPath] = sharedInfo = new SharedInfo();
        //     }
        //     sharedInfo.Changed = changed;
        // }

        private struct SaintsPropertyInfo
        {
            // ReSharper disable InconsistentNaming
            public SaintsPropertyDrawer Drawer;
            public ISaintsAttribute Attribute;
            public int Index;
            // ReSharper enable InconsistentNaming
        }

        #region UI
        protected static string NameLabelFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-label-field";
        protected static string ClassFieldUIToolkit(SerializedProperty property) => $"{property.propertyPath}__saints-field-field";

        protected const string ClassAllowDisable = "saints-field-allow-disable";

#if UNITY_2021_3_OR_NEWER
//         protected PropertyField SaintsFallbackUIToolkit(SerializedProperty property)
//         {
// //             var nestInfo = new NestInfo
// //             {
// //                 targetObject = property.serializedObject.targetObject,
// //                 propertyPath = property.propertyPath,
// //                 // count = _saintsPropertyDrawers.Count - 1,
// //                 count = _saintsPropertyDrawers.Count * 3 - 1,
// //             };
// //             PropertyNestInfo.Add(nestInfo);
// //
// // #if UNITY_EDITOR
// //             Debug.Log($"PropertyNestInfo Fallback {nestInfo.targetObject}.{nestInfo.propertyPath}.{nestInfo.count}");
// // #endif
//
//             return UnityFallbackUIToolkit(property);
//         }

        protected static PropertyField UnityFallbackUIToolkit(SerializedProperty property)
        {
            // PropertyField propertyField = new PropertyField(property, new string(' ', property.displayName.Length))
            PropertyField propertyField = new PropertyField(property)
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            propertyField.AddToClassList(SaintsFieldFallbackClass);
            propertyField.AddToClassList(ClassAllowDisable);
            // propertyField.RegisterValueChangeCallback(Debug.Log);
            return propertyField;
        }
#endif

        #region UI Toolkit
        protected const string SaintsFieldFallbackClass = "saints-field-fallback-property-field";

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Create property gui {property.propertyPath}/{property.displayName}/{this}");
#endif
            // InsideSaintsFieldScoop.PropertyKey insideKey = InsideSaintsFieldScoop.MakeKey(property);
            // object serTarget = property.serializedObject.targetObject;
            // string propPath = property.propertyPath;
            // NestInfo nestInfo = PropertyNestInfo.FirstOrDefault(each => each.targetObject == serTarget && each.propertyPath == propPath);
//             if (nestInfo != null && nestInfo.count > 0)
//             {
//                 nestInfo.count -= 1;
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//                 Debug.Log($"PropertyNestInfo capture sub drawer `{property.displayName}`:{property.propertyPath}@{nestInfo.count}");
// #endif
//
//                 if (nestInfo.count <= 0)
//                 {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//                     Debug.Log($"PropertyNestInfo removed `{property.displayName}`:{property.propertyPath}@{nestInfo.count}");
// #endif
//                     PropertyNestInfo.Remove(nestInfo);
//                 }
//
//                 return UnityFallbackUIToolkit(property);
//             }

            VisualElement containerElement = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                },
                name = $"{property.propertyPath}__SaintsFieldContainer",
            };

            (ISaintsAttribute[] iSaintsAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);

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
                },
                name = NameLabelFieldUIToolkit(property),
                userData = null,
            };

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

            // Type fieldDrawer = fieldAttributeWithIndex.Attribute == null
            //     ? null
            //     : GetFirstSaintsDrawerType(fieldAttributeWithIndex.Attribute.GetType());

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
                PropertyField fallback = UnityFallbackUIToolkit(property);
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

            containerElement.Add(fieldContainer);

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

            // Debug.Log($"ContainerElement={containerElement}");
            // _rootElement.RegisterCallback<AttachToPanelEvent>(evt => OnAwakeUiToolKitInternal(property, containerElement, parent, _saintsFieldDrawer==null));
            // _rootElement.RegisterCallback<AttachToPanelEvent>(evt => Debug.Log($"AttachToPanelEvent"));
            // _rootElement.RegisterCallback<GeometryChangedEvent>(evt => Debug.Log($"GeometryChangedEvent"));
            // _rootElement.schedule.Execute(() => Debug.Log("Execute"));
            rootElement.schedule.Execute(() =>
                OnAwakeUiToolKitInternal(property, containerElement, parent, saintsPropertyDrawers));


#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"Done property gui {property.propertyPath}/{this}");
#endif

            return rootElement;
        }
#endif
        #endregion

        #region IMGUI

        protected class OnGUIPayload
        {
            public bool changed;
            public object newValue;

            public void SetValue(object value)
            {
                changed = true;
                newValue = value;
            }
        }

        private class LabelDrawerInfo
        {
            public SaintsPropertyDrawer labelDrawerInstance;
            public Rect rect;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Debug.Log($"OnGui Start: {SepTitleAttributeDrawer.drawCounter}");
            // this is so weird... because of Unity's repaint, layout etc.
            if(position.width - 1 > Mathf.Epsilon)
            {
                _filedWidthCache = position.width;
            }
            // Debug.Log($"OnGUI: pos={position}");

            // Debug.Log($"raw pos={position.y} height={position.height}");
            // _cachedPropPath = property.propertyPath;
            //
            // if (!PropertyPathToShared.ContainsKey(property.propertyPath))
            // {
            //     PropertyPathToShared[property.propertyPath] = new SharedInfo();
            // }
            // Debug.Log($"OnGUI: {property.displayName} path {property.propertyPath}; obj={property.serializedObject.targetObject}");

            if (SubDrawCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideCount) && insideCount > 0)
            {
                // Debug.Log($"capture sub drawer `{property.displayName}`:{property.propertyPath}@{insideCount}");
                // EditorGUI.PropertyField(position, property, label, true);
                UnityDraw(position, property, label, fieldInfo);
                return;
            }

            OnGUIPayload onGUIPayload = new OnGUIPayload();

            (ISaintsAttribute[] iSaintsAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);

            IReadOnlyList<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
                .Select((each, index) => new SaintsWithIndex
                {
                    SaintsAttribute = each,
                    Index = index,
                })
                .ToArray();

            // Debug.Log($"Saints: {property.displayName} found {allSaintsAttributes.Count}");

            if (!GetVisibility(property, allSaintsAttributes.Where(each => each.SaintsAttribute is IImGuiVisibilityAttribute), parent))
            {
                return;
            }

            SaintsWithIndex labelAttributeWithIndex = allSaintsAttributes.FirstOrDefault(each => each.SaintsAttribute.AttributeType == SaintsAttributeType.Label);
            SaintsWithIndex fieldAttributeWithIndex = allSaintsAttributes.FirstOrDefault(each => each.SaintsAttribute.AttributeType == SaintsAttributeType.Field);

            // _usedAttributes.Clear();

            using(new EditorGUI.PropertyScope(position, label, property))
            {
                // propertyScope.Dispose();
                // GUIContent propertyScoopLabel = propertyScope.content;
                GUIContent bugFixCopyLabel = new GUIContent(label);

                // Debug.Log($"above: {label.text}");

                #region Above

                Rect aboveRect = EditorGUI.IndentedRect(position);

                Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>
                    groupedAboveDrawers =
                        new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);

                    // ReSharper disable once InvertIf
                    if (drawerInstance.WillDrawAbove(property, eachAttributeWithIndex.SaintsAttribute, fieldInfo, parent))
                    {
                        if (!groupedAboveDrawers.TryGetValue(eachAttributeWithIndex.SaintsAttribute.GroupBy,
                                out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                            groupedAboveDrawers[eachAttributeWithIndex.SaintsAttribute.GroupBy] = currentGroup;
                        }

                        currentGroup.Add((drawerInstance, eachAttributeWithIndex.SaintsAttribute));
                        // _usedDrawerTypes.Add(eachDrawer[0]);
                        // UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                    }
                }

                float aboveUsedHeight = 0;
                float aboveInitY = aboveRect.y;

                foreach (KeyValuePair<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>
                             drawerInfoKv in groupedAboveDrawers)
                {
                    string groupBy = drawerInfoKv.Key;
                    List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfos = drawerInfoKv.Value;

                    if (groupBy == "")
                    {
                        foreach ((SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) in drawerInfos)
                        {
                            Rect newAboveRect =
                                drawerInstance.DrawAboveImGui(aboveRect, property, bugFixCopyLabel, eachAttribute, onGUIPayload, fieldInfo, parent);
                            aboveUsedHeight = newAboveRect.y - aboveInitY;
                            aboveRect = newAboveRect;
                        }
                    }
                    else
                    {
                        float totalWidth = aboveRect.width;
                        float eachWidth = totalWidth / drawerInfos.Count;
                        float height = 0;
                        for (int index = 0; index < drawerInfos.Count; index++)
                        {
                            (SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) = drawerInfos[index];
                            Rect eachRect = new Rect(aboveRect)
                            {
                                x = aboveRect.x + eachWidth * index,
                                width = eachWidth,
                            };
                            Rect leftRect =
                                drawerInstance.DrawAboveImGui(eachRect, property, bugFixCopyLabel, eachAttribute, onGUIPayload, fieldInfo, parent);
                            height = Mathf.Max(height, leftRect.y - eachRect.y);
                            // Debug.Log($"height={height}");
                        }

                        // aboveRect.height = height;
                        aboveUsedHeight += height;
                        aboveRect = new Rect(aboveRect)
                        {
                            y = aboveRect.y + height,
                            height = aboveRect.height - height,
                        };
                    }

                    // Debug.Log($"aboveUsedHeight={aboveUsedHeight}");
                }

                // if(Event.current.type == EventType.Repaint)
                // {
                // _aboveUsedHeight = aboveUsedHeight;
                // }

                // Debug.Log($"{Event.current} {aboveUsedHeight} / {_aboveUsedHeight}");

                #endregion

                Rect labelFieldRowRect = EditorGUI.IndentedRect(new Rect(position)
                {
                    // y = aboveRect.y + (groupedAboveDrawers.Count == 0? 0: aboveRect.height),
                    y = position.y + aboveUsedHeight,
                    height = _labelFieldBasicHeight,
                });

                // Color backgroundColor = EditorGUIUtility.isProSkin
                //     ? new Color32(56, 56, 56, 255)
                //     : new Color32(194, 194, 194, 255);
                // UnityDraw(fieldRect, property, propertyScoopLabel);
                // EditorGUI.DrawRect(fieldRect, backgroundColor);

                // GUIContent newLabel = propertyScoopLabel;
                // float originalLabelWidth = EditorGUIUtility.labelWidth;

                // labelRect.height = EditorGUIUtility.singleLineHeight;

                // Debug.Log($"pre label: {label.text}");

                #region pre label

                float preLabelWidth = 0;

                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    float preLabelUseWidth =
                        drawerInstance.DrawPreLabelImGui(new Rect(labelFieldRowRect)
                        {
                            width = EditorGUIUtility.labelWidth,
                            height = EditorGUIUtility.singleLineHeight,
                        }, property, eachAttributeWithIndex.SaintsAttribute, fieldInfo, parent);
                    // ReSharper disable once InvertIf
                    if (preLabelUseWidth > 0)
                    {
                        preLabelWidth += preLabelUseWidth;
                        // UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                    }
                }

                #endregion

                Rect fieldUseRectWithPost = RectUtils.SplitWidthRect(labelFieldRowRect, preLabelWidth).leftRect;

                #region label info

                // bool completelyDisableLabel = string.IsNullOrEmpty(label.text);
                GUIContent useGuiContent;

                // Action saintsPropertyDrawerDrawLabelCallback = () => { };
                LabelDrawerInfo labelDrawerInfo = null;
                if (string.IsNullOrEmpty(label.text))
                {
                    // needFallbackLabel = true;
                    useGuiContent = new GUIContent(label);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                    Debug.Log($"use label empty: `{useGuiContent.text}`");
#endif
                    // hasLabelSpace = false;
                }
                else if (labelAttributeWithIndex.SaintsAttribute == null) // has label, no saints label drawer
                {
                    // needFallbackLabel = false;
                    useGuiContent = new GUIContent(label);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                    Debug.Log($"use label not saints label drawer: `{useGuiContent.text}`");
#endif
                }
                else
                {
                    SaintsPropertyDrawer labelDrawerInstance = GetOrCreateSaintsDrawer(labelAttributeWithIndex);
                    // UsedAttributesTryAdd(labelAttributeWithIndex, labelDrawerInstance);
                    // completelyDisableLabel = labelDrawerInstance.WillDrawLabel(property, label, labelAttributeWithIndex.SaintsAttribute);
                    bool hasLabelSpace =
                        labelDrawerInstance.WillDrawLabel(property, labelAttributeWithIndex.SaintsAttribute, fieldInfo, parent);
                    if (hasLabelSpace)
                    {
                        labelDrawerInfo = new LabelDrawerInfo
                        {
                            labelDrawerInstance = labelDrawerInstance,
                            rect = new Rect(fieldUseRectWithPost)
                            {
                                width = EditorGUIUtility.labelWidth - preLabelWidth,
                                height = EditorGUIUtility.singleLineHeight,
                            },
                        };
                        // saintsPropertyDrawerDrawLabelCallback = () =>
                        //     labelDrawerInstance.DrawLabel(new Rect(fieldUseRectWithPost)
                        //         {
                        //             width = EditorGUIUtility.labelWidth - preLabelWidth,
                        //             height = EditorGUIUtility.singleLineHeight,
                        //         }, property, label,
                        //         labelAttributeWithIndex.SaintsAttribute, fieldInfo, parent);
                    }

                    useGuiContent = hasLabelSpace
                        ? new GUIContent(label) { text = "                 " }
                        : new GUIContent(label) { text = "" };

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                    Debug.Log($"use label saints label drawer hasLabelSpace={hasLabelSpace}: `{useGuiContent.text}`");
#endif

                    // Debug.Log($"hasLabelSpace={hasLabelSpace}, guiContent.text.length={useGuiContent.text.Length}");
                }

                #endregion

                #region post field - width check

                float postFieldWidth = 0;
                List<(SaintsWithIndex attributeWithIndex, SaintsPropertyDrawer drawer, float width)> postFieldInfoList =
                    new List<(SaintsWithIndex attributeWithIndex, SaintsPropertyDrawer drawer, float width)>();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    float curWidth =
                        drawerInstance.GetPostFieldWidth(fieldUseRectWithPost, property, GUIContent.none,
                            eachAttributeWithIndex.SaintsAttribute, fieldInfo, parent);
                    postFieldWidth += curWidth;
                    postFieldInfoList.Add((
                        eachAttributeWithIndex,
                        drawerInstance,
                        curWidth
                    ));
                }

                #endregion

                (Rect fieldUseRectNoPost, Rect fieldPostRect) =
                    RectUtils.SplitWidthRect(fieldUseRectWithPost, fieldUseRectWithPost.width - postFieldWidth);

                #region field

                Type fieldDrawer = fieldAttributeWithIndex.SaintsAttribute == null
                    ? null
                    : GetFirstSaintsDrawerType(fieldAttributeWithIndex.SaintsAttribute.GetType());
                // Debug.Log($"field {fieldAttributeWithIndex.SaintsAttribute}->{fieldDrawer}");

                // Debug.Log($"{label.text}={_fieldControlName}");

                // EditorGUIUtility.labelWidth = ProperLabelWidth();
                // Debug.Log($"{property.propertyPath}=false");
                // EditorGUI.DrawRect(fieldUseRectNoPost, Color.yellow);
                using (new AdaptLabelWidth())
                using (new ResetIndentScoop())
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    if (fieldDrawer == null)
                    {
                        // GUI.SetNextControlName(_fieldControlName);
                        // Debug.Log($"default drawer for {label.text}");
                        DefaultDrawer(fieldUseRectNoPost, property, useGuiContent, fieldInfo);
                    }
                    else
                    {
                        // Debug.Log(fieldAttribute);
                        SaintsPropertyDrawer fieldDrawerInstance = GetOrCreateSaintsDrawer(fieldAttributeWithIndex);
                        // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                        // GUI.SetNextControlName(_fieldControlName);
                        fieldDrawerInstance.DrawField(fieldUseRectNoPost, property, useGuiContent,
                            fieldAttributeWithIndex.SaintsAttribute, onGUIPayload, fieldInfo, parent);
                        // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                        // UsedAttributesTryAdd(fieldAttributeWithIndex, fieldDrawerInstance);
                    }

                    // if (changed.changed && fieldDrawer == null)
                    // Debug.Log($"changed.changed={changed.changed}");
                    if (changed.changed && !onGUIPayload.changed)
                    {
                        // Debug.Log($"set changed to true");
                        // PropertyPathToShared[property.propertyPath].Changed = true;
                        // onGUIPayload.changed = true;
                        property.serializedObject.ApplyModifiedProperties();
                        // onGUIPayload.newValue = fieldInfo.GetValue();
                        object rawValue = fieldInfo.GetValue(parent);
                        int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                        object curValue = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);
                        onGUIPayload.SetValue(curValue);
                    }
                }

                // Debug.Log($"after field: ValueChange={_valueChange}");
                // saintsPropertyDrawerDrawLabelCallback?.Invoke();

                #endregion

                #region post field

                float postFieldAccWidth = 0f;
                foreach ((SaintsWithIndex attributeWithIndex, SaintsPropertyDrawer drawer, float width) in
                         postFieldInfoList)
                {
                    Rect eachRect = new Rect(fieldPostRect)
                    {
                        x = fieldPostRect.x + postFieldAccWidth,
                        width = width,
                    };
                    postFieldAccWidth += width;

                    // Debug.Log($"DrawPostField, valueChange={_valueChange}");
                    drawer.DrawPostFieldImGui(eachRect, property, bugFixCopyLabel,
                        attributeWithIndex.SaintsAttribute,
                        attributeWithIndex.Index,
                        onGUIPayload,
                        fieldInfo,
                        parent);
                    // ReSharper disable once InvertIf
                    // if (isActive)
                    // {
                    //     UsedAttributesTryAdd(attributeWithIndex, drawer);
                    // }
                }

                // foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                // {
                //     SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                //     (bool isActive, Rect newPostFieldRect) = drawerInstance.DrawPostField(postFieldRect, property, propertyScoopLabel, eachAttributeWithIndex.SaintsAttribute);
                //     // ReSharper disable once InvertIf
                //     if (isActive)
                //     {
                //         postFieldRect = newPostFieldRect;
                //         // _usedDrawerTypes.Add(eachDrawer[0]);
                //         _usedAttributes.TryAdd(eachAttributeWithIndex, drawerInstance);
                //     }
                // }

                #endregion

                // saintsPropertyDrawerDrawLabelCallback.Invoke();

                #region Actual draw label for rich text

                if (labelDrawerInfo != null)
                {
                    labelDrawerInfo.labelDrawerInstance.DrawLabel(labelDrawerInfo.rect, property, bugFixCopyLabel,
                        labelAttributeWithIndex.SaintsAttribute, fieldInfo, parent);
                }

                #endregion

                #region Overlay

                // List<Rect> overlayTakenPositions = new List<Rect>();
                bool hasLabelWidth = !string.IsNullOrEmpty(useGuiContent.text);
                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    drawerInstance.DrawOverlay(labelFieldRowRect, property, bugFixCopyLabel,
                            eachAttributeWithIndex.SaintsAttribute, hasLabelWidth, fieldInfo, parent);
                    // ReSharper disable once InvertIf
                    // if (isActive)
                    // {
                    //     UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                    //     // overlayTakenPositions.Add(newLabelRect);
                    // }
                }

                #endregion

                #region below

                // Debug.Log($"pos.y={position.y}; pos.h={position.height}; fieldRect.y={fieldRect.y}; fieldRect.height={fieldRect.height}");
                Rect belowRect = EditorGUI.IndentedRect(new Rect(position)
                {
                    y = labelFieldRowRect.y + labelFieldRowRect.height,
                    height = position.y + position.height - (labelFieldRowRect.y + labelFieldRowRect.height),
                });

                // Debug.Log($"belowRect={belowRect}");

                Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedDrawers =
                    new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
                // Debug.Log($"allSaintsAttributes={allSaintsAttributes.Count}");
                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    // Debug.Log($"get instance {eachAttribute}: {drawerInstance}");
                    // ReSharper disable once InvertIf
                    if (drawerInstance.WillDrawBelow(property, eachAttributeWithIndex.SaintsAttribute, fieldInfo, parent))
                    {
                        if (!groupedDrawers.TryGetValue(eachAttributeWithIndex.SaintsAttribute.GroupBy,
                                out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                            groupedDrawers[eachAttributeWithIndex.SaintsAttribute.GroupBy] = currentGroup;
                        }

                        currentGroup.Add((drawerInstance, eachAttributeWithIndex.SaintsAttribute));
                        // _usedDrawerTypes.Add(eachDrawer[0]);
                        // UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                    }
                }

                foreach (KeyValuePair<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>
                             // ReSharper disable once UseDeconstruction
                             groupedDrawerInfo in groupedDrawers)
                {
                    string groupBy = groupedDrawerInfo.Key;
                    List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfo =
                        groupedDrawerInfo.Value;
                    // Debug.Log($"draw below: {groupBy}/{bugFixCopyLabel.text}/{label.text}");
                    if (groupBy == "")
                    {
                        foreach ((SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) in drawerInfo)
                        {
                            belowRect = drawerInstance.DrawBelow(belowRect, property, bugFixCopyLabel, eachAttribute, fieldInfo, parent);
                        }
                    }
                    else
                    {
                        float totalWidth = belowRect.width;
                        float eachWidth = totalWidth / drawerInfo.Count;
                        float height = 0;
                        for (int index = 0; index < drawerInfo.Count; index++)
                        {
                            (SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) = drawerInfo[index];
                            Rect eachRect = new Rect(belowRect)
                            {
                                x = belowRect.x + eachWidth * index,
                                width = eachWidth,
                            };
                            Rect leftRect =
                                drawerInstance.DrawBelow(eachRect, property, bugFixCopyLabel, eachAttribute, fieldInfo, parent);
                            height = Mathf.Max(height, leftRect.y - eachRect.y);
                        }

                        // belowRect.height = height;
                        belowRect = new Rect(belowRect)
                        {
                            y = belowRect.y + height,
                            height = belowRect.height - height,
                        };
                    }
                }

                #endregion

                // Debug.Log($"reset {property.propertyPath}=false");
                // PropertyPathToShared[property.propertyPath].changed = false;
                // SetValueChanged(property, false);

                // Debug.Log($"OnGui End: {SepTitleAttributeDrawer.drawCounter}");
            }

            foreach (SaintsWithIndex saintsWithIndex in allSaintsAttributes)
            {
                GetOrCreateSaintsDrawer(saintsWithIndex).OnPropertyEndImGui(property, label, saintsWithIndex.SaintsAttribute, saintsWithIndex.Index, onGUIPayload, fieldInfo, parent);
            }
        }

        #endregion

        #endregion

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private Type GetFirstSaintsDrawerType(Type attributeType)
        {
            // Debug.Log(attributeType);
            // Debug.Log(string.Join(",", _propertyAttributeToDrawers.Keys));

            if (!PropertyAttributeToPropertyDrawers.TryGetValue(attributeType,
                    out IReadOnlyList<(bool isSaints, Type drawerType)> eachDrawer))
            {
                return null;
            }
            // Debug.Log($"{attributeType}/{eachDrawer.Count}");

            (bool isSaints, Type drawerType) = eachDrawer.FirstOrDefault(each => each.isSaints);

            return isSaints ? drawerType : null;
        }

        private SaintsPropertyDrawer GetOrCreateSaintsDrawer(SaintsWithIndex saintsAttributeWithIndex)
        {
            if (_cachedDrawer.TryGetValue(saintsAttributeWithIndex, out SaintsPropertyDrawer drawer))
            {
                return drawer;
            }

            // Debug.Log($"create new drawer for {saintsAttributeWithIndex.SaintsAttribute}[{saintsAttributeWithIndex.Index}]");
            // Type drawerType = PropertyAttributeToDrawers[saintsAttributeWithIndex.SaintsAttribute.GetType()].First(each => each.isSaints).drawerType;
            return _cachedDrawer[saintsAttributeWithIndex] = GetOrCreateSaintsDrawerByAttr(saintsAttributeWithIndex.SaintsAttribute);
        }

        private static SaintsPropertyDrawer GetOrCreateSaintsDrawerByAttr(ISaintsAttribute saintsAttribute)
        {
            Type drawerType = PropertyAttributeToPropertyDrawers[saintsAttribute.GetType()].First(each => each.isSaints).drawerType;
            return (SaintsPropertyDrawer)Activator.CreateInstance(drawerType);
        }

        protected void DefaultDrawer(Rect position, SerializedProperty property, GUIContent label, FieldInfo info)
        {
            // // this works nice
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw!.Invoke(null, new object[] { position, property, label });

            // // not work when only my custom dec
            // // Getting the field type this way assumes that the property instance is not a managed reference (with a SerializeReference attribute); if it was, it should be retrieved in a different way:
            // Type fieldType = fieldInfo.FieldType;
            //
            // Type propertyDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor")
            //     .GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            //     .Invoke(null, new object[] { fieldType });
            //
            // PropertyDrawer propertyDrawer = null;
            // if (typeof(PropertyDrawer).IsAssignableFrom(propertyDrawerType))
            //     propertyDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            //
            // if (propertyDrawer != null)
            // {
            //     typeof(PropertyDrawer)
            //         .GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            //         .SetValue(propertyDrawer, fieldInfo);
            // }

            // // ... just a much simple way?
            // EditorGUI.PropertyField(position, property, label, true);

            // OK this should deal everything

            // this is no longer needed because PropertyField will handle this

//             IEnumerable<PropertyAttribute> allOtherAttributes = SerializedUtils
//                 .GetAttributesAndDirectParent<PropertyAttribute>(property)
//                 .attributes
//                 .Where(each => !(each is ISaintsAttribute));
//             foreach (PropertyAttribute propertyAttribute in allOtherAttributes)
//             {
//                 // ReSharper disable once InvertIf
//                 if(PropertyAttributeToDrawers.TryGetValue(propertyAttribute.GetType(), out IReadOnlyList<(bool isSaints, Type drawerType)> eachDrawer))
//                 {
//                     (bool _, Type drawerType) = eachDrawer.FirstOrDefault(each => !each.isSaints);
//                     // SaintsPropertyDrawer drawerInstance = GetOrCreateDrawerInfo(drawerType);
//                     // ReSharper disable once InvertIf
//                     if(drawerType != null)
//                     {
//                         if (!_cachedOtherDrawer.TryGetValue(drawerType, out PropertyDrawer drawerInstance))
//                         {
//                             _cachedOtherDrawer[drawerType] =
//                                 drawerInstance = (PropertyDrawer)Activator.CreateInstance(drawerType);
//                         }
//
//                         FieldInfo drawerFieldInfo = drawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
//                         Debug.Assert(drawerFieldInfo != null);
//                         drawerFieldInfo.SetValue(drawerInstance, propertyAttribute);
//                         // drawerInstance.attribute = propertyAttribute;
//
//                         // UnityEditor.RangeDrawer
//                         // Debug.Log($"fallback drawerInstance={drawerInstance} for {propertyAttribute}");
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//                         Debug.Log($"drawerInstance {drawerInstance}={label?.text.Length}");
// #endif
//                         drawerInstance.OnGUI(position, property, label ?? GUIContent.none);
//                         // Debug.Log($"finished drawerInstance={drawerInstance}");
//                         return;
//                     }
//                 }
//             }

            // fallback to pure unity one (unity default attribute not included)
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw!.Invoke(null, new object[] { position, property, GUIContent.none });
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"use unity draw: {property.propertyType}");
#endif
            UnityDraw(position, property, label, info);

            // EditorGUI.PropertyField(position, property, GUIContent.none, true);
            // if (property.propertyType == SerializedPropertyType.Generic)
            // {
            //     EditorGUI.PropertyField(position, property, GUIContent.none, true);
            // }
            // else
            // {
            //     UnityDraw(position, property, GUIContent.none);
            // }
        }

        private static void UnityDraw(Rect position, SerializedProperty property, GUIContent label, FieldInfo fieldInfo)
        {
            using (new InsideSaintsFieldScoop(SubDrawCounter, InsideSaintsFieldScoop.MakeKey(property)))
            {
#if UNITY_2022_1_OR_NEWER
                Type dec = fieldInfo.GetCustomAttributes<PropertyAttribute>(true)
                    .Select(propertyAttribute =>
                    {
                        // Debug.Log(propertyAttribute.GetType());
                        Type results = PropertyAttributeToDecoratorDrawers.TryGetValue(propertyAttribute.GetType(),
                            out IReadOnlyList<Type> eachDrawers)
                            ? eachDrawers[0]
                            : null;

                        // Debug.Log($"Found {results}");

                        return results;
                    })
                    .FirstOrDefault(each => each?.IsSubclassOf(typeof(DecoratorDrawer)) ?? false);

                // Debug.Log(dec);

                if (dec != null && ImGuiRemoveDecDraw(position, property, label))
                {
                    return;
                }
#endif

                // // Debug.Log($"UnityDraw: `{property.displayName}`");
                //
                // var assembly = AppDomain.CurrentDomain.GetAssemblies()
                //     .First(each => each.GetName().Name == "UnityEditor");
                //
                // Type scriptAttributeUtilityType = assembly.GetType("UnityEditor.ScriptAttributeUtility");
                // MethodInfo getHandlerMethod = scriptAttributeUtilityType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
                // // Debug.Log(getHandlerMethod);
                // object[] parameters = new object[] { property };
                // object handler = getHandlerMethod.Invoke(null, parameters);
                //
                // Debug.Log(handler);
                // Type handlerType  = assembly.GetType("UnityEditor.PropertyHandler");
                // FieldInfo decoratorDrawersField = handlerType.GetField("m_DecoratorDrawers", BindingFlags.NonPublic | BindingFlags.Instance);
                // decoratorDrawersField.SetValue(handler, null);
                //
                // object[] methodArgs = new object[]
                // {
                //     position,  // position
                //     property, // property
                //     label, // label
                //     true, // includeChildren
                // };
                // MethodInfo methodInfo = handlerType.GetMethod("OnGUI");
                // bool result = (bool)methodInfo.Invoke(handler, methodArgs);
                // Debug.Log(result);

                EditorGUI.PropertyField(position, property, label, true);
                // Debug.Log($"UnityDraw done, isSub={isSubDrawer}");
            }
            // Debug.Log($"UnityDraw exit, isSub={isSubDrawer}");
        }

#if UNITY_2022_1_OR_NEWER
        private static bool ImGuiRemoveDecDraw(Rect position, SerializedProperty property, GUIContent label)
        {
            Assembly assembly = UnityEditorAssemble;
            if (assembly == null)
            {
                return false;
            }

            Type scriptAttributeUtilityType = assembly.GetType("UnityEditor.ScriptAttributeUtility");
            if (scriptAttributeUtilityType == null)
            {
                return false;
            }
            MethodInfo getHandlerMethod = scriptAttributeUtilityType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
            if (getHandlerMethod == null)
            {
                return false;
            }

            // Debug.Log(getHandlerMethod);
            object[] parameters = { property };
            object handler = getHandlerMethod.Invoke(null, parameters);
            if (handler == null)
            {
                return false;
            }

            // Debug.Log(handler);
            Type handlerType  = assembly.GetType("UnityEditor.PropertyHandler");
            if (handlerType == null)
            {
                return false;
            }
            FieldInfo decoratorDrawersField = handlerType.GetField("m_DecoratorDrawers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (decoratorDrawersField == null)
            {
                return false;
            }

            decoratorDrawersField.SetValue(handler, null);

            object[] methodArgs = {
                position,  // position
                property, // property
                label, // label
                true, // includeChildren
            };
            MethodInfo methodInfo = handlerType.GetMethod("OnGUI");
            if (methodInfo == null)
            {
                return false;
            }
            // bool result = (bool)methodInfo.Invoke(handler, methodArgs);
            // Debug.Log(result);
            methodInfo.Invoke(handler, methodArgs);

            return true;
        }
#endif

        // public abstract void OnSaintsGUI(Rect position, SerializedProperty property, GUIContent label);
        // protected virtual (bool isActive, Rect position) DrawAbove(Rect position, SerializedProperty property,
        //     GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     return (false, position);
        // }

        protected virtual bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return false;
        }

        protected virtual Rect DrawAboveImGui(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            return position;
        }

        #region UIToolkit

#if UNITY_2021_3_OR_NEWER


        private static string NameSaintsPropertyDrawerRoot(SerializedProperty property) =>
            $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}__SaintsFieldRoot";

        // private static VisualElement GetFirstAncestorName(VisualElement element, string name)
        // {
        //     if (element == null)
        //         return null;
        //
        //     if (element.name == name)
        //         return element;
        //
        //     return GetFirstAncestorName(element.parent, name);
        // }

        private static IEnumerable<VisualElement> FindParentClass(VisualElement element, string className)
        {
            if(element == null)
            {
                yield break;
            }

            if(element.ClassListContains(className))
            {
                yield return element;
            }

            foreach (VisualElement each in FindParentClass(element.parent, className))
            {
                yield return each;
            }
        }

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

        private void OnAwakeUiToolKitInternal(SerializedProperty property, VisualElement containerElement,
            object parent, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"On Awake");
#endif
            // ReSharper disable once ConvertToLocalFunction
            Action<object> onValueChangedCallback = null;
            onValueChangedCallback = obj =>
            {
                foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
                {
                    saintsPropertyInfo.Drawer.OnValueChanged(
                        property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement,
                        fieldInfo, parent,
                        onValueChangedCallback,
                        obj);
                }
            };

            PropertyField fallbackField = containerElement.Q<PropertyField>(className: SaintsFieldFallbackClass);

            if(fallbackField != null)
            {
                // containerElement.visible = true;

                List<VisualElement> parentRoots = FindParentClass(containerElement, NameSaintsPropertyDrawerRoot(property)).ToList();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"usingFallbackField, parentRoots={parentRoots.Count}, {saintsPropertyDrawers.Count}");
#endif
                if (parentRoots.Count != saintsPropertyDrawers.Count)
                {
                    return;
                }

                // ReSharper disable once UseIndexFromEndExpression
                VisualElement topRoot = parentRoots[parentRoots.Count - 1];

                // PropertyField thisPropField = containerElement.Q<PropertyField>(className: SaintsFieldFallbackClass);

                // var container = thisPropField.Query<VisualElement>(className: "unity-decorator-drawers-container").ToList();
                // Debug.Log($"container={container.Count}");

                // thisPropField.styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/UnityLabelTransparent.uss"));

                // really... this delay is not predictable
                containerElement.schedule.Execute(() =>
                {
                    // var container = thisPropField.Query<VisualElement>(className: "unity-decorator-drawers-container").ToList();
                    // Debug.Log($"container={container.Count}");
                    fallbackField.Query<VisualElement>(className: "unity-decorator-drawers-container").ForEach(each => each.RemoveFromHierarchy());
#if !SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
                    Label label = fallbackField.Q<Label>(className: "unity-label");
                    if (label != null)
                    {
                        UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
                    }
#endif

                    OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers);
                });

                topRoot.Clear();
                topRoot.Add(containerElement);

                // thisPropField.Bind(property.serializedObject);
                fallbackField.Unbind();
                fallbackField.BindProperty(property.serializedObject);
                fallbackField.RegisterValueChangeCallback(evt =>
                {
                    SerializedProperty prop = evt.changedProperty;
                    // Debug.Log($"changed: {prop.propertyPath}; {prop.propertyPath}; {prop.}");
                    if(SerializedProperty.EqualContents(prop, property))
                    {
                        int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                        object rawValue = fieldInfo.GetValue(parent);
                        object curValue = arrayIndex == -1
                            ? rawValue
                            : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);
                        onValueChangedCallback(curValue);
                    }
                });
            }
            else
            {
                OnAwakeReady(property, containerElement, parent, onValueChangedCallback, saintsPropertyDrawers);
            }


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

#if !SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
            Label label = containerElement.Q<PropertyField>(className: SaintsFieldFallbackClass)?.Q<Label>(className: "unity-label");
            if (label != null)
            {
                // UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
                label.schedule.Execute(() => UIToolkitUtils.FixLabelWidthUIToolkit(label));
            }
#endif

            foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            {
                saintsPropertyInfo.Drawer.OnAwakeUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, onValueChangedCallback, fieldInfo, parent);
            }

            foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            {
                saintsPropertyInfo.Drawer.OnStartUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement, onValueChangedCallback, fieldInfo, parent);
            }

            // containerElement.schedule.Execute(() => OnUpdateUiToolKitInternal(property, containerElement, parent, saintsPropertyDrawers));
            OnUpdateUiToolKitInternal(property, containerElement, parent, saintsPropertyDrawers, onValueChangedCallback, fieldInfo);
        }

        private static void OnUpdateUiToolKitInternal(SerializedProperty property, VisualElement container,
            object parent,
            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, Action<object> onValueChangedCallback,
            FieldInfo info
        )
        {
            foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            {
                saintsPropertyInfo.Drawer.OnUpdateUIToolkit(property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, container, onValueChangedCallback, info, parent);
            }

            container.parent.schedule.Execute(() => OnUpdateUiToolKitInternal(property, container, parent, saintsPropertyDrawers, onValueChangedCallback, info)).StartingIn(100);
        }

        protected virtual void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
        }

        protected virtual void OnStartUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
        }

        protected virtual void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
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

        protected static void OnLabelStateChangedUIToolkit(SerializedProperty property, VisualElement container,
            string toLabel, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried,
            RichTextDrawer richTextDrawer)
        {
            VisualElement saintsLabelField = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            object saintsLabelFieldDrawerData = saintsLabelField.userData;

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
#endif

        #endregion

        // <0 means not used
        protected virtual float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return -1f;
        }


        protected virtual float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return 0;
        }

        protected virtual bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            return false;
        }

        protected virtual bool WillDrawLabel(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return false;
        }

        protected virtual void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            // return false;
        }

        protected virtual void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
        }

        protected virtual bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return false;
        }

        protected virtual bool DrawOverlay(Rect position,
            SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabel, FieldInfo info, object parent)
        {
            return false;
        }

        protected virtual Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return position;
        }

        protected virtual void OnPropertyEndImGui(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
        }

        private bool _mouseHold;
        // private Vector2 _labelClickedMousePos = new Vector2(-1, -1);

        protected static void ClickFocus(Rect position, string focusName)
        {
            Event e = Event.current;
            // ReSharper disable once InvertIf
            if (e.isMouse && e.button == 0)
            {
                if(position.Contains(e.mousePosition))
                {
                    GUI.FocusControl(focusName);
                }
            }
        }


        //
        // private void LabelMouseProcess(Rect position, SerializedProperty property, string focusName)
        // {
        //     Event e = Event.current;
        //     // if (e.isMouse && e.type == EventType.MouseDown)
        //     // {
        //     //     _labelClickedMousePos = e.mousePosition;
        //     // }
        //
        //     if (e.isMouse && e.button == 0)
        //     {
        //         if(!_mouseHold && position.Contains(e.mousePosition))
        //         {
        //             // Debug.Log($"start hold");
        //             _mouseHold = true;
        //             // e.Use();
        //             // Debug.Log($"focus {_fieldControlName}");
        //             GUI.FocusControl(focusName);
        //         }
        //     }
        //
        //     if (e.type == EventType.MouseUp)
        //     {
        //         _mouseHold = false;
        //         // _labelClickedMousePos = new Vector2(-1, -1);
        //     }
        //
        //     if (property.propertyType == SerializedPropertyType.Integer ||
        //         property.propertyType == SerializedPropertyType.Float)
        //     {
        //         EditorGUIUtility.AddCursorRect(position, MouseCursor.SlideArrow);
        //         if (e.isMouse && e.button == 0
        //                       && _mouseHold
        //             // && position.Contains(e.mousePosition)
        //            )
        //         {
        //             int xOffset = Mathf.RoundToInt(e.delta.x);
        //             // if(xOffset)
        //             // Debug.Log(xOffset);
        //             if (xOffset != 0)
        //             {
        //                 if (property.propertyType == SerializedPropertyType.Float)
        //                 {
        //                     property.floatValue = (float)(Math.Truncate((property.floatValue + xOffset * 0.03d) * 100) / 100d);
        //                 }
        //                 else
        //                 {
        //                     property.intValue += xOffset;
        //                 }
        //                 // Debug.Log($"valueChange=true");
        //                 _valueChange = true;
        //             }
        //         }
        //     }
        // }
    }
}
