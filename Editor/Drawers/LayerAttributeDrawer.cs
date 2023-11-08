using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer: SaintsPropertyDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;

            // UnityEngine.Object target = property.serializedObject.targetObject;
            // Type targetType = target.GetType();

            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;

            int selectedIndex = property.propertyType == SerializedPropertyType.Integer ? property.intValue : Array.IndexOf(layers, property.stringValue);

            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int newIndex = EditorGUI.Popup(position, label, selectedIndex, layers.Select(each => new GUIContent(each)).ToArray());
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
                // try
                // {
                //     field.SetValue(target, newValue);
                // }
                // catch (ArgumentException)
                // {
                //     property.objectReferenceValue = (UnityEngine.GameObject)newValue;
                // }
            }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String
            ? HelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => HelpBox.Draw(position, $"Expect string or int, get {property.propertyType}", MessageType.Error);
    }
}
