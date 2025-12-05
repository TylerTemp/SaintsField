using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineIkConstraintPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineIkConstraintPickerAttribute), true)]
    public partial class SpineIkConstraintPickerAttributeDrawer: SaintsPropertyDrawer
    {
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__SpineEvent_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
            }

            SpineIkConstraintPickerElement element = new SpineIkConstraintPickerElement
            {
                bindingPath = property.propertyPath,
            };
            SpineIkConstraintPickerField field = new SpineIkConstraintPickerField(GetPreferredLabel(property), element);
            field.AddToClassList(ClassAllowDisable);
            field.AddToClassList(SpineIkConstraintPickerField.alignedFieldUssClassName);
            return field;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new HelpBox($"Type {property.propertyType} is not a string type.", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }

            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            SpineIkConstraintPickerAttribute spineIkConstraintPickerAttribute = (SpineIkConstraintPickerAttribute)saintsAttribute;
            SpineIkConstraintPickerField field = container.Q<SpineIkConstraintPickerField>();
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));
            field.SpineIkConstraintPickerElement.BindHelpBox(helpBox);

            CheckSkeletonData();

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(CheckSkeletonData);
            container.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(CheckSkeletonData));
            field.TrackSerializedObjectValue(property.serializedObject, _ => CheckSkeletonData());
            field.TrackPropertyValue(property, _ => onValueChangedCallback.Invoke(property.stringValue));
            return;

            void CheckSkeletonData()
            {
                (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineIkConstraintPickerAttribute.SkeletonTarget, property, info,
                    parent);
                UIToolkitUtils.SetHelpBox(helpBox, error);
                if (error != "")
                {
                    return;
                }

                field.SpineIkConstraintPickerElement.BindSkeletonData(skeletonDataAsset.GetSkeletonData(false));
            }
        }
    }
}
