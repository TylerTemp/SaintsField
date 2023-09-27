using System.Collections.Generic;
using System.Linq;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ExtInspector.Standalone.Editor
{
    [CustomPropertyDrawer(typeof(AnimState))]
    public class AnimatorStateSelectorDrawer : PropertyDrawer
    {
        private bool _onEnableChecked;
        private string _errorMsg;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight * 2;
            float errorHeight = string.IsNullOrEmpty(_errorMsg)
                ? 0
                : HelpBox.GetHeight(_errorMsg);
            return PopupHeight() + errorHeight + EditorGUIUtility.singleLineHeight * 2;
        }

        private static float PopupHeight() =>
            EditorStyles.popup.CalcHeight(new GUIContent("M"), EditorGUIUtility.currentViewWidth);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using EditorGUI.PropertyScope propertyScope = new EditorGUI.PropertyScope(position, label, property);

            label = propertyScope.content;

            // Object targetObject = property.serializedObject.targetObject;

            AnimStateSelector animStateSelector = SerializedUtil.GetAttribute<AnimStateSelector>(property);
            string animFieldName = animStateSelector?.AnimFieldName ?? "animator";
            SerializedObject targetSer = property.serializedObject;
            SerializedProperty animProp = targetSer.FindProperty(animFieldName) ?? SerializedUtil.FindPropertyByAutoPropertyName(targetSer, animFieldName);
            if(animProp == null)
            {
                RenderError($"Can't find Animator {animFieldName}{(animStateSelector == null ? ", use AnimStateSelector to specific one" : "")}",
                    position, property, label);
                return;
            }

            Animator animator = (Animator)animProp.objectReferenceValue;
            if (!animator)
            {
                RenderError($"Animator {animFieldName} is null", position, property, label);
                return;
            }

            _errorMsg = null;

            AnimatorController controller = (AnimatorController)animator.runtimeAnimatorController;

            List<AnimState> animatorStates = new List<AnimState>();
            foreach ((AnimatorControllerLayer animatorControllerLayer, int layerIndex) in controller.layers.Select((each, index) => (each, index)))
            {
                animatorStates.AddRange(
                    animatorControllerLayer.stateMachine.states.Select(
                        childAnimatorState =>
                        {
                            AnimationClip clip = (AnimationClip)childAnimatorState.state.motion;
                            // float clipLength = clip ? clip.length : 0;
                            float speed = childAnimatorState.state.speed;
                            return new AnimState
                            {
                                layerIndex = layerIndex,
                                stateName = childAnimatorState.state.name,
                                stateNameHash = childAnimatorState.state.nameHash,
                                stateSpeed = speed,
                                animationClip = clip,
                            };
                        })
                );
            }

            if (animatorStates.Count == 0)
            {
                RenderError($"Animator {animFieldName} has no states", position, property, label);
                return;
            }

            SerializedProperty curLayerIndexProp = property.FindPropertyRelative("layerIndex");
            SerializedProperty curStateNameHashProp = property.FindPropertyRelative("stateNameHash");
            SerializedProperty curStateNameProp = property.FindPropertyRelative("stateName");
            SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
            SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
            int curLayerIndex = curLayerIndexProp.intValue;
            int curStateNameHash = curStateNameHashProp.intValue;

            int curIndex = animatorStates.FindIndex((each) =>
                each.layerIndex == curLayerIndex && each.stateNameHash == curStateNameHash);
            if (curIndex == -1)
            {
                curIndex = 0;
            }

            if (!_onEnableChecked)  // check whether external source changed, to avoid caching an old value
            {
                _onEnableChecked = true;
                AnimState animState = animatorStates[curIndex];
                curLayerIndexProp.intValue = animState.layerIndex;
                curStateNameHashProp.intValue = animState.stateNameHash;
                curStateNameProp.stringValue = animState.stateName;
                curStateSpeedProp.floatValue = animState.stateSpeed;
                curAnimationClipProp.objectReferenceValue = animState.animationClip;
            }

            (Rect popupRect, Rect popupLeftRect) = RectUtils.SplitHeightRect(position, PopupHeight());
            using (EditorGUI.ChangeCheckScope popupChanged = new EditorGUI.ChangeCheckScope())
            {
                curIndex = EditorGUI.Popup(
                    popupRect,
                    label,
                    curIndex,
                    animatorStates.Select(each => new GUIContent(each.ToString())).ToArray(),
                    EditorStyles.popup);
                if (popupChanged.changed)
                {
                    AnimState animState = animatorStates[curIndex];
                    curLayerIndexProp.intValue = animState.layerIndex;
                    curStateNameHashProp.intValue = animState.stateNameHash;
                    curStateNameProp.stringValue = animState.stateName;
                    curStateSpeedProp.floatValue = animState.stateSpeed;
                    curAnimationClipProp.objectReferenceValue = animState.animationClip;
                    // Debug.Log($"change: animationClip = {animState.animationClip}/{curAnimationClipProp.objectReferenceValue}");
                }
            }

            RenderSubRow(popupLeftRect, property);
        }

        private void RenderError(string content, Rect position, SerializedProperty property, GUIContent label)
        {
            _errorMsg = content;
            Rect helpBoxLeftRect = HelpBox.Draw(position, _errorMsg, MessageType.Error);
            string propertyString = PropertyToString(property);
            (Rect popupRect, Rect popupLeftRect) = RectUtils.SplitHeightRect(helpBoxLeftRect, PopupHeight());
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.Popup(popupRect, label, 0, new[] { new GUIContent(propertyString) });
            }

            RenderSubRow(popupLeftRect, property);
        }

        private static void RenderSubRow(Rect position, SerializedProperty property)
        {
            SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
            SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
            // using (new EditorGUI.IndentLevelScope(1))
            using (new EditorGUI.DisabledScope(true))
            {
                (Rect speedRect, Rect speedLeftRect) =
                    RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(speedRect, curStateSpeedProp, new GUIContent( $"┣{curStateSpeedProp.displayName}"));
                EditorGUI.PropertyField(speedLeftRect, curAnimationClipProp, new GUIContent( $"┗{curAnimationClipProp.displayName}"));
            }
        }

        private static string PropertyToString(SerializedProperty property) =>
            new AnimState
            {
                layerIndex = property.FindPropertyRelative("layerIndex").intValue,
                stateName = property.FindPropertyRelative("stateName").stringValue,
                stateNameHash = property.FindPropertyRelative("stateNameHash").intValue,
                animationClip = (AnimationClip) property.FindPropertyRelative("animationClip").objectReferenceValue,
                stateSpeed = property.FindPropertyRelative("stateSpeed").floatValue,
            }.ToString();
    }
}
