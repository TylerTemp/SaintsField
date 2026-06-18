#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
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
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                return null;
            }

            return new HelpBox($"Type {property.propertyType} is not int.", HelpBoxMessageType.Error)
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

        private static void MakeDropdown(SerializedProperty property, VisualElement root,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownMetaInfo metaInfo = GetDropdownMetaInfo(property.intValue);

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                true,
                (curItem, isOn) => ApplyDropdownSelection(property, info, parent, (DropdownItem)curItem, isOn,
                    onValueChangedCallback)
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
