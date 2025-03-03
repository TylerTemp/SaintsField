#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.I2Loc.LocalizedStringPickerDrawer
{
    public partial class LocalizedStringPickerAttributeDrawer
    {
        private static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__LocalizedString_SelectorButton";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__LocalizedString_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            Button selectorButton = new Button
            {
                // text = "●",
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                    width = SingleLineHeight,
                    marginLeft = 0,
                    marginRight = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(14, 14),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
                name = NameSelectorButton(property),
            };
            selectorButton.AddToClassList(ClassAllowDisable);
            return selectorButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameHelpBox(property),
            };
            helpBoxElement.AddToClassList(ClassAllowDisable);
            return helpBoxElement;
        }

        // private bool _mismatch;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button selectorButton = container.Q<Button>(NameSelectorButton(property));
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            string mismatchError = MismatchError(property);
            if (mismatchError != "")
            {
                // _mismatch = true;
                UpdateHelpBox(helpBox, mismatchError);
                return;
            }

            VisualElement objectField = container.Q<VisualElement>(classes: ClassLabelFieldUIToolkit);

            selectorButton.clickable.clicked += () =>
            {
                string curValue = property.propertyType == SerializedPropertyType.String
                    ? property.stringValue
                    : property.FindPropertyRelative("mTerm").stringValue;
                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(curValue, false);
                Rect worldBound = new Rect(objectField.worldBound)
                {
                    width = objectField.worldBound.width + selectorButton.worldBound.width,
                };
                float maxHeight = Screen.currentResolution.height - worldBound.y - worldBound.height - 100;
                // Debug.Log(worldBound);
                if (maxHeight < 100)
                {
                    // worldBound.x -= 400;
                    worldBound.y -= 100 + worldBound.height;
                    // Debug.Log(worldBound);
                    maxHeight = 100;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        string newValue = (string)curItem;
                        SetValue(property, newValue);
                        property.serializedObject.ApplyModifiedProperties();
                        if(property.propertyType == SerializedPropertyType.String)
                        {
                            onValueChangedCallback.Invoke(newValue);
                            return;
                        }

                        object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        if (noCacheParent == null)
                        {
                            Debug.LogWarning("Property disposed unexpectedly, skip onChange callback.");
                            return;
                        }

                        (string error, int _, object reflectedValue) = Util.GetValue(property, info, noCacheParent);
                        if (error != "")
                        {
                            Debug.LogError(error);
                            return;
                        }

                        onValueChangedCallback.Invoke(reflectedValue);
                    }
                ));
            };
        }

        private static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }
    }
}
#endif
