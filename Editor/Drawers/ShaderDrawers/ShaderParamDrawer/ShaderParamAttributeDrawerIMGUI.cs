#if UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public partial class ShaderParamAttributeDrawer
    {
        private class ShaderParamInfoIMGUI
        {
            public string Error;
            public bool Changed;
            public object ChangedValue;
        }

        private static readonly Dictionary<string, ShaderParamInfoIMGUI> CachedIMGUI = new Dictionary<string, ShaderParamInfoIMGUI>();

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!CachedIMGUI.ContainsKey(key))
            {
                CachedIMGUI[key] = new ShaderParamInfoIMGUI
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
            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderParamInfoIMGUI infoIMGUI))
            {
                return;
            }

            if(property.propertyType != SerializedPropertyType.String &&
               property.propertyType != SerializedPropertyType.Integer)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            if (infoIMGUI.Changed)
            {
                onGUIPayload.SetValue(infoIMGUI.ChangedValue);
                infoIMGUI.Changed = false;
            }

            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute) saintsAttribute;
            (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, info, parent);
            infoIMGUI.Error = error;
            if (error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            ShaderInfo[] shaderInfos = GetShaderInfo(shader, shaderParamAttribute.PropertyType).ToArray();
            (bool foundShaderInfo, ShaderInfo selectedShaderInfo) = GetSelectedShaderInfo(property, GetShaderInfo(shader, shaderParamAttribute.PropertyType));

            Rect dropdownButtonRect = EditorGUI.PrefixLabel(position, label);
            if(EditorGUI.DropdownButton(dropdownButtonRect, new GUIContent(foundShaderInfo? selectedShaderInfo.ToString(): "-"), FocusType.Keyboard))
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(foundShaderInfo, selectedShaderInfo, shaderInfos, true);
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(dropdownMetaInfo.DropdownListValue, position.width);
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownMetaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        ShaderInfo shaderInfo = (ShaderInfo) curItem;
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            property.stringValue = shaderInfo.PropertyName;
                            property.serializedObject.ApplyModifiedProperties();
                            infoIMGUI.Changed = true;
                            infoIMGUI.ChangedValue = shaderInfo.PropertyName;
                        }
                        else if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            property.intValue = shaderInfo.PropertyID;
                            property.serializedObject.ApplyModifiedProperties();
                            infoIMGUI.Changed = true;
                            infoIMGUI.ChangedValue = shaderInfo.PropertyID;
                        }
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

            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderParamInfoIMGUI infoIMGUI))
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

            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderParamInfoIMGUI infoIMGUI) || infoIMGUI.Error == "")
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

            if(!CachedIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out ShaderParamInfoIMGUI infoIMGUI) || infoIMGUI.Error == "")
            {
                return position;
            }

            return ImGuiHelpBox.Draw(position, infoIMGUI.Error, MessageType.Error);
        }
    }
}
#endif
