#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ReferencePicker
{
    public partial class ReferencePickerAttributeDrawer
    {
        #region UI Toolkit
        // private static string NamePropertyContainer(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField_Container";
        // private static string NamePropertyField(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__Reference_Button";
        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Reference_Label";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            Button button = new Button
            {
                name = NameButton(property),
                // text = "▼",
                style =
                {
                    height = SingleLineHeight - 2,
                    // maxHeight = SingleLineHeight,
                    width = SingleLineHeight - 2,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    // marginTop = 5,
                    // borderTopLeftRadius = 0,
                    // borderTopRightRadius = 0,
                    // borderBottomLeftRadius = 0,
                    // borderBottomRightRadius = 0,
                    // backgroundColor = Color.clear,

                    flexDirection = FlexDirection.Row,
                    // justifyContent = Justify.FlexEnd,
                    overflow = Overflow.Visible,
                },
            };
            button.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    maxWidth = 12,
                    maxHeight = EditorGUIUtility.singleLineHeight,
                    position = Position.Absolute,
                    right = 4,
                },
            });

            object curValue = property.managedReferenceValue;
            string labelName = curValue == null
                ? ""
                : curValue.GetType().Name;

            Label label = new Label(labelName)
            {
                name = NameLabel(property),
                // style =
                // {
                //     minWidth = 0,
                // },
                style =
                {
                    position = Position.Absolute,
                    right = SingleLineHeight,
                    display = ((ReferencePickerAttribute) saintsAttribute).HideLabel? DisplayStyle.None: DisplayStyle.Flex,
                    // translate = new Translate(Length.Percent(-100), Length.Auto()),
                    // paddingRight = 1,
                },
                pickingMode = PickingMode.Ignore,
            };
            button.Add(label);

            // button.Add(new Label("▼")
            // {
            //     // style =
            //     // {
            //     //     flexShrink = 0,
            //     // },
            // });

            // root.Add(button);

            button.AddToClassList(ClassAllowDisable);

            return button;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));
            VisualElement root = container.Q<VisualElement>(name: NameLabelFieldUIToolkit(property));
            button.clicked += () =>
            {
                // object managedReferenceValue = property.managedReferenceValue;
                // GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                // genericDropdownMenu.AddItem("[Null]", managedReferenceValue == null, () =>
                // {
                //     PropSetValue(container, property, null);
                //     onValueChangedCallback(null);
                // });
                // genericDropdownMenu.AddSeparator("");
                //
                // foreach (Type type in GetTypes(property))
                // {
                //     string displayName = $"{type.Name}: {type.Namespace}";
                //
                //     genericDropdownMenu.AddItem(displayName, managedReferenceValue != null && managedReferenceValue.GetType() == type, () =>
                //     {
                //         object instance = CopyObj(managedReferenceValue, Activator.CreateInstance(type));
                //         PropSetValue(container, property, instance);
                //
                //         onValueChangedCallback(instance);
                //     });
                // }
                //
                // Rect fakePos = container.worldBound;
                // fakePos.height = SingleLineHeight;
                //
                // genericDropdownMenu.DropDown(fakePos, container, true);

                Rect worldBound = root.worldBound;
                float maxHeight = Screen.height - root.worldBound.y - root.worldBound.height - 100;
                if (maxHeight < 100)
                {
                    // Debug.LogError($"near out of screen: {maxHeight}");
                    worldBound.y -= 300 + worldBound.height;
                    maxHeight = 300;
                }
                worldBound.height = SingleLineHeight;

                object managedReferenceValue = property.managedReferenceValue;
                AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>
                {
                    {"[Null]", null},
                };
                foreach (Type type in GetTypes(property))
                {
                    string displayName = $"{type.Name}: <color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>{type.Namespace}</color>";
                    dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
                }

                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurDisplay = managedReferenceValue == null
                        ? "-"
                        : managedReferenceValue.GetType().Name,
                    CurValues = managedReferenceValue == null? Array.Empty<object>(): new []{managedReferenceValue},
                    DropdownListValue = dropdownList,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };


                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    root.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        object instance = curItem == null
                            ? null
                            : CopyObj(managedReferenceValue, Activator.CreateInstance((Type)curItem));

                        PropSetValue(container, property, instance);
                        onValueChangedCallback(instance);
                    }
                ));
            };
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateLabel(property, container, newValue);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            // Debug.Log(property.propertyPath);
            // var s = property.propertyPath;

            object managedReference;
            try
            {
                managedReference = property.managedReferenceValue;
            }
            // ReSharper disable once MergeIntoLogicalPattern
            catch (Exception e) when (e is ObjectDisposedException || e is NullReferenceException || e is InvalidOperationException)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
                return;
            }

            UpdateLabel(property, container, managedReference);
        }

        private static void PropSetValue(VisualElement container, SerializedProperty property, object newValue)
        {
            property.serializedObject.Update();
            // property.managedReferenceValue = null;
            property.managedReferenceValue = newValue;
            // property.managedReferenceId = -2L;
            property.serializedObject.ApplyModifiedProperties();
            // property.serializedObject.SetIsDifferentCacheDirty();

            container.Query<PropertyField>(name: UIToolkitFallbackName(property)).ForEach(each => each.BindProperty(property));
        }

        private static void UpdateLabel(SerializedProperty property, VisualElement container, object newValue)
        {
            Label label = container.Q<Label>(NameLabel(property));
            string newLabel = newValue == null
                ? ""
                : newValue.GetType().Name;

            if(label.text != newLabel)
            {
                label.text = newLabel;
            }
        }
        #endregion
    }
}

#endif
