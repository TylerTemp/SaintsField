using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
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
            string[] scenes = GetScenes();

            // const string optionName = "Edit Scenes In Build...";
            string[] sceneOptions = scenes
                .Select((name, index) => $"{index}: {name}")
                // .Concat(scenes.Length > 0? new[]{"", optionName}: new[]{optionName})
                .ToArray();

            _error = "";
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, scenes, sceneOptions);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, sceneOptions);
                    break;
                default:
                    _error = $"{property.name} must be an int or a string, get {property.propertyType}";
                    DefaultDrawer(position, property, label, info);
                    break;
            }
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label,
            string[] scenes, string[] sceneOptions)
        {
            int index = IndexOfOrZero(scenes, property.stringValue);

            if (string.IsNullOrEmpty(property.stringValue) && scenes.Length > 0)
            {
                property.stringValue = scenes[0];
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions
                    .Concat(new[] { "", "Edit Scenes In Build..." })
                    .ToArray());
                // ReSharper disable once InvertIf
                if (changeCheck.changed)
                {
                    if (newIndex >= sceneOptions.Length)
                    {
                        OpenBuildSettings();
                        return;
                    }

                    property.stringValue = scenes[newIndex];
                }
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label,
            string[] scenes)
        {
            int index = property.intValue;
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, scenes
                    .Concat(new[] { "", "Edit Scenes In Build..." })
                    .ToArray());
                // ReSharper disable once InvertIf
                if (changeCheck.changed)
                {
                    if (newIndex >= scenes.Length)
                    {
                        OpenBuildSettings();
                        return;
                    }

                    property.intValue = newIndex;
                }
            }
        }

        private static int IndexOfOrZero(string[] scenes, string scene)
        {
            int index = Array.IndexOf(scenes, scene);
            return index == -1 ? 0 : index;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
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
