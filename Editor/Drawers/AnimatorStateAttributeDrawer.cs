﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    // [CustomPropertyDrawer(typeof(AnimState))]
    [CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
    public class AnimatorStateAttributeDrawer : SaintsPropertyDrawer
    {
        private bool _onEnableChecked;
        private string _errorMsg = "";
        // private bool _targetIsString = true;

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public IReadOnlyList<AnimatorState> AnimatorStates;
            public string Error;
            // ReSharper enable InconsistentNaming
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth)
        {
            return EditorStyles.popup.CalcHeight(new GUIContent("M"), EditorGUIUtility.currentViewWidth);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            float errorHeight = _errorMsg == "" ? 0 : ImGuiHelpBox.GetHeight(_errorMsg, width, MessageType.Error);
            float subRowHeight = EditorGUIUtility.singleLineHeight * (property.propertyType == SerializedPropertyType.String ? 0 : 2);
            return errorHeight + subRowHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            if (metaInfo.Error != "")
            {
                _errorMsg = metaInfo.Error;
                RenderErrorFallback(position, property);
                return;
            }

            SerializedProperty curLayerIndexProp = property.FindPropertyRelative("layerIndex");
            SerializedProperty curStateNameHashProp = property.FindPropertyRelative("stateNameHash");

            string[] optionStrings = metaInfo.AnimatorStates.Select(each => each.stateName).ToArray();

            int curIndex;
            if (property.propertyType == SerializedPropertyType.String)
            {
                curIndex = Array.IndexOf(optionStrings, property.stringValue);
            }
            else
            {
                int curLayerIndex = curLayerIndexProp.intValue;
                int curStateNameHash = curStateNameHashProp.intValue;
                curIndex = Util.ListIndexOfAction(metaInfo.AnimatorStates, (each) =>
                    each.layerIndex == curLayerIndex && each.stateNameHash == curStateNameHash);
            }
            if (curIndex == -1)
            {
                curIndex = 0;
            }

            if (!_onEnableChecked)  // check whether external source changed, to avoid caching an old value
            {
                _onEnableChecked = true;
                SetPropValue(property, metaInfo.AnimatorStates[curIndex]);
            }

            // (Rect popupRect, Rect popupLeftRect) = RectUtils.SplitHeightRect(position, GetLabelFieldHeight(property, label, saintsAttribute));
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope popupChanged = new EditorGUI.ChangeCheckScope())
            {
                curIndex = EditorGUI.Popup(
                    position,
                    label,
                    curIndex,
                    metaInfo.AnimatorStates.Select(each => new GUIContent(each.ToString())).ToArray(),
                    EditorStyles.popup);

                // ReSharper disable once InvertIf
                if (popupChanged.changed)
                {
                    SetPropValue(property, metaInfo.AnimatorStates[curIndex]);
                }
            }
            // RenderSubRow(popupLeftRect, property);
        }
        #endregion

        private static void SetPropValue(SerializedProperty property, AnimatorState animatorState)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = animatorState.stateName;
            }
            else
            {
                SerializedProperty curLayerIndexProp = property.FindPropertyRelative("layerIndex");
                SerializedProperty curStateNameHashProp = property.FindPropertyRelative("stateNameHash");
                SerializedProperty curStateNameProp = property.FindPropertyRelative("stateName");
                SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
                SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");

                curLayerIndexProp.intValue = animatorState.layerIndex;
                curStateNameHashProp.intValue = animatorState.stateNameHash;
                curStateNameProp.stringValue = animatorState.stateName;
                curStateSpeedProp.floatValue = animatorState.stateSpeed;
                curAnimationClipProp.objectReferenceValue = animatorState.animationClip;
            }
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo fieldInfo, object parent)
        {
            AnimatorStateAttribute animatorStateAttribute = (AnimatorStateAttribute) saintsAttribute;
            string animFieldName = animatorStateAttribute.AnimFieldName;

            (string error, Animator animator) = AnimatorUtils.GetAnimator(animFieldName, property, fieldInfo, parent);
            if (error != "")
            {
                return new MetaInfo
                {
                    Error = error,
                    AnimatorStates = Array.Empty<AnimatorState>(),
                };
            }

            AnimatorController controller = (AnimatorController)animator.runtimeAnimatorController;

            List<AnimatorState> animatorStates = new List<AnimatorState>();
            foreach ((AnimatorControllerLayer animatorControllerLayer, int layerIndex) in controller.layers.Select((each, index) => (each, index)))
            {
                animatorStates.AddRange(
                    animatorControllerLayer.stateMachine.states.Select(
                        childAnimatorState =>
                        {
                            AnimationClip clip = (AnimationClip)childAnimatorState.state.motion;
                            // float clipLength = clip ? clip.length : 0;
                            float speed = childAnimatorState.state.speed;
                            return new AnimatorState
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

            // ReSharper disable once InvertIf
            if (animatorStates.Count == 0)
            {
                string errorMsg = $"Animator {animFieldName} has no states";
                return new MetaInfo
                {
                    Error = errorMsg,
                    AnimatorStates = Array.Empty<AnimatorState>(),
                };
            }

            return new MetaInfo
            {
                AnimatorStates = animatorStates,
                Error = "",
            };
        }

        private static void RenderErrorFallback(Rect position, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (EditorGUI.ChangeCheckScope onChange = new EditorGUI.ChangeCheckScope())
                {
                    string newContent = EditorGUI.TextField(position, GUIContent.none, property.stringValue);
                    if (onChange.changed)
                    {
                        property.stringValue = newContent;
                    }

                    return;
                }
            }

            SerializedProperty curStateNameProp = property.FindPropertyRelative("stateName");
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUI.TextField(position, GUIContent.none, curStateNameProp.stringValue);
            }
            // _errorMsg = content;
            // Rect helpBoxLeftRect = HelpBox.Draw(position, _errorMsg, MessageType.Error);
            // string propertyString = PropertyToString(property);
            // (Rect popupRect, Rect popupLeftRect) = RectUtils.SplitHeightRect(helpBoxLeftRect, PopupHeight());
            // using (new EditorGUI.DisabledScope(true))
            // {
            //     EditorGUI.Popup(popupRect, label, 0, new[] { new GUIContent(propertyString) });
            // }
            //
            // RenderSubRow(popupLeftRect, property);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return property.propertyType != SerializedPropertyType.String;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            // Debug.Log(_targetIsString);
            if(property.propertyType == SerializedPropertyType.String)
            {
                // (Rect valueRect, Rect leftRect) =
                //     RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                // using (new EditorGUI.DisabledGroupScope(true)) {
                //     EditorGUI.TextField(valueRect, "┗Value", property.stringValue);
                // }
                return position;
            }

            SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
            SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
            // using (new EditorGUI.IndentLevelScope(1))
            using (new EditorGUI.DisabledScope(true))
            {
                (Rect speedRect, Rect speedLeftRect) =
                    RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(speedRect, curStateSpeedProp, new GUIContent( $"┣{curStateSpeedProp.displayName}"));

                (Rect clipRect, Rect leftRect) =
                    RectUtils.SplitHeightRect(speedLeftRect, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(clipRect, curAnimationClipProp, new GUIContent( $"┗{curAnimationClipProp.displayName}"));

                return leftRect;
            }
        }

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_DropdownField";
        private static string NameSpeedField(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_SpeedField";
        private static string NameClipField(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_ClipField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, Label fakeLabel, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            VisualElement root = new VisualElement();

            DropdownField dropdownField = new DropdownField(new string(' ', property.displayName.Length))
            {
                style =
                {
                    flexGrow = 1,
                },
                userData = metaInfo,
                name = NameDropdownField(property),
            };
            // dropdownField.AddToClassList(ClassDropdownField(property));
            // dropdownField.choices = metaInfo.AnimatorStates.Select(each => each.ToString()).ToList();
            SetDropdownNoNotice(property, dropdownField, metaInfo);

            root.Add(dropdownField);

            SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
            // ReSharper disable once InvertIf
            if(curStateSpeedProp != null)
            {
                SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
                // using (new EditorGUI.IndentLevelScope(1))
                FloatField speedField = new FloatField($"┣{curStateSpeedProp.displayName}")
                {
                    value = curStateSpeedProp.floatValue,
                    name = NameSpeedField(property),
                };
                speedField.SetEnabled(false);
                root.Add(speedField);
                ObjectField animationClipField = new ObjectField($"┗{curAnimationClipProp.displayName}")
                {
                    value = curAnimationClipProp.objectReferenceValue,
                    name = NameClipField(property),
                };
                animationClipField.SetEnabled(false);
                root.Add(animationClipField);
            }

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
            dropdownField.RegisterValueChangedCallback(v =>
            {
                MetaInfo curMetaInfo = (MetaInfo) ((DropdownField) v.target).userData;
                AnimatorState selectedState = curMetaInfo.AnimatorStates[dropdownField.index];
                SetPropValue(property, selectedState);
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(selectedState);

                SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
                if (curStateSpeedProp == null)
                {
                    return;
                }

                SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
                container.Q<FloatField>(NameSpeedField(property)).value = curStateSpeedProp.floatValue;
                container.Q<ObjectField>(NameClipField(property)).value = curAnimationClipProp.objectReferenceValue;
            });
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));

            MetaInfo curMetaInfo = (MetaInfo) dropdownField.userData;
            dropdownField.userData = metaInfo;

            // Debug.Log($"AnimatorStateAttributeDrawer: {newAnimatorStates}");
            if (curMetaInfo.Error != metaInfo.Error)
            {
                // _helpBoxElement.visible = _errorMsg != "";
                HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
                helpBoxElement.style.display = _errorMsg == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBoxElement.text = metaInfo.Error;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_STATE_DRAW_PROCESS
                Debug.Log(metaInfo.Error);
#endif
            }

            if(!curMetaInfo.AnimatorStates.SequenceEqual(metaInfo.AnimatorStates))
            {
                SetDropdownNoNotice(property, dropdownField, metaInfo);
            }
        }

        private static void SetDropdownNoNotice(SerializedProperty property, DropdownField dropdownField, MetaInfo metaInfo)
        {
            dropdownField.choices = metaInfo.AnimatorStates.Select(each => each.ToString()).ToList();
            int curSelect;
            SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
            if (curStateSpeedProp == null)
            {
                curSelect = Util.ListIndexOfAction(metaInfo.AnimatorStates, each => each.stateName == property.stringValue);
            }
            else
            {
                curSelect = Util.ListIndexOfAction(metaInfo.AnimatorStates, each =>
                    each.stateNameHash == property.FindPropertyRelative("stateNameHash").intValue
                    && each.layerIndex == property.FindPropertyRelative("layerIndex").intValue
                );
            }

            // _dropdownField.index = curSelect;
            if(curSelect >= 0)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_STATE_DRAW_PROCESS
                    Debug.Log($"AnimatorStateAttributeDrawer: set to = {_dropdownField.choices[curSelect]}");
#endif
                dropdownField.SetValueWithoutNotify(metaInfo.AnimatorStates[curSelect].ToString());
                // _dropdownField.text = _dropdownField.choices[curSelect];
            }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_STATE_DRAW_PROCESS
                Debug.Log($"AnimatorStateAttributeDrawer: options={string.Join(",", _dropdownField.choices)}");
#endif
            // return curSelect;
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull)
        {
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
            dropdownField.label = labelOrNull;
        }

        #endregion

#endif
    }
}
