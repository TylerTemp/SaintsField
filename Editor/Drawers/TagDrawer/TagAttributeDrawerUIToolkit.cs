#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditorInternal;

namespace SaintsField.Editor.Drawers.TagDrawer
{
    public partial class TagAttributeDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            TagElement tagElement = new TagElement();
            tagElement.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), tagElement);
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return null;
            }

            return new HelpBox($"Type {property.propertyType} is not string.", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            StringDropdownField layerStringField = container.Q<StringDropdownField>();
            AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);

            layerStringField.Button.clicked += () =>
                MakeDropdown(property, layerStringField, onValueChangedCallback, info, parent);
        }

        private static void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                foreach (string tag in InternalEditorUtility.tags)
                {
                    // ReSharper disable once InvertIf
                    if (tag == clipboardText)
                    {
                        evt.menu.AppendAction($"Paste \"{tag}\"", _ =>
                        {
                            property.stringValue = tag;
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, tag);
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(tag);
                        });
                        return;
                    }
                }
            }));
        }

        private static void MakeDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();


            string selectedName = null;
            foreach (string tag in InternalEditorUtility.tags)
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(tag, tag);
                // ReSharper disable once InvertIf
                if (tag == property.stringValue)
                {
                    selectedName = tag;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", null, false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedName is null ? Array.Empty<object>(): new object[] { selectedName },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    string curValue = (string)curItem;
                    if (curValue == null)
                    {
                        Selection.activeObject =
                            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
                    }
                    else
                    {
                        property.stringValue = curValue;
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parent, curValue);
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(curValue);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

    }
}
#endif
