using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtInspector.Standalone.Editor
{
    [CustomPropertyDrawer(typeof(GOAttribute))]
    public class GOPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private Texture _seeTexture;
        private Texture _hiddenTexture;

        private void InitTexture()
        {
            if (_seeTexture && _hiddenTexture)
            {
                return;
            }

            _seeTexture = (Texture)EditorGUIUtility.Load("ExtInspector/eye-regular.png");
            _hiddenTexture = (Texture)EditorGUIUtility.Load("ExtInspector/eye-slash-regular.png");

            // Debug.Log(EditorGUIUtility.Load("Util/eye-regular.png"));

            // _seeTexture = ((Texture) EditorGUIUtility.Load("Util/eye-regular.png"));
            // _hiddenTexture = ((Texture) EditorGUIUtility.Load("Util/eye-slash-regular.png"));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitTexture();

            Type fieldType = GetType(property);
            GOAttribute goAttribute = (GOAttribute)attribute;
            Type requiredComp = goAttribute.requiredComp;
            // bool fieldIsGo = fieldType == typeof(GameObject);

            Type editorType = requiredComp ?? fieldType;

            // bool requiredIsGo = requiredComp == typeof(GameObject);

            // EditorGUILayout.BeginHorizontal();
            float totalWidth = position.width;
            const float btnWidth = 30;
            Rect fieldRect = new Rect(position)
            {
                width = totalWidth - btnWidth,
            };
            // EditorGUI.PropertyField(fieldRect, property, label);

            Object editorValue;
            try
            {
                editorValue = (fieldType == typeof(GameObject), editorType == typeof(GameObject)) switch
                {
                    (true, true) => property.objectReferenceValue,
                    (false, false) => ((Component)property.objectReferenceValue)?.GetComponent(editorType),
                    (true, false) => ((GameObject)property.objectReferenceValue)?.GetComponent(editorType),
                    (false, true) => ((Component)property.objectReferenceValue)?.gameObject,
                };
            }
            catch (Exception)
            {
                Debug.Log($"{fieldType}/{editorType}/{property.objectReferenceValue}");
                throw;
            }

            EditorGUI.BeginChangeCheck();
            Object goCompResult  =
                EditorGUI.ObjectField(fieldRect, label, editorValue, editorType, true);
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue =
                    (editorType == typeof(GameObject), fieldType == typeof(GameObject)) switch
                    {
                        (true, true) => goCompResult,
                        (false, false) => ((Component)goCompResult)?.GetComponent(fieldType),
                        (true, false) => ((GameObject)goCompResult)?.GetComponent(fieldType),
                        (false, true) => ((Component)goCompResult)?.gameObject,
                    };
            }

            GameObject go;
            if (property.objectReferenceValue is GameObject isGO)
            {
                go = isGO;
            }
            else
            {
                go = ((Component) property.objectReferenceValue)?.gameObject;
            }
            bool isActive = go && go.activeSelf;

            EditorGUI.BeginDisabledGroup(go == null);

            EditorGUI.BeginChangeCheck();

            GUIStyle style = new GUIStyle("Button")
            {
                fixedWidth = btnWidth,
            };

            Rect toggleRect = new Rect(position)
            {
                x = fieldRect.x + fieldRect.width,
                width = btnWidth,
            };

            isActive = GUI.Toggle(toggleRect, isActive, isActive? _seeTexture: _hiddenTexture, style);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(go);
                go!.SetActive(isActive);
            }
            EditorGUI.EndDisabledGroup();

            // EditorGUILayout.EndHorizontal();
        }

        private static Type GetType(SerializedProperty prop)
        {
            //gets parent type info
            string[] slices = prop.propertyPath.Split('.');
            Object targetObj = prop.serializedObject.targetObject;

            foreach (Type eachType in GetSelfAndBaseTypes(targetObj))
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

        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new List<Type>
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
    }

}
