using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FieldTypeAttribute))]
    public class FieldTypeAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type requiredComp = fieldTypeAttribute.CompType;
            Type fieldType = SerializedUtils.GetType(property);
            Object requiredValue;
            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _error = e.Message;
                DefaultDrawer(position, property, label);
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object fieldResult =
                    EditorGUI.ObjectField(position, label, requiredValue, requiredComp, true);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    Object result = GetNewValue(fieldResult, fieldType, requiredComp);
                    property.objectReferenceValue = result;

                    if (fieldResult != null && result == null)
                    {
                        _error = $"{fieldResult} has no component {fieldType}";
                    }
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private static Object GetValue(SerializedProperty property, Type fieldType, Type requiredComp)
        {
            bool fieldTypeIsGameObject = fieldType == typeof(GameObject);
            bool requiredCompIsGameObject = requiredComp == typeof(GameObject);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (fieldTypeIsGameObject && requiredCompIsGameObject)
            {
                return property.objectReferenceValue;
            }

            if (!fieldTypeIsGameObject && !requiredCompIsGameObject)
            {
                return ((Component)property.objectReferenceValue)?.GetComponent(requiredComp);
            }

            if (fieldTypeIsGameObject)
            {
                return ((GameObject)property.objectReferenceValue)?.GetComponent(requiredComp);
            }

            return ((Component)property.objectReferenceValue)?.gameObject;
        }

        private static Object GetNewValue(Object fieldResult, Type fieldType, Type requiredComp)
        {
            bool requiredCompIsGameObject = requiredComp == typeof(GameObject);
            bool fieldTypeIsGameObject = fieldType == typeof(GameObject);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (requiredCompIsGameObject && fieldTypeIsGameObject)
            {
                return fieldResult;
            }

            if (!requiredCompIsGameObject && !fieldTypeIsGameObject)
            {
                return ((Component)fieldResult)?.GetComponent(fieldType);
            }

            if (requiredCompIsGameObject)
            {
                return ((GameObject)fieldResult)?.GetComponent(fieldType);
            }
            return ((Component)fieldResult)?.gameObject;
        }

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FieldType_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type requiredComp = fieldTypeAttribute.CompType;
            Type fieldType = SerializedUtils.GetType(property);
            Object requiredValue;
            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                VisualElement root = new VisualElement();
                root.Add(SaintsFallbackUIToolkit(property));
                root.Add(new HelpBox(e.Message, HelpBoxMessageType.Error));
                return root;
            }

            // Debug.Log($"requiredValue={requiredValue}");

            ObjectField objectField = new ObjectField(new string(' ', property.displayName.Length))
            {
                objectType = requiredComp,
                allowSceneObjects = true,
                value = requiredValue,
            };

            objectField.Bind(property.serializedObject);

            // HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            // {
            //     style =
            //     {
            //         display = DisplayStyle.None,
            //     },
            // };
            objectField.RegisterValueChangedCallback(v =>
            {
                Object result = GetNewValue(v.newValue, fieldType, requiredComp);
                property.objectReferenceValue = result;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

                if (v.newValue != null && result == null)
                {
                    helpBox.style.display = DisplayStyle.Flex;
                    helpBox.text = $"{v.newValue} has no component {fieldType}";
                }
                else
                {
                    helpBox.style.display = DisplayStyle.None;
                }
            });

            return objectField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
        }

        #endregion
    }
}
