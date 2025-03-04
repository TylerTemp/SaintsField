using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorStateDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AnimatorStateAttribute), true)]
    [CustomPropertyDrawer(typeof(AnimatorStateBase), true)]
    [CustomPropertyDrawer(typeof(AnimatorState), true)]
    public partial class AnimatorStateAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private bool _onEnableChecked;
        private string _errorMsg = "";
        // private bool _targetIsString = true;

        private struct MetaInfo
        {
            public RuntimeAnimatorController RuntimeAnimatorController;
            public IReadOnlyList<AnimatorStateChanged> AnimatorStates;
            public string Error;
        }

        private static (string error, RuntimeAnimatorController animator) GetRuntimeAnimatorController(string animatorName, SerializedProperty property, MemberInfo info, object parent)
        {
            if (animatorName != null)
            {
                // search parent first
                (string error, object result) = Util.GetOf<object>(animatorName, null, property, info, parent);
                if (error != "")
                {
                    return (error, null);
                }
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (result)
                {
                    case Animator animatorResult:
                        return ("", animatorResult.runtimeAnimatorController);
                    case RuntimeAnimatorController controllerResult:
                        return ("", controllerResult);
                    default:
                        return ($"No Animator or RuntimeAnimatorController found in {animatorName}.", null);
                }
            }

            // otherwise, search on the serialized object
            Object targetObj = property.serializedObject.targetObject;
            RuntimeAnimatorController animatorController = null;
            switch (targetObj)
            {
                case GameObject go:
                {
                    Animator animator = go.GetComponent<Animator>();
                    if(animator != null)
                    {
                        animatorController = animator.runtimeAnimatorController;
                    }
                }
                    break;
                case Component component:
                {
                    Animator animator = component.GetComponent<Animator>();
                    if(animator != null)
                    {
                        animatorController = animator.runtimeAnimatorController;
                    }
                }
                    break;
                default:
                {
                    // string error = $"Animator controller not found in {targetObj}. Try specific a name instead.";
                    string error = $"Target {targetObj} is not a GameObject or Component";
                    return (error, null);
                }
            }

            return animatorController == null
                ? ($"Animator not found or is null in {targetObj}.", null)
                : ("", animatorController);
        }

        private static SerializedProperty FindPropertyRelative(SerializedProperty property, string name) =>
            property.FindPropertyRelative(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, name);

        private static bool SetPropValue(SerializedProperty property, AnimatorStateChanged animatorState)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (property.stringValue == animatorState.state.name)
                {
                    return false;
                }

                property.stringValue = animatorState.state.name;
                return true;
            }

            SerializedProperty curLayerIndexProp = FindPropertyRelative(property, "layerIndex");
            SerializedProperty curStateNameHashProp = FindPropertyRelative(property, "stateNameHash");
            SerializedProperty curStateNameProp = FindPropertyRelative(property, "stateName");
            SerializedProperty curStateSpeedProp = FindPropertyRelative(property, "stateSpeed");
            SerializedProperty curTagProp = FindPropertyRelative(property, "stateTag");
            SerializedProperty curAnimationClipProp = FindPropertyRelative(property, "animationClip");
            SerializedProperty curSubStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");

            bool changed = false;
            // must have
            if(curLayerIndexProp.intValue != animatorState.layerIndex)
            {
                // Debug.Log($"layerIndex changed");
                curLayerIndexProp.intValue = animatorState.layerIndex;
                changed = true;
            }

            // either have
            if(curStateNameHashProp != null && curStateNameHashProp.intValue != animatorState.state.nameHash)
            {
                // Debug.Log($"nameHash changed");
                curStateNameHashProp.intValue = animatorState.state.nameHash;
                changed = true;
            }
            if(curStateNameProp != null && curStateNameProp.stringValue != animatorState.state.name)
            {
                // Debug.Log($"name changed");
                curStateNameProp.stringValue = animatorState.state.name;
                changed = true;
            }

            // optional
            // we don't care about float comparison cuz it's a serialized value
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (curStateSpeedProp != null && curStateSpeedProp.floatValue != animatorState.state.speed)
            {
                // Debug.Log($"speed changed");
                curStateSpeedProp.floatValue = animatorState.state.speed;
                changed = true;
            }
            if (curAnimationClipProp != null && !ReferenceEquals(curAnimationClipProp.objectReferenceValue, animatorState.animationClip))
            {
                // Debug.Log($"animationClip changed");
                curAnimationClipProp.objectReferenceValue = animatorState.animationClip;
                changed = true;
            }
            if(curTagProp != null && curTagProp.stringValue != animatorState.state.tag)
            {
                // Debug.Log($"tag changed");
                curTagProp.stringValue = animatorState.state.tag;
                changed = true;
            }
            // ReSharper disable once InvertIf
            if(curSubStateMachineNameChainProp != null)
            {
                int newSize = animatorState.subStateMachineNameChain.Count;
                if(curSubStateMachineNameChainProp.arraySize != newSize)
                {
                    // Debug.Log($"arraySize changed");
                    curSubStateMachineNameChainProp.arraySize = newSize;
                    changed = true;
                }

                for (int index = 0; index < newSize; index++)
                {
                    SerializedProperty arrayProp = curSubStateMachineNameChainProp.GetArrayElementAtIndex(index);
                    string newValue = animatorState.subStateMachineNameChain[index];
                    // ReSharper disable once InvertIf
                    if(arrayProp.stringValue != newValue)
                    {
                        // Debug.Log($"array[{index}]({arrayProp.stringValue} -> {newValue}) changed");
                        arrayProp.stringValue = newValue;
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, string animFieldName, MemberInfo info, object parent)
        {
            // AnimatorStateAttribute animatorStateAttribute = (AnimatorStateAttribute) saintsAttribute;
            // string animFieldName = animatorStateAttribute.AnimFieldName;

            (string error, RuntimeAnimatorController runtimeAnimatorController) = GetRuntimeAnimatorController(animFieldName, property, info, parent);
            if (error != "")
            {
                return new MetaInfo
                {
                    Error = error,
                    AnimatorStates = Array.Empty<AnimatorStateChanged>(),
                };
            }

            // Debug.Log(animator.runtimeAnimatorController);

            AnimatorController controller = null;
            Dictionary<AnimationClip, AnimationClip> clipOverrideDict = new Dictionary<AnimationClip, AnimationClip>();

            switch (runtimeAnimatorController)
            {
                case AnimatorController ac:
                    controller = ac;
                    break;
                case AnimatorOverrideController overrideController:
                {
                    controller = (AnimatorController)overrideController.runtimeAnimatorController;

                    List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
                    overrideController.GetOverrides(overrides);
                    foreach (KeyValuePair<AnimationClip,AnimationClip> keyValuePair in overrides)
                    {
                        // Debug.Log($"{keyValuePair.Key} -> {keyValuePair.Value}");
                        clipOverrideDict[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
                    break;
            }

            if (controller == null)
            {
                return new MetaInfo
                {
                    Error = $"No AnimatorController on {runtimeAnimatorController}",
                    AnimatorStates = Array.Empty<AnimatorStateChanged>(),
                };
            }

            List<AnimatorStateChanged> animatorStates = new List<AnimatorStateChanged>();
            foreach ((AnimatorControllerLayer animatorControllerLayer, int layerIndex) in controller.layers.Select((each, index) => (each, index)))
            {
                foreach ((UnityEditor.Animations.AnimatorState state, IReadOnlyList<string> subStateMachineNameChain) in GetAnimatorStateRecursively(animatorControllerLayer.stateMachine, animatorControllerLayer.stateMachine.stateMachines.Select(each => each.stateMachine), Array.Empty<string>()))
                {
                    AnimationClip clip = (AnimationClip)state.motion;
                    if (clip != null && clipOverrideDict.TryGetValue(clip, out AnimationClip overrideClip))
                    {
                        clip = overrideClip;
                    }
                    animatorStates.Add(new AnimatorStateChanged
                    {
                        layer = animatorControllerLayer,
                        layerIndex = layerIndex,
                        state = state,

                        animationClip = clip,
                        subStateMachineNameChain = subStateMachineNameChain.ToArray(),
                    });
                }
            }

            return new MetaInfo
            {
                RuntimeAnimatorController = runtimeAnimatorController,
                AnimatorStates = animatorStates,
                Error = "",
            };
        }

        private static void OpenAnimator(Object animatorController)
        {
            Selection.activeObject = animatorController;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }

        private static IEnumerable<(UnityEditor.Animations.AnimatorState, IReadOnlyList<string>)> GetAnimatorStateRecursively(AnimatorStateMachine curStateMachine, IEnumerable<AnimatorStateMachine> subStateMachines, IReadOnlyList<string> accStateMachineNameChain)
        {
            foreach (ChildAnimatorState childAnimatorState in curStateMachine.states)
            {
                yield return (childAnimatorState.state, accStateMachineNameChain);
            }

            foreach (AnimatorStateMachine subStateMachine in subStateMachines)
            {
                // stateMachine.stateMachine
                foreach ((UnityEditor.Animations.AnimatorState, IReadOnlyList<string>) result in GetAnimatorStateRecursively(subStateMachine, subStateMachine.stateMachines.Select(each => each.stateMachine), accStateMachineNameChain.Append(subStateMachine.name).ToArray()))
                {
                    yield return result;
                }
            }
        }

        private static void RenderErrorFallback(Rect position, GUIContent label, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (EditorGUI.ChangeCheckScope onChange = new EditorGUI.ChangeCheckScope())
                {
                    string newContent = EditorGUI.TextField(position, label, property.stringValue);
                    if (onChange.changed)
                    {
                        property.stringValue = newContent;
                    }

                    return;
                }
            }

            SerializedProperty curStateDisplayProp = FindPropertyRelative(property, "stateName") ?? FindPropertyRelative(property, "stateNameHash");
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUI.TextField(position, label, curStateDisplayProp.stringValue);
            }
        }

        private static string FormatStateLabel(AnimatorStateChanged animatorStateInfo, string sep) => $"{animatorStateInfo.state.name}{(animatorStateInfo.animationClip == null ? "" : $" ({animatorStateInfo.animationClip.name})")}: {animatorStateInfo.layer.name}{(animatorStateInfo.subStateMachineNameChain.Count == 0 ? "" : $"{sep}{string.Join(sep, animatorStateInfo.subStateMachineNameChain)}")}";

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            AnimatorStateAttribute animatorStateAttribute = propertyAttribute as AnimatorStateAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, animatorStateAttribute?.AnimFieldName, memberInfo, parent);
            if (metaInfo.Error != "")
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = metaInfo.Error,
                    Error = "",
                };
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String when string.IsNullOrEmpty(property.stringValue):
                    return null;  // we don't care about empty string, user need to add a `Required`
                case SerializedPropertyType.String:
                {
                    if (metaInfo.AnimatorStates.Any(animatorControllerParameter => animatorControllerParameter.state.name == property.stringValue))
                    {
                        return null;
                    }

                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"State {property.stringValue} not found in {metaInfo.RuntimeAnimatorController.name}",
                    };
                }
                case SerializedPropertyType.Generic:
                {
                    int curIndex = Util.ListIndexOfAction(metaInfo.AnimatorStates,
                            eachStateInfo => EqualAnimatorState(eachStateInfo, property));
                    if (curIndex == -1)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"State not found in {metaInfo.RuntimeAnimatorController.name}",
                        };
                    }

                    return null;
                }
                default:
                    return null;
            }
        }
    }
}
