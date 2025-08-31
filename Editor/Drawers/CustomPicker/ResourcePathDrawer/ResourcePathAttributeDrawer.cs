﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.ResourcePathDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ResourcePathAttribute), true)]
    public partial class ResourcePathAttributeDrawer: RequireTypeAttributeDrawer
    {
        private class FieldResourcesSelectWindow : SaintsObjectPickerWindowIMGUI
        {
            private IReadOnlyList<Type> _requiredTypes;
            // private Type _interfaceType;
            private Action<Object> _onSelected;
            private EStr _editorStr;

            public static void Open(Object curValue, EStr editorStr, IEnumerable<Type> requiredTypes, Action<Object> onSelected)
            {
                FieldResourcesSelectWindow thisWindow = CreateInstance<FieldResourcesSelectWindow>();
                thisWindow._requiredTypes = requiredTypes.ToArray();
                thisWindow.titleContent = new GUIContent($"Select {string.Join(", ", thisWindow._requiredTypes.Select(t => t.Name))}");
                thisWindow._onSelected = onSelected;
                thisWindow._editorStr = editorStr;
                thisWindow.SetDefaultActive(curValue);
                thisWindow.ShowAuxWindow();
            }

            protected override bool AllowScene => false;

            protected override bool AllowAssets => true;

            private string _error = "";
            protected override string Error => _error;

            protected override bool IsEqual(ItemInfo itemInfo, Object target)
            {
                Object itemObject = itemInfo.Object;
                Debug.Assert(itemObject, itemObject);

                int targetInstanceId = target.GetInstanceID();
                if (itemInfo.InstanceID == targetInstanceId)
                {
                    return true;
                }

                return AssetDatabase.GetAssetPath(itemObject) == AssetDatabase.GetAssetPath(target);
            }

            protected override void OnSelect(ItemInfo itemInfo)
            {
                _error = "";
                Object obj = itemInfo.Object;

                if(!FetchFilter(itemInfo))
                {
                    // Debug.LogError($"Selected object {obj} has no component {expectedType}");
                    _error = $"{itemInfo.Label} is invalid";
                    return;
                }
                _onSelected(obj);
            }

            protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => false;

            protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            private bool FetchFilter(ItemInfo itemInfo)  // gameObject, Sprite, Texture2D, ...
            {
                return ValidateObject(itemInfo.Object, _editorStr, _requiredTypes) == "";
                // if (itemInfo.Object == null)
                // {
                //     return true;
                // }
                //
                // if (_editorStr == EStr.Resource)
                // {
                //     string path = AssetDatabase.GetAssetPath(itemInfo.Object);
                //     if (!path.Contains("/Resources/"))
                //     {
                //         return false;
                //     }
                // }
                //
                // IEnumerable<Type> checkTypes = _requiredTypes;
                // if (itemInfo.Object is GameObject go)
                // {
                //     return checkTypes.All(requiredType => go.GetComponent(requiredType) != null);
                // }
                //
                // Type itemType = itemInfo.Object.GetType();
                // return checkTypes.All(requiredType => itemType.IsInstanceOfType(requiredType));
            }

            public static string ValidateObject(Object obj, EStr editorStr, IEnumerable<Type> checkTypes)
            {
                if (obj == null)
                {
                    return "";
                }

                if (editorStr == EStr.Resource)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!path.Contains("/Resources/"))
                    {
                        return "Target is not in Resources folder.";
                    }
                }

                Type[] missing = checkTypes.Where(requiredType => Util.GetTypeFromObj(obj, requiredType) == null).ToArray();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RESOURCE_PATH
                Debug.Log($"target [{obj}]: {string.Join(",", missing.Cast<object>())}");
#endif

                return missing.Length > 0 ? $"target {obj} missing {string.Join(", ", missing.Select(t => t.Name))}." : "";

                // if (obj is GameObject go)
                // {
                //     // return ;
                //     var missing = checkTypes.Where(requiredType => go.GetComponent(requiredType) == null).ToArray();
                //     return missing.Length > 0 ? $"Missing component {string.Join(", ", missing.Select(t => t.Name))}." : "";
                // }
                //
                // Type itemType = obj.GetType();
                // var missingSign = checkTypes.Where(requiredType => !itemType.IsInstanceOfType(requiredType)).ToArray();
                // return missingSign.Length > 0 ? $"Unable to sign to {itemType} with {string.Join(", ", missingSign.Select(t => t.Name))}." : "";
            }
        }

        #region IMGUI
        private string _previousValue;

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            _previousValue = property.stringValue;
            return -1;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label, info);
                _error = $"Expecting string, got {property.propertyType}";
                return;
            }

            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)saintsAttribute;
            Type requiredComp = resourcePathAttribute.CompType;
            // Debug.Log(requiredComp);
            EStr eStr = resourcePathAttribute.EStr;
            string curStrValue = property.stringValue;
            Object requiredValue = GetObjFromStr(curStrValue, requiredComp, eStr);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object fieldResult =
                    EditorGUI.ObjectField(position, label, requiredValue, requiredComp, false);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    string validateError =
                        FieldResourcesSelectWindow.ValidateObject(fieldResult, eStr,
                            resourcePathAttribute.RequiredTypes);
                    if(validateError != "")
                    {
                        if (!ImGuiFirstChecked || resourcePathAttribute.FreeSign)
                        {
                            // Debug.Log($"isFirstCheck={isFirstCheck}/freeSign={fieldInterfaceAttribute.FreeSign}");
                            _error = validateError;
                        }
                        else  // it's not freeSign, and you've already got a correct answer. So revert to the old value.
                        {
                            RestorePreviousValue(property, info, parent);
                            onGUIPayload.SetValue(GetPreviousValue());
                            Debug.LogWarning($"{validateError} Change reverted to {(_previousValue==null? "null": _previousValue)}.");
                        }
                    }
                    else
                    {
                        string result = GetNewValue(fieldResult, eStr);
                        // Debug.Log($"field change {fieldResult} -> {result}, null={result==null}");
                        property.stringValue = result;
                        // has issue on null, need to use reflection
                        // nah... not work... still get an empty string if it's null
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, result);
                        onGUIPayload.SetValue(result);
                        property.serializedObject.ApplyModifiedProperties();
                        // Debug.Log($"set value to {result}");
                    }
                }
            }
        }

        protected override void RestorePreviousValue(SerializedProperty property, FieldInfo info, object parent)
        {
            property.stringValue = _previousValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, _previousValue);
        }

        protected override object GetPreviousValue() => _previousValue;

        protected override Object GetCurFieldValue(SerializedProperty property, RequireTypeAttribute requireTypeAttribute)
        {
            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)requireTypeAttribute;
            return GetObjFromStr(property.stringValue, resourcePathAttribute.CompType, resourcePathAttribute.EStr);
        }

        protected override void OpenSelectorWindowIMGUI(SerializedProperty property, RequireTypeAttribute requireTypeAttribute, FieldInfo info, Action<object> onChangeCallback, object parent)
        {
            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)requireTypeAttribute;

            FieldResourcesSelectWindow.Open(GetObjFromStr(property.stringValue, resourcePathAttribute.CompType, resourcePathAttribute.EStr), resourcePathAttribute.EStr, resourcePathAttribute.RequiredTypes, fieldResult =>
            {
                // Debug.Log($"get new value {fieldResult}, null={fieldResult==null}");
                string result = GetNewValue(fieldResult, resourcePathAttribute.EStr);
                // Debug.Log($"get new value {fieldResult}, null={fieldResult==null}, result={result}, null={result==null}");
                property.stringValue = result;
                property.serializedObject.ApplyModifiedProperties();
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, result);
                onChangeCallback(result);
            });
        }
        #endregion

        private static string GetNewValue(Object value, EStr eStr)
        {
            if(value == null)
            {
                return null;
            }

            switch (eStr)
            {
                case EStr.Resource:
                {
                    string resourcePath = AssetDatabase.GetAssetPath(value);
                    List<string> pathParts = new List<string>();
                    foreach (string pathPart in resourcePath.Split('/'))
                    {
                        if (pathPart == "Resources")
                        {
                            pathParts.Clear();
                        }
                        else
                        {
                            pathParts.Add(pathPart);
                        }
                    }
                    Debug.Assert(pathParts.Count > 0);
                    int lastIndex = pathParts.Count - 1;
                    string last = pathParts[lastIndex];
                    pathParts[lastIndex] = Path.GetFileNameWithoutExtension(last);

                    return string.Join("/", pathParts);
                }

                case EStr.AssetDatabase:
                {
                    return AssetDatabase.GetAssetPath(value);
                }

                case EStr.Guid:
                {
                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(eStr), eStr, null);
            }

        }

        private static Object GetObjFromStr(string curStrValue, Type requiredType, EStr eStr)
        {
            if (string.IsNullOrEmpty(curStrValue))
            {
                return null;
            }

            Object obj = null;

            switch (eStr)
            {
                case EStr.Resource:
                    obj = Resources.Load(curStrValue);
                    break;
                case EStr.AssetDatabase:
                    obj = AssetDatabase.LoadAssetAtPath<Object>(curStrValue);
                    break;
                case EStr.Guid:
                    obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(curStrValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eStr), eStr, null);
            }

            return obj == null ? null : Util.GetTypeFromObj(obj, requiredType);
        }

    }
}
