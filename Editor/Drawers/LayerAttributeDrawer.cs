﻿using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using UnityEngine;


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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String
            ? ImGuiHelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, $"Expect string or int, get {property.propertyType}", MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameLayer(SerializedProperty property) => $"{property.propertyPath}__Layer";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
        {
            int curSelected = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : LayerMask.NameToLayer(property.stringValue);

            LayerField layerField = new LayerField(new string(' ', property.displayName.Length), curSelected)
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

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, string labelOrNull)
        {
            container.Q<LayerField>(NameLayer(property)).label = labelOrNull;
        }

        #endregion

#endif
    }
}
