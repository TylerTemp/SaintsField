using System;
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
    public partial class ResourcePathAttributeDrawer
    {
        private static Texture2D _pickIcon;
        private GUIStyle _imGuiButtonStyle;

        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool HasCorrectValue;
            public string CorrectValue;
            public bool FreezeError;
            public string FrozenValue;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            if (property.propertyType != SerializedPropertyType.String)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                cache.Error = $"Expecting string, got {property.propertyType}";
                return;
            }

            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)saintsAttribute;
            Object currentValue = GetObjFromStr(property.stringValue, resourcePathAttribute.CompType, resourcePathAttribute.EStr);

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object fieldResult = EditorGUI.ObjectField(position, label, currentValue, resourcePathAttribute.CompType, false);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                if (!changed.changed)
                {
                    return;
                }

                string validateError = ValidateObject(fieldResult, resourcePathAttribute.EStr, resourcePathAttribute.RequiredTypes);
                if (validateError != "")
                {
                    if (resourcePathAttribute.FreeSign || !cache.HasCorrectValue)
                    {
                        cache.Error = validateError;
                        cache.FreezeError = true;
                        cache.FrozenValue = property.stringValue;
                    }
                    else
                    {
                        Debug.LogWarning($"{validateError} Change reverted to {(cache.CorrectValue == null ? "null" : cache.CorrectValue)}.");
                    }

                    return;
                }

                string result = GetNewValue(fieldResult, resourcePathAttribute.EStr);
                cache.Error = "";
                cache.HasCorrectValue = true;
                cache.CorrectValue = result;
                cache.FreezeError = false;
                property.stringValue = result;
                property.serializedObject.ApplyModifiedProperties();
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, result);
                TriggerChangedIMGUI(property, result);
            }
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)saintsAttribute;
            if (resourcePathAttribute.CustomPicker)
            {
                _imGuiButtonStyle ??= new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
                _pickIcon ??= Util.LoadResource<Texture2D>("d_pick");
                if (GUI.Button(position, new GUIContent(_pickIcon), _imGuiButtonStyle))
                {
                    OpenSelectorWindowIMGUI(property, resourcePathAttribute, info,
                        newValue => TriggerChangedIMGUI(property, newValue), parent);
                }
            }

            UpdateStatus(EnsureKey(property), property, resourcePathAttribute, info, parent);
            return resourcePathAttribute.CustomPicker;
        }

        private static void UpdateStatus(InfoIMGUI cache, SerializedProperty property,
            ResourcePathAttribute resourcePathAttribute, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                cache.Error = $"Expecting string, got {property.propertyType}";
                return;
            }

            if (cache.FreezeError && cache.FrozenValue == property.stringValue)
            {
                return;
            }

            cache.FreezeError = false;
            cache.Error = "";
            Object currentValue = GetObjFromStr(property.stringValue, resourcePathAttribute.CompType, resourcePathAttribute.EStr);
            string validateError = ValidateObject(currentValue, resourcePathAttribute.EStr, resourcePathAttribute.RequiredTypes);
            if (validateError == "")
            {
                cache.HasCorrectValue = true;
                cache.CorrectValue = property.stringValue;
                return;
            }

            if (resourcePathAttribute.FreeSign || !cache.HasCorrectValue)
            {
                cache.Error = validateError;
                return;
            }

            if (property.stringValue == cache.CorrectValue)
            {
                return;
            }

            property.stringValue = cache.CorrectValue;
            property.serializedObject.ApplyModifiedProperties();
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                cache.CorrectValue);
            TriggerChangedIMGUI(property, cache.CorrectValue);
            Debug.LogWarning(
                $"{validateError} Change reverted to {(cache.CorrectValue == null ? "null" : cache.CorrectValue)}.");
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, EnsureKey(property).Error, MessageType.Error);

        protected override Object GetCurFieldValue(SerializedProperty property, RequireTypeAttribute requireTypeAttribute)
        {
            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)requireTypeAttribute;
            return GetObjFromStr(property.stringValue, resourcePathAttribute.CompType, resourcePathAttribute.EStr);
        }

        protected override void OpenSelectorWindowIMGUI(SerializedProperty property,
            RequireTypeAttribute requireTypeAttribute, FieldInfo info, Action<object> onChangeCallback, object parent)
        {
            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)requireTypeAttribute;
            SaintsObjectPickerWindowIMGUI pickerWindow = ScriptableObject.CreateInstance<SaintsObjectPickerWindowIMGUI>();
            pickerWindow.ConfigAllowScene = false;
            pickerWindow.ConfigAllowAssets = true;
            pickerWindow.titleContent = new GUIContent(
                $"Select {string.Join(", ", resourcePathAttribute.RequiredTypes.Select(each => each.Name))}");
            pickerWindow.IsEqualCallback = IsEqual;
            pickerWindow.FetchAllSceneObjectFilterCallback = _ => false;
            pickerWindow.FetchAllAssetsFilterCallback = itemInfo =>
                ValidateObject(itemInfo.Object, resourcePathAttribute.EStr, resourcePathAttribute.RequiredTypes) == "";
            pickerWindow.OnSelectCallback = itemInfo =>
            {
                pickerWindow.ErrorMessage = "";
                if (ValidateObject(itemInfo.Object, resourcePathAttribute.EStr, resourcePathAttribute.RequiredTypes) != "")
                {
                    pickerWindow.ErrorMessage = $"{itemInfo.Label} is invalid";
                    return;
                }

                string result = GetNewValue(itemInfo.Object, resourcePathAttribute.EStr);
                InfoIMGUI cache = EnsureKey(property);
                cache.Error = "";
                cache.HasCorrectValue = true;
                cache.CorrectValue = result;
                cache.FreezeError = false;
                property.stringValue = result;
                property.serializedObject.ApplyModifiedProperties();
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, result);
                onChangeCallback(result);
            };
            pickerWindow.SetDefaultActive(GetCurFieldValue(property, resourcePathAttribute));
            pickerWindow.ShowAuxWindow();
        }

        private static bool IsEqual(SaintsObjectPickerWindowIMGUI.ItemInfo itemInfo, Object target)
        {
            Object itemObject = itemInfo.Object;
            Debug.Assert(itemObject, itemObject);

#if UNITY_6000_4_OR_NEWER
            EntityId targetInstanceId = target.GetEntityId();
#else
            int targetInstanceId = target.GetInstanceID();
#endif
            if (itemInfo.InstanceID == targetInstanceId)
            {
                return true;
            }

            return AssetDatabase.GetAssetPath(itemObject) == AssetDatabase.GetAssetPath(target);
        }

        private static string ValidateObject(Object obj, EStr editorStr, IEnumerable<Type> checkTypes)
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

            return missing.Length > 0
                ? $"target {obj} missing {string.Join(", ", missing.Select(t => t.Name))}."
                : "";
        }

        private static string GetNewValue(Object value, EStr eStr)
        {
            if (value == null)
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
                    pathParts[lastIndex] = Path.GetFileNameWithoutExtension(pathParts[lastIndex]);
                    return string.Join("/", pathParts);
                }

                case EStr.AssetDatabase:
                    return AssetDatabase.GetAssetPath(value);

                case EStr.Guid:
                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));

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

            Object obj;
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
