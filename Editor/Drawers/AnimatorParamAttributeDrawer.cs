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
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            _error = "";

            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;

            Animator animatorController;
            if (animatorParamAttribute.AnimatorName == null)
            {
                Object targetObj = property.serializedObject.targetObject;
                // animatorController = (Animator)animProp.objectReferenceValue;
                switch (targetObj)
                {
                    case GameObject go:
                        animatorController = go.GetComponent<Animator>();
                        break;
                    case Component component:
                        animatorController = component.GetComponent<Animator>();
                        break;
                    default:
                        _error = $"Animator controller not found in {targetObj}. Try specific a name instead.";
                        DefaultDrawer(position, property, label);
                        return;
                }
            }
            else
            {

                SerializedObject targetSer = property.serializedObject;
                SerializedProperty animProp = targetSer.FindProperty(animatorParamAttribute.AnimatorName) ??
                                              SerializedUtils.FindPropertyByAutoPropertyName(targetSer,
                                                  animatorParamAttribute.AnimatorName);

                bool invalidAnimatorController = animProp == null;

                if (invalidAnimatorController)
                {
                    _error = $"Animator controller `{animatorParamAttribute.AnimatorName}` is null";
                    DefaultDrawer(position, property, label);
                    return;
                }

                animatorController = (Animator)animProp.objectReferenceValue;
            }

            List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            {
                if (animatorParamAttribute.AnimatorParamType == null ||
                    parameter.type == animatorParamAttribute.AnimatorParamType)
                {
                    animatorParameters.Add(parameter);
                }
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, animatorParameters);
                    break;
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, animatorParameters);
                    break;
                default:
                    _error = $"Invalid property type: expect integer or string, get {property.propertyType}";
                    DefaultDrawer(position, property, label);
                    break;
            }
        }

        private static void DrawPropertyForInt(Rect position, SerializedProperty property, GUIContent label, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
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

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, displayOptions.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    int newValue = animatorParameters[newIndex].nameHash;

                    if (property.intValue != newValue)
                    {
                        property.intValue = newValue;
                    }
                }
            }
        }

        private static void DrawPropertyForString(Rect position, SerializedProperty property, GUIContent label, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            string paramName = property.stringValue;
            int index = animatorParameters
                .Select((value, index) => new {value, index})
                .FirstOrDefault(each => each.value.name == paramName)?.index ?? -1;
            // int index = 0;
            //
            // for (int i = 0; i < animatorParameters.Count; i++)
            // {
            //     // ReSharper disable once InvertIf
            //     if (paramName.Equals(animatorParameters[i].name, System.StringComparison.Ordinal))
            //     {
            //         index = i + 1; // +1 because the first option is reserved for (None)
            //         break;
            //     }
            // }

            IEnumerable<string> displayOptions = GetDisplayOptions(animatorParameters);

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, displayOptions.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    // string newValue = newIndex == 0 ? null : animatorParameters[newIndex - 1].name;
                    string newValue = animatorParameters[newIndex].name;

                    if (!property.stringValue.Equals(newValue, System.StringComparison.Ordinal))
                    {
                        property.stringValue = newValue;
                    }
                }
            }
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static IReadOnlyList<string> GetDisplayOptions(IEnumerable<AnimatorControllerParameter> animatorParams)
        {
            // string[] displayOptions = new string[animatorParams.Count + 1];
            // displayOptions[0] = "[None]";
            //
            // for (int i = 0; i < animatorParams.Count; i++)
            // {
            //     displayOptions[i + 1] = $"{animatorParams[i].name} [{animatorParams[i].type}]";
            // }
            //
            // return displayOptions;
            return animatorParams.Select(each => $"{each.name} [{each.type}]").ToList();
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => HelpBox.Draw(position, _error, MessageType.Error);
    }
}
