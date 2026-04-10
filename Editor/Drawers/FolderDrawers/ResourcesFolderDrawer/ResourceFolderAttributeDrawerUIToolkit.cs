#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


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
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                },
                name = ButtonName(property),
            };
            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
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

            ResourceFolderAttribute folderAttribute = (ResourceFolderAttribute)saintsAttribute;

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

            #region Drag
            VisualElement fieldContainer = container.Q<VisualElement>(name: NameLabelFieldUIToolkit(property));
            fieldContainer.RegisterCallback<DragEnterEvent>(_ =>
            {
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            fieldContainer.RegisterCallback<DragLeaveEvent>(_ =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.None;
            });
            fieldContainer.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                // Debug.Log($"Drag Update {string.Join<Object>(",", DragAndDrop.objectReferences)}");
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            fieldContainer.RegisterCallback<DragPerformEvent>(_ =>
            {
                string fineFolder = CanDrop(DragAndDrop.objectReferences).FirstOrDefault();
                if (string.IsNullOrEmpty(fineFolder))
                {
                    return;
                }

                property.stringValue = fineFolder;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(fineFolder);
            });
            #endregion

            CheckHelpBox(property.stringValue, helpBox);
            helpBox.TrackPropertyValue(property, p =>
            {
                CheckHelpBox(p.stringValue, helpBox);
            });
        }

        private static IEnumerable<string> CanDrop(IEnumerable<Object> objRefs)
        {
            foreach (Object objRef in objRefs)
            {
                string path = AssetDatabase.GetAssetPath(objRef);
                if (!Directory.Exists(path))
                {
                    continue;
                }
                (string error, string actualFolder) = ValidateAssetFolder(path);
                if (error == "")
                {
                    yield return actualFolder;
                }
            }
        }

        private static void CheckHelpBox(string value, HelpBox helpBox)
        {
            UIToolkitUtils.SetHelpBox(helpBox, CheckFolder(value));
        }
    }
}
#endif
