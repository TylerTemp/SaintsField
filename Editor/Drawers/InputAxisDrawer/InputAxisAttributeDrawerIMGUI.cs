using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public partial class InputAxisAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Display = "";
            public IReadOnlyList<string> AxisNames = Array.Empty<string>();
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            cache = new InfoIMGUI
            {
                AxisNames = InputAxisUtils.GetAxisNames(),
            };
            InfoCacheIMGUI[key] = cache;

            void RefreshAxisNames()
            {
                cache.AxisNames = InputAxisUtils.GetAxisNames();
            }

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshAxisNames);
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshAxisNames);
                InfoCacheIMGUI.Remove(key);
            });

            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            InfoIMGUI cache = EnsureKey(property);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetMetaInfo(property, cache, out cache.Display),
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        string curValue = (string)curItem;
                        if (curValue == null)
                        {
                            InputAxisUtils.OpenInputManager();
                            return null;
                        }

                        property.stringValue = curValue;
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, curValue);
                        cache.Display = GetDisplay(curValue, cache.AxisNames);
                        return new[] { curValue };
                    }));
            }
            else
            {
                cache.Display = GetDisplay(property.stringValue, cache.AxisNames);
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, RichTextDrawer.ParseRichXmlWithProvider(cache.Display, this));
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(SerializedProperty property, InfoIMGUI cache, out string display)
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();

            string selectedName = null;
            foreach (string axisName in cache.AxisNames)
            {
                dropdown.Add(axisName, axisName);
                if (axisName == property.stringValue)
                {
                    selectedName = axisName;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Open Input Manager...", null, false, "d_editicon.sml");

            display = GetDisplay(property.stringValue, cache.AxisNames);
            return new AdvancedDropdownMetaInfo
            {
                CurValues = selectedName is null ? Array.Empty<object>() : new object[] { selectedName },
                DropdownListValue = dropdown,
            };
        }

        private static string GetDisplay(string value, IReadOnlyList<string> axisNames)
        {
            foreach (string axisName in axisNames)
            {
                if (axisName == value)
                {
                    return value;
                }
            }

            return string.IsNullOrEmpty(value) ? "" : $"<color=red>?</color> ({value})";
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            property.propertyType != SerializedPropertyType.String
                ? ImGuiHelpBox.GetHeight($"Type {property.propertyType} is not string.", width, MessageType.Error)
                : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) => ImGuiHelpBox.Draw(position,
            $"Type {property.propertyType} is not string.", MessageType.Error);
    }
}
