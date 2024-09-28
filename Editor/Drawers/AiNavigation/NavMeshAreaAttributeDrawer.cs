#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
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
            VisualElement container, FieldInfo info, object parent)
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

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButtonField(property);
            dropdownButton.ButtonLabelElement.text = buttonLabel;
            dropdownButton.userData = new ButtonData(valueType)
            {
                selectedIndex = areaIndex,
            };

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField button = container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));
            ButtonData buttonData = (ButtonData) button.userData;
            // NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute) saintsAttribute;

            button.ButtonElement.clicked += () =>
                ShowDropdownUIToolkit(property, buttonData, button, onValueChangedCallback);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            UIToolkitUtils.DropdownButtonField button = container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));
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
                button.ButtonLabelElement.text = buttonLabel;
            }
        }

        private static void ShowDropdownUIToolkit(SerializedProperty property, ButtonData buttonData,
            UIToolkitUtils.DropdownButtonField button,
            Action<object> onValueChangedCallback)
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

                    button.ButtonLabelElement.text = curName;
                });
            }

            genericDropdownMenu.AddSeparator("");
            genericDropdownMenu.AddItem("Open Area Settings...", false, NavMeshEditorHelpers.OpenAreaSettings);

            genericDropdownMenu.DropDown(button.ButtonElement.worldBound, button.ButtonElement, true);
        }

        #endregion

#endif
    }
#endif  // SAINTSFIELD_AI_NAVIGATION
}
