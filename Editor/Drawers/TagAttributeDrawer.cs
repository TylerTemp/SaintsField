using System.Collections.Generic;
using System.Reflection;
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
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                string result = EditorGUI.TagField(position, label, property.stringValue);
                if (changed.changed)
                {
                    property.stringValue = result;
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => property.propertyType != SerializedPropertyType.String
            ? ImGuiHelpBox.GetHeight($"Expect string, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, $"Expect string, get {property.propertyType}", MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameTag(SerializedProperty property) => $"{property.propertyPath}__Tag";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            TagField tagField = new TagField(property.displayName)
            {
                value = property.stringValue,
                name = NameTag(property),
            };

            tagField.AddToClassList("unity-base-field__aligned");
            tagField.AddToClassList(ClassAllowDisable);

            return tagField;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            TagField tagField = container.Q<TagField>(NameTag(property));
            tagField.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();

                onValueChangedCallback.Invoke(evt.newValue);
            });
        }

        #endregion

#endif
    }
}
