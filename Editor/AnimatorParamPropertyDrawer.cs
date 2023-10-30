using System.Collections.Generic;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    public class AnimatorParamPropertyDrawer : SaintsPropertyDrawer
    {
        // private const string InvalidAnimatorControllerWarningMessage = "Target animator controller is null";
        // private const string InvalidTypeWarningMessage = "{0} must be an int or a string";

        private string _error = "";

        // private AnimatorParamAttribute AnimAttr => (AnimatorParamAttribute)attribute;

        protected override float GetLabelFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // AnimatorParamAttribute animatorParamAttribute = property .GetAttribute<AnimatorParamAttribute>(property);
            // AnimatorParamAttribute animAttr = (AnimatorParamAttribute)saintsAttribute;
            // bool validAnimatorController = GetAnimatorController(property, animAttr.AnimatorName) != null;
            // bool validPropertyType = property.propertyType is SerializedPropertyType.Integer or SerializedPropertyType.String;
            //
            // return validAnimatorController && validPropertyType
            //     ? EditorGUIUtility.singleLineHeight
            //     : EditorGUIUtility.singleLineHeight * 2;
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            _error = "";

            AnimatorParamAttribute animAttr = (AnimatorParamAttribute)saintsAttribute;
            AnimatorController animatorController = GetAnimatorController(property, animAttr.AnimatorName);
            bool invalidAnimatorController = animatorController == null;
            if (invalidAnimatorController)
            {
                // EditorGUI.HelpBox(new Rect(rect)
                // {
                //     height = EditorGUIUtility.singleLineHeight,
                // }, InvalidAnimatorControllerWarningMessage, MessageType.Info);
                // EditorGUI.PropertyField(new Rect(rect)
                // {
                //     y = rect.y + EditorGUIUtility.singleLineHeight,
                // }, property, label);
                // // DrawDefaultPropertyAndHelpBox(rect, property, InvalidAnimatorControllerWarningMessage, MessageType.Warning);
                // return;
                _error = $"Animator controller `{animAttr.AnimatorName}` is null";
            }

            int parametersCount = animatorController.parameters.Length;
            List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>(parametersCount);
            for (int i = 0; i < parametersCount; i++)
            {
                AnimatorControllerParameter parameter = animatorController.parameters[i];
                if (animAttr.AnimatorParamType == null || parameter.type == animAttr.AnimatorParamType)
                {
                    animatorParameters.Add(parameter);
                }
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, animatorParameters);
                    break;
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, animatorParameters);
                    break;
                default:
                    _error = $"Support int or string type, get {property.propertyType}";
                    // DrawDefaultPropertyAndHelpBox(rect, property, string.Format(InvalidTypeWarningMessage, property.name), MessageType.Warning);
                    // EditorGUI.HelpBox(new Rect(rect)
                    // {
                    //     height = EditorGUIUtility.singleLineHeight,
                    // }, InvalidAnimatorControllerWarningMessage, MessageType.Info);
                    // EditorGUI.PropertyField(new Rect(rect)
                    // {
                    //     y = rect.y + EditorGUIUtility.singleLineHeight,
                    // }, property, label);
                    break;
            }

            // EditorGUI.EndProperty();
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property,
            List<AnimatorControllerParameter> animatorParameters)
        {
            int paramNameHash = property.intValue;
            int index = 0;

            for (int i = 0; i < animatorParameters.Count; i++)
            {
                if (paramNameHash == animatorParameters[i].nameHash)
                {
                    index = i + 1; // +1 because the first option is reserved for (None)
                    break;
                }
            }

            string[] displayOptions = GetDisplayOptions(animatorParameters);

            int newIndex = EditorGUI.Popup(rect, index, displayOptions);
            int newValue = newIndex == 0 ? 0 : animatorParameters[newIndex - 1].nameHash;

            if (property.intValue != newValue)
            {
                property.intValue = newValue;
            }
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, List<AnimatorControllerParameter> animatorParameters)
        {
            string paramName = property.stringValue;
            int index = 0;

            for (int i = 0; i < animatorParameters.Count; i++)
            {
                if (paramName.Equals(animatorParameters[i].name, System.StringComparison.Ordinal))
                {
                    index = i + 1; // +1 because the first option is reserved for (None)
                    break;
                }
            }

            string[] displayOptions = GetDisplayOptions(animatorParameters);

            int newIndex = EditorGUI.Popup(rect, index, displayOptions);
            string newValue = newIndex == 0 ? null : animatorParameters[newIndex - 1].name;

            if (!property.stringValue.Equals(newValue, System.StringComparison.Ordinal))
            {
                property.stringValue = newValue;
            }
        }

        private static string[] GetDisplayOptions(List<AnimatorControllerParameter> animatorParams)
        {
            string[] displayOptions = new string[animatorParams.Count + 1];
            displayOptions[0] = "(None)";

            for (int i = 0; i < animatorParams.Count; i++)
            {
                displayOptions[i + 1] = animatorParams[i].name;
            }

            return displayOptions;
        }

        private AnimatorController GetAnimatorController(SerializedProperty property, string animatorName)
        {
            // switch (ReflectUil.GetProp(property.serializedObject.targetObject.GetType(), animatorName))
            // {
            //     case (ReflectUil.GetPropType.NotFound, _):
            //
            // }
            Object targetObject = property.serializedObject.targetObject;
            SerializedObject targetSer = new SerializedObject(targetObject);
            SerializedProperty animProp = targetSer.FindProperty(animatorName);
            Animator animator = (Animator)animProp?.objectReferenceValue;
            // ReSharper disable once Unity.NoNullPropagation
            return (AnimatorController)animator?.runtimeAnimatorController;
        }
    }
}
