#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TagDrawer
{
    public partial class TagAttributeDrawer
    {
        private static string NameTag(SerializedProperty property) => $"{property.propertyPath}__Tag";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            TagField tagField = new TagField(GetPreferredLabel(property))
            {
                value = property.stringValue,
                name = NameTag(property),
            };

            tagField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
            tagField.AddToClassList(ClassAllowDisable);

            return tagField;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            TagField tagField = container.Q<TagField>(NameTag(property));
            UIToolkitUtils.AddContextualMenuManipulator(tagField.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            tagField.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();

                onValueChangedCallback.Invoke(evt.newValue);
            });
        }
    }
}
#endif
