using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {

        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
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
            return GetHeight(
                property.stringValue,
                width
            ) + (hasLabelWidth ? EditorGUIUtility.singleLineHeight : 0f);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            // EditorGUI.DrawRect(position, Color.blue);

            if (property.propertyType != SerializedPropertyType.String)
            {
                _error = $"expect string, get {property.propertyType}";
                DefaultDrawer(position, property, label, info);
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
                EditorStyles.textField.wordWrap = true;
                if (label.text != "")
                {
                    (Rect labelFieldRect, Rect textAreaRect) =
                        RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(labelFieldRect, label);
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
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "F";
            }

            float areaHeight = style.CalcHeight(new GUIContent(text), width);
            return Mathf.Max(areaHeight,
                EditorGUIUtility.singleLineHeight * SaintsFieldConfigUtil.ResizableTextAreaMinRow());
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
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
