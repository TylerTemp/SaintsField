#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    public partial class AddressableLabelAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container1,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            AddressableLabelElement element = new AddressableLabelElement();
            element.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), element)
            {
                name = NameDropdownField(property),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new HelpBox($"Type {property.propertyType} is not a string type", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                        flexGrow = 1,
                    },
                };
            }

            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                return new HelpBox($"Addressable Settings not created.", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                    },
                    name = NameHelpBox(property),
                };
            }

            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }
            StringDropdownField field = container.Q<StringDropdownField>(NameDropdownField(property));

            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            field.Button.clicked += () => ShowDropdown(property, field, onValueChangedCallback, info, parent);

            // ReSharper disable once InvertIf
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

                void CheckHelpBoxDisplay()
                {
                    if (AddressableAssetSettingsDefaultObject.Settings != null)
                    {
                        helpBox.style.display = DisplayStyle.None;
                    }
                }

                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(CheckHelpBoxDisplay);
                field.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(CheckHelpBoxDisplay));
            }
        }

        private static void ShowDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();

            string selected = null;
            if (settings == null)
            {
                dropdown.Add("Create Addressable Settings...", null);
            }
            else
            {
                List<string> labels = settings.GetLabels();

                foreach (string label in labels)
                {
                    dropdown.Add(new AdvancedDropdownList<string>(label, label));
                    if (property.stringValue == label)
                    {
                        selected = label;
                    }
                }

                if (labels.Count > 0)
                {
                    dropdown.AddSeparator();
                }

                dropdown.Add("Edit Labels...", null, false, "d_editicon.sml");
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected is null
                    ? Array.Empty<object>()
                    : new object[] { selected },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    string newValue = (string)curItem;
                    if (newValue is null)
                    {
                        if (settings == null)
                        {
                            AddressableAssetSettingsDefaultObject.GetSettings(true);
                        }
                        else
                        {
                            AddressableUtil.OpenLabelEditor();
                        }
                        return;
                    }

                    property.stringValue = newValue;
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(newValue);
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
