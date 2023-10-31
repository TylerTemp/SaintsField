using System;
using System.Text.RegularExpressions;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(ResizableTextAreaAttribute))]
    public class ResizableTextAreaAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private float _width = -1;

        protected override float GetLabelFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => GetHeight(
            property.stringValue,
            _width < 0? EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth: _width
        );

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                _error = $"expect string, get {property.propertyType}";
                DefaultDrawer(position, property);
                return;
            }

            _width = position.width;

            // _width = GetHeight(
            //     property.stringValue,
            //     position.width
            // );

            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            // Debug.Log(position);
            GUIStyle style = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true,
            };
            EditorStyles.textField.wordWrap = true;

            string textAreaValue = EditorGUI.TextArea(position, property.stringValue, style);
            if (changed.changed)
            {
                property.stringValue = textAreaValue;
            }
        }

        private float GetHeight(string text, float width)
        {
            GUIStyle style = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true,
            };
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "F";
            }
            float areaHeight = style.CalcHeight(new GUIContent(text), width);
            return Mathf.Max(areaHeight, EditorGUIUtility.singleLineHeight * 3);
        }

        private static float GetTextAreaHeight(string text) => (EditorGUIUtility.singleLineHeight - 3.0f) * GetNumberOfLines(text) + 3.0f;

        private static int GetNumberOfLines(string text)
        {
            if (text == null)
            {
                return 1;
            }

            string content = Regex.Replace(text, @"\r\n|\n\r|\r|\n", Environment.NewLine);
            string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return lines.Length;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
