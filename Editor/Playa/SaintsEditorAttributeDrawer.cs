using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
#if SAINTSFIELD_DOTWEEN
using DG.DOTweenEditor;
#endif

namespace SaintsField.Editor.Playa
{
    [CustomPropertyDrawer(typeof(SaintsEditorAttribute))]
    public class SaintsEditorAttributeDrawer: PropertyDrawer
    {

#if SAINTSFIELD_DOTWEEN
        private static readonly HashSet<SaintsEditorAttributeDrawer> AliveInstances = new HashSet<SaintsEditorAttributeDrawer>();
        private void RemoveInstance()
        {
            AliveInstances.Remove(this);
            if (AliveInstances.Count == 0)
            {
                DOTweenEditorPreview.Stop();
            }
        }
#endif
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Debug.Log($"Drawer: {property.serializedObject}");
            // Debug.Log($"Drawer: {property.serializedObject.targetObject}");
            // Debug.Log($"Drawer.type: {property.type}");
            // Debug.Log($"Drawer.propertyType: {property.propertyType}");
            // Debug.Log($"Drawer.propertyPath: {property.propertyPath}");
            // Debug.Log($"field.DeclaringType: {fieldInfo.DeclaringType}");
            // Debug.Log($"field.FieldType: {fieldInfo.FieldType}");
            // Debug.Log($"field.GetValue: {fieldInfo.GetValue(property.serializedObject.targetObject)}");
            // var value = fieldInfo.GetValue(property.serializedObject.targetObject);
            //
            // MethodInfo[] methodInfos = fieldInfo.FieldType
            //     .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
            //                 BindingFlags.Public | BindingFlags.DeclaredOnly);
            // foreach (MethodInfo methodInfo in methodInfos)
            // {
            //     Debug.Log($"methodInfo: {methodInfo.Name}");
            //     methodInfo.Invoke(value, null);
            // }
            var value = fieldInfo.GetValue(property.serializedObject.targetObject);

            VisualElement root = new VisualElement();
            var renderer = SaintsEditor.Setup(true, property.serializedObject, fieldInfo.GetValue(property.serializedObject.targetObject));

//             VisualElement root = SaintsEditor.CreateVisualElement(false, property.serializedObject, property.serializedObject.targetObject);
// #if SAINTSFIELD_DOTWEEN
//             root.RegisterCallback<AttachToPanelEvent>(_ => AliveInstances.Add(this));
//             root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance());
// #endif
            return root;
        }

        // public static void SetValueDirect(SerializedProperty property, object value)
        // {
        //     object obj = property.serializedObject.targetObject;
        //     string propertyPath = property.propertyPath;
        //     var flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //     var paths = propertyPath.Split('.');
        //     FieldInfo field = null;
        //
        //     for (int i = 0; i < paths.Length; i++)
        //     {
        //         var path = paths[i];
        //         if (obj == null)
        //             throw new System.NullReferenceException("Can't set a value on a null instance");
        //
        //         var type = obj.GetType();
        //         if (path == "Array")
        //         {
        //             path = paths[++i];
        //             var iter = (obj as System.Collections.IEnumerable);
        //             if (iter == null)
        //                 //Property path thinks this property was an enumerable, but isn't. property path can't be parsed
        //                 throw new System.ArgumentException("SerializedProperty.PropertyPath [" + propertyPath + "] thinks that [" + paths[i-2] + "] is Enumerable.");
        //
        //             var sind = path.Split('[', ']');
        //             int index = -1;
        //
        //             if (sind == null || sind.Length < 2)
        //                 // the array string index is malformed. the property path can't be parsed
        //                 throw new System.FormatException("PropertyPath [" + propertyPath + "] is malformed");
        //
        //             if (!Int32.TryParse(sind[1], out index))
        //                 //the array string index in the property path couldn't be parsed,
        //                 throw new System.FormatException("PropertyPath [" + propertyPath + "] is malformed");
        //
        //             obj = iter.ElementAtOrDefault(index);
        //             continue;
        //         }
        //
        //         field = type.GetField(path, flag);
        //         if (field == null)
        //             //field wasn't found
        //             throw new System.MissingFieldException("The field ["+path+"] in ["+propertyPath+"] could not be found");
        //
        //         if(i< paths.Length-1)
        //             obj = field.GetValue(obj);
        //
        //     }
        //
        //     var valueType = value.GetType();
        //
        //     field.GetValue(obj, value);
        // }

#endif
    }
}
