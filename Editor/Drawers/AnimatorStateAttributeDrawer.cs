using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
            public IReadOnlyList<AnimatorState> AnimatorStates;
            public string Error;
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorStyles.popup.CalcHeight(new GUIContent("M"), EditorGUIUtility.currentViewWidth);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            float errorHeight = _errorMsg == "" ? 0 : ImGuiHelpBox.GetHeight(_errorMsg, width, MessageType.Error);
            float subRowHeight = EditorGUIUtility.singleLineHeight * (property.propertyType == SerializedPropertyType.String ? 0 : 2);
            return errorHeight + subRowHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute);

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

        // private DropdownField _dropdownField;
        // private FloatField _speedField;
        // private ObjectField _animationClipField;
        // private UnityEngine.UIElements.HelpBox _helpBoxElement;
        private static string ClassDropdownField(SerializedProperty property) => $"{property.propertyPath}__DropdownField";
        private static string ClassSpeedField(SerializedProperty property) => $"{property.propertyPath}__SpeedField";
        private static string ClassClipField(SerializedProperty property) => $"{property.propertyPath}__ClipField";
        private static string ClassHelpBox(SerializedProperty property) => $"{property.propertyPath}__HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, object parent, Action<object> onChange)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute);

            VisualElement container = new VisualElement();

            DropdownField dropdownField = new DropdownField(property.displayName)
            {
                style =
                {
                    flexGrow = 1,
                },
                userData = metaInfo,
            };
            dropdownField.AddToClassList(ClassDropdownField(property));
            // dropdownField.choices = metaInfo.AnimatorStates.Select(each => each.ToString()).ToList();
            SetDropdownNoNotice(property, dropdownField, metaInfo);
            dropdownField.RegisterValueChangedCallback(v =>
            {
                MetaInfo curMetaInfo = (MetaInfo) ((DropdownField) v.target).userData;
                AnimatorState selectedState = curMetaInfo.AnimatorStates[dropdownField.index];
                SetPropValue(property, selectedState);
                property.serializedObject.ApplyModifiedProperties();
                onChange?.Invoke(selectedState);

                SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
                if (curStateSpeedProp == null)
                {
                    return;
                }

                SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
                container.Query<FloatField>(className: ClassSpeedField(property)).First().value = curStateSpeedProp.floatValue;
                container.Query<ObjectField>(className: ClassClipField(property)).First().value = curAnimationClipProp.objectReferenceValue;
            });
            container.Add(dropdownField);

            SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
            // ReSharper disable once InvertIf
            if(curStateSpeedProp != null)
            {
                SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
                // using (new EditorGUI.IndentLevelScope(1))
                FloatField speedField = new FloatField($"┣{curStateSpeedProp.displayName}")
                {
                    value = curStateSpeedProp.floatValue,
                };
                speedField.AddToClassList(ClassSpeedField(property));
                speedField.SetEnabled(false);
                container.Add(speedField);
                ObjectField animationClipField = new ObjectField($"┗{curAnimationClipProp.displayName}")
                {
                    value = curAnimationClipProp.objectReferenceValue,
                };
                animationClipField.AddToClassList(ClassClipField(property));
                animationClipField.SetEnabled(false);
                container.Add(animationClipField);
            }

            return container;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBoxElement.AddToClassList(ClassHelpBox(property));
            return helpBoxElement;
        }

        // private string _preError = "";
        // private List<AnimatorState> _preAnimatorStates = new List<AnimatorState>();
        // private List<AnimatorState> _curAnimatorStates = new List<AnimatorState>();

        protected override void OnUpdateUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement containerElement, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute);
            DropdownField dropdownField = containerElement.Query<DropdownField>(className: ClassDropdownField(property))
                .First();

            MetaInfo curMetaInfo = (MetaInfo) dropdownField.userData;
            dropdownField.userData = metaInfo;

            // Debug.Log($"AnimatorStateAttributeDrawer: {newAnimatorStates}");
            if (curMetaInfo.Error != metaInfo.Error)
            {
                // _helpBoxElement.visible = _errorMsg != "";
                HelpBox helpBoxElement = containerElement.Query<HelpBox>(className: ClassHelpBox(property)).First();
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

        private void SetDropdownNoNotice(SerializedProperty property, DropdownField dropdownField, MetaInfo metaInfo)
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

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            AnimatorStateAttribute animatorStateAttribute = (AnimatorStateAttribute) saintsAttribute;
            string animFieldName = animatorStateAttribute.AnimFieldName;

            Animator animator;
            if (animFieldName == null)
            {
                Object targetObj = property.serializedObject.targetObject;
                // animatorController = (Animator)animProp.objectReferenceValue;
                switch (targetObj)
                {
                    case GameObject go:
                        animator = go.GetComponent<Animator>();
                        break;
                    case Component component:
                        animator = component.GetComponent<Animator>();
                        break;
                    default:
                        string errorMsg = $"Animator controller not found in {targetObj}. Try specific a name instead.";
                        return new MetaInfo
                        {
                            Error = errorMsg,
                            AnimatorStates = Array.Empty<AnimatorState>(),
                        };
                }
            }
            else
            {
                SerializedObject targetSer = property.serializedObject;
                SerializedProperty animProp = targetSer.FindProperty(animFieldName) ??
                                              SerializedUtils.FindPropertyByAutoPropertyName(targetSer, animFieldName);

                // Debug.Log(property.type);
                if (animProp == null)
                {
                    string errorMsg = $"Can't find Animator {animFieldName}";
                    return new MetaInfo
                    {
                        Error = errorMsg,
                        AnimatorStates = Array.Empty<AnimatorState>(),
                    };
                }
                animator = (Animator)animProp.objectReferenceValue;
            }


            if (!animator)
            {
                string errorMsg = $"Animator {animFieldName} is null";
                // Debug.Log(_errorMsg);
                return new MetaInfo
                {
                    Error = errorMsg,
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return property.propertyType != SerializedPropertyType.String;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute)
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

        // private static void RenderSubRow(Rect position, SerializedProperty property)
        // {
        //     SerializedProperty curStateSpeedProp = property.FindPropertyRelative("stateSpeed");
        //     SerializedProperty curAnimationClipProp = property.FindPropertyRelative("animationClip");
        //     // using (new EditorGUI.IndentLevelScope(1))
        //     using (new EditorGUI.DisabledScope(true))
        //     {
        //         (Rect speedRect, Rect speedLeftRect) =
        //             RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
        //         EditorGUI.PropertyField(speedRect, curStateSpeedProp, new GUIContent( $"┣{curStateSpeedProp.displayName}"));
        //
        //         (Rect clipRect, Rect leftRect) =
        //             RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
        //         EditorGUI.PropertyField(clipRect, curAnimationClipProp, new GUIContent( $"┗{curAnimationClipProp.displayName}"));
        //     }
        // }

        // private static string PropertyToString(SerializedProperty property) =>
        //     new AnimState
        //     {
        //         layerIndex = property.FindPropertyRelative("layerIndex").intValue,
        //         stateName = property.FindPropertyRelative("stateName").stringValue,
        //         stateNameHash = property.FindPropertyRelative("stateNameHash").intValue,
        //         animationClip = (AnimationClip) property.FindPropertyRelative("animationClip").objectReferenceValue,
        //         stateSpeed = property.FindPropertyRelative("stateSpeed").floatValue,
        //     }.ToString();
    }
}
