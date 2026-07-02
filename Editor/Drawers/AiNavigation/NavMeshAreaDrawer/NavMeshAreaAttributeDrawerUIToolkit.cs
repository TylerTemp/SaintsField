#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.AiNavigation;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
    public partial class NavMeshAreaAttributeDrawer
    {
        private static string NameButtonField(SerializedProperty property) =>
            $"{property.propertyPath}__NavMeshArea_Button";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute)saintsAttribute;
                    NavMeshAreaIntElement element = new NavMeshAreaIntElement(navMeshAreaAttribute.IsMask);
                    element.BindProperty(property);
                    IntDropdownField intDropdownField = new IntDropdownField(GetPreferredLabel(property), element)
                    {
                        name = NameButtonField(property),
                    };
                    if (!string.IsNullOrEmpty(property.tooltip) && intDropdownField.labelElement != null)
                    {
                        intDropdownField.labelElement.tooltip = property.tooltip;
                    }
                    return intDropdownField;
                }
                case SerializedPropertyType.String:
                {
                    NavMeshAreaStringElement element = new NavMeshAreaStringElement();
                    element.BindProperty(property);
                    StringDropdownField stringDropdownField = new StringDropdownField(GetPreferredLabel(property), element)
                    {
                        name = NameButtonField(property),
                    };
                    if (!string.IsNullOrEmpty(property.tooltip) && stringDropdownField.labelElement != null)
                    {
                        stringDropdownField.labelElement.tooltip = property.tooltip;
                    }
                    return stringDropdownField;
                }
                default:
                    return new VisualElement();
            }
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.String:
                    return null;
                default:
                    return new HelpBox($"Type {property.propertyType} is not int or string.", HelpBoxMessageType.Error)
                    {
                        style =
                        {
                            flexGrow = 1,
                        },
                    };
            }
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute)saintsAttribute;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    IntDropdownField intDropdownField = container.Q<IntDropdownField>(NameButtonField(property));
                    AddContextualMenuManipulator(navMeshAreaAttribute, intDropdownField, property, onValueChangedCallback, info, parent);

                    intDropdownField.Button.clicked += () => MakeDropdown(navMeshAreaAttribute, property, intDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    StringDropdownField layerStringField = container.Q<StringDropdownField>(NameButtonField(property));
                    UIToolkitUtils.AddContextualMenuManipulator(layerStringField, property,
                        () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

                    layerStringField.Button.clicked += () => MakeDropdown(navMeshAreaAttribute, property, layerStringField, onValueChangedCallback, info, parent);
                }
                    break;
            }
        }

        private static void MakeDropdown(NavMeshAreaAttribute navMeshAreaAttribute, SerializedProperty property,
            VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownMetaInfo metaInfo = GetDropdownMetaInfo(navMeshAreaAttribute, property);

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                property.propertyType == SerializedPropertyType.Integer && navMeshAreaAttribute.IsMask,
                (curItem, isOn) => ApplyDropdownSelection(navMeshAreaAttribute, property, info, parent,
                    (DropdownItem)curItem, isOn, onValueChangedCallback)
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private static void AddContextualMenuManipulator(NavMeshAreaAttribute navMeshAreaAttribute, IntDropdownField root, SerializedProperty property, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);
                if (!canBeInt)
                {
                    return;
                }

                int filteredMask = 0;
                List<string> matchedNames = new List<string>();

                foreach (AiNavigationUtils.NavMeshArea area in AiNavigationUtils.GetNavMeshAreas())
                {
                    if (navMeshAreaAttribute.IsMask)
                    {
                        if ((clipboardInt & area.Mask) != 0)
                        {
                            filteredMask |= area.Mask;
                            matchedNames.Add(area.Name);
                        }
                    }
                    else
                    {
                        if (area.Value == clipboardInt)
                        {
                            filteredMask = area.Value;
                            matchedNames.Add(area.Name);
                            break;
                        }
                    }
                }

                if (matchedNames.Count == 0)
                {
                    return;
                }

                string targetNames = matchedNames.Count == 1
                    ? $"\"{matchedNames[0]}\""
                    : $"\"{matchedNames[0]}\"...[{matchedNames.Count}]";

                evt.menu.AppendAction($"Paste {targetNames} ({filteredMask})", _ =>
                {
                    property.intValue = filteredMask;
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, filteredMask);
                    onValueChangedCallback.Invoke(filteredMask);
                });
            }));
        }
    }
}
#endif
