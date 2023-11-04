using System;
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

        // private IReadOnlyList<ISaintsAttribute> _allSaintsAttributes;
        // private SaintsPropertyDrawer _labelDrawer;
        // private SaintsPropertyDrawer _fieldDrawer;

        private readonly string _fieldControlName;

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

        protected SaintsPropertyDrawer()
        {
            // if (IsSubDrawer)
            // {
            //     return;
            // }

            _fieldControlName = Guid.NewGuid().ToString();

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

        // ~SaintsPropertyDrawer()
        // {
        //     PropertyAttributeToDrawers.Clear();
        // }

        private float _labelFieldBasicHeight = EditorGUIUtility.singleLineHeight;

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

            // float defaultHeight = base.GetPropertyHeight(property, label);
            (ISaintsAttribute iSaintsAttribute, SaintsPropertyDrawer drawer)[] filedOrLabel = _usedAttributes
                .Where(each => each.Key.SaintsAttribute.AttributeType is SaintsAttributeType.Field or SaintsAttributeType.Label)
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

            bool saintsDrawNoLabel = hasSaintsLabel &&
                                     !labelFound.drawer.WillDrawLabel(property, label, labelFound.iSaintsAttribute);
            // float labelWidth = hasLabel? EditorGUIUtility.labelWidth: 0f;
            // Debug.Log($"hasLabel={hasLabel}");
            // float labelHeight = drawSaintsLabel
            //     ? labelFound.drawer.GetLabelHeight(property, label, labelFound.iSaintsAttribute)
            //     : 0f;

            // bool saintsDrawNoLabel = hasSaintsLabel && !drawSaintsLabel;

            // Debug.Log($"hasSaintsLabel={hasSaintsLabel}, saintsDrawNoLabel={saintsDrawNoLabel}");

            bool hasSaintsField = fieldFound.iSaintsAttribute != null;
            // float fieldHeight = hasSaintsField
            //     ? fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute, !saintsDrawNoLabel)
            //     : 0f;

            bool fieldBreakLine = hasSaintsField && fieldFound.iSaintsAttribute.GroupBy != "__LABEL_FIELD__";

            FieldDrawerConfigAttribute fieldDrawerConfigAttribute = _usedAttributes
                .Select(each => each.Key.SaintsAttribute)
                .OfType<FieldDrawerConfigAttribute>()
                .FirstOrDefault() ?? GetDefaultFieldDrawerConfigAttribute(fieldBreakLine);

            // Debug.Log($"draw type {fieldDrawerConfigAttribute.FieldDraw}/{IsSubDrawer}");
            switch (fieldDrawerConfigAttribute.FieldDraw)
            {
                case FieldDrawerConfigAttribute.FieldDrawType.Inline:
                case FieldDrawerConfigAttribute.FieldDrawType.FullWidthOverlay:
                {
                    float labelBasicHeight = saintsDrawNoLabel? 0f: EditorGUIUtility.singleLineHeight;
                    float fieldBasicHeight = hasSaintsField
                        ? fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute, !saintsDrawNoLabel)
                        // : EditorGUIUtility.singleLineHeight;
                        : EditorGUI.GetPropertyHeight(property, label, true);
                    _labelFieldBasicHeight = Mathf.Max(labelBasicHeight, fieldBasicHeight);
                }
                    break;
                case FieldDrawerConfigAttribute.FieldDrawType.FullWidthNewLine:
                {
                    float labelBasicHeight = saintsDrawNoLabel? 0f: EditorGUIUtility.singleLineHeight;
                    float fieldBasicHeight = hasSaintsField
                        ? fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute,
                            !saintsDrawNoLabel)
                        : EditorGUI.GetPropertyHeight(property, label, true);
                    // Debug.Log($"FullWidthNewLine, labelBasicHeight={labelBasicHeight}");
                    _labelFieldBasicHeight = labelBasicHeight + fieldBasicHeight;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldDrawerConfigAttribute.FieldDraw), fieldDrawerConfigAttribute.FieldDraw, null);
            }

            // // float basicHeight;
            // if (fieldBreakLine)
            // {
            //     float labelBasicHeight = saintsDrawNoLabel? 0f: EditorGUIUtility.singleLineHeight;
            //     float fieldBasicHeight = fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute,
            //         !saintsDrawNoLabel);
            //     _fieldBasicHeight = labelBasicHeight + fieldBasicHeight;
            // }
            // else
            // {
            //     float labelBasicHeight = saintsDrawNoLabel? 0f: EditorGUIUtility.singleLineHeight;
            //     float fieldBasicHeight = hasSaintsField
            //         ? fieldFound.drawer.GetFieldHeight(property, label, fieldFound.iSaintsAttribute, !saintsDrawNoLabel)
            //         // : EditorGUIUtility.singleLineHeight;
            //         : EditorGUI.GetPropertyHeight(property, label);
            //     _fieldBasicHeight = Mathf.Max(labelBasicHeight, fieldBasicHeight);
            // }

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

            return _labelFieldBasicHeight + aboveHeight + belowHeight;
        }

        private static FieldDrawerConfigAttribute GetDefaultFieldDrawerConfigAttribute(bool newLine)
        {
            return new FieldDrawerConfigAttribute(newLine? FieldDrawerConfigAttribute.FieldDrawType.FullWidthNewLine: FieldDrawerConfigAttribute.FieldDrawType.Inline, 0);
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

        private bool _valueChange;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (SubCounter.TryGetValue(InsideSaintsFieldScoop.MakeKey(property), out int insideCount) && insideCount > 0)
            {
                // Debug.Log($"capture sub drawer {property.displayName} {insideCount}");
                // EditorGUI.PropertyField(position, property, label, true);
                UnityDraw(position, property);
                return;
            }

            IReadOnlyList<SaintsWithIndex> allSaintsAttributes = SerializedUtils.GetAttributes<ISaintsAttribute>(property).Select((each, index) => new SaintsWithIndex
            {
                SaintsAttribute = each,
                Index = index,
            }).ToArray();

            SaintsWithIndex labelAttributeWithIndex = allSaintsAttributes.FirstOrDefault(each => each.SaintsAttribute.AttributeType == SaintsAttributeType.Label);
            SaintsWithIndex fieldAttributeWithIndex = allSaintsAttributes.FirstOrDefault(each => each.SaintsAttribute.AttributeType == SaintsAttributeType.Field);

            // if (IsSubDrawer)
            // {
            //     (Rect subLabelRect, Rect subLeftPropertyRect) =
            //         RectUtils.SplitWidthRect(position, EditorGUIUtility.labelWidth);
            //
            //     bool subLabelDraw = DoDrawLabel(labelAttributeWithIndex, subLabelRect, property, label);
            //     Rect subFieldRect = subLabelDraw ? subLeftPropertyRect : position;
            //
            //         // EditorGUI.DrawRect(subFieldRect, Color.yellow);
            //     using(new ResetIndentScoop())
            //     {
            //         // Debug.Log($"property.isExpanded={property.isExpanded}");
            //         if (fieldAttributeWithIndex.SaintsAttribute != null)
            //         {
            //             GetOrCreateSaintsDrawer(fieldAttributeWithIndex)
            //                 .DrawField(subFieldRect, property, label, fieldAttributeWithIndex.SaintsAttribute);
            //         }
            //         else
            //         {
            //             EditorGUI.PropertyField(subFieldRect, property, GUIContent.none, true);
            //         }
            //     }
            //
            //     return;
            // }

            // Debug.Log($"position.height={position.height}");
            // // var attributes = SerializedUtil.GetAttributes<ISaintsAttribute>(property).Select(each => (PropertyAttribute) each);
            // var attributes = SerializedUtil.GetAttributes<PropertyAttribute>(property);
            // foreach (PropertyAttribute atb in attributes)
            // {
            //     Debug.Log($"atb={atb}");
            //
            //     Debug.Log(_propertyAttributeToDrawers.TryGetValue(atb.GetType(), out IReadOnlyList<Type> drawers));
            //     Debug.Log(drawers != null? string.Join(",", drawers): "nothing");
            // }
            // // base.OnGUI(position, property, label);
            // DefaultDrawer(fieldRect, property, newLabel);
            GUI.SetNextControlName(_fieldControlName);

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
                    _usedAttributes.TryAdd(eachAttributeWithIndex, drawerInstance);
                }
            }

            float aboveUsedHeight = 0;
            float aboveInitY = aboveRect.y;

            foreach ((string groupBy, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfos) in groupedAboveDrawers)
            {
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

            Rect fieldRect = new Rect(position)
            {
                // y = aboveRect.y + (groupedAboveDrawers.Count == 0? 0: aboveRect.height),
                y = position.y + aboveUsedHeight,
                height = _labelFieldBasicHeight,
            };

            // Color backgroundColor = EditorGUIUtility.isProSkin
            //     ? new Color32(56, 56, 56, 255)
            //     : new Color32(194, 194, 194, 255);
            // UnityDraw(fieldRect, property, propertyScoopLabel);
            // EditorGUI.DrawRect(fieldRect, backgroundColor);

            // GUIContent newLabel = propertyScoopLabel;
            (Rect labelRect, Rect leftPropertyRect) =
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
                    _usedAttributes.TryAdd(eachAttributeWithIndex, drawerInstance);
                }
            }
            #endregion

            // Debug.Log($"label: {label.text}");
            #region label

            _valueChange = false;
            // Debug.Log($"valueChange reset to false");

            bool anyLabelDrew = DoDrawLabel(labelAttributeWithIndex, labelRect, property, bugFixCopyLabel);
            fieldRect = anyLabelDrew ? leftPropertyRect : EditorGUI.IndentedRect(fieldRect);

            #endregion

            // adjust field rect
            bool fieldBreakLine = fieldAttributeWithIndex.SaintsAttribute != null && fieldAttributeWithIndex.SaintsAttribute.GroupBy != "__LABEL_FIELD__";
            FieldDrawerConfigAttribute fieldDrawerConfigAttribute = allSaintsAttributes
                .Select(each => each.SaintsAttribute)
                .OfType<FieldDrawerConfigAttribute>()
                .FirstOrDefault() ?? GetDefaultFieldDrawerConfigAttribute(fieldBreakLine);

            // Debug.Log($"{fieldDrawerConfigAttribute.FieldDraw}/{anyLabelDrew}");
            switch (fieldDrawerConfigAttribute.FieldDraw)
            {
                case FieldDrawerConfigAttribute.FieldDrawType.Inline:
                    break;
                case FieldDrawerConfigAttribute.FieldDrawType.FullWidthNewLine:
                {
                    if (anyLabelDrew)
                    {
                        fieldRect.x = labelRect.x;
                        fieldRect.y += EditorGUIUtility.singleLineHeight;
                        fieldRect.height -= EditorGUIUtility.singleLineHeight;
                        fieldRect.width = position.width;
                    }
                }
                    break;
                case FieldDrawerConfigAttribute.FieldDrawType.FullWidthOverlay:
                {
                    fieldRect.x = labelRect.x;
                    // fieldRect.y += EditorGUIUtility.singleLineHeight;
                    // fieldRect.height -= EditorGUIUtility.singleLineHeight;
                    fieldRect.width = position.width;
                    // fieldRect = new Rect(labelRect)
                    // {
                    //     height = fieldRect.height,
                    //     width = fieldRect.width,
                    // };

                    // EditorGUI.DrawRect(labelRect, Color.red);
                    //
                    // Debug.Log($"FullWidthOverlay");
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldDrawerConfigAttribute.FieldDraw), fieldDrawerConfigAttribute.FieldDraw, null);
            }

            // if (anyLabelDrew && fieldBreakLine)
            // {
            //     fieldRect.x = labelRect.x;
            //     fieldRect.y += EditorGUIUtility.singleLineHeight;
            //     fieldRect.height -= EditorGUIUtility.singleLineHeight;
            //     fieldRect.width = position.width;
            // }

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
            using(new ResetIndentScoop())
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                if (fieldDrawer == null)
                {
                    GUI.SetNextControlName(_fieldControlName);
                    // Debug.Log($"default drawer for {label.text}");
                    DefaultDrawer(fieldUseRect, property);
                }
                else
                {
                    // Debug.Log(fieldAttribute);
                    SaintsPropertyDrawer fieldDrawerInstance = GetOrCreateSaintsDrawer(fieldAttributeWithIndex);
                    // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                    GUI.SetNextControlName(_fieldControlName);
                    fieldDrawerInstance.DrawField(fieldUseRect, property, GUIContent.none,
                        fieldAttributeWithIndex.SaintsAttribute);
                    // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                    _usedAttributes.TryAdd(fieldAttributeWithIndex, fieldDrawerInstance);
                }

                if (!_valueChange && changed.changed)
                {
                    _valueChange = true;
                }
            }

            // Debug.Log($"after field: ValueChange={_valueChange}");
            #endregion

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
                bool isActive = drawer.DrawPostField(eachRect, property, label, attributeWithIndex.SaintsAttribute, _valueChange);
                // ReSharper disable once InvertIf
                if (isActive)
                {
                    _usedAttributes.TryAdd(attributeWithIndex, drawer);
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
                    _usedAttributes.TryAdd(eachAttributeWithIndex, drawerInstance);
                }
            }

            foreach ((string groupBy, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfo) in groupedDrawers)
            {
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

        private bool DoDrawLabel(SaintsWithIndex labelAttributeWithIndex, Rect labelRect, SerializedProperty property, GUIContent label)
        {
            Type labelDrawer = labelAttributeWithIndex.SaintsAttribute == null
                ? null
                : GetFirstSaintsDrawerType(labelAttributeWithIndex.SaintsAttribute.GetType());
            // bool anyLabelDrew = false;
            if (labelDrawer == null)
            {
                // anyLabelDrew = true;
                // Debug.Log(labelRect);
                // Debug.Log(_labelClickedMousePos);
                // if(labelRect.Contains(_labelClickedMousePos))
                // {
                //     GUI.Box(labelRect, GUIContent.none, "SelectionRect");
                // }
                // default label drawer
                EditorGUI.LabelField(labelRect, label);
                // RichLabelAttributeDrawer.LabelMouseProcess(labelRect, property);
                LabelMouseProcess(labelRect, property, _fieldControlName);
                // fieldRect = leftPropertyRect;
                // return (true, leftPropertyRect);
                return true;
            }

            SaintsPropertyDrawer labelDrawerInstance = GetOrCreateSaintsDrawer(labelAttributeWithIndex);

            // add anyway, cus it decide draw or not, which affects the break line type field drawing
            _usedAttributes.TryAdd(labelAttributeWithIndex, labelDrawerInstance);

            // Debug.Log(labelAttribute);
            if (!labelDrawerInstance.WillDrawLabel(property, label, labelAttributeWithIndex.SaintsAttribute))
            {
                return false;
            }

            // if(labelRect.Contains(_labelClickedMousePos))
            // {
            //     GUI.Box(labelRect, GUIContent.none, "SelectionRect");
            // }

            Rect indentedRect = EditorGUI.IndentedRect(labelRect);

            labelDrawerInstance.DrawLabel(indentedRect, property, label,
                labelAttributeWithIndex.SaintsAttribute);
            LabelMouseProcess(labelRect, property, _fieldControlName);
            // fieldRect = leftPropertyRect;
            // newLabel = GUIContent.none;

            // _usedAttributes.TryAdd(labelAttributeWithIndex, labelDrawerInstance);
            return true;
        }

        protected void DefaultDrawer(Rect position, SerializedProperty property)
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

            IEnumerable<PropertyAttribute> allOtherAttributes = SerializedUtils.GetAttributes<PropertyAttribute>(property)
                .Where(each => each is not ISaintsAttribute);
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
                        drawerInstance.OnGUI(position, property, GUIContent.none);
                        // Debug.Log($"finished drawerInstance={drawerInstance}");
                        return;
                    }
                }
            }

            // fallback to pure unity one (unity default attribute not included)
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw!.Invoke(null, new object[] { position, property, GUIContent.none });
            // Debug.Log($"use unity draw: {property.propertyType}");
            UnityDraw(position, property);
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

        private static void UnityDraw(Rect position, SerializedProperty property)
        {
            using (new InsideSaintsFieldScoop(InsideSaintsFieldScoop.MakeKey(property)))
            {
                // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField",
                //     BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                // defaultDraw!.Invoke(null, new object[] { position, property, label });
                // base.OnGUI(position, property, GUIContent.none);
                // Debug.Log($"UnityDraw: {property.displayName}");
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
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

        protected virtual Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return position;
        }

        private bool _mouseHold;
        // private Vector2 _labelClickedMousePos = new Vector2(-1, -1);

        private void LabelMouseProcess(Rect position, SerializedProperty property, string focusName)
        {
            Event e = Event.current;
            // if (e.isMouse && e.type == EventType.MouseDown)
            // {
            //     _labelClickedMousePos = e.mousePosition;
            // }

            if (e.isMouse && e.button == 0)
            {
                if(!_mouseHold && position.Contains(e.mousePosition))
                {
                    // Debug.Log($"start hold");
                    _mouseHold = true;
                    // e.Use();
                    // Debug.Log($"focus {_fieldControlName}");
                    GUI.FocusControl(focusName);
                }
            }

            if (e.type == EventType.MouseUp)
            {
                _mouseHold = false;
                // _labelClickedMousePos = new Vector2(-1, -1);
            }

            if (property.propertyType == SerializedPropertyType.Integer ||
                property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUIUtility.AddCursorRect(position, MouseCursor.SlideArrow);
                if (e.isMouse && e.button == 0
                              && _mouseHold
                    // && position.Contains(e.mousePosition)
                   )
                {
                    int xOffset = Mathf.RoundToInt(e.delta.x);
                    // if(xOffset)
                    // Debug.Log(xOffset);
                    if (xOffset != 0)
                    {
                        if (property.propertyType == SerializedPropertyType.Float)
                        {
                            property.floatValue += xOffset * 0.03f;
                        }
                        else
                        {
                            property.intValue += xOffset;
                        }
                        // Debug.Log($"valueChange=true");
                        _valueChange = true;
                    }
                }
            }
        }
    }
}
