#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
// using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
{
    public partial class AddressableResourceAttributeDrawer
    {
        private static string ToggleButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Button";
        private static string HelpBoxName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_HelpBox";

        private static string ActionAreaName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_ActionArea";
        private static string GroupDownName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Group";
        private static string LabelDownName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Label";
        private static string ResourceObjectName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Resource";

        private static string NameInputContainerName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_NameInputContainer";

        private static string NameInputName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_Name";
        private static string NameButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_NameButton";

        private static string SaveButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_SaveButton";

        private static string DeleteButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_DeleteButton";

        private static string CheckButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_CheckButton";

        private static string CloseButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableResource_CloseButton";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            ToolbarToggle toolbarToggle = new ToolbarToggle
            {
                // text = "A",
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("pencil.png"),
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
                name = ToggleButtonName(property),
            };
            return toolbarToggle;
        }

        private bool _isSprite;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement();

            VisualElement actionArea = new VisualElement
            {
                name = ActionAreaName(property),
                style =
                {
                    display = DisplayStyle.None,

                    backgroundColor = EColor.EditorEmphasized.GetColor(),
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 4,
                    paddingRight = 8,
                },
            };
            root.Add(actionArea);

            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            _isSprite = typeof(AssetReferenceSprite).IsAssignableFrom(fieldType);

            ObjectField objField = new ObjectField("Resource")
            {
                objectType = _isSprite? typeof(Sprite): typeof(Object),
                name = ResourceObjectName(property),
            };
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

            VisualElement nameInputContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                },
                name = NameInputContainerName(property),
            };
            TextField nameInput = new TextField("Name")
            {
                name = NameInputName(property),
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    // borderTopRightRadius = 0,
                    // borderBottomRightRadius = 0,
                },
            };
            nameInput.styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/TextFieldRightNoRadius.uss"));
            nameInput.AddToClassList(ClassAllowDisable);
            nameInput.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            nameInputContainer.Add(nameInput);

            Button nameButton = new Button
            {
                style =
                {
                    width = SingleLineHeight,
                    backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(SingleLineHeight - 4, SingleLineHeight),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    borderLeftWidth = 0,
                },
                name = NameButtonName(property),
            };
            nameInputContainer.Add(nameButton);

            actionArea.Add(nameInputContainer);

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
                    backgroundImage = Util.LoadResource<Texture2D>("save.png"),
                    unityBackgroundImageTintColor = Color.green,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
                name = SaveButtonName(property),
            });
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
                name = DeleteButtonName(property),
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
                name = CheckButtonName(property),
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
                name = CloseButtonName(property),
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));
            ToolbarToggle toggleButton = container.Q<ToolbarToggle>(ToggleButtonName(property));
            if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
            {
                helpBox.text = "No addressable config found. Please create one.";
                toggleButton.SetEnabled(false);
                return;
            }

            VisualElement actionArea = container.Q<VisualElement>(ActionAreaName(property));
            toggleButton.RegisterValueChangedCallback(evt =>
            {
                actionArea.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            // toggleButton.clicked += () => actionArea.style.display = actionArea.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;

            // Debug.Log(info.FieldType);
            ObjectField objField = container.Q<ObjectField>(ResourceObjectName(property));

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
                string[] curValues = (string[])labelDown.userData;
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues.Length > 0 ? curValues[curValues.Length - 1] : "",
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);

                bool hasLabels = false;
                foreach (string label in labels)
                {
                    hasLabels = true;
                    dropdownListValue.Add(label, label);
                }

                if (hasLabels)
                {
                    dropdownListValue.AddSeparator();
                }

                dropdownListValue.Add("Edit Labels...", "");

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
                        string selectedValue = (string)curItem;
                        if (selectedValue == "")
                        {
                            AddressableUtil.OpenLabelEditor();
                            return;
                        }

                        string[] newData = Array.IndexOf(curValues, selectedValue) == -1
                            ? curValues.Append(selectedValue).ToArray()
                            : curValues.Where(each => each != selectedValue).ToArray();

                        labelDown.userData = newData;
                        labelDown.ButtonLabelElement.text = newData.Length == 0 ? "" : string.Join(",", newData);
                    }
                ));
            };

            // Debug.Log(property.propertyType);
            // Debug.Log(property.FindPropertyRelative("m_AssetGUID").stringValue);

            VisualElement nameInputContainer = container.Q<VisualElement>(NameInputContainerName(property));
            TextField nameInput = nameInputContainer.Q<TextField>(NameInputName(property));
            Button nameButton = nameInputContainer.Q<Button>(NameButtonName(property));
            nameButton.userData = NameType.FilePath;
            nameButton.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                foreach (NameType nameType in Enum.GetValues(typeof(NameType)).Cast<NameType>())
                {
                    genericDropdownMenu.AddItem(nameType.ToFriendlyString(), false, () =>
                    {
                        nameButton.userData = nameType;
                        Object curObj = objField.value;
                        if (curObj == null)
                        {
                            return;
                        }

                        nameInput.value = GetObjectName(nameType, curObj);
                    });
                }
                genericDropdownMenu.DropDown(nameInputContainer.worldBound, nameInputContainer, true);
            };

            objField.RegisterValueChangedCallback(evt =>
            {
                Object curObj = evt.newValue;
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
                AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(curObj)).ToString());
                if(entry == null)
                {
                    nameInput.value = GetObjectName((NameType)nameButton.userData, curObj);
                    return;
                }

                groupDown.ButtonLabelElement.text = entry.parentGroup.Name;
                labelDown.userData = entry.labels.ToArray();
                labelDown.ButtonLabelElement.text = string.Join(",", entry.labels);
                nameInput.value = entry.address;
            });

            Button saveButton = container.Q<Button>(SaveButtonName(property));
            saveButton.clicked += () => SaveToAddressable();
            Button deleteButton = container.Q<Button>(DeleteButtonName(property));
            deleteButton.clicked += () =>
            {
                Object curObj = objField.value;
                if (curObj == null)
                {
                    return;
                }

                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(curObj));
                settings.RemoveAssetEntry(guid);

                objField.value = null;
                groupDown.ButtonLabelElement.text = "";
                labelDown.userData = Array.Empty<string>();
                labelDown.ButtonLabelElement.text = "";
                nameInput.value = "";
            };

            Button checkButton = container.Q<Button>(CheckButtonName(property));
            checkButton.clicked += () =>
            {
                (string guid, Object target) = SaveToAddressable();
                if (string.IsNullOrEmpty(guid))
                {
                    return;
                }

                property.FindPropertyRelative("m_AssetGUID").stringValue = guid;


                // sub asset
                if (_isSprite && target is Sprite sprite)
                {
                    property.FindPropertyRelative("m_SubObjectName").stringValue = sprite.name;
                    property.FindPropertyRelative("m_SubObjectType").stringValue = typeof(Sprite).AssemblyQualifiedName;
                }

                property.serializedObject.ApplyModifiedProperties();
                CloseActionArea();
            };

            Button closeButton = container.Q<Button>(CloseButtonName(property));
            closeButton.clicked += CloseActionArea;

            string guid = property.FindPropertyRelative("m_AssetGUID").stringValue;
            // ReSharper disable once InvertIf
            if (!string.IsNullOrEmpty(guid))
            {
                Object curObj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                if (curObj != null)
                {
                    UpdateValue(property, container, curObj);
                }
            }

            return;

            (string guid, Object target) SaveToAddressable() {
                Object curObj = objField.value;
                if (curObj == null)
                {
                    return (null, null);
                }

                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

                string groupName = groupDown.ButtonLabelElement.text;
                AddressableAssetGroup group = settings.groups.FirstOrDefault(each => each.Name == groupName);

                string curGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(curObj));
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(curGuid, group);
                entry.address = nameInput.value;

                IReadOnlyList<string> useLabels = (IReadOnlyList<string>)labelDown.userData;
                foreach (string eachLabel in settings.GetLabels())
                {
                    entry.SetLabel(eachLabel, useLabels.Contains(eachLabel));
                }

                return (curGuid, curObj);
            }

            void CloseActionArea()
            {
                // actionArea.style.display = DisplayStyle.None;
                toggleButton.value = false;
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateValue(property, container, ((AssetReference)newValue).editorAsset);
        }

        private static void UpdateValue(SerializedProperty property, VisualElement container, Object newTarget)
        {
            // Debug.Log(newTarget);
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            ObjectField objField = container.Q<ObjectField>(ResourceObjectName(property));
            UIToolkitUtils.DropdownButtonField groupDown = container.Q<UIToolkitUtils.DropdownButtonField>(GroupDownName(property));
            UIToolkitUtils.DropdownButtonField labelDown = container.Q<UIToolkitUtils.DropdownButtonField>(LabelDownName(property));

            VisualElement nameInputContainer = container.Q<VisualElement>(NameInputContainerName(property));
            TextField nameInput = nameInputContainer.Q<TextField>(NameInputName(property));

            if (helpBox.style.display != DisplayStyle.None)
            {
                helpBox.style.display = DisplayStyle.None;
            }

            bool notInAddressable = newTarget == null;
            AddressableAssetEntry entry = null;
            if (!notInAddressable)
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
                // AssetReference assetRef = (AssetReference)newValue;
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newTarget));
                entry = settings.FindAssetEntry(guid);
                // Debug.Log($"{entry}/{guid}/{newTarget}");
                notInAddressable = entry == null;
            }

            if (notInAddressable)
            {
                objField.value = null;
                groupDown.ButtonLabelElement.text = "";
                labelDown.userData = Array.Empty<string>();
                labelDown.ButtonLabelElement.text = "";
                nameInput.value = "";
                return;
            }

            objField.value = newTarget;
            groupDown.ButtonLabelElement.text = entry.parentGroup.Name;
            labelDown.userData = entry.labels.ToArray();
            labelDown.ButtonLabelElement.text = string.Join(",", entry.labels);
            nameInput.value = entry.address;
        }



    }


}

#endif
