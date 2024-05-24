using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils
{
    public static class AnimatorUtils
    {
        public static (string error, Animator animator) GetAnimator(string animatorName, SerializedProperty property, FieldInfo fieldInfo, object parent)
        {
            if (animatorName != null)
            {
                // search parent first
                (string error, Animator result) = Util.GetOf<Animator>(animatorName, null, property, fieldInfo, parent);
                if (result == null)
                {
                    return ($"Animator {animatorName} can not be null.", null);
                }
                if (error == "")
                {
                    return ("", result);
                }

                // otherwise, search the serialized property
                SerializedObject targetSer = property.serializedObject;
                SerializedProperty animProp = targetSer.FindProperty(animatorName) ??
                                              SerializedUtils.FindPropertyByAutoPropertyName(targetSer,
                                                  animatorName);
                // ReSharper disable once MergeIntoPattern
                if(animProp?.objectReferenceValue is Animator anim)
                {
                    if (anim == null)
                    {
                        return ($"Animator {animatorName} can not be null.", null);
                    }

                    return ("", anim);
                }
            }

            // otherwise, search on the serialized object
            Object targetObj = property.serializedObject.targetObject;
            Animator animator;
            switch (targetObj)
            {
                case GameObject go:
                    animator = go.GetComponent<Animator>();
                    break;
                case Component component:
                    animator = component.GetComponent<Animator>();
                    break;
                default:
                    // string error = $"Animator controller not found in {targetObj}. Try specific a name instead.";
                    string error = $"Target {targetObj} is not a GameObject or Component";
                    return (error, null);
            }

            return animator == null
                ? ($"Animator not found or is null in {targetObj}.", null)
                : ("", animator);

            // AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;
            //
            // Animator animatorController;
            // if (animatorParamAttribute.AnimatorName == null)
            // {
            //     Object targetObj = property.serializedObject.targetObject;
            //     // animatorController = (Animator)animProp.objectReferenceValue;
            //     switch (targetObj)
            //     {
            //         case GameObject go:
            //             animatorController = go.GetComponent<Animator>();
            //             break;
            //         case Component component:
            //             animatorController = component.GetComponent<Animator>();
            //             break;
            //         default:
            //             string error = $"Animator controller not found in {targetObj}. Try specific a name instead.";
            //             return new AdvancedDropdownAttributeDrawer.MetaInfo
            //             {
            //                 Error = error,
            //                 AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
            //             };
            //     }
            // }
            // else
            // {
            //
            //     SerializedObject targetSer = property.serializedObject;
            //     SerializedProperty animProp = targetSer.FindProperty(animatorParamAttribute.AnimatorName) ??
            //                                   SerializedUtils.FindPropertyByAutoPropertyName(targetSer,
            //                                       animatorParamAttribute.AnimatorName);
            //
            //     bool invalidAnimatorController = animProp == null;
            //
            //     if (invalidAnimatorController)
            //     {
            //         string error = $"Animator controller `{animatorParamAttribute.AnimatorName}` is null";
            //         return new AdvancedDropdownAttributeDrawer.MetaInfo
            //         {
            //             Error = error,
            //             AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
            //         };
            //     }
            //
            //     animatorController = (Animator)animProp.objectReferenceValue;
            // }
            //
            // List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();
            //
            // // ReSharper disable once LoopCanBeConvertedToQuery
            // foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            // {
            //     if (animatorParamAttribute.AnimatorParamType == null ||
            //         parameter.type == animatorParamAttribute.AnimatorParamType)
            //     {
            //         animatorParameters.Add(parameter);
            //     }
            // }
            //
            // return new AdvancedDropdownAttributeDrawer.MetaInfo
            // {
            //     Error = "",
            //     AnimatorParameters = animatorParameters,
            // };
        }
    }
}
