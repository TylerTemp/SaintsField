using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneReferenceTypeDrawer
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SceneReferenceElement sceneReferenceElement = new SceneReferenceElement
            {
                bindingPath = property.FindPropertyRelative(nameof(SceneReference.guid)).propertyPath,
            };
            SceneReferenceField field = new SceneReferenceField(GetPreferredLabel(property), sceneReferenceElement);
            field.AddToClassList(ClassAllowDisable);
            field.AddToClassList(SceneReferenceField.alignedFieldUssClassName);
            return field;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            return new SceneHelpBox();
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SceneReferenceField field = container.Q<SceneReferenceField>();
            SceneHelpBox helpBox = container.Q<SceneHelpBox>();
            field.SceneReferenceElement.BindStringHelpBox(helpBox);

            SerializedProperty sceneGuidProp = property.FindPropertyRelative(nameof(SceneReference.guid));
            SerializedProperty scenePathProp = property.FindPropertyRelative(nameof(SceneReference.path));
            SerializedProperty sceneIndexProp = property.FindPropertyRelative(nameof(SceneReference.index));
            field.TrackPropertyValue(sceneGuidProp, _ =>
            {
                if (!SerializedUtils.IsOk(property))
                {
                    return;
                }
                RefreshGuid(sceneGuidProp, scenePathProp, sceneIndexProp);
            });
            RefreshGuid(sceneGuidProp, scenePathProp, sceneIndexProp);

            field.RegisterCallback<DetachFromPanelEvent>(_ => UIToolkitUtils.Unbind(field));

            field.SceneReferenceElement.SetValueWithoutNotify(sceneGuidProp.stringValue);

            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => onValueChangedCallback(new SceneReference
            {
                guid = sceneGuidProp.stringValue,
                path = scenePathProp.stringValue,
                index = sceneIndexProp.intValue
            }));
        }

        private static void RefreshGuid(SerializedProperty sceneGuidProp, SerializedProperty scenePathProp, SerializedProperty sceneIndexProp)
        {
            string guid = sceneGuidProp.stringValue;
            if (!GUID.TryParse(guid, out GUID resultGuid))
            {
                return;
            }

#if UNITY_6000_2_OR_NEWER
            SceneAsset sceneAsset = AssetDatabase.LoadAssetByGUID<SceneAsset>(resultGuid);
#else
            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(resultGuid));
#endif

            if (sceneAsset == null)
            {
                return;
            }
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            foreach ((EditorBuildSettingsScene inBuild, int sceneIndex) in EditorBuildSettings.scenes.Where(each => each.enabled).WithIndex())
            {
                // ReSharper disable once InvertIf
                if (inBuild.path == scenePath)
                {
                    string sceneNamePath = RuntimeUtil.TrimScenePath(scenePath, true);
                    bool changed = false;
                    if (scenePathProp.stringValue != sceneNamePath)
                    {
                        scenePathProp.stringValue = sceneNamePath;
                        changed = true;
                    }

                    if (sceneIndexProp.intValue != sceneIndex)
                    {
                        sceneIndexProp.intValue = sceneIndex;
                        changed = true;
                    }

                    if (changed)
                    {
                        sceneGuidProp.serializedObject.ApplyModifiedProperties();
                    }

                    return;
                }
            }
        }
    }
}
