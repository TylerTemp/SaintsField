using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    public class AnimatorParamAttributeDrawer : SaintsPropertyDrawer
    {
        // private const string InvalidAnimatorControllerWarningMessage = "Target animator controller is null";
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            // AnimatorParamAttribute animatorParamAttribute = property .GetAttribute<AnimatorParamAttribute>(property);
            // AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;
            // bool validAnimatorController = GetAnimatorController(property, animatorParamAttribute.AnimatorName) != null;
            // bool validPropertyType = property.propertyType is SerializedPropertyType.Integer or SerializedPropertyType.String;

            // return (validAnimatorController && validPropertyType)
            //     ? EditorGUIUtility.singleLineHeight
            //     : EditorGUIUtility.singleLineHeight * 2;
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // EditorGUI.BeginProperty(rect, label, property);
            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;

            SerializedObject targetSer = property.serializedObject;
            SerializedProperty animProp = targetSer.FindProperty(animatorParamAttribute.AnimatorName) ?? SerializedUtils.FindPropertyByAutoPropertyName(targetSer, animatorParamAttribute.AnimatorName);

            List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();

            bool invalidAnimatorController = animProp == null;

            if (invalidAnimatorController)
            {
                _error = $"Animator controller `{animatorParamAttribute.AnimatorName}` is null";
                DefaultDrawer(position, property);
                return;
            }

            Animator animatorController = (Animator)animProp.objectReferenceValue;

            // int parametersCount = animatorController.parameters.Length;

            // for (int i = 0; i < parametersCount; i++)
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            {
                if (animatorParamAttribute.AnimatorParamType == null ||
                    parameter.type == animatorParamAttribute.AnimatorParamType)
                {
                    animatorParameters.Add(parameter);
                }
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, animatorParameters);
                    break;
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, animatorParameters);
                    break;
                default:
                    // DrawDefaultPropertyAndHelpBox(rect, property, string.Format(InvalidTypeWarningMessage, property.name), MessageType.Warning);
                    // EditorGUI.HelpBox(new Rect(rect)
                    // {
                    //     height = EditorGUIUtility.singleLineHeight,
                    // }, InvalidAnimatorControllerWarningMessage, MessageType.Info);
                    // EditorGUI.PropertyField(new Rect(rect)
                    // {
                    //     y = rect.y + EditorGUIUtility.singleLineHeight,
                    // }, property, label);
                    _error = $"Invalid property type: expect integer or string, get {property.propertyType}";
                    break;
            }

            // EditorGUI.EndProperty();
        }

        private static void DrawPropertyForInt(Rect position, SerializedProperty property, GUIContent label, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            // if (invalid)
            // {
            //     using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            //     int directIntValue = EditorGUI.IntField(position, property.intValue);
            //     if (changed.changed)
            //     {
            //         property.intValue = directIntValue;
            //     }
            //
            //     return;
            // }

            int paramNameHash = property.intValue;
            int index = 0;

            for (int i = 0; i < animatorParameters.Count; i++)
            {
                // ReSharper disable once InvertIf
                if (paramNameHash == animatorParameters[i].nameHash)
                {
                    index = i + 1; // +1 because the first option is reserved for (None)
                    break;
                }
            }

            IEnumerable<string> displayOptions = GetDisplayOptions(animatorParameters);

            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, displayOptions.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    int newValue = newIndex == 0 ? 0 : animatorParameters[newIndex - 1].nameHash;

                    if (property.intValue != newValue)
                    {
                        property.intValue = newValue;
                    }
                }
            }
        }

        private static void DrawPropertyForString(Rect position, SerializedProperty property, GUIContent label, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            // if (invalid)
            // {
            //     using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            //     string directIntValue = EditorGUI.TextField(position, property.stringValue);
            //     if (changed.changed)
            //     {
            //         property.stringValue = directIntValue;
            //     }
            //
            //     return;
            // }

            string paramName = property.stringValue;
            int index = 0;

            for (int i = 0; i < animatorParameters.Count; i++)
            {
                // ReSharper disable once InvertIf
                if (paramName.Equals(animatorParameters[i].name, System.StringComparison.Ordinal))
                {
                    index = i + 1; // +1 because the first option is reserved for (None)
                    break;
                }
            }

            IEnumerable<string> displayOptions = GetDisplayOptions(animatorParameters);

            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, displayOptions.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    string newValue = newIndex == 0 ? null : animatorParameters[newIndex - 1].name;

                    if (!property.stringValue.Equals(newValue, System.StringComparison.Ordinal))
                    {
                        property.stringValue = newValue;
                    }
                }
            }
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static IReadOnlyList<string> GetDisplayOptions(IReadOnlyList<AnimatorControllerParameter> animatorParams)
        {
            string[] displayOptions = new string[animatorParams.Count + 1];
            displayOptions[0] = "[None]";

            for (int i = 0; i < animatorParams.Count; i++)
            {
                displayOptions[i + 1] = $"{animatorParams[i].name} [{animatorParams[i].type}]";
            }

            return displayOptions;
        }

        // private AnimatorController GetAnimatorController(SerializedProperty property, string animatorName)
        // {
        //     Object targetObject = property.serializedObject.targetObject;
        //     SerializedObject targetSer = new SerializedObject(targetObject);
        //     SerializedProperty animProp = targetSer.FindProperty(animatorName);
        //     Animator animator = (Animator)animProp?.objectReferenceValue;
        //     // ReSharper disable once Unity.NoNullPropagation
        //     return (AnimatorController)animator?.runtimeAnimatorController;
        // }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => HelpBox.Draw(position, _error, MessageType.Error);
    }
}
