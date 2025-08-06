#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public partial class AdvancedDropdownAttributeDrawer
    {
        // private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            AdvancedDropdownMetaInfo initMetaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButton(property);
            dropdownButton.userData = initMetaInfo.CurValues;
            dropdownButton.ButtonLabelElement.text = GetMetaStackDisplay(initMetaInfo);

            dropdownButton.AddToClassList(ClassAllowDisable);

            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(dropdownButton);

            return emptyPrefabOverrideElement;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
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
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));

            // try
            // {
            //     dropdownButton.BindProperty(property);
            // }
            // catch(IndexOutOfRangeException)
            // {
            //     // wtf Unity...
            // }

            UIToolkitUtils.AddContextualMenuManipulator(dropdownButton.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            VisualElement root = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            dropdownButton.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

                SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    root.worldBound.width,
                    maxHeight,
                    false,
                    (newDisplay, curItem) =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();

                        dropdownButton.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property)).ButtonLabelElement
                            .text = newDisplay;
                        dropdownButton.userData = curItem;
                        onValueChangedCallback(curItem);
                        // dropdownButton.buttonLabelElement.text = newDisplay;
                    }
                );

                // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
                // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
                // editorWindow.Show();

                UnityEditor.PopupWindow.Show(worldBound, sa);

                string curError = metaInfo.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                // ReSharper disable once InvertIf
                if (helpBox.text != curError)
                {
                    helpBox.text = curError;
                    helpBox.style.display = curError == ""? DisplayStyle.None : DisplayStyle.Flex;
                }
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                return;
            }

            string display =
                GetMetaStackDisplay(GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false));
            if(dropdownButton.ButtonLabelElement.text != display)
            {
                dropdownButton.ButtonLabelElement.text = display;
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            string display =
                GetMetaStackDisplay(GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false));
            if(dropdownButton.ButtonLabelElement.text != display)
            {
                dropdownButton.ButtonLabelElement.text = display;
            }
        }
    }
}
#endif
