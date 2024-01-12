using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label);
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String
            ? ImGuiHelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => ImGuiHelpBox.Draw(position, $"Expect string or int, get {property.propertyType}", MessageType.Error);
        #endregion

        #region UIToolkit

        private static string NameLayer(SerializedProperty property) => $"{property.propertyPath}__Layer";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
        {
            int curSelected = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : LayerMask.NameToLayer(property.stringValue);

            LayerField layerField = new LayerField(property.displayName, curSelected)
            {
                name = NameLayer(property),
            };

            return layerField;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            container.Q<LayerField>(NameLayer(property)).RegisterValueChangedCallback(evt =>
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    property.intValue = evt.newValue;
                    onValueChangedCallback.Invoke(evt.newValue);
                }
                else
                {
                    string newValue = LayerMask.LayerToName(evt.newValue);
                    property.stringValue = newValue;
                    onValueChangedCallback.Invoke(newValue);
                }
            });
        }

        #endregion
    }
}
