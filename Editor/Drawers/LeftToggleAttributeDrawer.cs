using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    public class LeftToggleAttributeDrawer: SaintsPropertyDrawer
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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameLeftToggle(SerializedProperty property) => $"{property.propertyPath}__LeftToggle";
        // private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__LeftToggle_Label";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            // VisualElement root = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = FlexDirection.Row,
            //     },
            // };
            //
            // Toggle toggle = new Toggle(property.displayName)
            // {
            //     name = NameLeftToggle(property),
            // };
            //
            // // label.RegisterCallback();
            // // label.AddManipulator(new Clickable(evt =>
            // // {
            // //     toggle.value = !toggle.value;
            // //     // bool newValue = !toggle.value;
            // //     // property.boolValue = !toggle.value;
            // //     // onChange?.Invoke(property.boolValue);
            // //     // toggle.SetValueWithoutNotify(property.boolValue);
            // // }));
            //
            // root.Add(toggle);
            // // root.Add(label);
            //
            // return root;
            Toggle toggle = new Toggle(property.displayName)
            {
                name = NameLeftToggle(property),
                style =
                {
                    flexDirection = FlexDirection.RowReverse,
                    justifyContent = Justify.FlexEnd,
                },
            };

            toggle.BindProperty(property);

            toggle.styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/LeftToggle.uss"));
            toggle.AddToClassList(ClassAllowDisable);

            return toggle;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<Toggle>(NameLeftToggle(property)).RegisterValueChangedCallback(evt =>
            {
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, evt.newValue);
                onValueChangedCallback.Invoke(evt.newValue);
            });
        }

        #endregion

#endif
    }
}
