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

        protected SaintsPropertyDrawer()
        {
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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // var attributes = SerializedUtil.GetAttributes<ISaintsAttribute>(property).Select(each => (PropertyAttribute) each);
            var attributes = SerializedUtil.GetAttributes<PropertyAttribute>(property);
            foreach (PropertyAttribute atb in attributes)
            {
                Debug.Log($"atb={atb}");

                Debug.Log(_propertyAttributeToDrawers.TryGetValue(atb.GetType(), out IReadOnlyList<Type> drawers));
                Debug.Log(drawers != null? string.Join(",", drawers): "nothing");
            }
            // base.OnGUI(position, property, label);
            DefaultDrawer(position, property, label);
        }

        private void DefaultDrawer(Rect position, SerializedProperty property, GUIContent label)
        {
            // // this works nice
            // MethodInfo defaultDraw = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // defaultDraw.Invoke(null, new object[3] { position, property, label });

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

        public abstract void OnSaintsGUI(Rect position, SerializedProperty property, GUIContent label);
    }
}
