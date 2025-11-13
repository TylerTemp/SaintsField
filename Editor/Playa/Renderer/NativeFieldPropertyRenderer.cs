using System;
using System.Collections;
using SaintsField.Playa;
using UnityEditor;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.SaintsSerialization;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativeFieldPropertyRenderer: AbsRenderer
    {
        protected bool RenderField;

        public NativeFieldPropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            RenderField = fieldWithInfo.PlayaAttributes.Any(each => each is ShowInInspectorAttribute);
            if (RenderField && FieldWithInfo.PropertyInfo != null)
            {
                RenderField = FieldWithInfo.PropertyInfo.CanRead;
            }
        }

        public override void OnDestroy()
        {
        }

#if UNITY_2021_3_OR_NEWER
        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif
        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }

        private static (string error, object value) GetValue(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo != null)
            {
                // Debug.Log($"getting {fieldWithInfo.FieldInfo.Name}");
                return ("", fieldWithInfo.FieldInfo.GetValue(fieldWithInfo.Targets[0]));
            }

            if (fieldWithInfo.PropertyInfo.CanRead)
            {
                // Debug.Log($"getting {fieldWithInfo.PropertyInfo.Name}");
                try
                {
                    return ("", fieldWithInfo.PropertyInfo.GetValue(fieldWithInfo.Targets[0]));
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    string message = e.InnerException?.Message ?? e.Message;
                    return (message, null);
                }
            }

            return ("Can not get value", null);
        }

        private static string GetName(SaintsFieldWithInfo fieldWithInfo) =>
            fieldWithInfo.PropertyInfo?.Name ?? fieldWithInfo.FieldInfo.Name;

        private static string GetNiceName(SaintsFieldWithInfo fieldWithInfo) =>
            ObjectNames.NicifyVariableName(GetName(fieldWithInfo));

        private static Action<object> GetSetterOrNull(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo != null)
            {
                if (fieldWithInfo.FieldInfo.IsLiteral || fieldWithInfo.FieldInfo.IsInitOnly)
                {
                    return null;
                }
                return value =>
                {
                    fieldWithInfo.FieldInfo.SetValue(fieldWithInfo.Targets[0], value);
                    // Debug.Log(fieldWithInfo.SaintsSerializedProp?.propertyPath);
                    // SetAndApplySaintsSerialization(fieldWithInfo.SaintsSerializedProp, fieldWithInfo.FieldInfo.Name, value);
                };
            }

            if (fieldWithInfo.PropertyInfo.CanWrite)
            {
                return value => fieldWithInfo.PropertyInfo.SetValue(fieldWithInfo.Targets[0], value);
            }

            return null;

            // MethodInfo prop = fieldWithInfo.PropertyInfo?.GetSetMethod(true);
            // if (prop != null)
            // {
            //     return value => prop.Invoke(fieldWithInfo.Target, new[] {value});
            // }

            // return null;
        }

//         private static void SetAndApplySaintsSerialization(SerializedProperty saintsSerializedProp, string fieldInfoName, object value)
//         {
//             // if (EditorApplication.isPlayingOrWillChangePlaymode)
//             // {
//             //     return;
//             // }
//
//             if (saintsSerializedProp == null || !SerializedUtils.IsOk(saintsSerializedProp))
//             {
//                 // Debug.Log($"Not valid {saintsSerializedProp}");
//                 return;
//             }
//
//             SerializedProperty targetContainer = null;
//             for (int index = 0; index < saintsSerializedProp.arraySize; index++)
//             {
//                 SerializedProperty checkProp = saintsSerializedProp.GetArrayElementAtIndex(index);
//                 if (checkProp.FindPropertyRelative(nameof(SaintsSerializedProperty.name)).stringValue == fieldInfoName)
//                 {
//                     targetContainer = checkProp;
//                     break;
//                 }
//             }
//
//             if (targetContainer == null)
//             {
//                 // Debug.Log($"Nothing found for {fieldInfoName} in {saintsSerializedProp.propertyPath}");
//                 return;
//             }
//
//             if (targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue == (int)SaintsPropertyType.EnumLong)
//             {
//                 targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue)).longValue = Convert.ToInt64(value);
//                 targetContainer.serializedObject.ApplyModifiedProperties();
//             }
//             else if (targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue == (int)SaintsPropertyType.EnumULong)
//             {
//                 bool changed = false;
//                 if(targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.collectionType)).intValue == (int) CollectionType.Default)
//                 {
// #if UNITY_2022_1_OR_NEWER
//                     targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue)).ulongValue =
//                         Convert.ToUInt64(value);
//                     changed = true;
// #endif
//                 }
//                 else
//                 {
//                     SerializedProperty uLongValuesProp = targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValues));
//                     int arraySize = uLongValuesProp.arraySize;
//                     int index = 0;
//                     foreach (object v in (IEnumerable)value)
//                     {
//                         if (index >= arraySize)
//                         {
//                             uLongValuesProp.InsertArrayElementAtIndex(index);
//                             changed = true;
//                         }
//
// #if UNITY_2022_1_OR_NEWER
//                         SerializedProperty targetElement = uLongValuesProp.GetArrayElementAtIndex(index);
//                         ulong targetNewValue = Convert.ToUInt64(v);
//                         if (targetElement.ulongValue != targetNewValue)
//                         {
//                             targetElement.ulongValue = targetNewValue;
//                             changed = true;
//                         }
// #endif
//                         index++;
//                     }
//
//                     if (index < uLongValuesProp.arraySize)
//                     {
//                         uLongValuesProp.arraySize = index;
//                         changed = true;
//                     }
//                 }
//
//                 if(changed)
//                 {
//                     targetContainer.serializedObject.ApplyModifiedProperties();
//                 }
//             }
//             else if (targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue == (int)SaintsPropertyType.EnumLong)
//             {
//                 bool changed = false;
//                 if(targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.collectionType)).intValue == (int) CollectionType.Default)
//                 {
//                     targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue)).longValue = Convert.ToInt64(value);
//                     changed = true;
//                 }
//                 else
//                 {
//                     SerializedProperty longValuesProp = targetContainer.FindPropertyRelative(nameof(SaintsSerializedProperty.longValues));
//                     int arraySize = longValuesProp.arraySize;
//                     int index = 0;
//                     foreach (object v in (IEnumerable)value)
//                     {
//                         if (index >= arraySize)
//                         {
//                             longValuesProp.InsertArrayElementAtIndex(index);
//                             changed = true;
//                         }
//
// #if UNITY_2022_1_OR_NEWER
//                         SerializedProperty targetElement = longValuesProp.GetArrayElementAtIndex(index);
//                         long targetNewValue = Convert.ToInt64(v);
//                         if (targetElement.longValue != targetNewValue)
//                         {
//                             targetElement.longValue = targetNewValue;
//                             changed = true;
//                         }
// #endif
//                         index++;
//                     }
//
//                     if (index < longValuesProp.arraySize)
//                     {
//                         longValuesProp.arraySize = index;
//                         changed = true;
//                     }
//                 }
//
//                 if(changed)
//                 {
//                     targetContainer.serializedObject.ApplyModifiedProperties();
//                 }
//             }
//         }

        private static Type GetFieldType(SaintsFieldWithInfo fieldWithInfo) =>
            fieldWithInfo.FieldInfo?.FieldType ?? fieldWithInfo.PropertyInfo.PropertyType;

        public override string ToString()
        {
            return $"<NativeFP {GetFriendlyName(FieldWithInfo)}/>";
        }
    }
}
