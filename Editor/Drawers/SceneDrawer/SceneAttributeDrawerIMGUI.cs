using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public partial class SceneAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool Changed;
            public object ChangedValue;
        }

        private readonly struct ScenePayload
        {
            public readonly string Name;
            public readonly int Index;
            public readonly bool IsAction;

            public ScenePayload(string name, int index, bool isAction = false)
            {
                Name = name;
                Index = index;
                IsAction = isAction;
            }
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
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
            InfoIMGUI cache = EnsureKey(property);
            if (cache.Changed)
            {
                cache.Changed = false;
                TriggerChangedIMGUI(property, cache.ChangedValue);
            }

            string[] scenes = SceneUtils.GetTrimedScenePath(((SceneAttribute)saintsAttribute).FullPath).ToArray();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                case SerializedPropertyType.Integer:
                    cache.Error = "";
                    DrawDropdown(position, property, label, scenes, cache);
                    break;
                default:
                    cache.Error = $"{property.name} must be an int or a string, get {property.propertyType}";
                    RawDefaultDrawer(position, property, allAttributes, label, info);
                    break;
            }
        }

        private void DrawDropdown(Rect position, SerializedProperty property, GUIContent label,
            IReadOnlyList<string> scenes, InfoIMGUI cache)
        {
            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            string display = GetDisplay(property, scenes);

            GUI.SetNextControlName(FieldControlName);
            if (!GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                Rect drawRect = new Rect(fieldRect)
                {
                    xMin = fieldRect.xMin + 6f,
                    xMax = fieldRect.xMax - 18f,
                };
                EditorGUI.LabelField(drawRect, display);
                return;
            }

            PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                GetMetaInfo(property, scenes, display),
                fieldRect.width,
                320f,
                false,
                (curItem, _) =>
                {
                    ScenePayload payload = (ScenePayload)curItem;
                    if (payload.IsAction)
                    {
                        SceneUtils.OpenBuildSettings();
                        return null;
                    }

                    object changedValue;
                    if (property.propertyType == SerializedPropertyType.String)
                    {
                        property.stringValue = payload.Name;
                        changedValue = payload.Name;
                    }
                    else
                    {
                        property.intValue = payload.Index;
                        changedValue = payload.Index;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                    cache.Changed = true;
                    cache.ChangedValue = changedValue;
                    return null;
                }));
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(SerializedProperty property, IReadOnlyList<string> scenes, string display)
        {
            AdvancedDropdownList<ScenePayload> dropdown = new AdvancedDropdownList<ScenePayload>();

            if (property.propertyType == SerializedPropertyType.String)
            {
                dropdown.Add("[Empty]", new ScenePayload("", -1));
                dropdown.AddSeparator();
            }

            foreach ((string sceneName, int index) in scenes.WithIndex())
            {
                dropdown.Add($"[{index}] {sceneName}", new ScenePayload(sceneName, index));
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", new ScenePayload("", -1, true), false, "d_editicon.sml");
            dropdown.SelfCompact();

            ScenePayload? selected = GetSelectedPayload(property, scenes);
            return new AdvancedDropdownMetaInfo
            {
                CurDisplay = display,
                CurValues = selected.HasValue ? new object[] { selected.Value } : Array.Empty<object>(),
                DropdownListValue = dropdown,
            };
        }

        private static ScenePayload? GetSelectedPayload(SerializedProperty property, IReadOnlyList<string> scenes)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                string current = property.stringValue;
                if (string.IsNullOrEmpty(current))
                {
                    return new ScenePayload("", -1);
                }

                int index = Array.IndexOf(scenes.ToArray(), current);
                return index >= 0 ? new ScenePayload(current, index) : null;
            }

            if (property.intValue >= 0 && property.intValue < scenes.Count)
            {
                return new ScenePayload(scenes[property.intValue], property.intValue);
            }

            return null;
        }

        private static string GetDisplay(SerializedProperty property, IReadOnlyList<string> scenes)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                string current = property.stringValue;
                if (string.IsNullOrEmpty(current))
                {
                    return "[Empty]";
                }

                int index = Array.IndexOf(scenes.ToArray(), current);
                return index >= 0 ? $"[{index}] {current}" : current;
            }

            int selectedIndex = property.intValue;
            return selectedIndex >= 0 && selectedIndex < scenes.Count
                ? $"[{selectedIndex}] {scenes[selectedIndex]}"
                : $"{selectedIndex}";
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
