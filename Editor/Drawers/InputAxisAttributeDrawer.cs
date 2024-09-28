using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using System;
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InputAxisAttribute))]
    public class InputAxisAttributeDrawer: SaintsPropertyDrawer
    {
        private static IReadOnlyList<string> GetAxisNames()
        {
            SerializedObject inputAssetSettings = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/InputManager.asset"));
            SerializedProperty axesProperty = inputAssetSettings.FindProperty("m_Axes");
            List<string> axisNames = new List<string>();
            for (int index = 0; index < axesProperty.arraySize; index++)
            {
                axisNames.Add(axesProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_Name").stringValue);
            }

            return axisNames;
        }

        private static void OpenInputManager()
        {
            SettingsService.OpenProjectSettings("Project/Input Manager");
        }

        #region IMGUI

        private IReadOnlyList<string> _axisNames;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_axisNames == null)
            {
                _axisNames = GetAxisNames();
            }

            int index = IndexOf(_axisNames, property.stringValue);
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                GUIContent optionContent = new GUIContent("Open Input Manager...");
                int newIndex = EditorGUI.Popup(position, label, index, _axisNames
                    .Select(each => new GUIContent(each))
                    .Concat(new[]{GUIContent.none, optionContent})
                    .ToArray());
                // ReSharper disable once InvertIf
                if (changeCheck.changed)
                {
                    if(newIndex >= _axisNames.Count)
                    {
                        OpenInputManager();
                        return;
                    }
                    property.stringValue = _axisNames[newIndex];
                }
            }
        }

        private static int IndexOf(IEnumerable<string> axisNames, string value)
        {
            // int index = Array.IndexOf(scenes, scene);
            // return index == -1? 0: index;
            foreach ((string axisName, int index) in axisNames.Select((axisName, index) => (axisName, index)))
            {
                if (axisName == value)
                {
                    return index;
                }
            }

            return -1;
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit
        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__InputAxis";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            IReadOnlyList<string> axisNames = GetAxisNames();
            int selectedIndex = IndexOf(axisNames, property.stringValue);
            string buttonLabel = selectedIndex == -1 ? "-" : axisNames[selectedIndex];

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButtonField(property);
            dropdownButton.ButtonLabelElement.text = buttonLabel;

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property)).ButtonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, parent, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            IReadOnlyList<string> axisNames = GetAxisNames();

            // Button button = container.Q<Button>(NameButtonField(property));
            UIToolkitUtils.DropdownButtonField buttonLabel = container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));
            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

            int selectedIndex = IndexOf(axisNames, property.stringValue);

            // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
            foreach (int index in Enumerable.Range(0, axisNames.Count))
            {
                int curIndex = index;

                genericDropdownMenu.AddItem(axisNames[index], index == selectedIndex, () =>
                {
                    property.stringValue = axisNames[curIndex];
                    onChange(axisNames[curIndex]);
                    buttonLabel.ButtonLabelElement.text = axisNames[curIndex];
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            if(axisNames.Count > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }

            genericDropdownMenu.AddItem("Open Input Manager...", false, OpenInputManager);

            genericDropdownMenu.DropDown(buttonLabel.ButtonElement.worldBound, buttonLabel, true);
        }

        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, string labelOrNull, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks,
        //     bool tried, RichTextDrawer richTextDrawer)
        // {
        //     Label buttonLabel = container.Q<Label>(NameLabel(property));
        //     buttonLabel.text = labelOrNull ?? "";
        //     buttonLabel.style.display = labelOrNull == null? DisplayStyle.None: DisplayStyle.Flex;
        // }

        #endregion

#endif
    }
}
