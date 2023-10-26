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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // float defaultHeight = base.GetPropertyHeight(property, label);
            (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)[] usedDrawerInfos = _usedDrawerTypes.Select(each => _cachedDrawer[each]).ToArray();
            (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute)[] fieldInfos = usedDrawerInfos.Where(each => each.iAttribute.AttributeType == SaintsAttributeType.Field || each.iAttribute.AttributeType == SaintsAttributeType.Label).ToArray();

            float fieldHeight = fieldInfos.Length > 0
                ? fieldInfos.Select(each => each.drawer.GetLabelFieldHeight(property, label, each.iAttribute)).Max()
                : base.GetPropertyHeight(property, label);

            return fieldHeight + usedDrawerInfos.Select(each => each.drawer.GetExtraHeight(property, label, each.iAttribute)).Sum();
        }

        protected virtual float GetLabelFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        protected virtual float GetExtraHeight(SerializedProperty property, GUIContent label,
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

            // label
            ISaintsAttribute labelAttribute = allSaintsAttributes.FirstOrDefault(each => each.AttributeType == SaintsAttributeType.Label);
            Type labelDrawer = labelAttribute != null &&
                               _propertyAttributeToDrawers.TryGetValue(labelAttribute.GetType(),
                                   out IReadOnlyList<Type> labelDrawers)
                ? labelDrawers[0]
                : null;

            Rect fieldRect = position;
            GUIContent newLabel = propertyScoopLabel;
            (Rect labelRect, Rect leftPropertyRect) =
                RectUtils.SplitWidthRect(EditorGUI.IndentedRect(position), EditorGUIUtility.labelWidth);

            if (labelDrawer == null)
            {
                // default label drawer
                EditorGUI.LabelField(labelRect, propertyScoopLabel);
                fieldRect = leftPropertyRect;
            }
            else
            {
                if (!_cachedDrawer.TryGetValue(labelDrawer, out (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute) labelDrawerInfo))
                {
                    _cachedDrawer[labelDrawer] = labelDrawerInfo = (
                        (SaintsPropertyDrawer) Activator.CreateInstance(labelDrawer, false),
                        labelAttribute
                    );
                }
                // Debug.Log(labelAttribute);
                if(labelDrawerInfo.drawer.DrawLabel(labelRect, property, propertyScoopLabel, labelAttribute))
                {
                    fieldRect = leftPropertyRect;
                }
                newLabel = GUIContent.none;

                _usedDrawerTypes.Add(labelDrawer);
            }

            // field
            ISaintsAttribute fieldAttribute = allSaintsAttributes.FirstOrDefault(each => each.AttributeType == SaintsAttributeType.Field);
            Type fieldDrawer = fieldAttribute != null &&
                               _propertyAttributeToDrawers.TryGetValue(fieldAttribute.GetType(),
                                   out IReadOnlyList<Type> fieldDrawers)
                ? fieldDrawers[0]
                : null;
            if (fieldDrawer == null)
            {
                DefaultDrawer(fieldRect, property, newLabel);
            }
            else
            {
                // Debug.Log(fieldAttribute);
                if (!_cachedDrawer.TryGetValue(fieldDrawer, out (SaintsPropertyDrawer drawer, ISaintsAttribute iAttribute) fieldDrawerInfo))
                {
                    _cachedDrawer[fieldDrawer] = fieldDrawerInfo = (
                        (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false),
                        fieldAttribute
                    );
                }
                // _fieldDrawer ??= (SaintsPropertyDrawer) Activator.CreateInstance(fieldDrawer, false);
                fieldDrawerInfo.drawer.DrawField(fieldRect, property, newLabel, fieldAttribute);
                // _fieldDrawer.DrawField(fieldRect, property, newLabel, fieldAttribute);

                _usedDrawerTypes.Add(fieldDrawer);
            }
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

        protected virtual bool DrawLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return false;
        }

        protected virtual void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
        }
    }
}
