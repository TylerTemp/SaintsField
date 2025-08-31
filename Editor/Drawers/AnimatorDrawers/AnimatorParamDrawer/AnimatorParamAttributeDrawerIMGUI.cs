using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorParamDrawer
{
    public partial class AnimatorParamAttributeDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
                return;
            }

            _error = "";

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, metaInfo, onGUIPayload);
                    break;
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, metaInfo, onGUIPayload);
                    break;
                default:
                    _error = $"Invalid property type: expect integer or string, get {property.propertyType}";
                    DefaultDrawer(position, property, label, info);
                    break;
            }
        }

        private static void DrawPropertyForInt(Rect position, SerializedProperty property, GUIContent label,
            MetaInfo metaInfo, OnGUIPayload onGUIPayload)
        {
            int paramNameHash = property.intValue;
            int index = -1;

            foreach ((AnimatorControllerParameter value, int eachIndex) in metaInfo.AnimatorParameters.WithIndex())
            {
                if (value.nameHash == paramNameHash)
                {
                    index = eachIndex;
                    break;
                }
            }

            IEnumerable<string> displayOptions = GetDisplayOptions(metaInfo.AnimatorParameters);

            GUIContent[] contents = displayOptions
                .Select(each => new GUIContent(each))
                .Concat(new[]
                {
                    GUIContent.none,
                    new GUIContent($"Edit {metaInfo.Animator.runtimeAnimatorController.name}...")
                })
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newIndex < metaInfo.AnimatorParameters.Count)
                    {
                        int newValue = metaInfo.AnimatorParameters[newIndex].nameHash;
                        property.intValue = newValue;
                        onGUIPayload.SetValue(newValue);
                        if (ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        OpenAnimator(metaInfo.Animator.runtimeAnimatorController);
                    }
                }
            }
        }

        private static void DrawPropertyForString(Rect position, SerializedProperty property, GUIContent label,
            MetaInfo metaInfo, OnGUIPayload onGUIPayload)
        {
            string paramName = property.stringValue;
            int index = metaInfo.AnimatorParameters
                .Select((value, valueIndex) => new { value, index = valueIndex })
                .FirstOrDefault(each => each.value.name == paramName)?.index ?? -1;

            IEnumerable<string> displayOptions = GetDisplayOptions(metaInfo.AnimatorParameters);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                GUIContent[] contents = displayOptions
                    .Select(each => new GUIContent(each))
                    .Concat(new[]
                    {
                        GUIContent.none,
                        new GUIContent($"Edit {metaInfo.Animator.runtimeAnimatorController.name}..."),
                    })
                    .ToArray();

                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newIndex < metaInfo.AnimatorParameters.Count)
                    {
                        string newValue = metaInfo.AnimatorParameters[newIndex].name;
                        property.stringValue = newValue;
                        onGUIPayload.SetValue(newValue);
                        if (ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        OpenAnimator(metaInfo.Animator.runtimeAnimatorController);
                    }
                }
            }
        }

        private static IReadOnlyList<string>
            GetDisplayOptions(IEnumerable<AnimatorControllerParameter> animatorParams) =>
            animatorParams.Select(each => $"{each.name} [{each.type}]").ToList();

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""
            ? 0
            : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
