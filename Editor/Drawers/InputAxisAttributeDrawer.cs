using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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

        #region IMGUI

        private IReadOnlyList<string> _axisNames;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            if(_axisNames == null)
            {
                _axisNames = GetAxisNames();
            }

            int index = IndexOf(_axisNames, property.stringValue);
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, _axisNames.Select(each => new GUIContent(each)).ToArray());
                if (changeCheck.changed)
                {
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

        #region UIToolkit
        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__InputAxis_Button";
        private static string NameButtonLabelField(SerializedProperty property) => $"{property.propertyPath}__InputAxis_ButtonLabel";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            IReadOnlyList<string> axisNames = GetAxisNames();
            int selectedIndex = IndexOf(axisNames, property.stringValue);
            string buttonLabel = selectedIndex == -1 ? "-" : axisNames[selectedIndex];

            Button button = new Button(() => ShowDropdown(property, saintsAttribute, container, parent, onChange))
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

            buttonLabelContainer.Add(new Label(buttonLabel)
            {
                name = NameButtonLabelField(property),
                userData = selectedIndex,
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

            Debug.Log(EditorGUI.indentLevel);

            root.Add(Util.PrefixLabelUIToolKit(LabelState.AsIs, property.displayName, 1));
            root.Add(button);

            return root;
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            IReadOnlyList<string> axisNames = GetAxisNames();

            // Button button = container.Q<Button>(NameButtonField(property));
            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));
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
                    buttonLabel.text = axisNames[curIndex];
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            Button button = container.Q<Button>(NameButtonField(property));
            genericDropdownMenu.DropDown(button.worldBound, button, true);
        }

        #endregion
    }
}
