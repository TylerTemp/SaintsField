#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineSkinPickerDrawer
{
    public partial class SpineSkinPickerAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__SpineSkin_SelectorButton";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__SpineSkin_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new Label(GetPreferredLabel(property));
            }
            SpineSkinElement element = new SpineSkinElement();
            element.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), element)
            {
                name = NameDropdownField(property),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new HelpBox($"Type {property.propertyType} is not a string type.", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }

            return new HelpBox($"Type {property.propertyType} is not a string type.", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };
        }

        // private Texture2D _iconTexture2D;
        private ExposedList<Skin> _cachedSkins = new ExposedList<Skin>();

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            SpineSkinPickerAttribute spineSkinPickerAttribute = (SpineSkinPickerAttribute) saintsAttribute;

            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));

            StringDropdownField stringDropdownField = container.Q<StringDropdownField>(NameDropdownField(property));
            SpineSkinElement element = stringDropdownField.Q<SpineSkinElement>();
            UIToolkitUtils.AddContextualMenuManipulator(stringDropdownField, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            stringDropdownField.Button.clicked += () => MakeDropdown(GetSkinsRefresh, property,
                stringDropdownField, onValueChangedCallback, info, parent);

            ExposedList<Skin> GetSkinsRefresh()
            {
                (string error, ExposedList<Skin> skins) = SpineSkinUtils.GetSkins(spineSkinPickerAttribute.SkeletonTarget, property, info, parent);
                if (helpBox.text != error)
                {
                    helpBox.text = error;
                    helpBox.style.display = string.IsNullOrEmpty(error)? DisplayStyle.None : DisplayStyle.Flex;
                }

                if (error == "")
                {
                    if (!skins.SequenceEqual(_cachedSkins))
                    {
                        element.BindSkinList(_cachedSkins = skins);
                    }
                }

                return _cachedSkins;
            }

            GetSkinsRefresh();
        }

        private static void MakeDropdown(Func<ExposedList<Skin>> getSkinsRefresh, SerializedProperty property, StringDropdownField root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();

            dropdown.Add("[Empty String]", "");
            dropdown.AddSeparator();

            string selected = null;
            foreach (Skin skin in getSkinsRefresh())
            {
                dropdown.Add(skin.Name, skin.Name, false, SpineSkinUtils.IconPath);
                if (property.stringValue == skin.Name)
                {
                    selected = skin.Name;
                }
            }

            if (string.IsNullOrEmpty(property.stringValue))
            {
                selected = "";
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected is null ? Array.Empty<object>(): new[]{selected},
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
                    string curValue = (string)curItem;
                    property.stringValue = curValue;
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curValue);
                    onValueChangedCallback.Invoke(curValue);
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
