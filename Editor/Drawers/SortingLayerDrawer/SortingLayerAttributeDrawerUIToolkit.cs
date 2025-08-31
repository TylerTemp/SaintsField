#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public partial class SortingLayerAttributeDrawer
    {

        // private static string NameButtonField(SerializedProperty property) =>
        //     $"{property.propertyPath}__SortingLayer_Button";
        //
        // private static string NameHelpBox(SerializedProperty property) =>
        //     $"{property.propertyPath}__SortingLayer_HelpBox";

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
                    SortingLayerIntElement sortingLayerIntElement = new SortingLayerIntElement();
                    sortingLayerIntElement.BindProperty(property);
                    return new IntDropdownField(GetPreferredLabel(property), sortingLayerIntElement);
                }
                case SerializedPropertyType.String:
                {
                    SortingLayerStringElement sortingLayerStringElement = new SortingLayerStringElement();
                    sortingLayerStringElement.BindProperty(property);
                    return new StringDropdownField(GetPreferredLabel(property), sortingLayerStringElement);
                }
                default:
                    return new VisualElement();
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
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    IntDropdownField intDropdownField = container.Q<IntDropdownField>();
                    AddContextualMenuManipulator(intDropdownField, property, onValueChangedCallback, info, parent);

                    intDropdownField.Button.clicked += () => MakeDropdown(property, intDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    StringDropdownField layerStringField = container.Q<StringDropdownField>();
                    AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);

                    layerStringField.Button.clicked += () => MakeDropdown(property, layerStringField, onValueChangedCallback, info, parent);
                }
                    break;
                default:
                    return;
            }
        }

        private static void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
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
                    foreach (SortingLayer sortingLayer in SortingLayer.layers)
                    {
                        if (sortingLayer.name == clipboardText)
                        {
                            evt.menu.AppendAction($"Paste \"{sortingLayer.name}\"", _ =>
                            {
                                property.stringValue = sortingLayer.name;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, sortingLayer.name);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(sortingLayer.name);
                            });
                            return;
                        }
                    }
                }
                else
                {
                    foreach (SortingLayer sortingLayer in SortingLayer.layers)
                    {
                        // ReSharper disable once InvertIf
                        if (sortingLayer.name == clipboardText
                            || canBeInt && sortingLayer.value == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{sortingLayer.name}\"({sortingLayer.value})", _ =>
                            {
                                property.intValue = sortingLayer.value;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, sortingLayer.value);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(sortingLayer.value);
                            });
                            return;
                        }
                    }
                }
            }));
        }

        private static void MakeDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            AdvancedDropdownList<(string path, int index)> dropdown = new AdvancedDropdownList<(string path, int index)>();
            if (isString)
            {
                dropdown.Add("[Empty String]", (string.Empty, -1));
                dropdown.AddSeparator();
            }

            string selectedName = null;
            int selectedIndex = -1;
            foreach (SortingLayer sortingLayer in SortingLayer.layers)
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(new AdvancedDropdownList<(string path, int index)>($"<color=#808080>{sortingLayer.value}</color> {sortingLayer.name}", (sortingLayer.name, sortingLayer.value)));
                // ReSharper disable once InvertIf
                if (isString && sortingLayer.name == property.stringValue
                    || !isString && sortingLayer.value == property.intValue)
                {
                    selectedName = sortingLayer.name;
                    selectedIndex = sortingLayer.value;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", ("", -2), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedIndex >= 0 ? new object[] { (selectedName, selectedIndex) } : Array.Empty<object>(),
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

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

    }
}
#endif
