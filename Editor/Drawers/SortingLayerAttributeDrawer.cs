using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer: SaintsPropertyDrawer
    {
        private static string[] GetLayers()
        {
            Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(sortingLayersProperty != null);
            return (string[])sortingLayersProperty.GetValue(null, Array.Empty<object>());
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            string[] layers = GetLayers();

            int selectedIndex = property.propertyType == SerializedPropertyType.Integer ? property.intValue : Array.IndexOf(layers, property.stringValue);

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {

                int newIndex = EditorGUI.Popup(position, label, selectedIndex,
                    layers.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (property.propertyType == SerializedPropertyType.Integer)
                    {
                        property.intValue = newIndex;
                    }
                    else
                    {
                        property.stringValue = layers[newIndex];
                    }
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String
            ? ImGuiHelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => ImGuiHelpBox.Draw(position, $"Expect string or int, get {property.propertyType}", MessageType.Error);
    }
}
