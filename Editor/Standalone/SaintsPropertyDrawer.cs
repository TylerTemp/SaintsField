using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Utils;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Standalone
{
    // above
    // pre, label, field, post
    // below-
    public abstract class SaintsPropertyDrawer: PropertyDrawer
    {
        private readonly Dictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>> _propertyAttributeToDrawers =
            new Dictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>>();

        // private IReadOnlyList<ISaintsAttribute> _allSaintsAttributes;
        // private SaintsPropertyDrawer _labelDrawer;
        // private SaintsPropertyDrawer _fieldDrawer;

        private readonly Dictionary<Type, SaintsPropertyDrawer> _cachedDrawer = new Dictionary<Type, SaintsPropertyDrawer>();
        private readonly Dictionary<Type, PropertyDrawer> _cachedOtherDrawer = new Dictionary<Type, PropertyDrawer>();
        // private readonly HashSet<Type> _usedDrawerTypes = new HashSet<Type>();
        // private readonly Dictionary<ISaintsAttribute, >
        // private struct UsedAttributeInfo
        // {
        //     public Type DrawerType;
        //     public ISaintsAttribute Attribute;
        // }

        // private readonly List<UsedAttributeInfo> _usedAttributes = new List<UsedAttributeInfo>();
        private readonly Dictionary<ISaintsAttribute, Type> _usedAttributes = new Dictionary<ISaintsAttribute, Type>();

        protected SaintsPropertyDrawer(bool cache=true)
        {
            if (!cache)
            {
                return;
            }

            _usedAttributes.Clear();

            _propertyAttributeToDrawers.Clear();

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
                                 .Select(each => (CustomPropertyDrawer) each)
                                 .Select(instance => typeof(CustomPropertyDrawer)
                                     .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                                     ?.GetValue(instance))
                                 .Where(each => each != null))
                    {
                        if (!attrToDrawers.TryGetValue(attr, out HashSet<Type> attrList))
                        {
                            attrToDrawers[attr] = attrList = new HashSet<Type>();
                        }

                        attrList.Add(saintsSubDrawer);
                    }
                }
            }

            foreach (KeyValuePair<Type, HashSet<Type>> kv in attrToDrawers)
            {
                _propertyAttributeToDrawers[kv.Key] = kv.Value
                    .Select(each => (each.IsSubclassOf(typeof(SaintsPropertyDrawer)), each))
                    .ToArray();
#if EXT_INSPECTOR_LOG
                Debug.Log($"attr {kv.Key} has drawer(s) {string.Join(",", kv.Value)}");
#endif
            }
        }

        ~SaintsPropertyDrawer()
        {
            _propertyAttributeToDrawers.Clear();
        }

        private float _fieldBasicHeight = EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // float defaultHeight = base.GetPropertyHeight(property, label);
            KeyValuePair<ISaintsAttribute, Type>[] filedOrLabel = _usedAttributes
                .Where(each => each.Key.AttributeType is SaintsAttributeType.Field or SaintsAttributeType.Label)
                .ToArray();

            // SaintsPropertyDrawer[] usedDrawerInfos = _usedDrawerTypes.Select(each => _cachedDrawer[each]).ToArray();
            // SaintsPropertyDrawer[] fieldInfos = usedDrawerInfos.Where(each => each.AttributeType is SaintsAttributeType.Field or SaintsAttributeType.Label).ToArray();

            _fieldBasicHeight = filedOrLabel.Length > 0
                ? filedOrLabel
                    .ToLookup(
                        each => each.Key.AttributeType,
                        each => (each.Key, _cachedDrawer[each.Value] )
                    )
                    .Select(each =>
                    {
                        (ISaintsAttribute attr, SaintsPropertyDrawer drawer) = each.FirstOrDefault();
                        if (drawer == null)
                        {
                            return 0;
                        }

                        return drawer.GetLabelFieldHeight(property, label, attr);
                    })
                    .Max()
                    // .Select(each => each.GetLabelFieldHeight(property, label)).Max()
                : base.GetPropertyHeight(property, label);

            float aboveHeight = 0;
            float belowHeight = 0;

            foreach (IGrouping<string, KeyValuePair<ISaintsAttribute, Type>> grouped in _usedAttributes.ToLookup(each => each.Key.GroupBy))
            {
                IEnumerable<float> aboveHeights =
                    grouped.Select(each => _cachedDrawer[each.Value].GetAboveExtraHeight(property, label, each.Key));
                IEnumerable<float> belowHeights =
                    grouped.Select(each => _cachedDrawer[each.Value].GetBelowExtraHeight(property, label, each.Key));

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
            }

            return _fieldBasicHeight + aboveHeight + belowHeight;
        }

        protected virtual float GetLabelFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        protected virtual float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        protected virtual float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        private float _aboveUsedHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
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
            _usedAttributes.Clear();

            using EditorGUI.PropertyScope propertyScope = new EditorGUI.PropertyScope(position, label, property);
            GUIContent propertyScoopLabel = propertyScope.content;

            IReadOnlyList<ISaintsAttribute> allSaintsAttributes = SerializedUtil.GetAttributes<ISaintsAttribute>(property).ToArray();

            #region Above

            Rect aboveRect = new Rect(position);

            Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedAboveDrawers =
                new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {

                Type drawerType = GetFirstSaintsDrawerType(eachAttribute.GetType());
                // ReSharper disable once InvertIf
                if(drawerType != null)
                {
                    SaintsPropertyDrawer drawerInfo = GetOrCreateDrawerInfo(drawerType);
                    // ReSharper disable once InvertIf
                    if (drawerInfo.WillDrawAbove(aboveRect, property, propertyScoopLabel, eachAttribute))
                    {
                        if (!groupedAboveDrawers.TryGetValue(eachAttribute.GroupBy,
                                out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                            groupedAboveDrawers[eachAttribute.GroupBy] = currentGroup;
                        }

                        currentGroup.Add((drawerInfo, eachAttribute));
                        // _usedDrawerTypes.Add(eachDrawer[0]);
                        _usedAttributes.TryAdd(eachAttribute, drawerType);
                    }
                }
            }

            float aboveUsedHeight = 0;

            foreach ((string groupBy, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfo) in groupedAboveDrawers)
            {
                if (groupBy == "")
                {
                    foreach ((SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) in drawerInfo)
                    {
                        Rect newAboveRect = drawerInstance.DrawAbove(aboveRect, property, propertyScoopLabel, eachAttribute);
                        aboveUsedHeight = newAboveRect.y - aboveRect.height;
                        aboveRect = newAboveRect;
                    }
                }
                else
                {
                    float totalWidth = aboveRect.width;
                    float eachWidth = totalWidth / drawerInfo.Count;
                    float height = 0;
                    for (int index = 0; index < drawerInfo.Count; index++)
                    {
                        (SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) = drawerInfo[index];
                        Rect eachRect = new Rect(aboveRect)
                        {
                            x = aboveRect.x + eachWidth * index,
                            width = eachWidth,
                        };
                        Rect leftRect = drawerInstance.DrawAbove(eachRect, property, propertyScoopLabel, eachAttribute);
                        height = Mathf.Max(height, eachRect.y - leftRect.height);
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
            }

            if(Event.current.type == EventType.Repaint)
            {
                _aboveUsedHeight = aboveUsedHeight;
            }

            // Debug.Log($"{Event.current} {aboveUsedHeight} / {_aboveUsedHeight}");

            #endregion

            Rect fieldRect = new Rect(position)
            {
                // y = aboveRect.y + (groupedAboveDrawers.Count == 0? 0: aboveRect.height),
                y = position.y + _aboveUsedHeight,
                height = _fieldBasicHeight,
            };
            // GUIContent newLabel = propertyScoopLabel;
            (Rect labelRect, Rect leftPropertyRect) =
                RectUtils.SplitWidthRect(EditorGUI.IndentedRect(fieldRect), EditorGUIUtility.labelWidth);

            #region pre label
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                Type drawerType = GetFirstSaintsDrawerType(eachAttribute.GetType());
                // ReSharper disable once InvertIf
                if(drawerType != null)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateDrawerInfo(drawerType);
                    (bool isActive, Rect newLabelRect) =
                        drawerInstance.DrawPreLabel(labelRect, property, propertyScoopLabel, eachAttribute);
                    // ReSharper disable once InvertIf
                    if (isActive)
                    {
                        labelRect = newLabelRect;
                        _usedAttributes.TryAdd(eachAttribute, drawerType);
                    }
                }
            }
            #endregion

            #region label
            ISaintsAttribute labelAttribute = allSaintsAttributes.FirstOrDefault(each => each.AttributeType == SaintsAttributeType.Label);
            Type labelDrawer = labelAttribute == null
                ? null
                : GetFirstSaintsDrawerType(labelAttribute.GetType());
            if (labelDrawer == null)
            {
                // default label drawer
                EditorGUI.LabelField(labelRect, propertyScoopLabel);
                fieldRect = leftPropertyRect;
            }
            else
            {
                SaintsPropertyDrawer labelDrawerInstance = GetOrCreateDrawerInfo(labelDrawer);
                // Debug.Log(labelAttribute);
                if(labelDrawerInstance.DrawLabel(labelRect, property, propertyScoopLabel, labelAttribute))
                {
                    fieldRect = leftPropertyRect;
                }
                // newLabel = GUIContent.none;

                _usedAttributes.TryAdd(labelAttribute, labelDrawer);
            }
            #endregion

            // post field - width check
            float postFieldWidth = 0;
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                // ReSharper disable once InvertIf
                if (_propertyAttributeToDrawers.TryGetValue(eachAttribute.GetType(),
                        out IReadOnlyList<(bool isSaints, Type drawerType)> eachDrawer))
                {
                    (bool isSaints, Type drawerType) = eachDrawer.FirstOrDefault(each => each.isSaints);
                    if(isSaints)
                    {
                        SaintsPropertyDrawer drawerInstance = GetOrCreateDrawerInfo(drawerType);
                        postFieldWidth +=
                            drawerInstance.GetPostFieldWidth(fieldRect, property, GUIContent.none, eachAttribute);
                    }
                }
            }

            fieldRect = new Rect(fieldRect)
            {
                width = fieldRect.width - postFieldWidth,
            };
            Rect postFieldRect = new Rect
            {
                x = fieldRect.x + fieldRect.width,
                y = fieldRect.y,
                width = postFieldWidth,
                height = fieldRect.height,
            };

            #region field
            ISaintsAttribute fieldAttribute = allSaintsAttributes.FirstOrDefault(each => each.AttributeType == SaintsAttributeType.Field);
            Type fieldDrawer = fieldAttribute == null
                ? null
                : GetFirstSaintsDrawerType(fieldAttribute.GetType());
            if (fieldDrawer == null)
            {
                DefaultDrawer(fieldRect, property);
            }
            else
            {
                // Debug.Log(fieldAttribute);
                SaintsPropertyDrawer fieldDrawerInstance = GetOrCreateDrawerInfo(fieldDrawer);
                // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                fieldDrawerInstance.DrawField(fieldRect, property, GUIContent.none, fieldAttribute);
                // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                _usedAttributes.TryAdd(fieldAttribute, fieldDrawer);
            }
            #endregion

            #region post field
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                Type drawerType = GetFirstSaintsDrawerType(eachAttribute.GetType());
                // ReSharper disable once InvertIf
                if (drawerType != null)
                {
                    SaintsPropertyDrawer drawerInstance = GetOrCreateDrawerInfo(drawerType);
                    (bool isActive, Rect newPostFieldRect) = drawerInstance.DrawPostField(postFieldRect, property, propertyScoopLabel, eachAttribute);
                    // ReSharper disable once InvertIf
                    if (isActive)
                    {
                        postFieldRect = newPostFieldRect;
                        // _usedDrawerTypes.Add(eachDrawer[0]);
                        _usedAttributes.TryAdd(eachAttribute, drawerType);
                    }
                }
            }
            #endregion

            #region below
            Rect belowRect = new Rect(position)
            {
                y = fieldRect.y + _fieldBasicHeight,
                height = position.y - fieldRect.y - fieldRect.height,
            };

            Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedDrawers =
                new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                Type drawerType = GetFirstSaintsDrawerType(eachAttribute.GetType());
                // ReSharper disable once InvertIf
                if (drawerType != null)
                {
                    SaintsPropertyDrawer drawerInfo = GetOrCreateDrawerInfo(drawerType);
                    // ReSharper disable once InvertIf
                    if (drawerInfo.WillDrawBelow(belowRect, property, propertyScoopLabel, eachAttribute))
                    {
                        if(!groupedDrawers.TryGetValue(eachAttribute.GroupBy, out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                            groupedDrawers[eachAttribute.GroupBy] = currentGroup;
                        }
                        currentGroup.Add((drawerInfo, eachAttribute));
                        // _usedDrawerTypes.Add(eachDrawer[0]);
                        _usedAttributes.TryAdd(eachAttribute, drawerType);
                    }
                }
            }

            foreach ((string groupBy, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> drawerInfo) in groupedDrawers)
            {
                if (groupBy == "")
                {
                    foreach ((SaintsPropertyDrawer drawerInstance, ISaintsAttribute eachAttribute) in drawerInfo)
                    {
                        belowRect = drawerInstance.DrawBelow(belowRect, property, propertyScoopLabel, eachAttribute);
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
                        Rect nowRect = drawerInstance.DrawBelow(eachRect, property, propertyScoopLabel, eachAttribute);
                        height = Mathf.Max(height, nowRect.height);
                    }

                    belowRect.height = height;
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

        private Type GetFirstSaintsDrawerType(Type attributeType)
        {
            if (!_propertyAttributeToDrawers.TryGetValue(attributeType,
                    out IReadOnlyList<(bool isSaints, Type drawerType)> eachDrawer))
            {
                return null;
            }

            (bool isSaints, Type drawerType) = eachDrawer.FirstOrDefault(each => each.isSaints);

            return isSaints ? drawerType : null;
        }

        private SaintsPropertyDrawer GetOrCreateDrawerInfo(Type drawerType)
        {
            if (!_cachedDrawer.TryGetValue(drawerType, out SaintsPropertyDrawer drawer))
            {
                _cachedDrawer[drawerType] = drawer = (SaintsPropertyDrawer) Activator.CreateInstance(drawerType, false);
            }

            return drawer;
        }

        private void DefaultDrawer(Rect position, SerializedProperty property)
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

            IEnumerable<PropertyAttribute> allOtherAttributes = SerializedUtil.GetAttributes<PropertyAttribute>(property)
                .Where(each => each is not ISaintsAttribute);
            foreach (PropertyAttribute propertyAttribute in allOtherAttributes)
            {
                // ReSharper disable once InvertIf
                if(_propertyAttributeToDrawers.TryGetValue(propertyAttribute.GetType(), out IReadOnlyList<(bool isSaints, Type drawerType)> eachDrawer))
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
                        // Debug.Log($"drawerInstance={drawerInstance}");
                        drawerInstance.OnGUI(position, property, GUIContent.none);
                        // Debug.Log($"finished drawerInstance={drawerInstance}");
                        return;
                    }
                }
            }

            // fallback to pure unity one (unity default attribute not included)
            MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            defaultDraw!.Invoke(null, new object[] { position, property, GUIContent.none });
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

        protected virtual (bool isActive, Rect position) DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return (false, position);
        }

        protected virtual bool DrawLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return false;
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
    }
}
