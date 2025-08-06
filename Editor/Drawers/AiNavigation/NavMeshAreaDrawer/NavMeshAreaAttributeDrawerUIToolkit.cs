#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.AiNavigation;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
    public partial class NavMeshAreaAttributeDrawer
    {
        #region UI Toolkit

        private static string NameButtonField(SerializedProperty property) =>
            $"{property.propertyPath}__NavMeshArea_Button";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute)saintsAttribute;
                    NavMeshAreaIntElement element = new NavMeshAreaIntElement(navMeshAreaAttribute);
                    element.BindProperty(property);
                    return new IntDropdownField(GetPreferredLabel(property), element)
                    {
                        name = NameButtonField(property),
                    };
                }
                case SerializedPropertyType.String:
                {
                    NavMeshAreaStringElement element = new NavMeshAreaStringElement();
                    element.BindProperty(property);
                    return new StringDropdownField(GetPreferredLabel(property), element)
                    {
                        name = NameButtonField(property),
                    };
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
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
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
                default:
                    return;
            }
        }

        private enum SpecialDropdownPick
        {
            None,
            EmptyString,
            OpenSettings,
        }

        private readonly struct DropdownItem: IEquatable<DropdownItem>
        {
            public readonly AiNavigationUtils.NavMeshArea Area;
            public readonly SpecialDropdownPick Pick;

            public DropdownItem(AiNavigationUtils.NavMeshArea area, SpecialDropdownPick pick)
            {
                Area = area;
                Pick = pick;
            }

            public DropdownItem(SpecialDropdownPick pick)
            {
                Area = default;
                Pick = pick;
            }

            public bool Equals(DropdownItem other)
            {
                return Area.Equals(other.Area);
            }

            public override bool Equals(object obj)
            {
                return obj is DropdownItem other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Area.GetHashCode();
            }
        }

        private static bool IsMatchedInt(NavMeshAreaAttribute navMeshAreaAttribute, AiNavigationUtils.NavMeshArea area, int value)
        {
            return navMeshAreaAttribute.IsMask
                ? (area.Mask & value) != 0
                : area.Value == value;
        }

        private static void MakeDropdown(NavMeshAreaAttribute navMeshAreaAttribute, SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            AdvancedDropdownList<DropdownItem> dropdown = new AdvancedDropdownList<DropdownItem>();
            if (isString)
            {
                dropdown.Add("[Empty String]", new DropdownItem(SpecialDropdownPick.EmptyString));
                dropdown.AddSeparator();
            }

            List<DropdownItem> selectedItems = new List<DropdownItem>();
            foreach (AiNavigationUtils.NavMeshArea area in AiNavigationUtils.GetNavMeshAreas())
            {
                // dropdown.Add(path, (path, index));
                DropdownItem dropdownItem = new DropdownItem(area, SpecialDropdownPick.None);
                string displayName;
                if (isString)
                {
                    displayName = area.Name;
                }
                else if (navMeshAreaAttribute.IsMask)
                {
                    displayName = $"{area.Name} <color=#808080>({area.Mask})</color>";
                }
                else
                {
                    displayName = $"{area.Name} <color=#808080>({area.Value})</color>";
                }
                dropdown.Add(displayName, dropdownItem);
                // ReSharper disable once InvertIf
                if (isString && area.Name == property.stringValue
                    || !isString && IsMatchedInt(navMeshAreaAttribute, area, property.intValue))
                {
                    selectedItems.Add(dropdownItem);
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Open Area Settings...", new DropdownItem(SpecialDropdownPick.OpenSettings), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedItems.Count >= 0 ? selectedItems.Cast<object>().ToArray() : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                navMeshAreaAttribute.IsMask,
                (_, curItem) =>
                {
                    DropdownItem curValue = (DropdownItem)curItem;
                    switch (curValue.Pick)
                    {
                        case SpecialDropdownPick.EmptyString:
                        {
                            Debug.Assert(isString);
                            property.stringValue = "";
                            property.serializedObject.ApplyModifiedProperties();
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, "");
                            onValueChangedCallback.Invoke("");
                        }
                            break;
                        case SpecialDropdownPick.OpenSettings:
                        {
                            NavMeshEditorHelpers.OpenAreaSettings();
                        }
                            break;
                        default:
                        {
                            if (isString)
                            {
                                property.stringValue = curValue.Area.Name;
                                property.serializedObject.ApplyModifiedProperties();
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curValue.Area.Name);
                                onValueChangedCallback.Invoke(curValue.Area.Name);
                            }
                            else
                            {
                                int curMask = curValue.Area.Mask;
                                int oldMask = property.intValue;
                                int useValue = curMask;
                                if (navMeshAreaAttribute.IsMask)
                                {
                                    useValue = EnumFlagsUtil.ToggleBit(oldMask, curMask);
                                }
                                property.intValue = useValue;
                                property.serializedObject.ApplyModifiedProperties();
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, useValue);
                                onValueChangedCallback.Invoke(useValue);
                            }
                        }
                            break;
                    }
                }
            );

            // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
            // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
            // editorWindow.Show();

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
                    : $"\"{matchedNames[0]}\"...";

                evt.menu.AppendAction($"Paste {targetNames} ({filteredMask})", _ =>
                {
                    property.intValue = filteredMask;
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, filteredMask);
                    onValueChangedCallback.Invoke(filteredMask);
                });
            }));
        }

        #endregion
    }
}
#endif
