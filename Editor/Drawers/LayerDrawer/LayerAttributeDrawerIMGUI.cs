using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public partial class LayerAttributeDrawer
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
            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            int curSelected = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : LayerMask.NameToLayer(property.stringValue);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int selectedLayer = EditorGUI.LayerField(position, label, curSelected);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (property.propertyType == SerializedPropertyType.Integer)
                    {
                        property.intValue = selectedLayer;
                    }
                    else
                    {
                        // Debug.Log($"change index {selectedLayer} on {string.Join(", ", layers)}");
                        property.stringValue = LayerMask.LayerToName(selectedLayer);
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
