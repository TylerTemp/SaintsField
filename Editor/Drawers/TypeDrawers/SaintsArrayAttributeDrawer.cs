using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TypeDrawers
{
    [CustomPropertyDrawer(typeof(SaintsArrayAttribute))]
    public class SaintsArrayAttributeDrawer: SaintsPropertyDrawer
    {
        public static (string propName, int index) GetSerName(SerializedProperty property, SaintsArrayAttribute saintsArrayAttribute, FieldInfo fieldInfo, object parent)
        {
            if(saintsArrayAttribute.PropertyName != null)
            {
                return (saintsArrayAttribute.PropertyName, SerializedUtils.PropertyPathIndex(property.propertyPath));
            }

            object rawValue = fieldInfo.GetValue(parent);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            ISaintsArray curValue = (ISaintsArray)(arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex));

            return (curValue.EditorArrayPropertyName, arrayIndex);
        }

        #region IMGUI

        private string _imGuiPropRawName = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, (SaintsArrayAttribute) saintsAttribute, info, parent).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            return EditorGUI.GetPropertyHeight(arrProperty, label, true);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, (SaintsArrayAttribute) saintsAttribute, info, parent).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            EditorGUI.PropertyField(position, arrProperty, label, true);
        }

        #endregion

#if UNITY_2021_3_OR_NEWER
        #region UI Toolkit

        private static string NamePropertyField(SerializedProperty property) => $"{property.propertyPath}_SaintsArray";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            (string propName, int index) = GetSerName(property, (SaintsArrayAttribute) saintsAttribute, info, parent);
            SerializedProperty arrProperty = property.FindPropertyRelative(propName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propName);
            PropertyField propertyField = new PropertyField(arrProperty, index == -1? null: $"Element {index}")
            {
                name = NamePropertyField(property),
            };
            propertyField.AddToClassList(ClassAllowDisable);
            return propertyField;
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            string targetName = NamePropertyField(property);
            PropertyField propertyField = container.Q<PropertyField>();

            UIToolkitUtils.WaitUntilThenDo(container, () =>
            {
                Label target = propertyField.Q<Label>(className: "unity-label");
                // Debug.Log($"checking targetName {targetName} {target}");
                return (target != null, target);
            }, target =>
            {
                UIToolkitUtils.SetLabel(target, richTextChunks, richTextDrawer);
            });

            // Label target = propertyField.Q<Label>(className: "unity-label");
            // if(target != null)
            // {
            //     UIToolkitUtils.SetLabel(target, richTextChunks, richTextDrawer);
            // }
            // else
            // {
            //     Debug.Log($"label not found in {NamePropertyField(property)}");
            //     foreach (VisualElement child in propertyField.Children())
            //     {
            //         Debug.Log(child.name);
            //     }
            //     Debug.Log("=====================");
            // }
            //
            // Foldout foldout = container.Q<Foldout>(NameFoldout(property));
            // // foldout.text = labelOrNull ?? "";
            // Label foldoutLabel = foldout.Q<Label>();
            // if (foldoutLabel != null)
            // {
            //     UIToolkitUtils.SetLabel(foldoutLabel, richTextChunks, richTextDrawer);
            // }
        }

        #endregion
#endif
    }
}
