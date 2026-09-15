using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {

        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            // _hasLabel = hasLabel;
            // bool fullWidth = ((ResizableTextAreaAttribute)saintsAttribute).FullWidth;
            // Rect indented = EditorGUI.IndentedRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth,
            //     EditorGUIUtility.singleLineHeight));
            // float viewWidth = indented.width;
            // bool breakLine = BreakLine(saintsAttribute);

            // bool useFullView = !hasLabelWidth;

            // return GetHeight(
            //     property.stringValue,
            //     useFullView
            //         ? viewWidth
            //         : viewWidth - EditorGUIUtility.labelWidth
            // );
            // Debug.Log(hasLabelWidth);
            if (property.propertyType != SerializedPropertyType.String)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            bool inline = ((ResizableTextAreaAttribute)saintsAttribute).Inline;
            float textWidth = width - (inline && hasLabelWidth ? EditorGUIUtility.labelWidth : 0f);
            return Mathf.Max(GetHeight(property.stringValue, Mathf.Max(1f, textWidth)),
                EditorGUIUtility.singleLineHeight * Mathf.Max(0, ((ResizableTextAreaAttribute)saintsAttribute).MinRow)
            ) + (hasLabelWidth && !inline ? EditorGUIUtility.singleLineHeight : 0f);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            // EditorGUI.DrawRect(position, Color.blue);

            if (property.propertyType != SerializedPropertyType.String)
            {
                _error = $"expect string, get {property.propertyType}";
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            // _width = position.width;

            // _width = GetHeight(
            //     property.stringValue,
            //     position.width
            // );

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                // Debug.Log(position);
                GUIStyle style = new GUIStyle(EditorStyles.textField)
                {
                    wordWrap = true,
                };
                _error = "";
                if (!string.IsNullOrEmpty(label.text))
                {
                    (Rect labelFieldRect, Rect textAreaRect) = ((ResizableTextAreaAttribute)saintsAttribute).Inline
                        ? RectUtils.SplitWidthRect(position, EditorGUIUtility.labelWidth)
                        : RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                    labelFieldRect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(labelFieldRect, label);
                    DrawOverrideRichText(labelFieldRect, label, overrideRichTextChunks);
                    position = textAreaRect;
                }

                string textAreaValue = EditorGUI.TextArea(position, property.stringValue, style);
                if (changed.changed)
                {
                    property.stringValue = textAreaValue;
                }
            }
        }

        private float GetHeight(string text, float width)
        {
            GUIStyle style = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true,
            };
            if (string.IsNullOrEmpty(text))
            {
                text = "F";
            }

            return style.CalcHeight(new GUIContent(text), width);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) =>
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
