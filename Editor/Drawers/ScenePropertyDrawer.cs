using System;
using System.IO;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{

    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class ScenePropertyDrawer : SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            string[] scenes = GetScenes();
            bool anySceneInBuildSettings = scenes.Length > 0;
            if (!anySceneInBuildSettings)
            {
                // DrawDefaultPropertyAndHelpBox(rect, property, label, "No scenes in the build settings", MessageType.Warning);
                _error = "No scenes in the build settings";
                DefaultDrawer(position, property, label);
                return;
            }

            string[] sceneOptions = scenes
                .Select((name, index) => $"{name} [{index}]")
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
                    DefaultDrawer(position, property, label);
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
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
                if (changeCheck.changed)
                {
                    property.stringValue = scenes[newIndex];
                }
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] sceneOptions)
        {
            int index = property.intValue;
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
                if (changeCheck.changed)
                {
                    property.intValue = newIndex;
                }
            }
        }

        private static int IndexOfOrZero(string[] scenes, string scene)
        {
            int index = Array.IndexOf(scenes, scene);
            return index == -1? 0: index;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
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

        #region UIToolkit

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__Scene_Button";
        private static string NameButtonLabelField(SerializedProperty property) => $"{property.propertyPath}__Scene_ButtonLabel";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Scene_HelpBox";
        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Scene_Label";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
        {
            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                },
                name = NameButtonField(property),
            };

            VisualElement buttonLabelContainer = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                },
            };

            buttonLabelContainer.Add(new Label(property.stringValue)
            {
                name = NameButtonLabelField(property),
            });
            buttonLabelContainer.Add(new Label("▼"));

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            button.Add(buttonLabelContainer);

            Label label = Util.PrefixLabelUIToolKit(new string(' ', property.displayName.Length), 1);
            label.name = NameLabel(property);
            root.Add(label);
            root.Add(button);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));
            (int _, string displayName) = GetSelected(property);
            buttonLabel.text = displayName;

            container.Q<Button>(NameButtonField(property)).clicked += () =>
                ShowDropdown(property, saintsAttribute, container, parent, onValueChangedCallback);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull)
        {
            Label label = container.Q<Label>(NameLabel(property));
            label.text = labelOrNull ?? "";
            label.style.display = labelOrNull == null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            string[] scenes = GetScenes();

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            Button button = container.Q<Button>(NameButtonField(property));

            if (scenes.Length == 0)
            {
                genericDropdownMenu.AddDisabledItem("No scenes in the build settings", false);
                genericDropdownMenu.DropDown(button.worldBound, button, true);
                return;
            }

            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));
            (int selectedIndex, string _) = GetSelected(property);

            foreach (int index in Enumerable.Range(0, scenes.Length))
            {
                int curIndex = index;
                string curItem = scenes[index];
                string curName = $"{curItem} [{index}]";

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {

                    if(property.propertyType == SerializedPropertyType.String)
                    {
                        property.stringValue = curItem;
                    }
                    else
                    {
                        property.intValue = curIndex;
                    }
                    property.serializedObject.ApplyModifiedProperties();
                    buttonLabel.text = curName;
                    // buttonLabel.userData = curIndex;
                    // property.serializedObject.ApplyModifiedProperties();
                });
            }


            genericDropdownMenu.DropDown(button.worldBound, button, true);
        }

        private static (int index, string displayName) GetSelected(SerializedProperty property)
        {
            string[] scenes = GetScenes();
            if(property.propertyType == SerializedPropertyType.String)
            {
                string scene = property.stringValue;
                int index = Array.IndexOf(scenes, scene);
                return (index, index == -1? scene: $"{scene} [{index}]");
            }
            else
            {
                int index = property.intValue;
                string scene = scenes[index];
                return (index, $"{scene} [{index}]");
            }
        }

        #endregion
    }
}
