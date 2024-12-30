#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.CustomPicker.FieldTypeDrawer
{
    public partial class FieldTypeAttributeDrawer
    {


        #region UIToolkit

        private static string NameObjectField(SerializedProperty property) => $"{property.propertyPath}__FieldType_ObjectField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FieldType_HelpBox";
        private static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__FieldType_SelectorButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            bool customPicker = fieldTypeAttribute.CustomPicker;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            UnityEngine.Object requiredValue;

            // Debug.Log($"property.Object={property.objectReferenceValue}");

            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                VisualElement root = new VisualElement();
                root.Add(PropertyFieldFallbackUIToolkit(property));
                root.Add(new HelpBox(e.Message, HelpBoxMessageType.Error));
                root.AddToClassList(ClassAllowDisable);
                return root;
            }

            // Debug.Log($"requiredValue={requiredValue}");

            ObjectField objectField = new ObjectField(property.displayName)
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
            objectField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);

            if (customPicker)
            {
                StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                objectField.styleSheets.Add(hideStyle);
                Button selectorButton = new Button
                {
                    text = "●",
                    style =
                    {
                        position = Position.Absolute,
                        right = 0,
                        width = 18,
                        marginLeft = 0,
                        marginRight = 0,
                    },
                    name = NameSelectorButton(property),
                };

                objectField.Add(selectorButton);
            }

            objectField.AddToClassList(ClassAllowDisable);

            return objectField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            EPick editorPick = fieldTypeAttribute.EditorPick;

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));

            objectField.RegisterValueChangedCallback(v =>
            {
                UnityEngine.Object result = GetNewValue(v.newValue, fieldType, requiredComp);
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

            Button selectorButton = container.Q<Button>(NameSelectorButton(property));
            if (selectorButton != null)
            {
                selectorButton.clicked += () =>
                {
                    FieldTypeSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp, fieldResult =>
                    {
                        UnityEngine.Object result = OnSelectWindowSelected(fieldResult, fieldType);
                        // Debug.Log($"fieldType={fieldType} fieldResult={fieldResult}, result={result}");
                        property.objectReferenceValue = result;
                        property.serializedObject.ApplyModifiedProperties();
                        objectField.SetValueWithoutNotify(result);
                        // objectField.Unbind();
                        // objectField.BindProperty(property);
                        onValueChangedCallback.Invoke(result);

                        // Debug.Log($"property new value = {property.objectReferenceValue}, objectField={objectField.value}");
                    });
                };
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(newValue is UnityEngine.Object) && newValue != null)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = $"Value `{newValue}` is not a UnityEngine.Object";
                return;
            }

            UnityEngine.Object uObjectValue = (UnityEngine.Object) newValue;
            UnityEngine.Object result = GetNewValue(uObjectValue, info.FieldType, requiredComp);

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));
            if(objectField.value != result)
            {
                objectField.SetValueWithoutNotify(result);
            }

            if (newValue != null && result == null)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = $"{newValue} has no component {fieldType}";
            }
            else
            {
                helpBox.style.display = DisplayStyle.None;
            }
        }


        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
        //     IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        // {
        //     ObjectField target = container.Q<ObjectField>(NameObjectField(property));
        //     target.label = labelOrNull;
        // }

        #endregion
    }
}
#endif
