#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public partial class ShaderKeywordAttributeDrawer
    {
        private class ShaderKeywordInfoIMGUI
        {
            public string Error;
            public bool Changed;
            public string ChangedValue;
        }

        private static readonly Dictionary<string, ShaderKeywordInfoIMGUI> CachedIMGUI = new Dictionary<string, ShaderKeywordInfoIMGUI>();

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!CachedIMGUI.ContainsKey(key))
            {
                CachedIMGUI[key] = new ShaderKeywordInfoIMGUI
                {
                    Error = "",
                };

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    CachedIMGUI.Remove(key);
                });
            }

            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderKeywordInfoIMGUI infoIMGUI))
            {
                return;
            }

            if(property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            if (infoIMGUI.Changed)
            {
                onGUIPayload.SetValue(infoIMGUI.ChangedValue);
                infoIMGUI.Changed = false;
            }

            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute) saintsAttribute;
            (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
            infoIMGUI.Error = error;
            if (error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            string[] shaderKeywords = GetShaderKeywords(shader).ToArray();
            int selectedIndex = Array.IndexOf(shaderKeywords, property.stringValue);
            Rect dropdownButtonRect = EditorGUI.PrefixLabel(position, label);
            // ReSharper disable once InvertIf
            if(EditorGUI.DropdownButton(dropdownButtonRect, new GUIContent(selectedIndex >= 0? shaderKeywords[selectedIndex]: "-"), FocusType.Keyboard))
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(selectedIndex, shaderKeywords, true);
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(dropdownMetaInfo.DropdownListValue, position.width);
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownMetaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        string shaderKeyword = (string) curItem;
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        property.stringValue = shaderKeyword;
                        property.serializedObject.ApplyModifiedProperties();
                        infoIMGUI.Changed = true;
                        infoIMGUI.ChangedValue = shaderKeyword;
                    },
                    _ => null);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            string mismatch = GetTypeMismatchError(property);
            if (mismatch != "")
            {
                return true;
            }

            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderKeywordInfoIMGUI infoIMGUI))
            {
                return false;
            }

            return infoIMGUI.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = GetTypeMismatchError(property);
            if (error != "")
            {
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderKeywordInfoIMGUI infoIMGUI) || infoIMGUI.Error == "")
            {
                return 0;
            }

            return ImGuiHelpBox.GetHeight(infoIMGUI.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = GetTypeMismatchError(property);
            if (error != "")
            {
                return ImGuiHelpBox.Draw(position, error, MessageType.Error);
            }

            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderKeywordInfoIMGUI infoIMGUI) || infoIMGUI.Error == "")
            {
                return position;
            }

            return ImGuiHelpBox.Draw(position, infoIMGUI.Error, MessageType.Error);
        }
    }
}
#endif
