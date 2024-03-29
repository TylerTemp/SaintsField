﻿#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using SaintsField.AiNavigation;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor.AI;
using UnityEngine;

#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif  // UNITY_2021_3_OR_NEWER

#endif  // SAINTSFIELD_AI_NAVIGATION

namespace SaintsField.Editor.Drawers.AiNavigation
{
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute))]
    public class NavMeshAreaAttributeDrawer: SaintsPropertyDrawer
    {
        private enum ValueType
        {
            Mask,
            Index,
            String,
        }

        private static ValueType GetValueType(SerializedProperty property, NavMeshAreaAttribute navMeshAreaAttribute)
        {
            if(property.propertyType == SerializedPropertyType.Integer)
            {
                return navMeshAreaAttribute.IsMask
                    ? ValueType.Mask
                    : ValueType.Index;
            }
            return ValueType.String;
        }

        private static string FormatAreaName(AiNavigationUtils.NavMeshArea area, ValueType valueType) =>
            $"{(valueType == ValueType.Mask ? area.Mask : area.Value)}: {area.Name}";

        #region IMGUI

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            AiNavigationUtils.NavMeshArea[] areas = AiNavigationUtils.GetNavMeshAreas().ToArray();
            ValueType valueType = GetValueType(property, (NavMeshAreaAttribute)saintsAttribute);

            int areaIndex = Util.ListIndexOfAction(areas, area =>
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (valueType)
                {
                    case ValueType.Index:
                        return area.Value == property.intValue;
                    case ValueType.Mask:
                        return area.Mask == property.intValue;
                    case ValueType.String:
                        return area.Name == property.stringValue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
                }
            });

            string[] areaNames = areas
                .Select(each => FormatAreaName(each, valueType))
                .Append("")
                .Append("Open Area Settings...")
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newAreaIndex = EditorGUI.Popup(position, label.text, areaIndex, areaNames);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newAreaIndex < areas.Length)
                    {
                        if (valueType == ValueType.String)
                        {
                            property.stringValue = areas[newAreaIndex].Name;
                        }
                        else
                        {
                            property.intValue = valueType == ValueType.Mask
                                ? areas[newAreaIndex].Mask
                                : areas[newAreaIndex].Value;
                        }
                    }
                    else
                    {
                        NavMeshEditorHelpers.OpenAreaSettings();
                    }
                }
            }
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__NavMeshArea_Button";
        private static string NameButtonLabelField(SerializedProperty property) => $"{property.propertyPath}__NavMeshArea_ButtonLabel";
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__NavMeshArea_HelpBox";
        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__NavMeshArea_Label";

        private class ButtonData
        {
            // ReSharper disable once InconsistentNaming
            public readonly ValueType ValueType;
            public int selectedIndex;

            public ButtonData(ValueType valueType)
            {
                ValueType = valueType;
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, FieldInfo info, object parent)
        {
            NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute) saintsAttribute;
            ValueType valueType = GetValueType(property, navMeshAreaAttribute);

            List<AiNavigationUtils.NavMeshArea> areas = AiNavigationUtils.GetNavMeshAreas().ToList();
            int areaIndex = areas.FindIndex(area =>
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (valueType)
                {
                    case ValueType.Index:
                        return area.Value == property.intValue;
                    case ValueType.Mask:
                        return area.Mask == property.intValue;
                    case ValueType.String:
                        return area.Name == property.stringValue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
                }
            });

            string buttonLabel = areaIndex == -1
                ? "-"
                : FormatAreaName(areas[areaIndex], valueType);

            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                    paddingLeft = 1,
                    paddingRight = 1,
                },
                name = NameButtonField(property),
                userData = new ButtonData(valueType)
                {
                    selectedIndex = areaIndex,
                },
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
            });
            buttonLabelContainer.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                style =
                {
                    width = 15,
                },
            });

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            button.Add(buttonLabelContainer);

            Label label = Util.PrefixLabelUIToolKit(new string(' ', property.displayName.Length), 0);
            label.name = NameLabel(property);
            root.Add(label);
            root.Add(button);

            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButtonField(property));
            ButtonData buttonData = (ButtonData) button.userData;
            Label label = button.Q<Label>(NameButtonLabelField(property));
            // NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute) saintsAttribute;

            container.Q<Button>(NameButtonField(property)).clicked += () =>
                ShowDropdownUIToolkit(property, buttonData, button, label, onValueChangedCallback);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButtonField(property));
            ButtonData buttonData = (ButtonData) button.userData;

            ValueType valueType = buttonData.ValueType;
            int areaIndex = Util.ListIndexOfAction(AiNavigationUtils.GetNavMeshAreas(), area =>
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (valueType)
                {
                    case ValueType.Index:
                        return area.Value == property.intValue;
                    case ValueType.Mask:
                        return area.Mask == property.intValue;
                    case ValueType.String:
                        return area.Name == property.stringValue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
                }
            });

            // ReSharper disable once InvertIf
            if(areaIndex != buttonData.selectedIndex)
            {
                buttonData.selectedIndex = areaIndex;
                string buttonLabel = areaIndex == -1
                    ? "-"
                    : FormatAreaName(AiNavigationUtils.GetNavMeshAreas().ElementAt(areaIndex), valueType);
                button.Q<Label>(NameButtonLabelField(property)).text = buttonLabel;
            }
        }

        private static void ShowDropdownUIToolkit(SerializedProperty property, ButtonData buttonData,
            // ReSharper disable once SuggestBaseTypeForParameter
            Button button,
            // ReSharper disable once SuggestBaseTypeForParameter
            Label label, Action<object> onValueChangedCallback)
        {
            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

            int selectedIndex = buttonData.selectedIndex;
            // var areas = .ToList();
            foreach ((AiNavigationUtils.NavMeshArea area, int index) in AiNavigationUtils.GetNavMeshAreas().WithIndex())
            {
                int curIndex = index;
                string curName = FormatAreaName(area, buttonData.ValueType);
                genericDropdownMenu.AddItem(curName, curIndex == selectedIndex, () =>
                {
                    buttonData.selectedIndex = curIndex;
                    if (buttonData.ValueType == ValueType.String)
                    {
                        string newValue = area.Name;
                        property.stringValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(newValue);
                    }
                    else
                    {
                        int newValue = buttonData.ValueType == ValueType.Mask
                            ? area.Mask
                            : area.Value;
                        property.intValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(newValue);
                    }

                    label.text = curName;
                });
            }

            genericDropdownMenu.AddSeparator("");
            genericDropdownMenu.AddItem("Open Area Settings...", false, NavMeshEditorHelpers.OpenAreaSettings);

            genericDropdownMenu.DropDown(button.worldBound, button, true);
        }

        #endregion

#endif
    }
#endif  // SAINTSFIELD_AI_NAVIGATION
}
