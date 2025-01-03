using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    public partial class SaintsPropertyDrawer
    {
        #region IMGUI Drawer

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
            // Debug.Log($"{position.width}/{Event.current.type}");
            // Debug.Log($"OnGui Start: {SepTitleAttributeDrawer.drawCounter}");
            // this is so weird... because of Unity's repaint, layout etc.
            if(position.width - 1 > Mathf.Epsilon && Event.current.type == EventType.Repaint)
            {
                _filedWidthCache = position.width;
            }
            // Debug.Log($"OnGUI: pos={position}");

            if (SubDrawCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideCount) && insideCount > 0)
            {
                // Debug.Log($"capture sub drawer `{property.displayName}`:{property.propertyPath}@{insideCount}");
                // EditorGUI.PropertyField(position, property, label, true);
                UnityDraw(position, property, label, fieldInfo);
                return;
            }

            OnGUIPayload onGUIPayload = new OnGUIPayload();

            (ISaintsAttribute[] iSaintsAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);

            if (parent == null)
            {
                Debug.LogWarning($"Property {property.propertyPath} disposed unexpectedly.");
                return;
            }

            IReadOnlyList<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
                .Select((each, index) => new SaintsWithIndex
                {
                    SaintsAttribute = each,
                    Index = index,
                })
                .ToArray();

            // Debug.Log($"Saints: {property.displayName} found {allSaintsAttributes.Count}");

            if (!GetVisibility(property, allSaintsAttributes.Where(each => each.SaintsAttribute is ShowIfAttribute), parent))
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
                        else if(hasTextArea)
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
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    float curWidth =
                        drawerInstance.GetPostFieldWidth(fieldUseRectWithPost, property, GUIContent.none,
                            eachAttributeWithIndex.SaintsAttribute, eachAttributeWithIndex.Index, onGUIPayload, fieldInfo, parent);
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

                Dictionary<string, List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>> groupedDrawers =
                    new Dictionary<string, List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>>();
                // Debug.Log($"allSaintsAttributes={allSaintsAttributes.Count}");
                foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                    // Debug.Log($"get instance {eachAttribute}: {drawerInstance}");
                    // ReSharper disable once InvertIf
                    if (drawerInstance.WillDrawBelow(property, eachAttributeWithIndex.SaintsAttribute, eachAttributeWithIndex.Index, fieldInfo, parent))
                    {
                        if (!groupedDrawers.TryGetValue(eachAttributeWithIndex.SaintsAttribute.GroupBy,
                                out List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, SaintsWithIndex saintsWithIndex)>();
                            groupedDrawers[eachAttributeWithIndex.SaintsAttribute.GroupBy] = currentGroup;
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
                            belowRect = drawerInstance.DrawBelow(belowRect, property, bugFixCopyLabel, saintsWithIndex.SaintsAttribute, saintsWithIndex.Index, fieldInfo, parent);
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
                                drawerInstance.DrawBelow(eachRect, property, bugFixCopyLabel, saintsWithIndex.SaintsAttribute, saintsWithIndex.Index, fieldInfo, parent);
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
            GUIContent label, ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return position;
        }

        protected virtual void OnPropertyEndImGui(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
                if(position.Contains(e.mousePosition))
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
    }
}
