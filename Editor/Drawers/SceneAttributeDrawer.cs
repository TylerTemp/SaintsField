using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{

    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneAttributeDrawer : SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string[] scenes = GetScenes();

            // const string optionName = "Edit Scenes In Build...";
            string[] sceneOptions = scenes
                .Select((name, index) => $"{index}: {name}")
                // .Concat(scenes.Length > 0? new[]{"", optionName}: new[]{optionName})
                .ToArray();

            _error = "";
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, scenes, sceneOptions);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, sceneOptions);
                    break;
                default:
                    _error = $"{property.name} must be an int or a string, get {property.propertyType}";
                    DefaultDrawer(position, property, label, info);
                    break;
            }
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] scenes, string[] sceneOptions)
        {
            int index = IndexOfOrZero(scenes, property.stringValue);

            if (string.IsNullOrEmpty(property.stringValue) && scenes.Length > 0)
            {
                property.stringValue = scenes[0];
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions
                    .Concat(new[]{"", "Edit Scenes In Build..."})
                    .ToArray());
                // ReSharper disable once InvertIf
                if (changeCheck.changed)
                {
                    if(newIndex >= sceneOptions.Length)
                    {
                        OpenBuildSettings();
                        return;
                    }
                    property.stringValue = scenes[newIndex];
                }
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] scenes)
        {
            int index = property.intValue;
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, scenes
                    .Concat(new[]{"", "Edit Scenes In Build..."})
                    .ToArray());
                // ReSharper disable once InvertIf
                if (changeCheck.changed)
                {
                    if(newIndex >= scenes.Length)
                    {
                        OpenBuildSettings();
                        return;
                    }
                    property.intValue = newIndex;
                }
            }
        }

        private static int IndexOfOrZero(string[] scenes, string scene)
        {
            int index = Array.IndexOf(scenes, scene);
            return index == -1? 0: index;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        /// <summary>
        ///  <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Editor/PropertyDrawers/ScenePropertyDrawer.cs#L10" />
        /// </summary>
        private static string[] GetScenes() =>
            EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();

        private static void OpenBuildSettings()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__Scene_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Scene_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButtonField(property);

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField buttonLabel = container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));
            (int _, string displayName) = GetSelected(property);
            buttonLabel.ButtonLabelElement.text = displayName;

            container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property)).ButtonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, parent, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            string[] scenes = GetScenes();

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            UIToolkitUtils.DropdownButtonField buttonDropdown = container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));

            (int selectedIndex, string _) = GetSelected(property);

            foreach (int index in Enumerable.Range(0, scenes.Length))
            {
                int curIndex = index;
                string curItem = scenes[index];
                string curName = $"{index}: {curItem}";

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {

                    if(property.propertyType == SerializedPropertyType.String)
                    {
                        property.stringValue = curItem;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curItem);
                    }
                    else
                    {
                        property.intValue = curIndex;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curIndex);
                    }
                    buttonDropdown.ButtonLabelElement.text = curName;
                });
            }

            if(scenes.Length > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }
            genericDropdownMenu.AddItem("Edit Scenes In Build...", false, OpenBuildSettings);

            genericDropdownMenu.DropDown(buttonDropdown.ButtonElement.worldBound, buttonDropdown, true);
        }

        private static (int index, string displayName) GetSelected(SerializedProperty property)
        {
            string[] scenes = GetScenes();
            if(property.propertyType == SerializedPropertyType.String)
            {
                string scene = property.stringValue;
                int index = Array.IndexOf(scenes, scene);
                return (index, index == -1? scene: $"{index}: {scene}");
            }
            else
            {
                int index = property.intValue;
                if(index >= scenes.Length)
                {
                    return (-1, $"{index}: ?");
                }
                string scene = scenes[index];
                return (index, $"{index}: {scene}");
            }
        }

        #endregion

#endif
    }
}
