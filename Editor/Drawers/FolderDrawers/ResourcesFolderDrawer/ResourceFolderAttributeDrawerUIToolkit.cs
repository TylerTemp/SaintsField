#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.FolderDrawers.ResourcesFolderDrawer
{
    public partial class ResourceFolderAttributeDrawer
    {
        private static string ButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__ResourcesFolder_Button";
        private static string HelpBoxName(SerializedProperty property) =>
            $"{property.propertyPath}__ResourcesFolder_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            Button button = new Button
            {
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("resources-folder.png"),
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
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = HelpBoxName(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: HelpBoxName(property));
            Button button = container.Q<Button>(name: ButtonName(property));

            if (property.propertyType != SerializedPropertyType.String)
            {
                helpBox.text = $"Target is not a string: {property.propertyType}";
                helpBox.style.display = DisplayStyle.Flex;
                button.SetEnabled(false);
                return;
            }

            FolderAttribute folderAttribute = (FolderAttribute)saintsAttribute;

            button.clickable.clicked += () =>
            {
                (string error, string actualFolder) = OnClick(property, folderAttribute);
                if(error == "")
                {
                    if (actualFolder != "")
                    {
                        property.stringValue = actualFolder;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(actualFolder);
                    }
                }
                else
                {
                    helpBox.text = error;
                    helpBox.style.display = DisplayStyle.Flex;
                }
            };
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: HelpBoxName(property));

            if (helpBox.style.display != DisplayStyle.None)
            {
                helpBox.style.display = DisplayStyle.None;
            }
        }
    }
}
#endif
