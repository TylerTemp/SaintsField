using System;
using System.IO;
using System.Linq;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Standalone
{
    /// <summary>
    ///  <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Editor/PropertyDrawers/ScenePropertyDrawer.cs#L10" />
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class ScenePropertyDrawer : PropertyDrawer
    {
        private static float GetHelpBoxHeight() => EditorGUIUtility.singleLineHeight * 2.0f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Debug.Log($"get height");

            bool validPropertyType = property.propertyType is SerializedPropertyType.String or SerializedPropertyType.Integer;
            bool anySceneInBuildSettings = GetScenes().Length > 0;

            return validPropertyType && anySceneInBuildSettings
                ? EditorGUIUtility.singleLineHeight
                : EditorGUIUtility.singleLineHeight + GetHelpBoxHeight();
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            using EditorGUI.PropertyScope propertyScoop = new EditorGUI.PropertyScope(rect, label, property);

            label = propertyScoop.content;

            string[] scenes = GetScenes();
            bool anySceneInBuildSettings = scenes.Length > 0;
            if (!anySceneInBuildSettings)
            {
                DrawDefaultPropertyAndHelpBox(rect, property, label, "No scenes in the build settings", MessageType.Warning);
                return;
            }

            string[] sceneOptions = scenes
                .Select((name, index) => $"[{index}]{name}")
                .ToArray();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(rect, property, label, scenes, sceneOptions);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(rect, property, label, sceneOptions);
                    break;
                default:
                    DrawDefaultPropertyAndHelpBox(rect, property, label, $"{property.name} must be an int or a string, get {property.propertyType}", MessageType.Warning);
                    break;
            }
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

        private static string[] GetScenes() =>
            EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] scenes, string[] sceneOptions)
        {
            int index = IndexOfOrZero(scenes, property.stringValue);
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
    }
}
