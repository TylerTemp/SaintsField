using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using UnityEngine;
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
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
                DefaultDrawer(position, property, label, info);
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameObjectField(SerializedProperty property) => $"{property.propertyPath}__FieldType_ObjectField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FieldType_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
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
                name = NameObjectField(property),
                objectType = requiredComp,
                allowSceneObjects = true,
                value = requiredValue,
                style =
                {
                    flexShrink = 1,
                },
            };

            objectField.Bind(property.serializedObject);

            return objectField;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type requiredComp = fieldTypeAttribute.CompType;
            Type fieldType = SerializedUtils.GetType(property);

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));

            objectField.RegisterValueChangedCallback(v =>
            {
                Object result = GetNewValue(v.newValue, fieldType, requiredComp);
                property.objectReferenceValue = result;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(result);

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
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
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

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull)
        {
            ObjectField target = container.Q<ObjectField>(NameObjectField(property));
            target.label = labelOrNull;
        }

        #endregion

#endif
    }
}
