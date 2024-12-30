using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.OldGetter
{
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute))]
    public class GetScriptableObjectAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }

            (string error, Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
            if (error != "")
            {
                _error = error;
                return false;
            }
            if(result != null)
            {
                onGUIPayload.SetValue(result);
                if(ExpandableIMGUIScoop.IsInScoop)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);

        #endregion

        private static (string error, Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            SerializedProperty targetProperty = property;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type interfaceType = null;
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

                if (error == "" && propertyValue is IWrapProp wrapProp)
                {
                    Util.SaintsInterfaceInfo saintsInterfaceInfo = Util.GetSaintsInterfaceInfo(property, wrapProp);
                    if(saintsInterfaceInfo.Error != "")
                    {
                        return (saintsInterfaceInfo.Error, null);
                    }

                    fieldType = saintsInterfaceInfo.FieldType;
                    targetProperty = saintsInterfaceInfo.TargetProperty;
                    interfaceType = saintsInterfaceInfo.InterfaceType;

                    if (interfaceType != null && fieldType != typeof(ScriptableObject) && !fieldType.IsSubclassOf(typeof(ScriptableObject)) && typeof(ScriptableObject).IsSubclassOf(fieldType))
                    {
                        fieldType = typeof(ScriptableObject);
                    }
                }
            }

            if (targetProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                return ($"{targetProperty.propertyType} type is not supported by GetScriptableObject", null);
            }

            GetScriptableObjectAttribute getScriptableObjectAttribute = (GetScriptableObjectAttribute) saintsAttribute;

            string nameNoArray = fieldType.Name;
            if (nameNoArray.EndsWith("[]"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                nameNoArray = nameNoArray.Substring(0, nameNoArray.Length - 2);
            }

            IEnumerable<string> paths = AssetDatabase.FindAssets($"t:{nameNoArray}")
                .Select(AssetDatabase.GUIDToAssetPath);

            if (getScriptableObjectAttribute.PathSuffix != null)
            {
                paths = paths.Where(each => each.EndsWith(getScriptableObjectAttribute.PathSuffix));
            }
            Object[] results = paths
                .Select(each => AssetDatabase.LoadAssetAtPath(each, fieldType))
                // ReSharper disable once MergeConditionalExpression
                .Where(each => interfaceType == null? each != null: interfaceType.IsInstanceOfType(each))
                .ToArray();

            if (results.Length == 0)
            {
                return ($"Can not find {nameNoArray} type asset", null);
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (indexInArray == 0)
            {
                SerializedProperty arrayProp = SerializedUtils.GetArrayProperty(property).property;
                if (arrayProp.arraySize != results.Length)
                {
                    arrayProp.arraySize = results.Length;
                    arrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
            int useIndexInArray = indexInArray != -1 ? indexInArray: 0;

            if (useIndexInArray >= results.Length)
            {
                return ($"Out of range {nameNoArray} type asset{(indexInArray == -1 ? "": $"[{indexInArray}]")}", null);
            }

            Object result = results[useIndexInArray];

            if (!ReferenceEquals(targetProperty.objectReferenceValue, result))
            {
                targetProperty.objectReferenceValue = result;
            }

            return ("", result);
        }


        public static int HelperGetArraySize(GetScriptableObjectAttribute getScriptableObjectAttribute, FieldInfo info)
        {
            if (EditorApplication.isPlaying)
            {
                return -1;
            }

            Type fieldType = info.FieldType.IsGenericType? info.FieldType.GetGenericArguments()[0]: info.FieldType.GetElementType();
            if (fieldType == null)
            {
                return -1;
            }

            Type interfaceType = null;

            if (typeof(IWrapProp).IsAssignableFrom(fieldType))
            {
                Type mostBaseType = ReflectUtils.GetMostBaseType(fieldType);
                if (mostBaseType.IsGenericType && mostBaseType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
                {
                    IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
                    if (genericArguments.Count == 2)
                    {
                        fieldType = genericArguments[0];
                        interfaceType = genericArguments[1];
                    }
                }

                if (interfaceType != null && fieldType != typeof(ScriptableObject) && !fieldType.IsSubclassOf(typeof(ScriptableObject)) && typeof(ScriptableObject).IsSubclassOf(fieldType))
                {
                    fieldType = typeof(ScriptableObject);
                }
            }

            string nameNoArray = fieldType.Name;
            if (nameNoArray.EndsWith("[]"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                nameNoArray = nameNoArray.Substring(0, nameNoArray.Length - 2);
            }

            IEnumerable<string> paths = AssetDatabase.FindAssets($"t:{nameNoArray}")
                .Select(AssetDatabase.GUIDToAssetPath);

            if (getScriptableObjectAttribute.PathSuffix != null)
            {
                paths = paths.Where(each => each.EndsWith(getScriptableObjectAttribute.PathSuffix));
            }
            bool found = paths
                .Select(each => AssetDatabase.LoadAssetAtPath(each, fieldType))
                // ReSharper disable once MergeConditionalExpression
                .Any(each => interfaceType == null? each != null: interfaceType.IsInstanceOfType(each));
            return found ? 1 : 0;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetScriptableObject";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_SCRIPTABLE_OBJECT
            Debug.Log($"GetScriptableObject DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
            HelpBox helpBox = container.Q<HelpBox>(NamePlaceholder(property, index));
            if (error != helpBox.text)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

            // ReSharper disable once InvertIf
            if (result)
            {
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(result);
            }
        }

        // NOTE: ensure the post field is added to the container!
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NamePlaceholder(property, index),
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }
        #endregion

#endif
    }
}
