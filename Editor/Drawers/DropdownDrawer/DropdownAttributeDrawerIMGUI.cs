using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    public partial class DropdownAttributeDrawer
    {
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private static readonly Dictionary<string, object> AsyncChangedCache = new Dictionary<string, object>();

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            string key = GetKey(property);
            if (AsyncChangedCache.TryGetValue(key, out object changedValue))
            {
                onGUIPayload.SetValue(changedValue);
                AsyncChangedCache.Remove(key);
            }

            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            // Object target = property.serializedObject.targetObject;
            // Type parentType = parent.GetType();
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
                return;
            }

            bool hasLabel = label.text != "";
            float labelWidth = hasLabel? EditorGUIUtility.labelWidth: 0;
            Rect labelRect = new Rect(position)
            {
                width = labelWidth,
            };
            // (Rect labelRect, Rect fieldRect) = RectUtils.SplitWidthRect(position, labelWidth);
            //
            // EditorGUI.LabelField(labelRect, label);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            // int newIndex = EditorGUI.Popup(position, label, selectedIndex, options.Select(each => new GUIContent(each)).ToArray());
            GUI.SetNextControlName(FieldControlName);
            string curDisplay = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(curDisplay), FocusType.Keyboard))
            {
                ShowGenericMenu(metaInfo, curDisplay, fieldRect, (_, item) =>
                {
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, item);
                    Util.SignPropertyValue(property, info, parent, item);
                    property.serializedObject.ApplyModifiedProperties();
                    AsyncChangedCache[key] = item;
                    if(ExpandableIMGUIScoop.IsInScoop)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }

                }, !dropdownAttribute.SlashAsSub);
            }

            if(hasLabel)
            {
                ClickFocus(labelRect, FieldControlName);
            }

            // int newIndex = selectedIndex;
            // // ReSharper disable once InvertIf
            // if (changed.changed)
            // {
            //     Undo.RecordObject(target, "Dropdown");
            //     object newValue = values[newIndex];
            //     field.SetValue(target, newValue);
            // }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}
