#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaMaskDrawer
{
    public partial class NavMeshAreaMaskAttributeDrawer
    {
        private static string NameMaskField(SerializedProperty property) => $"{property.propertyPath}__NavMeshAreaMask";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                return new VisualElement();
            }
            NavMeshAreaIntElement element = new NavMeshAreaIntElement(true);
            element.BindProperty(property);
            return new IntDropdownField(GetPreferredLabel(property), element)
            {
                name = NameMaskField(property),
            };

        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return null;
            }

            return new HelpBox($"Type {property.propertyType} is not int or string.", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                return;
            }

            IntDropdownField intDropdownField = container.Q<IntDropdownField>(NameMaskField(property));
            AddContextualMenuManipulator(intDropdownField, property,
                onValueChangedCallback, info, parent);

            intDropdownField.Button.clicked += () => MakeDropdown(property,
                intDropdownField, onValueChangedCallback, info, parent);
        }

        private enum SpecialDropdownPick
        {
            Value,
            Everything,
            Nothing,
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

        private static void MakeDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownList<DropdownItem> dropdown = new AdvancedDropdownList<DropdownItem>();
            AiNavigationUtils.NavMeshArea[] allAreas = AiNavigationUtils.GetNavMeshAreas().ToArray();

            List<DropdownItem> selectedItems = new List<DropdownItem>();

            int allMask = allAreas.Aggregate(0, (current, area) => current | area.Mask);
            AiNavigationUtils.NavMeshArea allArea = new AiNavigationUtils.NavMeshArea
            {
                Name = "[Everything]",
                Value = int.MaxValue,
                Mask = allMask,
            };
            DropdownItem allItem = new DropdownItem(allArea, SpecialDropdownPick.Everything);
            dropdown.Add($"<b>Everything</b> <color=#808080>({allMask})</color>", allItem);
            if ((property.intValue & allMask) == allMask)
            {
                selectedItems.Add(allItem);
            }

            AiNavigationUtils.NavMeshArea noArea = new AiNavigationUtils.NavMeshArea
            {
                Name = "[Nothing]",
                Value = int.MinValue,
                Mask = 0,
            };
            DropdownItem noItem = new DropdownItem(noArea, SpecialDropdownPick.Nothing);
            dropdown.Add("<b>Nothing</b>", noItem);
            if (property.intValue == 0)
            {
                selectedItems.Add(noItem);
            }

            dropdown.AddSeparator();

            foreach (AiNavigationUtils.NavMeshArea area in allAreas)
            {
                // dropdown.Add(path, (path, index));
                DropdownItem dropdownItem = new DropdownItem(area, SpecialDropdownPick.Value);
                string displayName = $"{area.Name} <color=#808080>({area.Mask})</color>";
                dropdown.Add(displayName, dropdownItem);
                // ReSharper disable once InvertIf
                if ((area.Mask & property.intValue) != 0)
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
                true,
                (_, curItem) =>
                {
                    DropdownItem curValue = (DropdownItem)curItem;
                    switch (curValue.Pick)
                    {
                        case SpecialDropdownPick.OpenSettings:
                        {
                            NavMeshEditorHelpers.OpenAreaSettings();
                        }
                            break;
                        case SpecialDropdownPick.Everything:
                        {
                            property.intValue = curValue.Area.Mask;
                            property.serializedObject.ApplyModifiedProperties();
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curValue.Area.Mask);
                            onValueChangedCallback.Invoke(curValue.Area.Mask);
                        }
                            break;
                        case SpecialDropdownPick.Nothing:
                        {
                            property.intValue = 0;
                            property.serializedObject.ApplyModifiedProperties();
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, 0);
                            onValueChangedCallback.Invoke(0);
                        }
                            break;
                        case SpecialDropdownPick.Value:
                        {
                            int curMask = curValue.Area.Mask;
                            int oldMask = property.intValue;
                            int useValue = EnumFlagsUtil.ToggleBit(oldMask, curMask);
                            property.intValue = useValue;
                            property.serializedObject.ApplyModifiedProperties();
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, useValue);
                            onValueChangedCallback.Invoke(useValue);
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(curValue.Pick), curValue.Pick, null);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private static void AddContextualMenuManipulator(IntDropdownField root, SerializedProperty property, Action<object> onValueChangedCallback, FieldInfo info, object parent)
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
                    // ReSharper disable once InvertIf
                    if ((clipboardInt & area.Mask) != 0)
                    {
                        filteredMask |= area.Mask;
                        matchedNames.Add(area.Name);
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
