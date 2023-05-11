using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    public static class Util
    {
        public static void BeginBoxGroup_Layout(string label = "")
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }
        }

        public static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            T[] attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }


        public static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            FieldInfo fieldInfo = GetField(GetTargetObjectWithProperty(property), property.name);
            if (fieldInfo == null)
            {
                return new T[] { };
            }

            return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
        }

        public static FieldInfo GetField(object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        public static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new()
            {
                target.GetType(),
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            types.Reverse();

            return types;
        }

        public static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            if (GetValue_Imp(source, name) is not IEnumerable enumerable)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }


        // public static void NonSerializedField_Layout(UnityEngine.Object target, FieldInfo field)
        // {
        //     object value = field.GetValue(target);
        //
        //     if (value == null)
        //     {
        //         string warning = string.Format("{0} is null. {1} doesn't support reference types with null value", ObjectNames.NicifyVariableName(field.Name), typeof(ShowNonSerializedFieldAttribute).Name);
        //         HelpBox_Layout(warning, MessageType.Warning, context: target);
        //     }
        //     else if (!Field_Layout(value, ObjectNames.NicifyVariableName(field.Name)))
        //     {
        //         string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNonSerializedFieldAttribute).Name, field.FieldType.Name);
        //         HelpBox_Layout(warning, MessageType.Warning, context: target);
        //     }
        // }

        // public static GUIContent GetLabel(SerializedProperty property)
        // {
        //     LabelAttribute labelAttribute = GetAttribute<LabelAttribute>(property);
        //     string labelText = (labelAttribute == null)
        //         ? property.displayName
        //         : labelAttribute.Label;
        //
        //     GUIContent label = new GUIContent(labelText);
        //     return label;
        // }

        // public static void PropertyField_Layout(SerializedProperty property, bool includeChildren)
        // {
        //     Rect dummyRect = new Rect();
        //     PropertyField_Implementation(dummyRect, property, includeChildren, DrawPropertyField_Layout);
        // }
        //
        // private static void PropertyField_Implementation(Rect rect, SerializedProperty property, bool includeChildren, PropertyFieldFunction propertyFieldFunction)
        // {
        //     SpecialCaseDrawerAttribute specialCaseAttribute = PropertyUtility.GetAttribute<SpecialCaseDrawerAttribute>(property);
        //     if (specialCaseAttribute != null)
        //     {
        //         specialCaseAttribute.GetDrawer().OnGUI(rect, property);
        //     }
        //     else
        //     {
        //         // Check if visible
        //         bool visible = PropertyUtility.IsVisible(property);
        //         if (!visible)
        //         {
        //             return;
        //         }
        //
        //         // Validate
        //         ValidatorAttribute[] validatorAttributes = PropertyUtility.GetAttributes<ValidatorAttribute>(property);
        //         foreach (var validatorAttribute in validatorAttributes)
        //         {
        //             validatorAttribute.GetValidator().ValidateProperty(property);
        //         }
        //
        //         // Check if enabled and draw
        //         EditorGUI.BeginChangeCheck();
        //         bool enabled = PropertyUtility.IsEnabled(property);
        //
        //         using (new EditorGUI.DisabledScope(disabled: !enabled))
        //         {
        //             propertyFieldFunction.Invoke(rect, property, PropertyUtility.GetLabel(property), includeChildren);
        //         }
        //
        //         // Call OnValueChanged callbacks
        //         if (EditorGUI.EndChangeCheck())
        //         {
        //             PropertyUtility.CallOnValueChangedCallbacks(property);
        //         }
        //     }
        // }
        //
        // private static void DrawPropertyField_Layout(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
        // {
        //     EditorGUILayout.PropertyField(property, label, includeChildren);
        // }
        public static void EndBoxGroup_Layout()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
