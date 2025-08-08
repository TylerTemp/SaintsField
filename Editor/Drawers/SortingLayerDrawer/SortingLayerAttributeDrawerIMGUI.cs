using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public partial class SortingLayerAttributeDrawer
    {
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
            string[] layers = SortingLayer.layers.Select(each => each.name).ToArray();

            int selectedIndex = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : Array.IndexOf(layers, property.stringValue);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, selectedIndex,
                    layers
                        .Select(each => each == "" ? new GUIContent("<empty string>") : new GUIContent(each))
                        .Concat(new[] { GUIContent.none, new GUIContent("Edit Sorting Layers...") })
                        .ToArray());
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newIndex >= layers.Length)
                    {
                        SortingLayerUtils.OpenSortingLayerInspector();
                        return;
                    }

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

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.Integer &&
                              property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            property.propertyType != SerializedPropertyType.Integer &&
            property.propertyType != SerializedPropertyType.String
                ? ImGuiHelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
                : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position,
            $"Expect string or int, get {property.propertyType}", MessageType.Error);

    }
}
