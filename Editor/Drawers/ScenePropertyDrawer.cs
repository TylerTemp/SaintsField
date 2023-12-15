using System;
using System.IO;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    /// <summary>
    ///  <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Editor/PropertyDrawers/ScenePropertyDrawer.cs#L10" />
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class ScenePropertyDrawer : SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            string[] scenes = GetScenes();
            bool anySceneInBuildSettings = scenes.Length > 0;
            if (!anySceneInBuildSettings)
            {
                // DrawDefaultPropertyAndHelpBox(rect, property, label, "No scenes in the build settings", MessageType.Warning);
                _error = "No scenes in the build settings";
                DefaultDrawer(position, property, label);
                return;
            }

            string[] sceneOptions = scenes
                .Select((name, index) => $"{name} [{index}]")
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
                    DefaultDrawer(position, property, label);
                    break;
            }
        }

        private static string[] GetScenes() =>
            EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] scenes, string[] sceneOptions)
        {
            int index = IndexOfOrZero(scenes, property.stringValue);

            if (string.IsNullOrEmpty(property.stringValue) && scenes.Length > 0)
            {
                property.stringValue = scenes[0];
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
                if (changeCheck.changed)
                {
                    property.stringValue = scenes[newIndex];
                }
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] sceneOptions)
        {
            int index = property.intValue;
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
                if (changeCheck.changed)
                {
                    property.intValue = newIndex;
                }
            }
        }

        private static int IndexOfOrZero(string[] scenes, string scene)
        {
            int index = Array.IndexOf(scenes, scene);
            return index == -1? 0: index;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
