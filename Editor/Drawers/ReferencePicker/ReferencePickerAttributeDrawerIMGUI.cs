#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ReferencePicker
{
    public partial class ReferencePickerAttributeDrawer
    {
        private const float ImGuiButtonWidth = 20f;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            object managedReferenceValue = property.managedReferenceValue;

            string displayLabel = managedReferenceValue == null
                ? ""
                : managedReferenceValue.GetType().Name;

            GUIContent fullLabel = new GUIContent(displayLabel);
            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };
            float width = textStyle.CalcSize(fullLabel).x;
            if(!((ReferencePickerAttribute)saintsAttribute).HideLabel)
            {
                GUI.Label(new Rect(position)
                {
                    x = position.x - width,
                    width = width,
                    height = SingleLineHeight,
                }, fullLabel, textStyle);
            }

            Rect dropdownRect = new Rect(position)
            {
                height = SingleLineHeight,
            };

            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(" "), FocusType.Keyboard))
            {
                AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>
                {
                    {"[Null]", null},
                    AdvancedDropdownList<Type>.Separator(),
                };

                int totalCount = 1;
                foreach (Type type in GetTypes(property))
                {
                    totalCount += 1;
                    string displayName = $"{type.Name}: {type.Namespace}";
                    dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
                }

                Vector2 size = new Vector2(position.width, totalCount * SingleLineHeight + AdvancedDropdownAttribute.DefaultTitleHeight);

                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownList,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        object instance = curItem == null
                            ? null
                            : Activator.CreateInstance((Type)curItem);
                        property.managedReferenceValue = instance;
                        property.serializedObject.ApplyModifiedProperties();
                        onGUIPayload.SetValue(instance);
                    },
                    _ => null);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            return true;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return ImGuiButtonWidth;
        }
    }
}
#endif
