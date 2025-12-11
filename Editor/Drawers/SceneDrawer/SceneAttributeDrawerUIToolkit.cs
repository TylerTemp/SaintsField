#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Path = System.IO.Path;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public partial class SceneAttributeDrawer
    {
        // private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__Scene_Button";
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Scene_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            SceneAttribute sceneAttribute = saintsAttribute as SceneAttribute ?? new SceneAttribute();
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    ScenePickerIntElement e = new ScenePickerIntElement(sceneAttribute)
                    {
                        bindingPath = property.propertyPath
                    };
                    e.BindProperty(property);
                    ScenePickerIntField r = new ScenePickerIntField(GetPreferredLabel(property), e);
                    r.AddToClassList(ClassAllowDisable);
                    r.AddToClassList(ScenePickerIntField.alignedFieldUssClassName);
                    return r;
                }
                case SerializedPropertyType.String:
                {
                    ScenePickerStringField r = new ScenePickerStringField(GetPreferredLabel(property),
                        new ScenePickerStringElement(sceneAttribute)
                        {
                            bindingPath = property.propertyPath
                        });
                    r.AddToClassList(ClassAllowDisable);
                    r.AddToClassList(ScenePickerStringField.alignedFieldUssClassName);
                    return r;
                }
                default:
                    return new Label(GetPreferredLabel(property));
            }
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.String:
                    return new SceneHelpBox();
                default:
                    return new HelpBox($"Type {property.propertyType} is not int or string.", HelpBoxMessageType.Error)
                    {
                        style =
                        {
                            flexGrow = 1,
                        },
                    };
            }

        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            bool fullPath = ((SceneAttribute)saintsAttribute).FullPath;
            SceneHelpBox helpBox = container.Q<SceneHelpBox>();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    ScenePickerIntField scenePickerIntField = container.Q<ScenePickerIntField>();
                    AddContextualMenuManipulator(fullPath, scenePickerIntField, property, onValueChangedCallback, info, parent);

                    scenePickerIntField.ScenePickerIntElement.BindStringHelpBox(helpBox);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    ScenePickerStringField layerStringField = container.Q<ScenePickerStringField>();
                    AddContextualMenuManipulator(fullPath, layerStringField, property, onValueChangedCallback, info, parent);

                    layerStringField.ScenePickerStringElement.BindStringHelpBox(helpBox);
                    // ScenePickerStringElement.HelpBoxErrorHandler(helpBox, helpBoxButton, layerStringField.ScenePickerStringElement.ErrorEventString, layerStringField.ScenePickerStringElement.ErrorEventScene);
                    // layerStringField.ScenePickerStringElement.ErrorEvent.AddListener(
                    //     (str, scene) =>
                    //         ScenePickerStringElement.HelpBoxErrorHandler(helpBox, helpBoxButton, str, scene)
                    // );
                    // helpBoxButton.clicked += ()
                    // layerStringField.Button.clicked += () => MakeDropdown(fullPath, property, layerStringField, onValueChangedCallback, info, parent);
                }
                    break;
                default:
                    return;
            }
        }

        private static void AddContextualMenuManipulator(bool fullPath, VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            bool isString = property.propertyType == SerializedPropertyType.String;
            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);

                if (isString)
                {
                    foreach (string scenePath in SceneUtils.GetTrimedScenePath(fullPath))
                    {
                        if (scenePath == clipboardText)
                        {
                            evt.menu.AppendAction($"Paste \"{Path.GetFileName(scenePath)}\"", _ =>
                            {
                                property.stringValue = scenePath;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, scenePath);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(scenePath);
                            });
                            return;
                        }
                    }
                }
                else
                {
                    foreach ((string scenePath, int index) in SceneUtils.GetTrimedScenePath(fullPath).WithIndex())
                    {
                        if (scenePath == clipboardText
                            || canBeInt && index == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{Path.GetFileName(scenePath)}\"({index})", _ =>
                            {
                                property.intValue = index;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, index);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(index);
                            });
                            return;
                        }
                    }
                }
            }));
        }
    }
}
#endif
