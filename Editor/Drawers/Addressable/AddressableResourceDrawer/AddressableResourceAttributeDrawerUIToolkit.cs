#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

#if UNITY_2021_3_OR_NEWER

namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
{
    public partial class AddressableResourceAttributeDrawer
    {
        private static string ButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Button";
        private static string HelpBoxName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_HelpBox";

        private static string GroupDownName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Group";
        private static string LabelDownName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Label";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            Button button = new Button
            {
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("folder.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(14, 14),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    paddingLeft = 8,
                    paddingRight = 8,
                },
                name = ButtonName(property),
            };
            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    backgroundColor = EColor.EditorEmphasized.GetColor(),
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 4,
                    paddingRight = 8,
                },
            };

            VisualElement actionArea = new VisualElement();
            root.Add(actionArea);

            ObjectField objField = new ObjectField("Resource");
            objField.AddToClassList(ClassAllowDisable);
            objField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            actionArea.Add(objField);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit("Group");
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = GroupDownName(property);
            dropdownButton.AddToClassList(ClassAllowDisable);
            actionArea.Add(dropdownButton);
            // dropdownButton.ButtonLabelElement.text = GetSelectedNames(metaInfo.BitValueToName, property.intValue);

            UIToolkitUtils.DropdownButtonField dropdownLabel = UIToolkitUtils.MakeDropdownButtonUIToolkit("Label");
            dropdownLabel.style.flexGrow = 1;
            dropdownLabel.name = LabelDownName(property);
            dropdownLabel.AddToClassList(ClassAllowDisable);
            actionArea.Add(dropdownLabel);

            VisualElement buttons = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    height = SingleLineHeight,
                },
            };
            buttons.Add(new Button
            {
                style =
                {
                    // flexGrow = 1,
                    backgroundImage = Util.LoadResource<Texture2D>("trash.png"),
                    unityBackgroundImageTintColor = Color.red,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            });

            buttons.Add(new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            });

            buttons.Add(new Button
            {
                style =
                {
                    // flexGrow = 1,
                    backgroundImage = Util.LoadResource<Texture2D>("check.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            });

            buttons.Add(new Button
            {
                style =
                {
                    // flexGrow = 1,
                    backgroundImage = Util.LoadResource<Texture2D>("close.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            });
            actionArea.Add(buttons);

            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = HelpBoxName(property),
            };

            root.Add(helpBox);
            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));
            Button button = container.Q<Button>(ButtonName(property));
            if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
            {
                helpBox.text = "No addressable config found. Please create one.";
                button.SetEnabled(false);
                // EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
                return;
            }

            UIToolkitUtils.DropdownButtonField groupDown = container.Q<UIToolkitUtils.DropdownButtonField>(GroupDownName(property));
            groupDown.ButtonElement.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
                foreach (AddressableAssetGroup group in settings.groups)
                {
                    genericDropdownMenu.AddItem(group.Name, group.Name == groupDown.ButtonLabelElement.text, () =>
                    {
                        groupDown.ButtonLabelElement.text = group.name;
                    });
                }
                genericDropdownMenu.AddSeparator("");
                genericDropdownMenu.AddItem("Edit Groups...", false, AddressableUtil.OpenGroupEditor);
                genericDropdownMenu.DropDown(groupDown.ButtonElement.worldBound, groupDown, true);
            };

            UIToolkitUtils.DropdownButtonField labelDown = container.Q<UIToolkitUtils.DropdownButtonField>(LabelDownName(property));
            labelDown.userData = new string[]{};
            labelDown.ButtonElement.clicked += () =>
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
                List<string> labels = settings.GetLabels();

                AdvancedDropdownList<string> dropdownListValue = new AdvancedDropdownList<string>();
                List<AdvancedDropdownMetaInfo> metaInfos = new List<AdvancedDropdownMetaInfo>();
                string[] curValues = (string[])labelDown.userData;
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues.Length > 0 ? curValues[curValues.Length - 1] : "",
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);

                foreach (string label in labels)
                {
                    dropdownListValue.Add(label, label);
                }

                AdvancedDropdownMetaInfo dropdownMetaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurValues = curValues,
                    DropdownListValue = dropdownListValue,
                    SelectStacks = stacks,
                };

                float maxHeight = Screen.currentResolution.height - labelDown.worldBound.y - labelDown.worldBound.height - 100;
                Rect worldBound = labelDown.worldBound;
                if (maxHeight < 100)
                {
                    worldBound.y -= 100 + worldBound.height;
                    maxHeight = 100;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    dropdownMetaInfo,
                    labelDown.worldBound.width,
                    maxHeight,
                    true,
                    (_, curItem) =>
                    {
                        string selected = (string)curItem;
                        if (selected == "")
                        {
                            AddressableUtil.
                        }
                        int selectedValue = (int)curItem;
                        int newMask = selectedValue == 0
                            ? 0
                            : EnumFlagsUtil.ToggleBit(property.intValue, selectedValue);
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newMask);
                        property.intValue = newMask;
                        property.serializedObject.ApplyModifiedProperties();

                        dropdownButton.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property)).ButtonLabelElement.text = GetSelectedNames(metaInfo.BitValueToName, newMask);
                        dropdownButton.userData = curItem;
                        onValueChangedCallback(curItem);
                    }
                ));
            };
        }
    }
}

#endif

#endif
