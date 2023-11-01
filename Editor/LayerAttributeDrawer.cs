using System;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer: SaintsPropertyDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel)
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

            // const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
            //                                   BindingFlags.Public | BindingFlags.DeclaredOnly;
            // FieldInfo field = targetType.GetField(property.propertyPath, bindAttr);
            // Debug.Assert(field != null, $"{property.propertyPath}/{target}");
            // object curValue = field!.GetValue(target);
            // List<string> options = new List<string>();
            // List<object> values = new List<object>();
            //
            // foreach ((KeyValuePair<string, object> keyValuePair, int index) in dropdownListValue!.Select((keyValuePair, index) => (keyValuePair, index)))
            // {
            //     // Debug.Log($"{keyValuePair.Key} -> {keyValuePair.Value}");
            //     // bool bothNull = curValue == null && keyValuePair.Value == null;
            //
            //     // Debug.Log(keyValuePair.Value);
            //     // Debug.Log(curValue);
            //
            //     // ReSharper disable once ConvertIfStatementToSwitchStatement
            //     if (curValue == null && keyValuePair.Value == null)
            //     {
            //         selectedIndex = index;
            //     }
            //     else if (curValue is UnityEngine.Object curValueObj
            //               && curValueObj == keyValuePair.Value as UnityEngine.Object)
            //     {
            //         selectedIndex = index;
            //     }
            //     else if (keyValuePair.Value == null)
            //     {
            //         // nothing
            //     }
            //     else if (keyValuePair.Value.Equals(curValue))
            //     {
            //         selectedIndex = index;
            //     }
            //     options.Add(keyValuePair.Key);
            //     values.Add(keyValuePair.Value);
            // }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int newIndex = EditorGUI.Popup(position, selectedIndex, layers);
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
