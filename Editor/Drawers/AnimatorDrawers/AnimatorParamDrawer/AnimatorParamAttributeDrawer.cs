#if UNITY_2021_3_OR_NEWER
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorParamDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute), true)]
    public partial class AnimatorParamAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        // private const string InvalidAnimatorControllerWarningMessage = "Target animator controller is null";
        private string _error = "";

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public Animator Animator;
            public IReadOnlyList<AnimatorControllerParameter> AnimatorParameters;
            // ReSharper enable InconsistentNaming
        }

        private static bool ParamNameEquals(AnimatorControllerParameter param, SerializedProperty prop) =>
            param.name == prop.stringValue;

        private static bool ParamHashEquals(AnimatorControllerParameter param, SerializedProperty prop) =>
            param.nameHash == prop.intValue;

        private static (string error, Animator animator) GetAnimator(string animatorName, SerializedProperty property, MemberInfo info, object parent)
        {
            if (animatorName != null)
            {
                // search parent first
                (string error, Animator result) = Util.GetOf<Animator>(animatorName, null, property, info, parent);
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
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, MemberInfo info, object parent)
        {
            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;

            (string error, Animator animator) = GetAnimator(animatorParamAttribute.AnimatorName, property, info, parent);
            if (error != "")
            {
                return new MetaInfo
                {
                    Error = error,
                    AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                };
            }

            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;

            if (runtimeController == null)
            {
                return new MetaInfo
                {
                    Error = $"RuntimeAnimatorController must not be null in {animator.name}",
                    AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                };
            }

            string loadPath;
            if(runtimeController is AnimatorOverrideController aoc)
            {
                loadPath = AssetDatabase.GetAssetPath(aoc.runtimeAnimatorController);
            }
            else
            {
                loadPath = AssetDatabase.GetAssetPath(runtimeController);
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(loadPath);
            // AnimatorOverrideController oc = (AnimatorOverrideController);
            // if (runtimeController is AnimatorOverrideController aoc)
            // {
            //     Debug.Log(aoc.runtimeAnimatorController);
            //     Debug.Log(AssetDatabase.GetAssetPath(aoc.runtimeAnimatorController));
            // }
            // AnimatorController controller = (AnimatorController)runtimeController;
            // Debug.Log($"runtimeController={runtimeController}/controller={controller}/{AssetDatabase.GetAssetPath(runtimeController)}");
            // for override controller, this hack won't work.
            // TODO: if the target is inside a prefab which is not loaded yet, does it works?
            if (controller == null)
            {
                // Debug.Log(runtimeController.GetType());
                // controller = (AnimatorController)runtimeController;
                return new MetaInfo
                {
                    Error = $"Can not obtain AnimatorController from {animator.name}: {runtimeController.GetType()}",
                    AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                };
            }

            List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (animatorParamAttribute.AnimatorParamType == null ||
                    parameter.type == animatorParamAttribute.AnimatorParamType)
                {
                    animatorParameters.Add(parameter);
                }
            }

            return new MetaInfo
            {
                Error = "",
                Animator = animator,
                AnimatorParameters = animatorParameters,
            };
        }

        private static void OpenAnimator(Object animatorController)
        {
            Selection.activeObject = animatorController;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, (AnimatorParamAttribute)propertyAttribute, memberInfo, parent);
            if (metaInfo.Error != "")
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = metaInfo.Error,
                    Error = "",
                };
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.String when string.IsNullOrEmpty(property.stringValue):
                    return null;  // we don't care about empty string, user need to add a `Required`
                case SerializedPropertyType.String:
                {
                    if (metaInfo.AnimatorParameters.Any(animatorControllerParameter => ParamNameEquals(animatorControllerParameter, property)))
                    {
                        return null;
                    }

                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"Parameter {property.stringValue} not found in {metaInfo.Animator.name}",
                    };
                }
                case SerializedPropertyType.Integer:
                {
                    if (metaInfo.AnimatorParameters.Any(animatorControllerParameter => ParamHashEquals(animatorControllerParameter, property)))
                    {
                        return null;
                    }

                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"Parameter {property.intValue} not found in {metaInfo.Animator.name}",
                    };
                }

                default:
                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"Invalid property type: expect integer or string, get {property.propertyType}",
                    };
            }
        }
    }
}
