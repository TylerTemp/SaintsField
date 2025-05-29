using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
    public partial class SceneViewPickerAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__SceneViewPicker";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new Button(EditorGUIUtility.IconContent("d_scenepicking_pickable_hover").image as Texture2D);
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));
            button.clicked += () =>
            {

            };
        }
    }
}
