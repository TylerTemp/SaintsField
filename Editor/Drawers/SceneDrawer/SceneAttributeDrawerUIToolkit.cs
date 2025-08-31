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
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    SceneIntDropdownElement intDropdownElement = new SceneIntDropdownElement();
                    intDropdownElement.BindProperty(property);
                    return new IntDropdownField(GetPreferredLabel(property), intDropdownElement);
                }
                case SerializedPropertyType.String:
                {
                    SceneStringDropdownElement layerStringStringDropdown = new SceneStringDropdownElement(
                        ((SceneAttribute)saintsAttribute).FullPath);
                    layerStringStringDropdown.BindProperty(property);
                    return new StringDropdownField(GetPreferredLabel(property), layerStringStringDropdown);
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
                    return null;
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
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    IntDropdownField intDropdownField = container.Q<IntDropdownField>();
                    AddContextualMenuManipulator(fullPath, intDropdownField, property, onValueChangedCallback, info, parent);

                    intDropdownField.Button.clicked += () => MakeDropdown(fullPath, property, intDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    StringDropdownField layerStringField = container.Q<StringDropdownField>();
                    AddContextualMenuManipulator(fullPath, layerStringField, property, onValueChangedCallback, info, parent);

                    layerStringField.Button.clicked += () => MakeDropdown(fullPath, property, layerStringField, onValueChangedCallback, info, parent);
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

        private static void MakeDropdown(bool isFullPath, SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            AdvancedDropdownList<(string path, int index)> dropdown = new AdvancedDropdownList<(string path, int index)>();
            if (isString)
            {
                dropdown.Add("[Empty String]", (string.Empty, -1));
                dropdown.AddSeparator();
            }

            string selectedPath = null;
            int selectedIndex = -1;
            foreach ((string path, int index) in SceneUtils.GetTrimedScenePath(isFullPath).WithIndex())
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(new AdvancedDropdownList<(string path, int index)>(path, (path, index)));
                // ReSharper disable once InvertIf
                if (isString && path == property.stringValue
                    || !isString && index == property.intValue)
                {
                    selectedPath = path;
                    selectedIndex = index;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", ("", -2), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedIndex >= 0 ? new object[] { (selectedPath, selectedIndex) } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    (string path, int index) = ((string path, int index))curItem;
                    switch (index)
                    {
                        case -1:
                        {
                            Debug.Assert(isString);
                            property.stringValue = "";
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, "");
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke("");
                        }
                            break;
                        case -2:
                        {
                            SceneUtils.OpenBuildSettings();
                        }
                            break;
                        default:
                        {
                            if (isString)
                            {
                                property.stringValue = path;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, path);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(path);
                            }
                            else
                            {
                                property.intValue = index;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, index);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(index);
                            }
                        }
                            break;
                    }
                }
            );

            // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
            // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
            // editorWindow.Show();

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
