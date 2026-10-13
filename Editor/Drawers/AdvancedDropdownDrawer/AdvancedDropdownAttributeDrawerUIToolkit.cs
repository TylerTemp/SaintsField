#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public partial class AdvancedDropdownAttributeDrawer
    {
        // private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_Button";
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_HelpBox";

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<Attribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            UIToolkitUtils.FancyButtonField dropdownButton = new UIToolkitUtils.FancyButtonField(GetPreferredLabel(property))
            {
                name = NameButton(property),
            };
            dropdownButton.FancyButton.DisplayDropdown();

            if (!string.IsNullOrEmpty(property.tooltip) && dropdownButton.labelElement != null)
            {
                dropdownButton.labelElement.tooltip = property.tooltip;
            }

            dropdownButton.AddToClassList(ClassAllowDisable);
            dropdownButton.AddToClassList(UIToolkitUtils.FancyButtonField.alignedFieldUssClassName);

            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(dropdownButton);

            return emptyPrefabOverrideElement;
        }

        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes,
        //     VisualElement container, FieldInfo info, object parent)
        // {
        //     HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
        //     {
        //         style =
        //         {
        //             display = DisplayStyle.None,
        //         },
        //         name = NameHelpBox(property),
        //     };
        //
        //     return helpBox;
        // }

        // public class DebugPopupExample : EditorWindow
        // {
        //     public static SaintsAdvancedDropdownUIToolkit SaintsAdvancedDropdownUIToolkit;
        //
        //     private void CreateGUI()
        //     {
        //         // Create an instance of your PopupWindowContent
        //         var popupContent = SaintsAdvancedDropdownUIToolkit;
        //
        //         // Manually call OnOpen to initialize the UI
        //         // popupContent.editorWindow = this;
        //         // popupContent.OnOpen();
        //
        //         var r = popupContent.DebugCloneTree();
        //         // Add the PopupWindowContent's root VisualElement to the EditorWindow
        //         rootVisualElement.Add(r);
        //     }
        // }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<Attribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.FancyButtonField dropdownButton = container.Q<UIToolkitUtils.FancyButtonField>(NameButton(property));

            UIToolkitUtils.AddContextualMenuManipulator(dropdownButton, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            dropdownButton.FancyButton.MainButton.clicked += () =>
            {
                GetMetaInfoAsync(
                    dropdownButton,
                    metaInfo =>
                    {
                        if (!SerializedUtils.IsOk(property))
                        {
                            return;
                        }

                        if (metaInfo.Error != "")
                        {
                            VisualElement result = dropdownButton.FancyButton.ShowResult(true);
                            result.Clear();
                            result.Add(new HelpBox(metaInfo.Error, HelpBoxMessageType.Error));
                            return;
                        }

                        dropdownButton.FancyButton.ShowResult(false);

                        (Rect worldBound, float maxHeight) =
                            SaintsAdvancedDropdownUIToolkit.GetProperPos(dropdownButton.worldBound);

                        // Debug.Log(metaInfo.DropdownListValue.Count);
                        // Debug.Log(metaInfo.DropdownListValue.displayName);
                        // Debug.Log(metaInfo.DropdownListValue.value);
                        // Debug.Log(metaInfo.DropdownListValue.ChildCount());

                        SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                            metaInfo,
                            dropdownButton.worldBound.width,
                            maxHeight,
                            false,
                            (newDisplay, curItem) =>
                            {
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                                    parent, curItem);
                                Util.SignPropertyValue(property, info, parent, curItem);
                                property.serializedObject.ApplyModifiedProperties();

                                dropdownButton.FancyButton.MainLabel.userData = newDisplay;
                                UIToolkitUtils.SetLabel(dropdownButton.FancyButton.MainLabel,
                                    RichTextDrawer.ParseRichXmlWithProvider(newDisplay, new RichTextDrawer.EmptyRichTextTagProvider()),
                                    _richTextDrawer);
                                dropdownButton.userData = curItem;
                                onValueChangedCallback(curItem);
                            }
                        );

                        // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
                        // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
                        // editorWindow.Show();

                        UnityEditor.PopupWindow.Show(worldBound, sa);
                    },
                    property,
                    (PathedDropdownAttribute)saintsAttribute,
                    info,
                    parent,
                    false
                );
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<Attribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            UIToolkitUtils.FancyButtonField dropdownButton = container.Q<UIToolkitUtils.FancyButtonField>(NameButton(property));
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                return;
            }

            GetMetaInfoAsync(dropdownButton,
                metaInfo =>
                {
                    string display = GetMetaStackDisplay(metaInfo);
                    // ReSharper disable once InvertIf
                    if((string)dropdownButton.FancyButton.MainLabel.userData != display)
                    {
                        dropdownButton.FancyButton.MainLabel.userData = display;
                        UIToolkitUtils.SetLabel(dropdownButton.FancyButton.MainLabel, RichTextDrawer.ParseRichXmlWithProvider(display, new RichTextDrawer.EmptyRichTextTagProvider()), _richTextDrawer);
                    }
                }, property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UIToolkitUtils.FancyButtonField dropdownButton = container.Q<UIToolkitUtils.FancyButtonField>(NameButton(property));
            GetMetaInfoAsync(dropdownButton,
                metaInfo =>
                {
                    string display = GetMetaStackDisplay(metaInfo);
                    // ReSharper disable once InvertIf
                    if((string)dropdownButton.FancyButton.MainLabel.userData != display)
                    {
                        dropdownButton.FancyButton.MainLabel.userData = display;
                        UIToolkitUtils.SetLabel(dropdownButton.FancyButton.MainLabel, RichTextDrawer.ParseRichXmlWithProvider(display, new RichTextDrawer.EmptyRichTextTagProvider()), _richTextDrawer);
                    }
                }, property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);
        }
    }
}
#endif
