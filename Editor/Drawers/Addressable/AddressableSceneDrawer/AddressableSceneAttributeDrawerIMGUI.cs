using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSceneDrawer
{
    public partial class AddressableSceneAttributeDrawer
    {
        private class InfoIMGUI
        {
            public string Error = "";
            public Texture2D DropdownIconGray;
            public GUIStyle ButtonStyle;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI ensureInfo))
            {
                return ensureInfo;
            }

            InfoCacheIMGUI[key] = ensureInfo = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return ensureInfo;
        }

        private static (string error, AddressableAssetEntry sceneEntry) UpdateStatus(
            SerializedProperty property, AddressableSceneAttribute addressableSceneAttribute, out InfoIMGUI cache)
        {
            cache = EnsureKey(property);
            (string error, AddressableAssetEntry sceneEntry) = GetSceneEntry(property.stringValue, addressableSceneAttribute);
            cache.Error = error;
            return (error, sceneEntry);
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            UpdateStatus(property, (AddressableSceneAttribute)saintsAttribute, out _);
            return SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)saintsAttribute;
            (string _, AddressableAssetEntry sceneEntry) =
                UpdateStatus(property, addressableSceneAttribute, out InfoIMGUI cachedInfo);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object newObj = EditorGUI.ObjectField(position, label, sceneEntry?.MainAsset as SceneAsset,
                    typeof(SceneAsset), false);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    (string newError, AddressableAssetEntry newSceneEntry) = GetSceneEntryFromSceneAsset(newObj, addressableSceneAttribute);
                    if (newError != "")
                    {
                        cachedInfo.Error = newError;
                    }
                    else
                    {
                        cachedInfo.Error = "";
                        string newValue = newSceneEntry?.address ?? "";
                        ApplyAddressableSceneSelection(property, info, parent, newValue,
                            changedValue => TriggerChangedIMGUI(property, changedValue));
                    }
                }
            }
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            cachedInfo.ButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(2, 2, 2, 2),
            };
            cachedInfo.DropdownIconGray ??= Util.LoadResource<Texture2D>("classic-dropdown-gray.png");

            // ReSharper disable once InvertIf
            if (GUI.Button(position, cachedInfo.DropdownIconGray, cachedInfo.ButtonStyle))
            {
                AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)saintsAttribute;

                (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
                if (error != "")
                {
                    cachedInfo.Error = error;
                    return true;
                }

                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, assetGroups.Where(each => each.MainAsset is SceneAsset), addressableSceneAttribute.SepAsSub, true);
                PopupWindow.Show(position, new SaintsTreeDropdownIMGUI(
                    metaInfo,
                    Mathf.Max(fullRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        AddressableAssetEntry entry = (AddressableAssetEntry)curItem;
                        string newValue = entry?.address ?? "";
                        cachedInfo.Error = "";
                        ApplyAddressableSceneSelection(property, info, parent, newValue,
                            changedValue => TriggerChangedIMGUI(property, changedValue));
                        return null;
                    }));

            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)saintsAttribute;
            return UpdateStatus(property, addressableSceneAttribute, out _).error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            return cachedInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            return cachedInfo.Error == ""
                ? position
                : ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
