using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    public partial class SaintsPropertyDrawer
    {
        protected virtual void OnDisposeIMGUI()
        {
        }

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
            if (Array.IndexOf(Selection.objects, _imGuiObject) == -1)
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

        private static readonly Dictionary<UnityEngine.Object, Dictionary<string, Action>> InspectingTargetCallback =
            new Dictionary<UnityEngine.Object, Dictionary<string, Action>>();

        private static bool _inspectingWatchingImGui;

        private static void EnsureInsectingTargetDisposer()
        {
            if (_inspectingWatchingImGui)
            {
                return;
            }

            _inspectingWatchingImGui = true;
            Selection.selectionChanged += OnInspectingChangedImGui;
        }

        private static void OnInspectingChangedImGui()
        {
            List<UnityEngine.Object> toRemove = new List<UnityEngine.Object>();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (KeyValuePair<UnityEngine.Object, Dictionary<string, Action>> inspectingKv in InspectingTargetCallback)
            {
                // ReSharper disable once InvertIf
                if (Array.IndexOf(Selection.objects, inspectingKv.Key) == -1) // disposed
                {
                    foreach (KeyValuePair<string, Action> r in inspectingKv.Value)
                    {
                        r.Value.Invoke();
                    }

                    toRemove.Add(inspectingKv.Key);
                }
            }

            foreach (UnityEngine.Object each in toRemove)
            {
                // Debug.Log($"Remove callback for {each}");
                InspectingTargetCallback.Remove(each);
            }
        }

        protected static void NoLongerInspectingWatch(UnityEngine.Object target, string callbackKey, Action callback)
        {
            if (!InspectingTargetCallback.TryGetValue(target, out Dictionary<string, Action> callbacks))
            {
                // Debug.Log($"Add callback set for {target}");

                InspectingTargetCallback[target] = callbacks = new Dictionary<string, Action>();
            }

            // Debug.Log($"Add callback for {target}: {callback}");

            callbacks[callbackKey] = callback;
            EnsureInsectingTargetDisposer();
        }

        #endregion

        private bool GetVisibility(SerializedProperty property, IEnumerable<SaintsWithIndex> saintsAttributeWithIndexes,
            object parent)
        {
            List<bool> showAndResults = new List<bool>();
            foreach (SaintsWithIndex saintsAttributeWithIndex in saintsAttributeWithIndexes)
            {
                if (saintsAttributeWithIndex.SaintsAttribute is ShowIfAttribute showIfAttribute)
                {
                    SaintsPropertyDrawer drawer = GetOrCreateSaintsDrawer(saintsAttributeWithIndex);
                    showAndResults.Add(drawer.GetThisDecoratorVisibility(showIfAttribute, property, fieldInfo, parent));
                }
            }
            // Debug.Log($"visibility={string.Join(", ", showAndResults)}");

            return showAndResults.Count == 0 || showAndResults.Any(each => each);
        }

        #region IMGUI Drawer

        #region GetPropertyHeight

        private float _filedWidthCache = -1;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // if (IsSubDrawer)
            // {
            //     return EditorGUI.GetPropertyHeight(property, label);
            // }
            // Debug.Log($"GetPropertyHeight/{this}");

            if (SubDrawCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideDrawCount) &&
                insideDrawCount > 0)
            {
                // Debug.Log($"Sub Draw GetPropertyHeight/{this}");
                // return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
                return GetPropertyHeightFallback(property, label, fieldInfo, GetPreferredLabel(property));
            }

            if (SubGetHeightCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property),
                    out int insideGetHeightCount) && insideGetHeightCount > 0)
            {
                // Debug.Log($"Sub GetHeight GetPropertyHeight/{this}");
                // return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
                return GetPropertyHeightFallback(property, label, fieldInfo, GetPreferredLabel(property));
            }

            (PropertyAttribute[] allAttributes, object parent) =
                SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);

            if (parent == null)
            {
                Debug.LogWarning($"Property {property.propertyPath} disposed unexpectedly.");
                return 0;
            }

            // (ISaintsAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);
            List<SaintsWithIndex> saintsAttributeWithIndexes = allAttributes
                .OfType<ISaintsAttribute>()
                // .Where(each => !(each is VisibilityAttribute))
                .Select((each, index) => new SaintsWithIndex(each, index))
                .ToList();

            Dictionary<SaintsWithIndex, SaintsPropertyDrawer> usedAttributes = saintsAttributeWithIndexes
                .ToDictionary(each => each, GetOrCreateSaintsDrawer);

            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer)[] filedOrLabel = usedAttributes
                .Where(each =>
                    each.Key.SaintsAttribute.AttributeType == SaintsAttributeType.Field
                    // ReSharper disable once MergeIntoLogicalPattern
                    || each.Key.SaintsAttribute.AttributeType == SaintsAttributeType.Label)
                .Select(each => (IsaintsAttribute: each.Key.SaintsAttribute, each.Value))
                .ToArray();

            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer) fieldFound =
                filedOrLabel.FirstOrDefault(each => each.iSaintsAttribute.AttributeType == SaintsAttributeType.Field);


            if (UseCreateFieldIMGUI && fieldFound.iSaintsAttribute is null)
            {
                SaintsWithIndex thisFake = new SaintsWithIndex(null, -1);
                saintsAttributeWithIndexes.Insert(0, thisFake);
                usedAttributes[thisFake] = this;
            }

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

            // float defaultHeight = base.GetPropertyHeight(property, label);


            // foreach ((ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer) in filedOrLabel)
            // {
            //     Debug.Log($"GetHeight found {iSaintsAttribute} {iSaintsAttribute.AttributeType} {drawer}");
            // }

            // SaintsPropertyDrawer[] usedDrawerInfos = _usedDrawerTypes.Select(each => _cachedDrawer[each]).ToArray();
            // SaintsPropertyDrawer[] fieldInfos = usedDrawerInfos.Where(each => each.AttributeType is SaintsAttributeType.Field or SaintsAttributeType.Label).ToArray();

            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer) labelFound =
                filedOrLabel.FirstOrDefault(each => each.iSaintsAttribute?.AttributeType == SaintsAttributeType.Label);

            // Debug.Log($"labelFound.iSaintsAttribute={labelFound.iSaintsAttribute}");
            bool hasSaintsLabel = labelFound.iSaintsAttribute != null;
            // Debug.Log($"hasSaintsLabel={hasSaintsLabel}");

            SaintsPropertyDrawer labelDrawer = labelFound.drawer;

            bool saintsDrawNoLabel = hasSaintsLabel &&
                                     !labelDrawer.WillDrawLabel(property, labelFound.iSaintsAttribute, fieldInfo,
                                         parent);

            bool hasSaintsField = fieldFound.iSaintsAttribute != null || UseCreateFieldIMGUI;

            bool disabledLabelField = label.text == "" || saintsDrawNoLabel;
            // Debug.Log(disabledLabelField);

            float fullWidth = _filedWidthCache - 1 <= Mathf.Epsilon
                ? EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15
                : _filedWidthCache;

            float labelBasicHeight = saintsDrawNoLabel ? 0f : EditorGUIUtility.singleLineHeight;
            float fieldBasicHeight;
            if (hasSaintsField)
            {
                SaintsPropertyDrawer drawer = fieldFound.drawer ?? this;
                fieldBasicHeight = drawer.GetFieldHeight(property, label, fullWidth, fieldFound.iSaintsAttribute,
                    fieldInfo,
                    !disabledLabelField, parent);
            }
            else
            {
                fieldBasicHeight = GetPropertyHeightFallback(property, label, fieldInfo, GetPreferredLabel(property));
            }

            // Debug.Log($"hasSaintsField={hasSaintsField}, labelBasicHeight={labelBasicHeight}, fieldBasicHeight={fieldBasicHeight}");
            _labelFieldBasicHeight = Mathf.Max(labelBasicHeight, fieldBasicHeight);

            float aboveHeight = 0;
            float belowHeight = 0;


            // Nah, Unity will give `EditorGUIUtility.currentViewWidth=0` on first render...
            // Let Drawer decide what to do then...
            // float fullWidth = 100;
            // Debug.Log($"fullWidth={fullWidth}, _filedWidthCache={_filedWidthCache}; EditorGUIUtility.currentViewWidth={EditorGUIUtility.currentViewWidth}, EditorGUI.indentLevel={EditorGUI.indentLevel}");

            // Debug.Log(usedAttributes.Count);

            foreach (IGrouping<string, KeyValuePair<SaintsWithIndex, SaintsPropertyDrawer>> grouped in
                     usedAttributes.ToLookup(each => each.Key.SaintsAttribute?.GroupBy ?? ""))
            {
                float eachWidth = grouped.Key == ""
                    ? fullWidth
                    : fullWidth / grouped.Count();

                IEnumerable<float> aboveHeights = grouped
                    .Select(each => each.Value.GetAboveExtraHeight(property, label, eachWidth, each.Key.SaintsAttribute,
                        each.Key.Index, fieldInfo, parent))
                    .Where(each => each > 0)
                    .DefaultIfEmpty(0);
                IEnumerable<float> belowHeights = grouped
                    .Select(each => each.Value.GetBelowExtraHeight(property, label, eachWidth, each.Key.SaintsAttribute,
                        each.Key.Index, fieldInfo, parent))
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

        private static float GetPropertyHeightFallback(SerializedProperty property, GUIContent label,
            FieldInfo fieldInfo, string preferredLabel)
        {
            (Attribute _, Type attributeDrawerType) = GetOtherAttributeDrawerType(fieldInfo);
            if (attributeDrawerType == null)
            {
                Type drawerType = FindTypeDrawerNonSaints(SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(fieldInfo.FieldType): fieldInfo.FieldType);
                if (drawerType != null)
                {
                    // type drawer has no attribute
                    PropertyDrawer drawerInstance = MakePropertyDrawer(drawerType, fieldInfo, null, preferredLabel);
                    if (drawerInstance != null)
                    {
                        return drawerInstance.GetPropertyHeight(property, label);
                    }
                }
            }

            // TODO: check, if it has dec, this value might be wrong
            using (new InsideSaintsFieldScoop(SubGetHeightCounter, InsideSaintsFieldScoop.MakeKey(property)))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
        }

        protected virtual float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
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
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return 0;
        }

        protected virtual float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return 0;
        }

        #endregion


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

        protected virtual bool UseCreateFieldIMGUI => false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Debug.Log($"{position.width}/{Event.current.type}");
            // Debug.Log($"OnGui Start: {SepTitleAttributeDrawer.drawCounter}");
            // this is so weird... because of Unity's repaint, layout etc.
            if (position.width - 1 > Mathf.Epsilon && Event.current.type == EventType.Repaint)
            {
                _filedWidthCache = position.width;
                // Debug.Log($"draw update _filedWidthCache={_filedWidthCache}");
            }
            // Debug.Log($"OnGUI: pos={position}");

            if (SubDrawCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideCount) &&
                insideCount > 0)
            {
                // Debug.Log($"capture sub drawer `{property.displayName}`:{property.propertyPath}@{insideCount}");
                // EditorGUI.PropertyField(position, property, label, true);
                UnityDraw(position, property, label, fieldInfo, GetPreferredLabel(property));
                return;
            }

            OnGUIPayload onGUIPayload = new OnGUIPayload();

            (PropertyAttribute[] allAttributes, object parent) =
                SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);
            ISaintsAttribute[] iSaintsAttributes = allAttributes.OfType<ISaintsAttribute>().ToArray();

            if (parent == null)
            {
                Debug.LogWarning($"Property {property.propertyPath} disposed unexpectedly.");
                return;
            }

            List<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
                .Select((each, index) => new SaintsWithIndex(each, index))
                .ToList();

            SaintsWithIndex fieldAttributeWithIndex =
                allSaintsAttributes.FirstOrDefault(each =>
                    each.SaintsAttribute.AttributeType == SaintsAttributeType.Field);

            bool useCreateFieldIMGUI = fieldAttributeWithIndex.SaintsAttribute is null && UseCreateFieldIMGUI;

            if (useCreateFieldIMGUI)
            {
                fieldAttributeWithIndex = new SaintsWithIndex(null, -1);
                allSaintsAttributes.Insert(0, fieldAttributeWithIndex);
            }

            // foreach (SaintsWithIndex saintsWithIndex in allSaintsAttributes)
            // {
            //     Debug.Log($"{saintsWithIndex.SaintsAttribute}/{saintsWithIndex.Index}");
            // }

            // Debug.Log($"Saints: {property.displayName} found {allSaintsAttributes.Count}");

            if (!GetVisibility(property, allSaintsAttributes.Where(each => each.SaintsAttribute is ShowIfAttribute),
                    parent))
            {
                return;
            }

            SaintsWithIndex labelAttributeWithIndex =
                allSaintsAttributes.FirstOrDefault(each =>
                    each.SaintsAttribute?.AttributeType == SaintsAttributeType.Label);

            // _usedAttributes.Clear();

            using (new EditorGUI.PropertyScope(position, label, property))
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
                    SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.SaintsAttribute is null
                        ? this
                        : GetOrCreateSaintsDrawer(eachAttributeWithIndex);

                    // ReSharper disable once InvertIf
                    if (drawerInstance.WillDrawAbove(property, eachAttributeWithIndex.SaintsAttribute, fieldInfo,
                            parent))
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
                                drawerInstance.DrawAboveImGui(aboveRect, property, bugFixCopyLabel, eachAttribute,
                                    onGUIPayload, fieldInfo, parent);
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
                                drawerInstance.DrawAboveImGui(eachRect, property, bugFixCopyLabel, eachAttribute,
                                    onGUIPayload, fieldInfo, parent);
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
                    // Debug.Log($"{eachAttributeWithIndex.Index}/{eachAttributeWithIndex.SaintsAttribute}");
                    SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.SaintsAttribute is null
                        ? this
                        : GetOrCreateSaintsDrawer(eachAttributeWithIndex);
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
                    }
                }
                // Debug.Log("=====================================");

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
                        labelDrawerInstance.WillDrawLabel(property, labelAttributeWithIndex.SaintsAttribute, fieldInfo,
                            parent);
                    if (hasLabelSpace)
                    {
                        bool hasLeftToggle = false;
                        bool hasTextArea = false;
                        foreach (SaintsWithIndex saintsWithIndex in allSaintsAttributes)
                        {
                            if (saintsWithIndex.SaintsAttribute is LeftToggleAttribute)
                            {
                                hasLeftToggle = true;
                            }
                            else if (saintsWithIndex.SaintsAttribute is ResizableTextAreaAttribute)
                            {
                                hasTextArea = true;
                            }
                        }

                        const float leftToggleSpace = 18;
                        float useWidth = EditorGUIUtility.labelWidth - preLabelWidth;
                        float useX = fieldUseRectWithPost.x;

                        if (hasLeftToggle)
                        {
                            useWidth = fieldUseRectWithPost.width - preLabelWidth - leftToggleSpace;
                            useX = fieldUseRectWithPost.x + leftToggleSpace;
                        }
                        else if (hasTextArea)
                        {
                            useWidth = fieldUseRectWithPost.width - preLabelWidth;
                            useX = fieldUseRectWithPost.x;
                        }

                        labelDrawerInfo = new LabelDrawerInfo
                        {
                            labelDrawerInstance = labelDrawerInstance,
                            rect = new Rect(fieldUseRectWithPost)
                            {
                                x = useX,
                                width = useWidth,
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
                    SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.SaintsAttribute is null
                        ? this
                        : GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    float curWidth =
                        drawerInstance.GetPostFieldWidth(fieldUseRectWithPost, property, GUIContent.none,
                            eachAttributeWithIndex.SaintsAttribute, eachAttributeWithIndex.Index, onGUIPayload,
                            fieldInfo, parent);
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

                using (new AdaptLabelWidth())
                using (new ResetIndentScoop())
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    if (UseCreateFieldIMGUI && fieldDrawer == null)
                    {
                        DrawField(fieldUseRectNoPost, property, useGuiContent,
                            null, allAttributes, onGUIPayload, fieldInfo, parent);
                    }
                    else if (fieldDrawer == null)
                    {
                        DefaultDrawer(fieldUseRectNoPost, property, useGuiContent, fieldInfo);
                    }
                    else
                    {
                        // Debug.Log(fieldAttribute);
                        SaintsPropertyDrawer fieldDrawerInstance = GetOrCreateSaintsDrawer(fieldAttributeWithIndex);
                        // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                        // GUI.SetNextControlName(_fieldControlName);
                        fieldDrawerInstance.DrawField(fieldUseRectNoPost, property, useGuiContent,
                            fieldAttributeWithIndex.SaintsAttribute, allAttributes, onGUIPayload, fieldInfo, parent);
                        // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                        // UsedAttributesTryAdd(fieldAttributeWithIndex, fieldDrawerInstance);
                    }

                    // if (changed.changed && fieldDrawer == null)
                    // Debug.Log($"changed.changed={changed.changed}");
                    if (changed.changed && !onGUIPayload.changed)
                    {
                        property.serializedObject.ApplyModifiedProperties();

                        (string error, int _, object value) = Util.GetValue(property, fieldInfo, parent);

                        if (error == "")
                        {
                            onGUIPayload.SetValue(value);
                        }
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
                    drawer.DrawPostFieldImGui(eachRect, fieldUseRectWithPost, property, bugFixCopyLabel,
                        attributeWithIndex.SaintsAttribute,
                        attributeWithIndex.Index,
                        allAttributes,
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
                    SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.SaintsAttribute is null
                        ? this
                        : GetOrCreateSaintsDrawer(eachAttributeWithIndex);
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

                Dictionary<string, List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>>
                    groupedDrawers =
                        new Dictionary<string, List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>>();
                // Debug.Log($"allSaintsAttributes={allSaintsAttributes.Count}");
                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = eachAttributeWithIndex.SaintsAttribute is null
                        ? this
                        : GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    // Debug.Log($"get instance {eachAttribute}: {drawerInstance}");
                    // ReSharper disable once InvertIf
                    if (drawerInstance.WillDrawBelow(property, eachAttributeWithIndex.SaintsAttribute,
                            eachAttributeWithIndex.Index, fieldInfo, parent))
                    {
                        if (!groupedDrawers.TryGetValue(eachAttributeWithIndex.SaintsAttribute?.GroupBy ?? "",
                                out List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>();
                            groupedDrawers[eachAttributeWithIndex.SaintsAttribute?.GroupBy ?? ""] = currentGroup;
                        }

                        currentGroup.Add((drawerInstance, eachAttributeWithIndex));
                        // _usedDrawerTypes.Add(eachDrawer[0]);
                        // UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                    }
                }

                foreach (KeyValuePair<string, List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>>
                             // ReSharper disable once UseDeconstruction
                             groupedDrawerInfo in groupedDrawers)
                {
                    string groupBy = groupedDrawerInfo.Key;
                    List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)> drawerInfo =
                        groupedDrawerInfo.Value;
                    // Debug.Log($"draw below: {groupBy}/{bugFixCopyLabel.text}/{label.text}");
                    if (groupBy == "")
                    {
                        foreach ((SaintsPropertyDrawer drawerInstance, SaintsWithIndex saintsWithIndex) in drawerInfo)
                        {
                            belowRect = drawerInstance.DrawBelow(belowRect, property, bugFixCopyLabel,
                                saintsWithIndex.SaintsAttribute, saintsWithIndex.Index, allAttributes, onGUIPayload,
                                fieldInfo, parent);
                        }
                    }
                    else
                    {
                        float totalWidth = belowRect.width;
                        float eachWidth = totalWidth / drawerInfo.Count;
                        float height = 0;
                        for (int index = 0; index < drawerInfo.Count; index++)
                        {
                            (SaintsPropertyDrawer drawerInstance, SaintsWithIndex saintsWithIndex) = drawerInfo[index];
                            Rect eachRect = new Rect(belowRect)
                            {
                                x = belowRect.x + eachWidth * index,
                                width = eachWidth,
                            };
                            Rect leftRect =
                                drawerInstance.DrawBelow(eachRect, property, bugFixCopyLabel,
                                    saintsWithIndex.SaintsAttribute, saintsWithIndex.Index, allAttributes, onGUIPayload,
                                    fieldInfo, parent);
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
                SaintsPropertyDrawer drawer = saintsWithIndex.SaintsAttribute is null
                    ? this
                    : GetOrCreateSaintsDrawer(saintsWithIndex);
                drawer.OnPropertyEndImGui(property, label,
                    saintsWithIndex.SaintsAttribute, saintsWithIndex.Index, onGUIPayload, fieldInfo, parent);
            }
        }

        #endregion

        #region Callbacks

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

        // <0 means not used
        protected virtual float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return -1f;
        }


        protected virtual float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return 0;
        }

        protected virtual bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
        }

        protected virtual bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
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
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return position;
        }

        protected virtual void OnPropertyEndImGui(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
        }

        #endregion

        private bool _mouseHold;
        // private Vector2 _labelClickedMousePos = new Vector2(-1, -1);

        protected static void ClickFocus(Rect position, string focusName)
        {
            Event e = Event.current;
            // ReSharper disable once InvertIf
            if (e.isMouse && e.button == 0)
            {
                if (position.Contains(e.mousePosition))
                {
                    GUI.FocusControl(focusName);
                }
            }
        }

#if UNITY_2022_1_OR_NEWER
        private static bool ImGuiRemoveDecDraw(Rect position, SerializedProperty property, GUIContent label)
        {
            Assembly assembly = _unityEditorAssemble;
            if (assembly == null)
            {
                return false;
            }

            Type scriptAttributeUtilityType = assembly.GetType("UnityEditor.ScriptAttributeUtility");
            if (scriptAttributeUtilityType == null)
            {
                return false;
            }
            MethodInfo getHandlerMethod =
                scriptAttributeUtilityType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
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

            Type handlerType = assembly.GetType("UnityEditor.PropertyHandler");
            if (handlerType == null)
            {
                return false;
            }
            FieldInfo decoratorDrawersField =
                handlerType.GetField("m_DecoratorDrawers", BindingFlags.NonPublic | BindingFlags.Instance);
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
        protected void DefaultDrawer(Rect position, SerializedProperty property, GUIContent label, FieldInfo info)
        {
            // // this works nice
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw!.Invoke(null, new object[] { position, property, label });
            // return;

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

            // ... just a much simple way?
            // EditorGUI.PropertyField(position, property, label, false);
            // return;

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
            UnityDraw(position, property, label, info, GetPreferredLabel(property));

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

        private static void UnityDraw(Rect position, SerializedProperty property, GUIContent label, FieldInfo fieldInfo, string preferredLabel)
        {
            // Wait... it works now?
            (Attribute attributeInstance, Type attributeDrawerType) = GetOtherAttributeDrawerType(fieldInfo);

            if(attributeDrawerType != null)
            {
                PropertyDrawer propertyDrawerInstance =
                    MakePropertyDrawer(attributeDrawerType, fieldInfo, attributeInstance, preferredLabel);
                if (propertyDrawerInstance != null)
                {
                    propertyDrawerInstance.OnGUI(position, property, label);
                    return;
                }
            }
            else  // not attribute drawer, use type drawer
            {
                Type drawerType = FindTypeDrawerNonSaints(SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(fieldInfo.FieldType) : fieldInfo.FieldType);
                if (drawerType != null)
                {
                    // type drawer has no attribute
                    PropertyDrawer drawerInstance = MakePropertyDrawer(drawerType, fieldInfo, null, preferredLabel);
                    if (drawerInstance != null)
                    {
                        // drawerInstance.GetPropertyHeight(property, label);
                        drawerInstance.OnGUI(position, property, label);
                        return;
                    }
                }
            }

            InsideSaintsFieldScoop.PropertyKey key = InsideSaintsFieldScoop.MakeKey(property);
            using (new InsideSaintsFieldScoop(SubDrawCounter, key))
            {
                // this is no longer needed for no good reason. Need more investigation and testing
                // this code is used to prevent the decorator to be drawn everytime a fallback happens
                // the marco is not added by default
#if UNITY_2022_1_OR_NEWER && SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX
                Type dec = ReflectCache.GetCustomAttributes<PropertyAttribute>(fieldInfo)
                    .Select(propertyAttribute =>
                    {
                        // Debug.Log(propertyAttribute.GetType());
                        Type results = _propertyAttributeToDecoratorDrawers.TryGetValue(propertyAttribute.GetType(),
                            out IReadOnlyList<PropertyDrawerInfo> eachDrawers)
                            ? eachDrawers[0].DrawerType
                            : null;

                        // Debug.Log($"Found {results}");

                        return results;
                    })
                    .FirstOrDefault(each => each?.IsSubclassOf(typeof(DecoratorDrawer)) ?? false);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                Debug.Log($"get dec {dec} for {property.propertyPath}");
#endif

                ImGuiRemoveDecDraw(position, property, label);
                if (dec != null && ImGuiRemoveDecDraw(position, property, label))
                {
                    return;
                }
#endif

                try
                {
                    // this somehow not work...
                    EditorGUI.PropertyField(position, property, label, true);
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogError(e);
                }
                // Debug.Log($"UnityDraw done, isSub={isSubDrawer}");
            }
            // Debug.Log($"UnityDraw exit, isSub={isSubDrawer}");
        }
    }
}
