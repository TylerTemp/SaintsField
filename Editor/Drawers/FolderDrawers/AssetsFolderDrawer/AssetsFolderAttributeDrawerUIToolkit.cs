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


namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
    public partial class AssetsFolderAttributeDrawer
    {
        private static string ButtonName(SerializedProperty property) =>
            $"{property.propertyPath}__AssetsFolder_Button";
        private static string HelpBoxName(SerializedProperty property) =>
            $"{property.propertyPath}__AssetsFolder_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return new AssetsFolderButtonsElement
            {
                name = ButtonName(property),
            };
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
            AssetsFolderButtonsElement element = container.Q<AssetsFolderButtonsElement>(name: ButtonName(property));

            if (property.propertyType != SerializedPropertyType.String)
            {
                helpBox.text = $"Target is not a string: {property.propertyType}";
                helpBox.style.display = DisplayStyle.Flex;
                element.SetEnabled(false);
                return;
            }

            FolderAttribute folderAttribute = (FolderAttribute)saintsAttribute;

            element.PickButton.clicked += () =>
            {
                (string error, string actualFolder) = OnClick(property, folderAttribute);
                if(error == "")
                {
                    if(actualFolder != "")
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

            element.LinkButton.clicked += () =>
            {
                if (!SerializedUtils.IsOk(property))
                {
                    return;
                }

                Object folderObj = GetFolderObject(property.stringValue);
                if (folderObj != null)
                {
                    EditorGUIUtility.PingObject(folderObj);
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

            CheckHelpBox(property.stringValue, helpBox, element);
            helpBox.TrackPropertyValue(property, p =>
                CheckHelpBox(p.stringValue, helpBox, element));
        }

        private static void CheckHelpBox(string value, HelpBox helpBox, AssetsFolderButtonsElement folderButtons)
        {
            string error = "";
            Object folderObj = GetFolderObject(value);
            bool folderIsNull = folderObj == null;
            if (!string.IsNullOrEmpty(value) && folderIsNull)
            {
                error = $"Folder \"{value}\" does not exists";
            }

            UIToolkitUtils.SetHelpBox(helpBox, error);

            folderButtons.LinkButton.SetEnabled(!folderIsNull);
        }

        private static Object GetFolderObject(string value)
        {
            if (!Directory.Exists(value))
            {
                return null;
            }
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(value);
            return folder;
        }
    }
}

#endif
