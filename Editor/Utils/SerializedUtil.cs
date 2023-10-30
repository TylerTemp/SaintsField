using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace ExtInspector.Editor.Utils
{
    public static class SerializedUtil
    {
        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedObject obj, string propName)
        {
            return obj.FindProperty($"<{propName}>k__BackingField");
        }

        public static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            T[] attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        public static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            FieldInfo fieldInfo = ReflectUil.GetField(GetTargetObjectWithProperty(property), property.name);
            if (fieldInfo == null)
            {
                return new T[] { };
            }

            return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
        }

        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
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

        public static Type GetType(SerializedProperty prop)
        {
            //gets parent type info
            string[] slices = prop.propertyPath.Split('.');
            object targetObj = prop.serializedObject.targetObject;

            foreach (Type eachType in ReflectUil.GetSelfAndBaseTypes(targetObj))
            {
                // foreach (FieldInfo field in type!.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                // {
                //     Debug.Log($"name={field.Name}");
                // }
                Type getType = eachType;

                for(int i = 0; i < slices.Length; i++)
                {
                    if (slices[i] == "Array")
                    {
                        i++; //skips "data[x]"
                        // type = type!.GetElementType(); //gets info on array elements
                        getType = getType.GetElementType()!;
                    }
                    else  //gets info on field and its type
                    {
                        // Debug.Log($"{slices[i]}, {type!.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)}");
                        FieldInfo field = getType!.GetField(slices[i],
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                            BindingFlags.Instance);
                        if (field != null)
                        {
                            return field.FieldType;
                        }
                        // getType =
                        //     !.FieldType;
                    }
                }

                //type is now the type of the property
                // return type;
            }

            throw new Exception($"Unable to get type from {targetObj}");

            // Type type = prop.serializedObject.targetObject.GetType()!;
            // Debug.Log($"{prop.propertyPath}, {type}");
            // foreach (FieldInfo field in type!.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            // {
            //     Debug.Log($"name={field.Name}");
            // }
            //
            // for(int i = 0; i < slices.Length; i++)
            // {
            //     if (slices[i] == "Array")
            //     {
            //         i++; //skips "data[x]"
            //         type = type!.GetElementType(); //gets info on array elements
            //     }
            //     else  //gets info on field and its type
            //     {
            //         Debug.Log($"{slices[i]}, {type!.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)}");
            //         type = type
            //             !.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
            //             !.FieldType;
            //     }
            // }
            //
            // //type is now the type of the property
            // return type;
        }

    }
}
