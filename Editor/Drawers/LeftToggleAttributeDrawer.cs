﻿using System;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    public class LeftToggleAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool result = EditorGUI.ToggleLeft(position, label, property.boolValue);
                if (changed.changed)
                {
                    property.boolValue = result;
                }
            }
        }

        #endregion

        #region UIToolkit

        private static string NameLeftToggle(SerializedProperty property) => $"{property.propertyPath}__LeftToggle";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };

            Toggle toggle = new Toggle("")
            {
                name = NameLeftToggle(property),
            };

            Label label = new Label(property.displayName)
            {
                style =
                {
                    marginLeft = 5,
                    flexGrow = 1,
                },
            };
            // label.RegisterCallback();
            label.AddManipulator(new Clickable(evt =>
            {
                toggle.value = !toggle.value;
                // bool newValue = !toggle.value;
                // property.boolValue = !toggle.value;
                // onChange?.Invoke(property.boolValue);
                // toggle.SetValueWithoutNotify(property.boolValue);
            }));

            root.Add(toggle);
            root.Add(label);

            return root;
        }

        protected override void OnAwakeUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            container.Q<Toggle>(NameLeftToggle(property)).RegisterValueChangedCallback(evt =>
            {
                property.boolValue = evt.newValue;
                onValueChangedCallback.Invoke(evt.newValue);
            });
        }

        #endregion
    }
}
