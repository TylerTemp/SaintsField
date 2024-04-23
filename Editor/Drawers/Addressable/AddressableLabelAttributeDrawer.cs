#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable
{
    [CustomPropertyDrawer(typeof(AddressableLabelAttribute))]
    public class AddressableLabelAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // ReSharper disable once Unity.NoNullPropagation
            List<string> labels = AddressableAssetSettingsDefaultObject.Settings?.GetLabels() ?? new List<string>();

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int index = labels.IndexOf(property.stringValue);
                int newIndex = EditorGUI.Popup(position, label, index, labels.Select(each => new GUIContent(each)).ToArray());
                if (changed.changed)
                {
                    property.stringValue = labels[newIndex];
                }
            }
        }
        #endregion

        #region UIToolkit

        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container1, FieldInfo info, object parent)
        {
            DropdownField dropdownField = new DropdownField(property.displayName)
            {
                userData = Array.Empty<string>(),
                name = NameDropdownField(property),
            };
            dropdownField.AddToClassList(ClassAllowDisable);
            return dropdownField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
            helpBoxElement.AddToClassList(ClassAllowDisable);
            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
            dropdownField.RegisterValueChangedCallback(v =>
            {
                // IReadOnlyList<string> curMetaInfo = (IReadOnlyList<string>) ((DropdownField) v.target).userData;
                // string selectedKey = curMetaInfo[dropdownField.index];
                property.stringValue = v.newValue;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(v.newValue);
            });
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            List<string> keys = new List<string>();
            string error = "";
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                error = "Addressable not set up";
            }
            else
            {
                keys = AddressableAssetSettingsDefaultObject.Settings.GetLabels();
            }

            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));

            IReadOnlyList<string> curKeys = (IReadOnlyList<string>) dropdownField.userData;

            if(!curKeys.SequenceEqual(keys))
            {
                dropdownField.userData = keys;
                dropdownField.choices = keys;
                dropdownField.SetValueWithoutNotify(property.stringValue);
            }

            // Debug.Log($"AnimatorStateAttributeDrawer: {newAnimatorStates}");
            HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
            // ReSharper disable once InvertIf
            if (error != helpBoxElement.text)
            {
                helpBoxElement.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBoxElement.text = error;
            }
        }

        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
        //     IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        // {
        //     DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
        //     dropdownField.label = labelOrNull;
        // }
        #endregion
    }
}
#endif
