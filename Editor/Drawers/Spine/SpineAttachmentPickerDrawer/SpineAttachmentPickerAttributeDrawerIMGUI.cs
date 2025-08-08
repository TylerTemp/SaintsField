using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineAttachmentPickerDrawer
{
    public partial class SpineAttachmentPickerAttributeDrawer
    {
        private class CachedImGui
        {
            public string Error = "";

            public bool Changed;
            public string ChangedValue;
        }

        private static readonly Dictionary<string, CachedImGui> CachedImGuiDictionary = new Dictionary<string, CachedImGui>();

        private readonly Dictionary<string, Texture2D> _cachedIconPathToTexture2D = new Dictionary<string, Texture2D>();

        private static CachedImGui EnsureCache(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if(!CachedImGuiDictionary.TryGetValue(key, out CachedImGui cachedImGui))
            {
                CachedImGuiDictionary[key] = cachedImGui = new CachedImGui();
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    CachedImGuiDictionary.Remove(key);
                });
            }

            return cachedImGui;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private Texture2D GetIcon(string iconPath)
        {
            if (_cachedIconPathToTexture2D.TryGetValue(iconPath, out Texture2D texture) && texture != null)
            {
                return texture;
            }

            return _cachedIconPathToTexture2D[iconPath] = Util.LoadResource<Texture2D>(iconPath);
        }


        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            CachedImGui cached = EnsureCache(property);
            if (cached.Changed)
            {
                cached.Changed = false;
                onGUIPayload.SetValue(cached.ChangedValue);
            }

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(property.stringValue)
                {
                    image = string.IsNullOrEmpty(property.stringValue)? null: GetIcon(IconAttachment),
                }, FocusType.Keyboard))
            {
                SpineAttachmentPickerAttribute spineAttachmentPickerAttribute = (SpineAttachmentPickerAttribute) saintsAttribute;

                SpineAttachmentUtils.AttachmentsResult attachmentsResult = GetAttachments(spineAttachmentPickerAttribute, property, info, parent);
                if (attachmentsResult.Error != "")
                {
                    cached.Error = attachmentsResult.Error;
                    return;
                }

                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, attachmentsResult, true);

                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(metaInfo.DropdownListValue, position.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    metaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        string newValue = (string)curItem;
                        if (property.stringValue != newValue)
                        {
                            property.stringValue = newValue;
                            property.serializedObject.ApplyModifiedProperties();
                            cached.Changed = true;
                            cached.ChangedValue = newValue;
                        }

                        cached.Error = "";
                    },
                    GetIcon);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureCache(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureCache(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureCache(property).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
