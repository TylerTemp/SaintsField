using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public partial class SceneAttributeDrawer
    {
        private string _error = "";

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
            string[] scenes = SceneUtils.GetTrimedScenePath(((SceneAttribute)saintsAttribute).FullPath).ToArray();

            // const string optionName = "Edit Scenes In Build...";
            // string[] sceneOptions = scenes
            //     .Select((name, index) => $"{index}: {name}")
            //     // .Concat(scenes.Length > 0? new[]{"", optionName}: new[]{optionName})
            //     .ToArray();

            _error = "";
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, scenes);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, scenes);
                    break;
                default:
                    _error = $"{property.name} must be an int or a string, get {property.propertyType}";
                    DefaultDrawer(position, property, label, info);
                    break;
            }
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label,
            string[] scenes)
        {
            int index = IndexOfOrZero(scenes, property.stringValue);

            if (string.IsNullOrEmpty(property.stringValue) && scenes.Length > 0)
            {
                property.stringValue = scenes[0];
            }

            DrawDropdown(scenes, index, property.stringValue, rect, label, (scene, _) =>
            {
                property.stringValue = scene;
                property.serializedObject.ApplyModifiedProperties();
            });

            // // ReSharper disable once ConvertToUsingDeclaration
            // using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            // {
            //     int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions
            //         .Concat(new[] { "", "Edit Scenes In Build..." })
            //         .ToArray());
            //     // ReSharper disable once InvertIf
            //     if (changeCheck.changed)
            //     {
            //         if (newIndex >= sceneOptions.Length)
            //         {
            //             OpenBuildSettings();
            //             return;
            //         }
            //
            //         property.stringValue = scenes[newIndex];
            //     }
            // }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label,
            string[] scenes)
        {
            int index = property.intValue;

            DrawDropdown(scenes, index, $"{property.intValue}", rect, label, (_, sceneIndex) =>
            {
                property.intValue = sceneIndex;
                property.serializedObject.ApplyModifiedProperties();
            });

            // ReSharper disable once ConvertToUsingDeclaration
            // using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            // {
            //     int newIndex = EditorGUI.Popup(rect, label.text, index, scenes
            //         .Concat(new[] { "", "Edit Scenes In Build..." })
            //         .ToArray());
            //     // ReSharper disable once InvertIf
            //     if (changeCheck.changed)
            //     {
            //         if (newIndex >= scenes.Length)
            //         {
            //             OpenBuildSettings();
            //             return;
            //         }
            //
            //         property.intValue = newIndex;
            //     }
            // }
        }

        private static void DrawDropdown(IReadOnlyList<string> sceneNames, int selectedIndex, string fallbackName, Rect position, GUIContent label, Action<string, int> onSelected)
        {
            AdvancedDropdownList<int> dropdownList = new AdvancedDropdownList<int>("Pick a Scene");
            foreach ((string sceneName, int index) in sceneNames.WithIndex())
            {
                dropdownList.Add(new AdvancedDropdownList<int>($"[{index}] {sceneName}", index));
            }

            dropdownList.AddSeparator();
            dropdownList.Add("Edit Scenes In Build...", -1);

            string display = selectedIndex >= sceneNames.Count || selectedIndex < 0
                ? fallbackName
                : $"[{selectedIndex}] {sceneNames[selectedIndex]}";

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                CurDisplay = display,
                CurValues = new object[] { selectedIndex },
                DropdownListValue = dropdownList,
                SelectStacks = new []
                {
                    new AdvancedDropdownAttributeDrawer.SelectStack
                    {
                        Display = display,
                        Index = selectedIndex,
                    },
                },
            };

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(metaInfo.DropdownListValue, position.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    metaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        int index = (int)curItem;
                        if (index == -1)
                        {
                            SceneUtils.OpenBuildSettings();
                            return;
                        }

                        onSelected(sceneNames[index], index);
                    },
                    _ => null);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }
        }

        private static int IndexOfOrZero(string[] scenes, string scene)
        {
            int index = Array.IndexOf(scenes, scene);
            return index == -1 ? 0 : index;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}
