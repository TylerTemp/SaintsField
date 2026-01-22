#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsTreeDropdownDrawer
{
    public partial class FlagsTreeDropdownAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__FlagsTreeDropdown";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FlagsTreeDropdown_HelpBox";

        private EnumFlagsMetaInfo _enumMeta;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                return new Label(GetPreferredLabel(property));
            }

            _enumMeta = EnumFlagsUtil.GetMetaInfo(property, info);
            FlagsDropdownElement intDropdownElement = new FlagsDropdownElement(_enumMeta);
            intDropdownElement.BindProperty(property);
            return new IntDropdownField(GetPreferredLabel(property), intDropdownElement)
            {
                name = NameButton(property),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                return new HelpBox($"Type {property.propertyType} is not a enum type", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }

            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };

            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                return;
            }

            IntDropdownField intDropdownField = container.Q<IntDropdownField>(NameButton(property));
            AddContextualMenuManipulator(intDropdownField, property, onValueChangedCallback, info, parent);

            intDropdownField.Button.clicked += () => MakeDropdown(property, intDropdownField, onValueChangedCallback, info, parent);
        }

        private void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
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

                if (!int.TryParse(clipboardText, out int clipboardInt))
                {
                    return;
                }

                if ((clipboardInt & _enumMeta.AllCheckedLong) != clipboardInt)
                {
                    return;
                }

                evt.menu.AppendAction($"Paste \"{clipboardInt}\"", _ =>
                {
                    property.intValue = clipboardInt;
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, clipboardInt);
                    onValueChangedCallback.Invoke(clipboardInt);
                });
            }));
        }

        private void MakeDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            EnumFlagsMetaInfo enumInfo =  EnumFlagsUtil.GetMetaInfo(property, info);
            AdvancedDropdownMetaInfo metaInfo = EnumFlagsUtil.GetDropdownMetaInfo(EnumFlagsUtil.GetSerializedPropertyEnumValue(enumInfo.EnumType, property), _enumMeta.AllCheckedLong, _enumMeta.BitValueToName);

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                true,
                (curItem, on) =>
                {
                    long selectedValue = (long)curItem;
                    long newMask;
                    if (selectedValue == 0)
                    {
                        newMask = 0;
                    }
                    else if (on)
                    {
                        newMask = EnumFlagsUtil.GetSerializedPropertyEnumValue(enumInfo.EnumType, property) | selectedValue;
                    }
                    else
                    {
                        newMask = EnumFlagsUtil.SetOffBit(property.intValue, selectedValue);
                    }

                    EnumFlagsUtil.SetSerializedPropertyEnumValue(enumInfo.EnumType, property, newMask);
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback(curItem);
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, Enum.ToObject(enumInfo.EnumType, newMask));
                    return EnumFlagsUtil.GetDropdownMetaInfo(newMask, _enumMeta.AllCheckedLong, _enumMeta.BitValueToName).CurValues;
                }
            );

            // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
            // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
            // editorWindow.Show();

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
