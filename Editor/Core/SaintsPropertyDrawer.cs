﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    // above
    // pre, label, field, post
    // below-
    public abstract class SaintsPropertyDrawer: PropertyDrawer
    {
        // public static bool IsSubDrawer = false;
        public static readonly Dictionary<InsideSaintsFieldScoop.PropertyKey, int> SubCounter = new Dictionary<InsideSaintsFieldScoop.PropertyKey, int>();

        private static readonly Dictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>> PropertyAttributeToDrawers =
            new Dictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>>();

        public class SharedInfo
        {
            public bool changed;
            public object parentTarget;
        }

        private static readonly Dictionary<string, SharedInfo> PropertyPathToShared = new Dictionary<string, SharedInfo>();

        // private IReadOnlyList<ISaintsAttribute> _allSaintsAttributes;
        // private SaintsPropertyDrawer _labelDrawer;
        // private SaintsPropertyDrawer _fieldDrawer;

        protected readonly string FieldControlName;

        private struct SaintsWithIndex
        {
            public ISaintsAttribute SaintsAttribute;
            // ReSharper disable once NotAccessedField.Local
            public int Index;
        }

        private readonly Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _cachedDrawer = new Dictionary<SaintsWithIndex, SaintsPropertyDrawer>();
        private readonly Dictionary<Type, PropertyDrawer> _cachedOtherDrawer = new Dictionary<Type, PropertyDrawer>();
        // private readonly HashSet<Type> _usedDrawerTypes = new HashSet<Type>();
        // private readonly Dictionary<ISaintsAttribute, >
        // private struct UsedAttributeInfo
        // {
        //     public Type DrawerType;
        //     public ISaintsAttribute Attribute;
        // }

        // private readonly List<UsedAttributeInfo> _usedAttributes = new List<UsedAttributeInfo>();
        private readonly Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _usedAttributes = new Dictionary<SaintsWithIndex, SaintsPropertyDrawer>();

        // private static readonly FieldDrawerConfigAttribute DefaultFieldDrawerConfigAttribute =
        //     new FieldDrawerConfigAttribute(FieldDrawerConfigAttribute.FieldDrawType.Inline, 0);

        private string _cachedPropPath;

        public SaintsPropertyDrawer()
        {
            // Debug.Log("new SaintsPropertyDrawer");
            // if (IsSubDrawer)
            // {
            //     return;
            // }

            FieldControlName = Guid.NewGuid().ToString();

            _usedAttributes.Clear();

            // _propertyAttributeToDrawers.Clear();

            // ReSharper disable once InvertIf
            if(PropertyAttributeToDrawers.Count == 0)
            {
                Dictionary<Type, HashSet<Type>> attrToDrawers = new Dictionary<Type, HashSet<Type>>();

                foreach (Assembly asb in AppDomain.CurrentDomain.GetAssemblies())
                {
                    List<Type> saintsSubDrawers = asb.GetTypes()
                        // .Where(type => type.IsSubclassOf(typeof(SaintsPropertyDrawer)))
                        .Where(type => type.IsSubclassOf(typeof(PropertyDrawer)))
                        .ToList();
                    foreach (Type saintsSubDrawer in saintsSubDrawers)
                    {
                        foreach (Type attr in saintsSubDrawer.GetCustomAttributes(typeof(CustomPropertyDrawer), true)
                                     .Select(each => (CustomPropertyDrawer)each)
                                     .Select(instance => typeof(CustomPropertyDrawer)
                                         .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                                         ?.GetValue(instance))
                                     .Where(each => each != null))
                        {
                            if (!attrToDrawers.TryGetValue(attr, out HashSet<Type> attrList))
                            {
                                attrToDrawers[attr] = attrList = new HashSet<Type>();
                            }

                            // Debug.Log($"Found drawer: {attr} -> {saintsSubDrawer}");

                            attrList.Add(saintsSubDrawer);
                        }
                    }
                }

                foreach (KeyValuePair<Type, HashSet<Type>> kv in attrToDrawers)
                {
                    PropertyAttributeToDrawers[kv.Key] = kv.Value
                        .Select(each => (each.IsSubclassOf(typeof(SaintsPropertyDrawer)), each))
                        .ToArray();
#if EXT_INSPECTOR_LOG
                    Debug.Log($"attr {kv.Key} has drawer(s) {string.Join(",", kv.Value)}");
#endif
                }
            }
        }

        ~SaintsPropertyDrawer()
        {
            if (!string.IsNullOrEmpty(_cachedPropPath) && PropertyPathToShared.ContainsKey(_cachedPropPath))
            {
                PropertyPathToShared.Remove(_cachedPropPath);
            }
        }

        // ~SaintsPropertyDrawer()
        // {
        //     PropertyAttributeToDrawers.Clear();
        // }

        private float _labelFieldBasicHeight = EditorGUIUtility.singleLineHeight;

        protected virtual (bool isForHide, bool orResult) GetAndVisibility(SerializedProperty property,
            ISaintsAttribute saintsAttribute)
        {
            return (false, true);
        }

        private bool GetVisibility(SerializedProperty property, IEnumerable<SaintsWithIndex> saintsAttributeWithIndexes)
        {
            List<bool> showAndResults = new List<bool>();
            // List<bool> hideAndResults = new List<bool>();
            // private SaintsPropertyDrawer GetOrCreateSaintsDrawer(SaintsWithIndex saintsAttributeWithIndex);

            string propPath = property.propertyPath;
            if(!PropertyPathToShared.ContainsKey(propPath))
            {
                PropertyPathToShared[propPath] = new SharedInfo
                {
                    parentTarget = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property).parent,
                };
            }

            foreach (SaintsWithIndex saintsAttributeWithIndex in saintsAttributeWithIndexes)
            {
                SaintsPropertyDrawer drawer = GetOrCreateSaintsDrawer(saintsAttributeWithIndex);
                (bool isForHide, bool andResult) = drawer.GetAndVisibility(property, saintsAttributeWithIndex.SaintsAttribute);
                if (isForHide)
                {
                    // Debug.Log($"hide or: {orResult}");
                    showAndResults.Add(!andResult);
                }
                else
                {
                    // Debug.Log($"show or: {orResult}");
                    showAndResults.Add(andResult);
                }
            }

            return showAndResults.Count == 0 || showAndResults.Any(each => each);

            // bool showResult = showAndResults.Count == 0
            //     ? true
            //     : showAndResults.Any(each => each);
            // bool hideResult = hideAndResults.Count == 0
            //     ? false
            //     : hideAndResults.Any(each => each);
            //
            // return showResult && !hideResult;

            // // bool showResult = showAndResults.Any(each => each);
            // // bool hideResult = hideAndResults.Any(each => each);
            // // return showResult && !hideResult;
            // if (hideAndResults.Count > 0 && hideAndResults.Any(each => each))
            // {
            //     return false;
            // }
            //
            // if (showAndResults.Count == 0)
            // {
            //     return true;
            // }
            //
            // return showAndResults.All(each => each);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // if (IsSubDrawer)
            // {
            //     return EditorGUI.GetPropertyHeight(property, label);
            // }

            if (SubCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideCount) && insideCount > 0)
            {
                return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
            }

            if (!GetVisibility(property, SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property).attributes
                    .Select((each, index) => new SaintsWithIndex
                    {
                        SaintsAttribute = each,
                        Index = index,
                    })
                    .Where(each => each.SaintsAttribute is VisibilityAttribute)))
            {
                return 0f;
            }

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
                                     !labelFound.drawer.WillDrawLabel(property, label, labelFound.iSaintsAttribute);

            bool hasSaintsField = fieldFound.iSaintsAttribute != null;

            bool disabledLabelField = label.text == "" || saintsDrawNoLabel;
            // Debug.Log(disabledLabelField);

            float labelBasicHeight = saintsDrawNoLabel? 0f: EditorGUIUtility.singleLineHeight;
            float fieldBasicHeight = hasSaintsField
                ? fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute,
                    !disabledLabelField)
                // : EditorGUIUtility.singleLineHeight;
                : EditorGUI.GetPropertyHeight(property, label, true);

            // Debug.Log($"hasSaintsField={hasSaintsField}, labelBasicHeight={labelBasicHeight}, fieldBasicHeight={fieldBasicHeight}");
            _labelFieldBasicHeight = Mathf.Max(labelBasicHeight, fieldBasicHeight);


            float aboveHeight = 0;
            float belowHeight = 0;

            float fullWidth = EditorGUIUtility.currentViewWidth;
            foreach (IGrouping<string, KeyValuePair<SaintsWithIndex, SaintsPropertyDrawer>> grouped in _usedAttributes.ToLookup(each => each.Key.SaintsAttribute.GroupBy))
            {
                float eachWidth = grouped.Key == ""
                    ? fullWidth
                    : fullWidth / grouped.Count();

                IEnumerable<float> aboveHeights =
                    grouped.Select(each => each.Value.GetAboveExtraHeight(property, label, eachWidth, each.Key.SaintsAttribute));
                IEnumerable<float> belowHeights =
                    grouped.Select(each => each.Value.GetBelowExtraHeight(property, label, eachWidth, each.Key.SaintsAttribute));

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

            return _labelFieldBasicHeight + aboveHeight + belowHeight;
        }

        protected virtual float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return 0;
        }

        // protected virtual float GetLabelHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     return 0;
        // }

        protected virtual float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        protected virtual float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        // private float _aboveUsedHeight;

        private void UsedAttributesTryAdd(SaintsWithIndex key, SaintsPropertyDrawer value)
        {
#if UNITY_2021_3_OR_NEWER
            _usedAttributes.TryAdd(key, value);
#else
            if (!_usedAttributes.TryGetValue(key, out SaintsPropertyDrawer _))
            {
                _usedAttributes[key] = value;
            }
#endif
        }

        // protected bool _valueChange { get; private set; }

        protected static void SetValueChanged(SerializedProperty property)
        {
            // Debug.LogWarning($"set {property.propertyPath}=true");
            PropertyPathToShared[property.propertyPath].changed = true;
        }

        protected object GetParentTarget(SerializedProperty property)
        {
            return PropertyPathToShared[property.propertyPath].parentTarget;
        }

        // protected object DirectParentObject { get; private set; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _cachedPropPath = property.propertyPath;

            if (!PropertyPathToShared.ContainsKey(property.propertyPath))
            {
                PropertyPathToShared[property.propertyPath] = new SharedInfo();
            }
            // Debug.Log($"OnGUI: {property.displayName} path {property.propertyPath}; obj={property.serializedObject.targetObject}");

            if (SubCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideCount) && insideCount > 0)
            {
                // Debug.Log($"capture sub drawer `{property.displayName}`:{property.propertyPath}@{insideCount}");
                // EditorGUI.PropertyField(position, property, label, true);
                UnityDraw(position, property, label);
                return;
            }

            (ISaintsAttribute[] iSaintsAttributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ISaintsAttribute>(property);
            PropertyPathToShared[property.propertyPath].parentTarget = parent;

            IReadOnlyList<SaintsWithIndex> allSaintsAttributes = iSaintsAttributes
                .Select((each, index) => new SaintsWithIndex
                {
                    SaintsAttribute = each,
                    Index = index,
                })
                .ToArray();

            // Debug.Log($"Saints: {property.displayName} found {allSaintsAttributes.Count}");

            if (!GetVisibility(property, allSaintsAttributes.Where(each => each.SaintsAttribute is VisibilityAttribute)))
            {
                return;
            }

            SaintsWithIndex labelAttributeWithIndex = allSaintsAttributes.FirstOrDefault(each => each.SaintsAttribute.AttributeType == SaintsAttributeType.Label);
            SaintsWithIndex fieldAttributeWithIndex = allSaintsAttributes.FirstOrDefault(each => each.SaintsAttribute.AttributeType == SaintsAttributeType.Field);

            _usedAttributes.Clear();

            using EditorGUI.PropertyScope propertyScope = new EditorGUI.PropertyScope(position, label, property);
            // GUIContent propertyScoopLabel = propertyScope.content;
            GUIContent bugFixCopyLabel = new GUIContent(label);

            // Debug.Log($"above: {label.text}");

            #region Above

            Rect aboveRect = EditorGUI.IndentedRect(position);

            Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedAboveDrawers =
                new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
            foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
            {
                SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);

                // ReSharper disable once InvertIf
                if (drawerInstance.WillDrawAbove(aboveRect, property, bugFixCopyLabel, eachAttributeWithIndex.SaintsAttribute))
                {
                    if (!groupedAboveDrawers.TryGetValue(eachAttributeWithIndex.SaintsAttribute.GroupBy,
                            out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                    {
                        currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                        groupedAboveDrawers[eachAttributeWithIndex.SaintsAttribute.GroupBy] = currentGroup;
                    }

                    currentGroup.Add((drawerInstance, eachAttributeWithIndex.SaintsAttribute));
                    // _usedDrawerTypes.Add(eachDrawer[0]);
                    UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                }
            }

            float aboveUsedHeight = 0;
            float aboveInitY = aboveRect.y;

            foreach (KeyValuePair<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> drawerInfoKv in groupedAboveDrawers)
            {
                string groupBy = drawerInfoKv.Key;
                List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfos = drawerInfoKv.Value;

                if (groupBy == "")
                {
                    foreach ((SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) in drawerInfos)
                    {
                        Rect newAboveRect = drawerInstance.DrawAbove(aboveRect, property, bugFixCopyLabel, eachAttribute);
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
                        Rect leftRect = drawerInstance.DrawAbove(eachRect, property, bugFixCopyLabel, eachAttribute);
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

            Rect fieldRect = EditorGUI.IndentedRect(new Rect(position)
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
            (Rect labelRect, Rect _) =
                RectUtils.SplitWidthRect(fieldRect, EditorGUIUtility.labelWidth);

            labelRect.height = EditorGUIUtility.singleLineHeight;

            // Debug.Log($"pre label: {label.text}");
            #region pre label
            foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
            {
                SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                (bool isActive, Rect newLabelRect) =
                    drawerInstance.DrawPreLabel(labelRect, property, bugFixCopyLabel, eachAttributeWithIndex.SaintsAttribute);
                // ReSharper disable once InvertIf
                if (isActive)
                {
                    labelRect = newLabelRect;
                    UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                }
            }
            #endregion

            #region label info

            // bool completelyDisableLabel = string.IsNullOrEmpty(label.text);
            GUIContent useGuiContent;

            Action saintsPropertyDrawerDrawLabelCallback = () => { };
            if (string.IsNullOrEmpty(label.text))
            {
                // needFallbackLabel = true;
                useGuiContent = new GUIContent(label);
                // hasLabelSpace = false;
            }
            else if (labelAttributeWithIndex.SaintsAttribute == null)  // has label, no saints label drawer
            {
                // needFallbackLabel = false;
                useGuiContent = new GUIContent(label);
                // hasLabelSpace = false;
            }
            else
            {
                // needFallbackLabel = false;
                SaintsPropertyDrawer labelDrawerInstance = GetOrCreateSaintsDrawer(labelAttributeWithIndex);
                UsedAttributesTryAdd(labelAttributeWithIndex, labelDrawerInstance);
                // completelyDisableLabel = labelDrawerInstance.WillDrawLabel(property, label, labelAttributeWithIndex.SaintsAttribute);
                bool hasLabelSpace = labelDrawerInstance.WillDrawLabel(property, label, labelAttributeWithIndex.SaintsAttribute);
                if (hasLabelSpace)
                {
                    // labelDrawerInstance.DrawLabel(labelRect, property, label, labelAttributeWithIndex.SaintsAttribute);

                    saintsPropertyDrawerDrawLabelCallback = () =>
                        labelDrawerInstance.DrawLabel(labelRect, property, label,
                            labelAttributeWithIndex.SaintsAttribute);
                }
                useGuiContent = hasLabelSpace
                    ? new GUIContent(label) {text = "                 "}
                    : new GUIContent(label) {text = ""};

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
                    drawerInstance.GetPostFieldWidth(fieldRect, property, GUIContent.none, eachAttributeWithIndex.SaintsAttribute);
                postFieldWidth += curWidth;
                postFieldInfoList.Add((
                    eachAttributeWithIndex,
                    drawerInstance,
                    curWidth
                ));
            }
            #endregion

            (Rect fieldUseRect, Rect fieldPostRect) = RectUtils.SplitWidthRect(fieldRect, fieldRect.width - postFieldWidth);

            // Debug.Log($"field: {label.text}");

            // if(!property.displayName)
            // Debug.Log(property.name);
            // if(property.name == "LabelFloat") {
            //     EditorGUI.DrawRect(fieldUseRect, Color.black);
            // }

            #region field
            Type fieldDrawer = fieldAttributeWithIndex.SaintsAttribute == null
                ? null
                : GetFirstSaintsDrawerType(fieldAttributeWithIndex.SaintsAttribute.GetType());
            // Debug.Log($"field {fieldAttributeWithIndex.SaintsAttribute}->{fieldDrawer}");

            // Debug.Log($"{label.text}={_fieldControlName}");

            // EditorGUIUtility.labelWidth = ProperLabelWidth();
            // Debug.Log($"{property.propertyPath}=false");
            using(new AdaptLabelWidth())
            using(new ResetIndentScoop())
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                if (fieldDrawer == null)
                {
                    // GUI.SetNextControlName(_fieldControlName);
                    // Debug.Log($"default drawer for {label.text}");
                    DefaultDrawer(fieldUseRect, property, useGuiContent);
                }
                else
                {
                    // Debug.Log(fieldAttribute);
                    SaintsPropertyDrawer fieldDrawerInstance = GetOrCreateSaintsDrawer(fieldAttributeWithIndex);
                    // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                    // GUI.SetNextControlName(_fieldControlName);
                    fieldDrawerInstance.DrawField(fieldUseRect, property, useGuiContent,
                        fieldAttributeWithIndex.SaintsAttribute);
                    // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                    UsedAttributesTryAdd(fieldAttributeWithIndex, fieldDrawerInstance);
                }

                if (changed.changed)
                {
                    PropertyPathToShared[property.propertyPath].changed = true;
                }
            }

            // Debug.Log($"after field: ValueChange={_valueChange}");
            saintsPropertyDrawerDrawLabelCallback?.Invoke();
            #endregion

            // #region label click

            // if (anyLabelDrew)
            // {
            //     LabelMouseProcess(labelRect, property, _fieldControlName);
            // }

            // #endregion

            // Debug.Log($"post field: {label.text}");

            #region post field

            float postFieldAccWidth = 0f;
            foreach ((SaintsWithIndex attributeWithIndex, SaintsPropertyDrawer drawer, float width) in postFieldInfoList)
            {
                Rect eachRect = new Rect(fieldPostRect)
                {
                    x = fieldPostRect.x + postFieldAccWidth,
                    width = width,
                };
                postFieldAccWidth += width;

                // Debug.Log($"DrawPostField, valueChange={_valueChange}");
                bool isActive = drawer.DrawPostField(eachRect, property, label, attributeWithIndex.SaintsAttribute, PropertyPathToShared[property.propertyPath].changed);
                // ReSharper disable once InvertIf
                if (isActive)
                {
                    UsedAttributesTryAdd(attributeWithIndex, drawer);
                }
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

            #region Overlay

            List<Rect> overlayTakenPositions = new List<Rect>();
            bool hasLabelWidth = !string.IsNullOrEmpty(useGuiContent.text);
            foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
            {
                SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                (bool isActive, Rect newLabelRect) =
                    drawerInstance.DrawOverlay(fieldUseRect, property, bugFixCopyLabel, eachAttributeWithIndex.SaintsAttribute, hasLabelWidth, overlayTakenPositions);
                // ReSharper disable once InvertIf
                if (isActive)
                {
                    UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                    overlayTakenPositions.Add(newLabelRect);
                }
            }

            #endregion

            #region below
            Rect belowRect = EditorGUI.IndentedRect(new Rect(position)
            {
                y = fieldRect.y + _labelFieldBasicHeight,
                height = position.y - fieldRect.y - fieldRect.height,
            });

            Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedDrawers =
                new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
            // Debug.Log($"allSaintsAttributes={allSaintsAttributes.Count}");
            foreach (SaintsWithIndex eachAttributeWithIndex in allSaintsAttributes)
            {
                SaintsPropertyDrawer drawerInstance = GetOrCreateSaintsDrawer(eachAttributeWithIndex);
                // Debug.Log($"get instance {eachAttribute}: {drawerInstance}");
                // ReSharper disable once InvertIf
                if (drawerInstance.WillDrawBelow(belowRect, property, bugFixCopyLabel, eachAttributeWithIndex.SaintsAttribute))
                {
                    if(!groupedDrawers.TryGetValue(eachAttributeWithIndex.SaintsAttribute.GroupBy, out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                    {
                        currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                        groupedDrawers[eachAttributeWithIndex.SaintsAttribute.GroupBy] = currentGroup;
                    }
                    currentGroup.Add((drawerInstance, eachAttributeWithIndex.SaintsAttribute));
                    // _usedDrawerTypes.Add(eachDrawer[0]);
                    UsedAttributesTryAdd(eachAttributeWithIndex, drawerInstance);
                }
            }

            foreach (KeyValuePair<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedDrawerInfo in groupedDrawers)
            {
                string groupBy = groupedDrawerInfo.Key;
                List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfo = groupedDrawerInfo.Value;
                // Debug.Log($"draw below: {groupBy}/{bugFixCopyLabel.text}/{label.text}");
                if (groupBy == "")
                {
                    foreach ((SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) in drawerInfo)
                    {
                        belowRect = drawerInstance.DrawBelow(belowRect, property, bugFixCopyLabel, eachAttribute);
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
                        Rect leftRect = drawerInstance.DrawBelow(eachRect, property, bugFixCopyLabel, eachAttribute);
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
            // foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            // {
            //     // ReSharper disable once InvertIf
            //     if (_propertyAttributeToDrawers.TryGetValue(eachAttribute.GetType(),
            //             out IReadOnlyList<Type> eachDrawer))
            //     {
            //         (SaintsPropertyDrawer drawerInstance, ISaintsAttribute _) = GetOrCreateDrawerInfo(eachDrawer[0], eachAttribute);
            //         // ReSharper disable once InvertIf
            //         if (drawerInstance.WillDrawBelow(belowRect, property, propertyScoopLabel, eachAttribute))
            //         {
            //             belowRect = drawerInstance.DrawBelow(belowRect, property, propertyScoopLabel, eachAttribute);
            //             _usedDrawerTypes.Add(eachDrawer[0]);
            //         }
            //     }
            // }

            // Debug.Log($"reset {property.propertyPath}=false");
            PropertyPathToShared[property.propertyPath].changed = false;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private Type GetFirstSaintsDrawerType(Type attributeType)
        {
            // Debug.Log(attributeType);
            // Debug.Log(string.Join(",", _propertyAttributeToDrawers.Keys));

            if (!PropertyAttributeToDrawers.TryGetValue(attributeType,
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
            Type drawerType = PropertyAttributeToDrawers[saintsAttributeWithIndex.SaintsAttribute.GetType()].First(each => each.isSaints)!.drawerType;
            return _cachedDrawer[saintsAttributeWithIndex] =
                (SaintsPropertyDrawer)Activator.CreateInstance(drawerType);
        }

        // private SaintsPropertyDrawer GetOrCreateDrawerInfo(Type drawerType)
        // {
        //     if (!_cachedDrawer.TryGetValue(drawerType, out SaintsPropertyDrawer drawer))
        //     {
        //         _cachedDrawer[drawerType] = drawer = (SaintsPropertyDrawer) Activator.CreateInstance(drawerType, false);
        //     }
        //
        //     return drawer;
        // }

        // private bool DoDrawLabel(SaintsWithIndex labelAttributeWithIndex, Rect labelRect, SerializedProperty property, GUIContent label)
        // {
        //     // Type labelDrawer = labelAttributeWithIndex.SaintsAttribute == null
        //     //     ? null
        //     //     : GetFirstSaintsDrawerType(labelAttributeWithIndex.SaintsAttribute.GetType());
        //     // // bool anyLabelDrew = false;
        //     // if (labelDrawer == null)
        //     // {
        //     //     // anyLabelDrew = true;
        //     //     // Debug.Log(labelRect);
        //     //     // Debug.Log(_labelClickedMousePos);
        //     //     // if(labelRect.Contains(_labelClickedMousePos))
        //     //     // {
        //     //     //     GUI.Box(labelRect, GUIContent.none, "SelectionRect");
        //     //     // }
        //     //     // default label drawer
        //     //     EditorGUI.LabelField(labelRect, label);
        //     //     // RichLabelAttributeDrawer.LabelMouseProcess(labelRect, property);
        //     //     // fieldRect = leftPropertyRect;
        //     //     // return (true, leftPropertyRect);
        //     //     return true;
        //     // }
        //
        //     SaintsPropertyDrawer labelDrawerInstance = GetOrCreateSaintsDrawer(labelAttributeWithIndex);
        //
        //     // add anyway, cus it decide draw or not, which affects the break line type field drawing
        //     _usedAttributes.TryAdd(labelAttributeWithIndex, labelDrawerInstance);
        //
        //     // Debug.Log(labelAttribute);
        //     if (!labelDrawerInstance.WillDrawLabel(property, label, labelAttributeWithIndex.SaintsAttribute))
        //     {
        //         return false;
        //     }
        //
        //     // if(labelRect.Contains(_labelClickedMousePos))
        //     // {
        //     //     GUI.Box(labelRect, GUIContent.none, "SelectionRect");
        //     // }
        //
        //     Rect indentedRect = EditorGUI.IndentedRect(labelRect);
        //
        //     labelDrawerInstance.DrawLabel(indentedRect, property, label,
        //         labelAttributeWithIndex.SaintsAttribute);
        //     // fieldRect = leftPropertyRect;
        //     // newLabel = GUIContent.none;
        //
        //     // _usedAttributes.TryAdd(labelAttributeWithIndex, labelDrawerInstance);
        //     return true;
        // }

        protected void DefaultDrawer(Rect position, SerializedProperty property, GUIContent label)
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

            IEnumerable<PropertyAttribute> allOtherAttributes = SerializedUtils
                .GetAttributesAndDirectParent<PropertyAttribute>(property)
                .attributes
                .Where(each => !(each is ISaintsAttribute));
            foreach (PropertyAttribute propertyAttribute in allOtherAttributes)
            {
                // ReSharper disable once InvertIf
                if(PropertyAttributeToDrawers.TryGetValue(propertyAttribute.GetType(), out IReadOnlyList<(bool isSaints, Type drawerType)> eachDrawer))
                {
                    (bool _, Type drawerType) = eachDrawer.FirstOrDefault(each => !each.isSaints);
                    // SaintsPropertyDrawer drawerInstance = GetOrCreateDrawerInfo(drawerType);
                    // ReSharper disable once InvertIf
                    if(drawerType != null)
                    {
                        if (!_cachedOtherDrawer.TryGetValue(drawerType, out PropertyDrawer drawerInstance))
                        {
                            _cachedOtherDrawer[drawerType] =
                                drawerInstance = (PropertyDrawer)Activator.CreateInstance(drawerType);
                        }

                        FieldInfo drawerFieldInfo = drawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
                        drawerFieldInfo!.SetValue(drawerInstance, propertyAttribute);
                        // drawerInstance.attribute = propertyAttribute;

                        // UnityEditor.RangeDrawer
                        // Debug.Log($"fallback drawerInstance={drawerInstance} for {propertyAttribute}");
                        // Debug.Log($"drawerInstance {drawerInstance}={label?.text.Length}");
                        drawerInstance.OnGUI(position, property, label ?? GUIContent.none);
                        // Debug.Log($"finished drawerInstance={drawerInstance}");
                        return;
                    }
                }
            }

            // fallback to pure unity one (unity default attribute not included)
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw!.Invoke(null, new object[] { position, property, GUIContent.none });
            // Debug.Log($"use unity draw: {property.propertyType}");
            UnityDraw(position, property, label);
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

        private static void UnityDraw(Rect position, SerializedProperty property, GUIContent label=null)
        {
            using (new InsideSaintsFieldScoop(InsideSaintsFieldScoop.MakeKey(property)))
            {
                // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField",
                //     BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                // defaultDraw!.Invoke(null, new object[] { position, property, label });
                // base.OnGUI(position, property, GUIContent.none);
                // Debug.Log($"UnityDraw: `{property.displayName}`");
                EditorGUI.PropertyField(position, property, label ?? GUIContent.none, true);
                // Debug.Log($"UnityDraw done, isSub={isSubDrawer}");
            }
            // Debug.Log($"UnityDraw exit, isSub={isSubDrawer}");
        }

        // public abstract void OnSaintsGUI(Rect position, SerializedProperty property, GUIContent label);
        // protected virtual (bool isActive, Rect position) DrawAbove(Rect position, SerializedProperty property,
        //     GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     return (false, position);
        // }

        protected virtual bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return false;
        }

        protected virtual Rect DrawAbove(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return position;
        }

        protected virtual (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return (false, position);
        }

        protected virtual float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        protected virtual bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            return false;
        }

        protected virtual bool WillDrawLabel(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return false;
        }

        protected virtual void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            // return false;
        }

        protected virtual void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
        }

        protected virtual bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return false;
        }

        protected virtual (bool willDraw, Rect drawPosition) DrawOverlay(Rect position,
            SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabel,
            IReadOnlyCollection<Rect> takenPositions)
        {
            return (false, default);
        }

        protected virtual Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return position;
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
