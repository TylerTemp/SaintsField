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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    public partial class DropdownAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_Button";
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
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

            (string error, int _, object value) = Util.GetValue(property, info, parent);
            if (error == "")
            {
                dropdownButton.FancyButton.MainLabel.text = $"{value}";
            }

            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(dropdownButton);

            return emptyPrefabOverrideElement;
        }

        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.FancyButtonField dropdownButtonField = container.Q<UIToolkitUtils.FancyButtonField>(NameButton(property));

            // VisualElement root = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            // var serObj = property.serializedObject;
            Object[] targetObjects = property.serializedObject.targetObjects;
            string propPath = property.propertyPath;

            UIToolkitUtils.AddContextualMenuManipulator(dropdownButtonField, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            // HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            dropdownButtonField.FancyButton.MainButton.clicked += () =>
            {
                AdvancedDropdownAttributeDrawer.GetMetaInfoAsync(
                    dropdownButtonField,
                    metaInfo =>
                    {
                        if (!SerializedUtils.IsOk(property))
                        {
                            return;
                        }

                        if (metaInfo.Error != "")
                        {
                            VisualElement result = dropdownButtonField.FancyButton.ShowResult(true);
                            result.Clear();
                            result.Add(new HelpBox(metaInfo.Error, HelpBoxMessageType.Error));
                            return;
                        }

                        dropdownButtonField.FancyButton.ShowResult(false);

                        (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(dropdownButtonField.worldBound);

                        SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                            metaInfo,
                            dropdownButtonField.worldBound.width,
                            maxHeight,
                            false,
                            (curItem, _) =>
                            {
                                SerializedObject serObj = null;
                                if (!SerializedUtils.IsOk(property))
                                {
                                    // https://github.com/TylerTemp/SaintsField/issues/367
                                    // Only happens in prefab
                                    Object[] inspecting = targetObjects
                                        .Where(each => each != null).ToArray();
                                    if (inspecting.Length == 0)
                                    {
#if SAINTSFIELD_DEBUG
                                        Debug.Log("No inspecting");
#endif
                                        return null;
                                    }

                                    serObj = new SerializedObject(inspecting);
                                    property = serObj.FindProperty(propPath);
                                    // Debug.Log(property.propertyPath);
                                    // return null;
                                }
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                                    parent, curItem);
                                Util.SignPropertyValue(property, info, parent, curItem);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback(curItem);
                                // UpdateButtonLabel(property);
                                serObj?.Dispose();
                                return null;
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

            dropdownButtonField.TrackPropertyValue(property, UpdateButtonLabel);
            UpdateButtonLabel(property);
            return;

            void UpdateButtonLabel(SerializedProperty p)
            {
                // Issue 379
                // see Samples/Scripts/SaintsEditor/Issues/Issue379/CustomEditorWindow.cs
                object useParent = parent;
                (SerializedUtils.FieldOrProp _, object refreshedParent) =
                    SerializedUtils.GetFieldInfoAndDirectParent(property);
                if (refreshedParent != null)
                {
                    // Debug.Log($"rewrite parent {refreshedParent}");
                    useParent = refreshedParent;
                }

                AdvancedDropdownAttributeDrawer.GetMetaInfoAsync(dropdownButtonField,
                    metaInfo =>
                    {
                        string display = AdvancedDropdownAttributeDrawer.GetMetaStackDisplay(metaInfo);
                        // Debug.Log(metaInfo.CurValues[0]);
                        // Debug.Log(display);
                        if((string)dropdownButtonField.FancyButton.MainLabel.userData != display)
                        {
                            dropdownButtonField.FancyButton.MainLabel.userData = display;
                            UIToolkitUtils.SetLabel(dropdownButtonField.FancyButton.MainLabel, RichTextDrawer.ParseRichXmlWithProvider(display, new RichTextDrawer.EmptyRichTextTagProvider()), _richTextDrawer);
                        }
                    }, p, (PathedDropdownAttribute)saintsAttribute,
                    info, useParent, false);

            }
        }

        private readonly struct DrawInfo
        {
            public readonly struct EnumValueInfo
            {
                public readonly bool HasValue;
                public readonly object Value;
                public readonly string Label;

                public EnumValueInfo(object value, string label)
                {
                    HasValue = true;
                    Value = value;
                    Label = label;
                }
            }

            public readonly IReadOnlyList<EnumValueInfo> EnumValues;
            public readonly EnumValueInfo NothingValue;
            public readonly EnumValueInfo EverythingValue;
            public readonly object EverythingBit;
            public readonly bool IsFlags;
            public readonly bool IsULong;

            public DrawInfo(IReadOnlyList<EnumValueInfo> enumValues, EnumValueInfo everythingValue, EnumValueInfo nothingValue, object everythingBit, bool isFlags, bool isULong)
            {
                EnumValues = enumValues;
                EverythingValue = everythingValue;
                NothingValue = nothingValue;
                EverythingBit = everythingBit;
                IsFlags = isFlags;
                IsULong = isULong;
            }
        }


    }
}
#endif
