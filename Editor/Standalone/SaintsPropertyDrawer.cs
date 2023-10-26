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
        private readonly Dictionary<Type, IReadOnlyList<Type>> _propertyAttributeToDrawers =
            new Dictionary<Type, IReadOnlyList<Type>>();

        // private IReadOnlyList<ISaintsAttribute> _allSaintsAttributes;
        // private SaintsPropertyDrawer _labelDrawer;
        // private SaintsPropertyDrawer _fieldDrawer;

        private readonly Dictionary<Type, (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> _cachedDrawer = new Dictionary<Type, (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
        private readonly HashSet<Type> _usedDrawerTypes = new HashSet<Type>();

        protected SaintsPropertyDrawer(bool cache=true)
        {
            if (!cache)
            {
                return;
            }

            _usedDrawerTypes.Clear();

            _propertyAttributeToDrawers.Clear();

            Dictionary<Type, HashSet<Type>> attrToDrawers = new Dictionary<Type, HashSet<Type>>();

            foreach (Assembly asb in AppDomain.CurrentDomain.GetAssemblies())
            {
                List<Type> saintsSubDrawers = asb.GetTypes()
                    .Where(type => type.IsSubclassOf(typeof(SaintsPropertyDrawer)))
                    // .Where(type => type.IsSubclassOf(typeof(PropertyDrawer)))
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
                _propertyAttributeToDrawers[kv.Key] = kv.Value.ToList();
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
            (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)[] usedDrawerInfos = _usedDrawerTypes.Select(each => _cachedDrawer[each]).ToArray();
            (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)[] fieldInfos = usedDrawerInfos.Where(each => each.iAttribute.AttributeType == SaintsAttributeType.Field || each.iAttribute.AttributeType == SaintsAttributeType.Label).ToArray();

            _fieldBasicHeight = fieldInfos.Length > 0
                ? fieldInfos.Select(each => each.drawer.GetLabelFieldHeight(property, label, each.iAttribute)).Max()
                : base.GetPropertyHeight(property, label);

            float aboveHeight = 0;
            float belowHeight = 0;

            foreach (IGrouping<string, (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> grouped in usedDrawerInfos.ToLookup(each => each.iAttribute.GroupBy))
            {
                if (grouped.Key == "")
                {
                    aboveHeight += grouped.Select(each => each.drawer.GetAboveExtraHeight(property, label, each.iAttribute)).Sum();
                    belowHeight += grouped.Select(each => each.drawer.GetBelowExtraHeight(property, label, each.iAttribute)).Sum();
                }
                else
                {
                    aboveHeight += grouped.Select(each => each.drawer.GetAboveExtraHeight(property, label, each.iAttribute)).Max();
                    belowHeight += grouped.Select(each => each.drawer.GetBelowExtraHeight(property, label, each.iAttribute)).Max();
                }
            }

            return _fieldBasicHeight + aboveHeight + belowHeight;
        }

        protected virtual float GetLabelFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
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
            _usedDrawerTypes.Clear();

            using EditorGUI.PropertyScope propertyScope = new EditorGUI.PropertyScope(position, label, property);
            GUIContent propertyScoopLabel = propertyScope.content;

            IReadOnlyList<ISaintsAttribute> allSaintsAttributes = SerializedUtil.GetAttributes<ISaintsAttribute>(property).ToArray();

            Rect fieldRect = new Rect(position)
            {
                height = _fieldBasicHeight,
            };
            // GUIContent newLabel = propertyScoopLabel;
            (Rect labelRect, Rect leftPropertyRect) =
                RectUtils.SplitWidthRect(EditorGUI.IndentedRect(fieldRect), EditorGUIUtility.labelWidth);

            // pre label
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                // ReSharper disable once InvertIf
                if (_propertyAttributeToDrawers.TryGetValue(eachAttribute.GetType(),
                        out IReadOnlyList<Type> eachDrawer))
                {
                    (SaintsPropertyDrawer drawerInstance, ISaintsAttribute _) = GetOrCreateDrawerInfo(eachDrawer[0], eachAttribute);
                    (bool isActive, Rect newLabelRect) = drawerInstance.DrawPreLabel(labelRect, property, propertyScoopLabel, eachAttribute);
                    if (isActive)
                    {
                        labelRect = newLabelRect;
                        _usedDrawerTypes.Add(eachDrawer[0]);
                    }
                }
            }
            // label
            ISaintsAttribute labelAttribute = allSaintsAttributes.FirstOrDefault(each => each.AttributeType == SaintsAttributeType.Label);
            Type labelDrawer = labelAttribute != null &&
                               _propertyAttributeToDrawers.TryGetValue(labelAttribute.GetType(),
                                   out IReadOnlyList<Type> labelDrawers)
                ? labelDrawers[0]
                : null;

            if (labelDrawer == null)
            {
                // default label drawer
                EditorGUI.LabelField(labelRect, propertyScoopLabel);
                fieldRect = leftPropertyRect;
            }
            else
            {
                (SaintsPropertyDrawer labelDrawerInstance, ISaintsAttribute _) = GetOrCreateDrawerInfo(labelDrawer, labelAttribute);
                // Debug.Log(labelAttribute);
                if(labelDrawerInstance.DrawLabel(labelRect, property, propertyScoopLabel, labelAttribute))
                {
                    fieldRect = leftPropertyRect;
                }
                // newLabel = GUIContent.none;

                _usedDrawerTypes.Add(labelDrawer);
            }

            // post field - width check
            float postFieldWidth = 0;
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                // ReSharper disable once InvertIf
                if (_propertyAttributeToDrawers.TryGetValue(eachAttribute.GetType(),
                        out IReadOnlyList<Type> eachDrawer))
                {
                    (SaintsPropertyDrawer drawerInstance, ISaintsAttribute _) = GetOrCreateDrawerInfo(eachDrawer[0], eachAttribute);
                    postFieldWidth += drawerInstance.GetPostFieldWidth(fieldRect, property, GUIContent.none, eachAttribute);
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

            // field
            ISaintsAttribute fieldAttribute = allSaintsAttributes.FirstOrDefault(each => each.AttributeType == SaintsAttributeType.Field);
            Type fieldDrawer = fieldAttribute != null &&
                               _propertyAttributeToDrawers.TryGetValue(fieldAttribute.GetType(),
                                   out IReadOnlyList<Type> fieldDrawers)
                ? fieldDrawers[0]
                : null;
            if (fieldDrawer == null)
            {
                DefaultDrawer(fieldRect, property, GUIContent.none);
            }
            else
            {
                // Debug.Log(fieldAttribute);
                (SaintsPropertyDrawer fieldDrawerInstance, ISaintsAttribute _) = GetOrCreateDrawerInfo(fieldDrawer, fieldAttribute);
                // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                fieldDrawerInstance.DrawField(fieldRect, property, GUIContent.none, fieldAttribute);
                // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                _usedDrawerTypes.Add(fieldDrawer);
            }

            // post field
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                // ReSharper disable once InvertIf
                if (_propertyAttributeToDrawers.TryGetValue(eachAttribute.GetType(),
                        out IReadOnlyList<Type> eachDrawer))
                {
                    (SaintsPropertyDrawer drawerInstance, ISaintsAttribute _) = GetOrCreateDrawerInfo(eachDrawer[0], eachAttribute);
                    (bool isActive, Rect newPostFieldRect) = drawerInstance.DrawPostField(postFieldRect, property, propertyScoopLabel, eachAttribute);
                    if (isActive)
                    {
                        postFieldRect = newPostFieldRect;
                        _usedDrawerTypes.Add(eachDrawer[0]);
                    }
                }
            }

            // below
            Rect belowRect = new Rect(position)
            {
                y = fieldRect.y + _fieldBasicHeight,
                height = position.y - fieldRect.y - fieldRect.height,
            };

            Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>> groupedDrawers =
                new Dictionary<string, List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>>();
            foreach (ISaintsAttribute eachAttribute in allSaintsAttributes)
            {
                // ReSharper disable once InvertIf
                if (_propertyAttributeToDrawers.TryGetValue(eachAttribute.GetType(),
                        out IReadOnlyList<Type> eachDrawer))
                {
                    (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute) drawerInfo = GetOrCreateDrawerInfo(eachDrawer[0], eachAttribute);
                    // ReSharper disable once InvertIf
                    if (drawerInfo.drawer.WillDrawBelow(belowRect, property, propertyScoopLabel, eachAttribute))
                    {
                        if(!groupedDrawers.TryGetValue(eachAttribute.GroupBy, out List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)> currentGroup))
                        {
                            currentGroup = new List<(SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)>();
                            groupedDrawers[eachAttribute.GroupBy] = currentGroup;
                        }
                        currentGroup.Add(drawerInfo);
                        _usedDrawerTypes.Add(eachDrawer[0]);
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

        private (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute) GetOrCreateDrawerInfo(Type drawerType, ISaintsAttribute drawerAttribute)
        {
            if (!_cachedDrawer.TryGetValue(drawerType, out (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute) drawerInfo))
            {
                _cachedDrawer[drawerType] = drawerInfo = (
                    (SaintsPropertyDrawer) Activator.CreateInstance(drawerType, false),
                    drawerAttribute
                );
            }

            return drawerInfo;
        }

        private static void DefaultDrawer(Rect position, SerializedProperty property, GUIContent label)
        {
            // // this works nice
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw!.Invoke(null, new object[3] { position, property, label });

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
            EditorGUI.PropertyField(position, property, label, true);
        }

        // public abstract void OnSaintsGUI(Rect position, SerializedProperty property, GUIContent label);
        // protected virtual (bool isActive, Rect position) DrawAbove(Rect position, SerializedProperty property,
        //     GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     return (false, position);
        // }

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
