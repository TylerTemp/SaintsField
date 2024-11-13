using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ResizableTextAreaAttribute))]
    public class ResizableTextAreaAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        // private float _width = -1;

        // private bool _hasLabel = true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            // _hasLabel = hasLabel;
            // bool fullWidth = ((ResizableTextAreaAttribute)saintsAttribute).FullWidth;
            Rect indented = EditorGUI.IndentedRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight));
            float viewWidth = indented.width;
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
                viewWidth
            ) + (hasLabelWidth ? EditorGUIUtility.singleLineHeight : 0f);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
            return Mathf.Max(areaHeight, EditorGUIUtility.singleLineHeight * SaintsFieldConfigUtil.ResizableTextAreaMinRow());
        }

        // private static float GetTextAreaHeight(string text) => (EditorGUIUtility.singleLineHeight - 3.0f) * GetNumberOfLines(text) + 3.0f;

        // private static int GetNumberOfLines(string text)
        // {
        //     if (text == null)
        //     {
        //         return 1;
        //     }
        //
        //     string content = Regex.Replace(text, @"\r\n|\n\r|\r|\n", Environment.NewLine);
        //     string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        //     return lines.Length;
        // }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameLabelPlaceholder(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea_LabelPlaceholder";
        private static string NameTextArea(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement();
            root.Add(new Label(property.displayName)
            {
                name = NameLabelPlaceholder(property),
                style =
                {
                    height = SingleLineHeight,
                    paddingLeft = 4,
                },
                pickingMode = PickingMode.Ignore,
            });

            const float singleLineHeight = 47 / 3f;

            root.Add(new TextField
            {
                value = property.stringValue,
                name = NameTextArea(property),
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    minHeight = singleLineHeight * SaintsFieldConfigUtil.ResizableTextAreaMinRow(),
                },
            });

            root.AddToClassList(ClassAllowDisable);

            return root;
            // return new TextField(property.displayName)
            // {
            //     value = property.stringValue,
            //     name = NameTextArea(property),
            //     multiline = true,
            //     style =
            //     {
            //         whiteSpace = WhiteSpace.Normal,
            //         minHeight = 47,
            //     },
            // };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<TextField>(NameTextArea(property)).RegisterValueChangedCallback(changed =>
            {
                property.stringValue = changed.newValue;
                property.serializedObject.ApplyModifiedProperties();

                onValueChangedCallback?.Invoke(changed.newValue);
            });
        }

        #endregion

#endif
    }
}
