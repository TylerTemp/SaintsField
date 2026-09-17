#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneReferenceTypeDrawer
{
    public partial class SceneReferenceDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SceneReferenceField field = new SceneReferenceField(GetPreferredLabel(property))
            {
                bindingPath = property.FindPropertyRelative(nameof(SceneReference.guid)).propertyPath,
            };
            field.AddToClassList(ClassAllowDisable);
            field.AddToClassList(SceneReferenceField.alignedFieldUssClassName);
            if (!string.IsNullOrEmpty(property.tooltip) && field.labelElement != null)
            {
                field.labelElement.tooltip = property.tooltip;
            }
            return field;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            return new SceneHelpBox();
        }

        private bool _listeningToSceneList;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SceneReferenceField field = container.Q<SceneReferenceField>();
            SceneHelpBox helpBox = container.Q<SceneHelpBox>();
            field.SceneReferenceElement.BindStringHelpBox(helpBox);

            (string error, SceneReferenceContext context) = GetSceneReferenceContext(property);
            if (error != "")
            {
                helpBox.text = error;
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.EnableButton.style.display = DisplayStyle.None;
                helpBox.AddButton.style.display = DisplayStyle.None;
                return;
            }

            SerializedProperty sceneGuidProp = context.GuidProp;
            SerializedProperty scenePathProp = context.PathProp;
            SerializedProperty sceneIndexProp = context.IndexProp;

            field.TrackPropertyValue(sceneGuidProp, _ => RefreshField());

            if (!_listeningToSceneList)
            {
                EditorBuildSettings.sceneListChanged += RefreshField;
                _listeningToSceneList = true;
            }
            field.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (evt.target != field)
                {
                    return;
                }

                EditorBuildSettings.sceneListChanged -= RefreshField;
                _listeningToSceneList = false;
                UIToolkitUtils.Unbind(field);
            });

            RefreshField();

            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => onValueChangedCallback(new SceneReference
            {
                guid = sceneGuidProp.stringValue,
                path = scenePathProp.stringValue,
                index = sceneIndexProp.intValue,
            }));
            return;

            void RefreshField()
            {
                if (!SerializedUtils.IsOk(property))
                {
                    return;
                }

                RefreshGuid(context);
                field.SetValueWithoutNotify(sceneGuidProp.stringValue);
            }
        }
    }
}
#endif
