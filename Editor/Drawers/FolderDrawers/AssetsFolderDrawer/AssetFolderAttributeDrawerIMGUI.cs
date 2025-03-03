using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
    public partial class AssetFolderAttributeDrawer
    {
        private static Texture2D _folderIcon;

        // private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private class CacheInfo
        {
            public string ChangedValue;
            public string Error = "";
            // public string OldValue;
        }

        private static readonly Dictionary<string, CacheInfo> AsyncCacheInfo = new Dictionary<string, CacheInfo>();

        private static readonly Dictionary<Object, HashSet<string>> InspectingTargets = new Dictionary<Object, HashSet<string>>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return property.propertyType == SerializedPropertyType.String
                ? SingleLineHeight
                : 0;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return false;
            }

            string key = SerializedUtils.GetUniqueId(property);

            Object curInspectingTarget = property.serializedObject.targetObject;

            if (!InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> keySet))
            {
                InspectingTargets[curInspectingTarget] = keySet = new HashSet<string>();

                void OnSelectionChangedIMGUI()
                {
                    bool stillSelected = Array.IndexOf(Selection.objects, curInspectingTarget) != -1;
                    // Debug.Log($"{stillSelected}/{string.Join(", ", Selection.objects.Cast<Object>())}");
                    if (stillSelected)
                    {
                        return;
                    }

                    Selection.selectionChanged -= OnSelectionChangedIMGUI;
                    if (InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> set))
                    {
                        foreach (string removeKey in set)
                        {
                            // Debug.Log($"remove key {removeKey}");
                            AsyncCacheInfo.Remove(removeKey);
                        }
                    }
                    InspectingTargets.Remove(curInspectingTarget);
                }

                Selection.selectionChanged += OnSelectionChangedIMGUI;
            }
            keySet.Add(key);

            if (onGUIPayload.changed)
            {
                if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
                {
                    cacheInfo.Error = "";
                }
            }
            else
            {
                if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
                {
                    if(cacheInfo.ChangedValue != null)
                    {
                        onGUIPayload.SetValue(cacheInfo.ChangedValue);
                        AsyncCacheInfo.Remove(key);
                    }
                }
            }

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_folderIcon is null)
            {
                _folderIcon = Util.LoadResource<Texture2D>("folder.png");
            }

            AssetFolderAttribute folderAttribute = (AssetFolderAttribute)saintsAttribute;

            // ReSharper disable once InvertIf
            if(GUI.Button(new Rect(position)
               {
                   x = position.x + 1,
                   width = position.width - 2,
               }, _folderIcon, GUIStyle.none))
            {
                (string error, string actualFolder) = OnClick(property, folderAttribute);
                if(error == "")
                {
                    // ReSharper disable once InvertIf
                    if(actualFolder != "")
                    {
                        property.stringValue = actualFolder;
                        property.serializedObject.ApplyModifiedProperties();
                        // onGUIPayload.SetValue(actualFolder);
                        AsyncCacheInfo[key] = new CacheInfo { ChangedValue = actualFolder };
                    }
                }
                else
                {
                    // Debug.Log($"add error key {key} = {error}");
                    AsyncCacheInfo[key] = new CacheInfo { Error = error };
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return true;
            }
            string key = SerializedUtils.GetUniqueId(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
            {
                return cacheInfo.Error != "";
            }

            return false;
        }

        private static string GetMismatchError(SerializedProperty property) => $"target {property.propertyPath} is not a string: {property.propertyType}";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return ImGuiHelpBox.GetHeight(GetMismatchError(property), width, MessageType.Error);
            }

            string key = SerializedUtils.GetUniqueId(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo) && cacheInfo.Error != "")
            {
                return ImGuiHelpBox.GetHeight(cacheInfo.Error, width, MessageType.Error);
            }

            return 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return ImGuiHelpBox.Draw(position, GetMismatchError(property), MessageType.Error);
            }

            string key = SerializedUtils.GetUniqueId(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
            {
                return ImGuiHelpBox.Draw(position, cacheInfo.Error, MessageType.Error);
            }

            return position;
        }
    }
}
