using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagAttributeDrawer: SaintsPropertyDrawer
    {
        private static string[] GetLayers()
        {
            Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty!.GetValue(null, Array.Empty<object>());
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            List<string> tagList = new List<string>();
            tagList.Add("(None)");
            tagList.Add("Untagged");
            tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

            int selectedIndex = -1;
            for (int i = 1; i < tagList.Count; i++)
            {
                // ReSharper disable once InvertIf
                if (tagList[i].Equals(property.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i;
                    break;
                }
            }

            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int newIndex = EditorGUI.Popup(position, selectedIndex, tagList.ToArray());
            // ReSharper disable once InvertIf
            if (changed.changed)
            {
                property.stringValue = tagList[newIndex];
            }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.String
            ? HelpBox.GetHeight($"Expect string, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => HelpBox.Draw(position, $"Expect string, get {property.propertyType}", MessageType.Error);
    }
}
