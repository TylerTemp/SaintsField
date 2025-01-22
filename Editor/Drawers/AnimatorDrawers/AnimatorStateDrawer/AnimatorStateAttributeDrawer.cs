﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorStateDrawer
{
    [CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
    public partial class AnimatorStateAttributeDrawer : SaintsPropertyDrawer
    {
        private bool _onEnableChecked;
        private string _errorMsg = "";
        // private bool _targetIsString = true;

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public RuntimeAnimatorController RuntimeAnimatorController;
            public IReadOnlyList<AnimatorStateChanged> AnimatorStates;
            public string Error;
            // ReSharper enable InconsistentNaming
        }

        private static (string error, RuntimeAnimatorController animator) GetRuntimeAnimatorController(string animatorName, SerializedProperty property, FieldInfo fieldInfo, object parent)
        {
            if (animatorName != null)
            {
                // search parent first
                (string error, object result) = Util.GetOf<object>(animatorName, null, property, fieldInfo, parent);
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
            RuntimeAnimatorController animatorController;
            switch (targetObj)
            {
                case GameObject go:
                    animatorController = go.GetComponent<Animator>().runtimeAnimatorController;
                    break;
                case Component component:
                    animatorController = component.GetComponent<Animator>().runtimeAnimatorController;
                    break;
                default:
                    // string error = $"Animator controller not found in {targetObj}. Try specific a name instead.";
                    string error = $"Target {targetObj} is not a GameObject or Component";
                    return (error, null);
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

        private static MetaInfo GetMetaInfo(SerializedProperty property, string animFieldName, FieldInfo fieldInfo, object parent)
        {
            // AnimatorStateAttribute animatorStateAttribute = (AnimatorStateAttribute) saintsAttribute;
            // string animFieldName = animatorStateAttribute.AnimFieldName;

            (string error, RuntimeAnimatorController runtimeAnimatorController) = GetRuntimeAnimatorController(animFieldName, property, fieldInfo, parent);
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

    }
}
