using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSceneDrawer
{
    public partial class AddressableSceneAttributeDrawer
    {
        private class InfoIMGUI
        {
            public string Error = "";

            public bool Changed;
            public string ChangedValue;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheImGui = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureInfo(SerializedProperty property, string key)
        {
            if (InfoCacheImGui.TryGetValue(key, out InfoIMGUI ensureInfo))
            {
                return ensureInfo;
            }

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoCacheImGui.Remove(key);
            });

            return InfoCacheImGui[key] = new InfoIMGUI();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)saintsAttribute;
            (string error, AddressableAssetEntry sceneEntry) = GetSceneEntry(property.stringValue, addressableSceneAttribute);
            InfoIMGUI cachedInfo = EnsureInfo(property, SerializedUtils.GetUniqueId(property));
            if (cachedInfo.Changed)
            {
                onGUIPayload.SetValue(cachedInfo.ChangedValue);
                cachedInfo.Changed = false;
            }

            if (error != "")
            {
                cachedInfo.Error = error;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object newObj = EditorGUI.ObjectField(position, label, sceneEntry.MainAsset as SceneAsset, typeof(SceneAsset), false);
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
                        property.stringValue = newSceneEntry.address;
                        onGUIPayload.SetValue(newSceneEntry.address);
                    }
                }
            }
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return SingleLineHeight;
        }

        private static Texture2D _dropdownIconGray;
        private static Texture2D DropdownIconGray {
            get
            {
                if (_dropdownIconGray == null)
                {
                    _dropdownIconGray = Util.LoadResource<Texture2D>("classic-dropdown-gray.png");
                }

                return _dropdownIconGray;
            }
        }

        private static GUIStyle _buttonStyle;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_buttonStyle is null)
            {
                _buttonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(2, 2, 2, 2),
                };
            }

            // ReSharper disable once InvertIf
            if (GUI.Button(position, DropdownIconGray, _buttonStyle))
            {
                AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)saintsAttribute;

                InfoIMGUI cachedInfo = EnsureInfo(property, SerializedUtils.GetUniqueId(property));

                (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
                if (error != "")
                {
                    cachedInfo.Error = error;
                    return true;
                }

                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, assetGroups.Where(each => each.MainAsset is SceneAsset), addressableSceneAttribute.SepAsSub, true);
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(metaInfo.DropdownListValue, fullRect.width);
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    metaInfo.DropdownListValue,
                    size,
                    fullRect,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        AddressableAssetEntry entry = (AddressableAssetEntry)curItem;
                        string newValue = entry?.address ?? "";
                        property.stringValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();

                        cachedInfo.Error = "";
                        cachedInfo.Changed = true;
                        cachedInfo.ChangedValue = newValue;
                    },
                    _ => null);
                dropdown.Show(position);
                dropdown.BindWindowPosition();

            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            InfoIMGUI cachedInfo = EnsureInfo(property, SerializedUtils.GetUniqueId(property));
            return cachedInfo.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureInfo(property, SerializedUtils.GetUniqueId(property));
            return cachedInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureInfo(property, SerializedUtils.GetUniqueId(property));
            return cachedInfo.Error == ""
                ? position
                : ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
