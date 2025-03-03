using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineSkinPickerDrawer
{
    public partial class SpineSkinPickerAttributeDrawer
    {
        private class CachedImGui
        {
            public string Error = "";

            public bool Changed;
            public string ChangedValue;
        }

        private static readonly Dictionary<string, CachedImGui> _cachedImGui = new Dictionary<string, CachedImGui>();

        private CachedImGui EnsureCache(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if(!_cachedImGui.TryGetValue(key, out CachedImGui cachedImGui))
            {
                _cachedImGui[key] = cachedImGui = new CachedImGui();
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    _cachedImGui.Remove(key);
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

        private static Texture2D _iconSkin;

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

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_iconSkin is null)
            {
                _iconSkin = Util.LoadResource<Texture2D>(IconPath);
            }

            if (EditorGUI.DropdownButton(leftRect, new GUIContent(property.stringValue)
                {
                    image = string.IsNullOrEmpty(property.stringValue)? null: _iconSkin,
                }, FocusType.Keyboard))
            {
                SpineSkinPickerAttribute spineSkinPickerAttribute = (SpineSkinPickerAttribute) saintsAttribute;

                (string error, ExposedList<Skin> skins) = GetSkins(spineSkinPickerAttribute.SkeletonTarget, property, info, parent);
                if (error != "")
                {
                    cached.Error = error;
                    return;
                }

                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, skins, true);

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
                    _ => _iconSkin);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureCache(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
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
