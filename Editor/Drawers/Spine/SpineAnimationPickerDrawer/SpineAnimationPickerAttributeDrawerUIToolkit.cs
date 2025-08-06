#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
    public partial class SpineAnimationPickerAttributeDrawer
    {
        private static string DropdownButtonName(SerializedProperty property) => $"{property.propertyPath}__SpineAnimationPicker_DropdownButton";
        private static string HelpBoxName(SerializedProperty property) => $"{property.propertyPath}__SpineAnimationPicker_HelpBox";

        // protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     VisualElement container, FieldInfo info, object parent)
        // {
        //     UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
        //     dropdownButton.name = DropdownButtonName(property);
        //     dropdownButton.AddToClassList(ClassAllowDisable);
        //     return dropdownButton;
        // }

        private Texture2D _icon;

        private Texture2D Icon => _icon ??= Util.LoadResource<Texture2D>(IconDropdownPath);

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return new Button
            {
                style =
                {
                    backgroundImage = Icon,
                    width = EditorGUIUtility.singleLineHeight,
                    height = EditorGUIUtility.singleLineHeight,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 5, EditorGUIUtility.singleLineHeight - 5),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    // paddingTop = 2,
                    // paddingBottom = 2,
                    // height = EditorGUIUtility.singleLineHeight,
                },
                name = DropdownButtonName(property),
            };
            // UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            // dropdownButton.name = DropdownButtonName(property);
            // dropdownButton.AddToClassList(ClassAllowDisable);
            // return dropdownButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = HelpBoxName(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            string typeMismatchError = GetTypeMismatchError(property, info);
            if (typeMismatchError != "")
            {
                if(helpBox.text != typeMismatchError)
                {
                    helpBox.text = typeMismatchError;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                return;
            }

            SpineAnimationPickerAttribute spineAnimationPickerAttribute = (SpineAnimationPickerAttribute) saintsAttribute;

            UpdateDisplay(container, spineAnimationPickerAttribute, property, info, parent);

            Button dropdownButton = container.Q<Button>(DropdownButtonName(property));
            VisualElement fieldContainer = container.Q<VisualElement>(name: NameLabelFieldUIToolkit(property));

            dropdownButton.clicked += () =>
            {
                (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineAnimationPickerAttribute.SkeletonTarget, property, info, parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    UpdateDisplay(container, spineAnimationPickerAttribute, property, info, parent);
                    return;
                }

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(fieldContainer.worldBound);

                AdvancedDropdownMetaInfo dropdownMetaInfo = property.propertyType == SerializedPropertyType.String
                    ? GetMetaInfoString(property.stringValue, skeletonDataAsset)
                    : GetMetaInfoAsset(property.objectReferenceValue as AnimationReferenceAsset, skeletonDataAsset);

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    dropdownMetaInfo,
                    worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            string curValue = (string)curItem;
                            curValue ??= "";
                            property.stringValue = curValue;
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback(curValue);
                        }
                        else
                        {
                            AnimationReferenceAsset curValue = (AnimationReferenceAsset)curItem;
                            property.objectReferenceValue = curValue;
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback(curValue);
                        }
                    }
                ));
            };
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            // Debug.Log(newValue);
            UpdateDisplay(container, (SpineAnimationPickerAttribute) saintsAttribute, property, info, parent);
        }

        private static void UpdateDisplay(VisualElement container, SpineAnimationPickerAttribute spineAnimationPickerAttribute, SerializedProperty property, FieldInfo info, object parent)
        {
            // UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(DropdownButtonName(property));
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineAnimationPickerAttribute.SkeletonTarget, property, info, parent);
            if (error == "")
            {
                SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
                if (skeletonData == null)
                {
                    error = $"SkeletonData of {skeletonDataAsset} is null";
                }
            }
            if (error != "")
            {
                // dropdownButton.SetEnabled(false);

                // ReSharper disable once InvertIf
                if(helpBox.text != error)
                {
                    helpBox.text = error;
                    helpBox.style.display = DisplayStyle.Flex;
                }

                return;
            }

            // Debug.Log(skeletonDataAsset);

            bool found = GetSelectedAnimation(property, skeletonDataAsset);

            if(!found)
            {
                // dropdownButton.SetEnabled(true);
                string notFoundError;
                if (property.propertyType == SerializedPropertyType.String)
                {
                    string stringValue = property.stringValue;
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        // ReSharper disable once InvertIf
                        if(helpBox.style.display != DisplayStyle.None)
                        {
                            helpBox.text = "";
                            helpBox.style.display = DisplayStyle.None;
                        }
                        return;
                    }

                    notFoundError = $"{stringValue} not found in animations of {skeletonDataAsset}";
                }
                else
                {
                    if (property.objectReferenceValue == null)
                    {
                        // ReSharper disable once InvertIf
                        if(helpBox.style.display != DisplayStyle.None)
                        {
                            helpBox.text = "";
                            helpBox.style.display = DisplayStyle.None;
                        }
                        return;
                    }
                    notFoundError = $"{property.objectReferenceValue} not found in animations of {skeletonDataAsset}";
                }
                // ReSharper disable once InvertIf
                if(helpBox.text != notFoundError)
                {
                    helpBox.text = notFoundError;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                return;
            }

            if(helpBox.text != "")
            {
                helpBox.text = "";
                helpBox.style.display = DisplayStyle.None;
            }

            // dropdownButton.SetEnabled(true);
            // string label = selectedSpineAnimationInfo.ToString();
            // if (dropdownButton.ButtonLabelElement.text != label)
            // {
            //     dropdownButton.ButtonLabelElement.text = label;
            // }
        }
    }
}
#endif
