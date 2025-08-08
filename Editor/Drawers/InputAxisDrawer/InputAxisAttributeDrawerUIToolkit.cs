#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public partial class InputAxisAttributeDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            InputAxisElement inputAxisElement = new InputAxisElement();
            inputAxisElement.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), inputAxisElement);
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return null;
            }

            return new HelpBox($"Type {property.propertyType} is not string.", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            StringDropdownField layerStringField = container.Q<StringDropdownField>();
            AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);

            layerStringField.Button.clicked += () =>
                MakeDropdown(property, layerStringField, onValueChangedCallback, info, parent);
        }

        private static void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                foreach (string axisName in InputAxisUtils.GetAxisNames())
                {
                    // ReSharper disable once InvertIf
                    if (axisName == clipboardText)
                    {
                        evt.menu.AppendAction($"Paste \"{axisName}\"", _ =>
                        {
                            property.stringValue = axisName;
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, axisName);
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(axisName);
                        });
                        return;
                    }
                }
            }));
        }

        private static void MakeDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();

            string selectedName = null;
            foreach (string axisName in InputAxisUtils.GetAxisNames())
            {
                dropdown.Add(axisName, axisName);
                // ReSharper disable once InvertIf
                if (axisName == property.stringValue)
                {
                    selectedName = axisName;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Open Input Manager...", null, false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedName is null ? Array.Empty<object>(): new object[] { selectedName },
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
                    string curValue = (string)curItem;
                    if (curValue == null)
                    {
                        InputAxisUtils.OpenInputManager();
                    }
                    else
                    {
                        property.stringValue = curValue;
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parent, curValue);
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(curValue);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }


    }
}
#endif
