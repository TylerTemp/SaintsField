using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone.Editor
{
    /// <summary>
    ///  <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Editor/PropertyDrawers/ScenePropertyDrawer.cs#L10" />
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class ScenePropertyDrawer : PropertyDrawer
    {
        private const string SceneListItem = "{0} ({1})";
        private const string ScenePattern = @".+\/(.+)\.unity";
        private const string TypeWarningMessage = "{0} must be an int or a string";
        private const string BuildSettingsWarningMessage = "No scenes in the build settings";

        private float GetHelpBoxHeight() => EditorGUIUtility.singleLineHeight * 2.0f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Debug.Log($"get height");

            bool validPropertyType = property.propertyType is SerializedPropertyType.String or SerializedPropertyType.Integer;
            bool anySceneInBuildSettings = GetScenes().Length > 0;

            return (validPropertyType && anySceneInBuildSettings)
                ? EditorGUIUtility.singleLineHeight
                : EditorGUIUtility.singleLineHeight + GetHelpBoxHeight();
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // Debug.Log("On GUI Scene");

            EditorGUI.BeginProperty(rect, label, property);

            string[] scenes = GetScenes();
            bool anySceneInBuildSettings = scenes.Length > 0;
            if (!anySceneInBuildSettings)
            {
                DrawDefaultPropertyAndHelpBox(rect, property, label, BuildSettingsWarningMessage, MessageType.Warning);
                return;
            }

            string[] sceneOptions = GetSceneOptions(scenes);
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(rect, property, label, scenes, sceneOptions);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(rect, property, label, sceneOptions);
                    break;
                default:
                    string message = string.Format(TypeWarningMessage, property.name);
                    DrawDefaultPropertyAndHelpBox(rect, property, label, message, MessageType.Warning);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private void DrawDefaultPropertyAndHelpBox(Rect rect, SerializedProperty property, GUIContent label, string message,
            MessageType messageType)
        {
            // float indentLength = NaughtyEditorGUI.GetIndentLength(rect);
            Rect helpBoxRect = new Rect(
                rect.x,
                rect.y,
                rect.width,
                GetHelpBoxHeight());

            EditorGUI.HelpBox(helpBoxRect, message, messageType);

            Rect propertyRect = new Rect(
                rect.x,
                rect.y + GetHelpBoxHeight(),
                rect.width,
                base.GetPropertyHeight(property, label));

            // EditorGUI.PropertyField(propertyRect, property, true);
            EditorGUI.TextField(propertyRect, label, property.stringValue);
        }

        private string[] GetScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Regex.Match(scene.path, ScenePattern).Groups[1].Value)
                .ToArray();
        }

        private string[] GetSceneOptions(string[] scenes)
        {
            return scenes.Select((s, i) => string.Format(SceneListItem, s, i)).ToArray();
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] scenes, string[] sceneOptions)
        {
            int index = IndexOf(scenes, property.stringValue);
            int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
            string newScene = scenes[newIndex];

            if (!property.stringValue.Equals(newScene, StringComparison.Ordinal))
            {
                property.stringValue = scenes[newIndex];
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] sceneOptions)
        {
            int index = property.intValue;
            int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);

            if (property.intValue != newIndex)
            {
                property.intValue = newIndex;
            }
        }

        private static int IndexOf(string[] scenes, string scene)
        {
            var index = Array.IndexOf(scenes, scene);
            return Mathf.Clamp(index, 0, scenes.Length - 1);
        }
    }
}
