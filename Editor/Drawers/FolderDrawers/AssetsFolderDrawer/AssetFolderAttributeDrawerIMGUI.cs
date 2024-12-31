using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
    public partial class AssetFolderAttributeDrawer
    {
        private static Texture2D _folderIcon;

        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private class CacheInfo
        {
            public string ChangedValue;
            public string Error = "";
        }

        private static readonly Dictionary<string, CacheInfo> AsyncCacheInfo = new Dictionary<string, CacheInfo>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return property.propertyType == SerializedPropertyType.String
                ? SingleLineHeight
                : 0;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return false;
            }

            string key = GetKey(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo) && cacheInfo.ChangedValue != null)
            {
                onGUIPayload.SetValue(cacheInfo.ChangedValue);
                AsyncCacheInfo.Remove(key);
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
                    AsyncCacheInfo[key] = new CacheInfo { Error = error };
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            string key = GetKey(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
            {
                return cacheInfo.Error != "";
            }

            return false;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string key = GetKey(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
            {
                return ImGuiHelpBox.GetHeight(cacheInfo.Error, width, MessageType.Error);
            }

            return 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string key = GetKey(property);
            if (AsyncCacheInfo.TryGetValue(key, out CacheInfo cacheInfo))
            {
                return ImGuiHelpBox.Draw(position, cacheInfo.Error, MessageType.Error);
            }

            return position;
        }
    }
}
