using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
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
            public Animator Animator;
            public IReadOnlyList<AnimatorStateChanged> AnimatorStates;
            public string Error;
            // ReSharper enable InconsistentNaming
        }

        #region IMGUI
        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if(property.propertyType == SerializedPropertyType.String)
            {
                return -1;
            }
            bool curExpanded = property.isExpanded;
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                using(new GUIEnabledScoop(true))
                {
                    bool newExpanded = EditorGUI.Foldout(position, curExpanded,
                        new GUIContent(new string(' ', property.displayName.Length)), true);
                    if (changed.changed)
                    {
                        property.isExpanded = newExpanded;
                    }
                }
            }

            return 13;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorStyles.popup.CalcHeight(new GUIContent("M"), EditorGUIUtility.currentViewWidth);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            _errorMsg = metaInfo.Error;

            if (_errorMsg != "")
            {
                RenderErrorFallback(position, label, property);
                return;
            }

            GUIContent[] optionContents = metaInfo.AnimatorStates.Select(each => new GUIContent(FormatStateLabel(each, " > "))).ToArray();

            int curIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates, eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates, eachStateInfo => EqualAnimatorState(eachStateInfo, property));

            // Debug.Log($"curIndex={curIndex}");

            if (!_onEnableChecked)  // check whether external source changed, to avoid caching an old value
            {
                _onEnableChecked = true;
                if(curIndex != -1 && property.propertyType != SerializedPropertyType.String)
                {
                    // if some attribute changed, we need to update them
                    // var curSelected = metaInfo.AnimatorStates[curIndex];
                    if (SetPropValue(property, metaInfo.AnimatorStates[curIndex]))
                    {
                        // Debug.Log($"IMGUI init changed");
                        // ReSharper disable once RedundantCast
                        onGUIPayload.SetValue(property.propertyType == SerializedPropertyType.String? (object)metaInfo.AnimatorStates[curIndex].state.name : metaInfo.AnimatorStates[curIndex]);
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            // (Rect popupRect, Rect popupLeftRect) = RectUtils.SplitHeightRect(position, GetLabelFieldHeight(property, label, saintsAttribute));
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope popupChanged = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(
                    position,
                    label,
                    curIndex,
                    optionContents.Concat(new[]
                    {
                        GUIContent.none,
                        new GUIContent($"Edit {metaInfo.Animator.runtimeAnimatorController.name}..."),
                    }).ToArray(),
                    EditorStyles.popup);

                // ReSharper disable once InvertIf
                if (popupChanged.changed)
                {
                    if (newIndex >= optionContents.Length)
                    {
                        // Selection.activeObject = metaInfo.Animator.runtimeAnimatorController;
                        // EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                        OpenAnimator(metaInfo.Animator.runtimeAnimatorController);
                    }
                    else
                    {
                        SetPropValue(property, metaInfo.AnimatorStates[newIndex]);
                        // ReSharper disable once RedundantCast
                        onGUIPayload.SetValue(property.propertyType == SerializedPropertyType.String? (object)metaInfo.AnimatorStates[newIndex].state.name : metaInfo.AnimatorStates[newIndex]);
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            // RenderSubRow(popupLeftRect, property);
        }

        private static bool EqualAnimatorState(AnimatorStateChanged eachStateInfo, SerializedProperty property)
        {
            bool layerIndexEqual = FindPropertyRelative(property, "layerIndex")?.intValue == eachStateInfo.layerIndex;
            bool stateNameEqual = FindPropertyRelative(property, "stateName")?.stringValue == eachStateInfo.state.name;
            bool stateNameHashEqual =
                FindPropertyRelative(property, "stateNameHash")?.intValue == eachStateInfo.state.nameHash;

            if (!layerIndexEqual || !stateNameEqual || !stateNameHashEqual)
            {
                return false;
            }

            SerializedProperty subStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");
            if (subStateMachineNameChainProp == null)
            {
                return true;
            }

            int arraySize = subStateMachineNameChainProp.arraySize;
            IReadOnlyList<string> eachChain = eachStateInfo.subStateMachineNameChain;
            if (arraySize != eachChain.Count)
            {
                return false;
            }
            for (int arrayIndex = 0; arrayIndex < arraySize; arrayIndex++)
            {
                string thisSubName = subStateMachineNameChainProp.GetArrayElementAtIndex(arrayIndex).stringValue;
                if (thisSubName != eachStateInfo.subStateMachineNameChain[arrayIndex])
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _errorMsg != "" || property.isExpanded;
        }
        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            float errorHeight = _errorMsg == "" ? 0 : ImGuiHelpBox.GetHeight(_errorMsg, width, MessageType.Error);

            if (!property.isExpanded)
            {
                return errorHeight;
            }

            int rowCount;
            if (property.propertyType == SerializedPropertyType.String)
            {
                rowCount = 0;
            }
            else
            {
                // must have: layerIndex; must one of: stateNameHash/stateName, + optionals
                rowCount = 1 + new[]
                {
                    "stateNameHash",
                    "stateName",
                    "stateSpeed",
                    "stateTag",
                    "animationClip",
                    "subStateMachineNameChain",
                }.Count(each => FindPropertyRelative(property, each) != null);
            }
            float subRowHeight = EditorGUIUtility.singleLineHeight * rowCount;
            return errorHeight + subRowHeight;
        }
        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index1, FieldInfo info, object parent)
        {
            // Debug.Log(_targetIsString);
            if(property.propertyType == SerializedPropertyType.String || !property.isExpanded)
            {
                return _errorMsg == ""? position: ImGuiHelpBox.Draw(position, _errorMsg, MessageType.Error);
            }

            IReadOnlyList<SerializedProperty> renders = new[]
                {
                    "layerIndex",
                    "stateName",
                    "stateNameHash",
                    "stateSpeed",
                    "stateTag",
                    "animationClip",
                    // "subStateMachineNameChain",
                }
                .Select(each => FindPropertyRelative(property, each))
                .Where(each => each != null)
                .ToArray();

            SerializedProperty subStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");

            // Rect leftRectForError = position;
            int willRenderCount = renders.Count + (subStateMachineNameChainProp == null ? 0 : 1);
            Rect willRenderRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight * willRenderCount,
            };

            EditorGUI.DrawRect(willRenderRect, EColor.EditorEmphasized.GetColor());

            using (new EditorGUI.DisabledScope(true))
            {
                Rect indentedRect = new Rect(position)
                {
                    x = position.x + IndentWidth,
                    width = position.width - IndentWidth,
                };

                foreach ((SerializedProperty prop, int index) in renders.WithIndex())
                {
                    // bool isLast = subStateMachineNameChainProp == null && index == renders.Count - 1;
                    EditorGUI.PropertyField(new Rect(indentedRect)
                    {
                        y = indentedRect.y + EditorGUIUtility.singleLineHeight * index,
                        height = EditorGUIUtility.singleLineHeight,
                    }, prop, new GUIContent(ObjectNames.NicifyVariableName(prop.displayName)));
                    // useRect.y += EditorGUIUtility.singleLineHeight;
                }

                if (subStateMachineNameChainProp != null)
                {
                    string subStateStr = subStateMachineNameChainProp.arraySize == 0
                        ? ""
                        : string.Join(" > ", Enumerable
                            .Range(0, subStateMachineNameChainProp.arraySize)
                            .Select(each => subStateMachineNameChainProp.GetArrayElementAtIndex(each).stringValue)
                        );

                    Rect subStateRect = new Rect(indentedRect)
                    {
                        y = indentedRect.y + EditorGUIUtility.singleLineHeight * renders.Count,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    EditorGUI.TextField(subStateRect, ObjectNames.NicifyVariableName("subStateMachineNameChain"),
                        subStateStr);
                }
            }

            Rect leftRectForError = new Rect(position)
            {
                y = position.y + EditorGUIUtility.singleLineHeight * (renders.Count + (subStateMachineNameChainProp == null? 0: 1)),
            };
            return _errorMsg == ""? leftRectForError: ImGuiHelpBox.Draw(leftRectForError, _errorMsg, MessageType.Error);
        }

        #endregion

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
                    AnimatorStates = Array.Empty<AnimatorStateChanged>(),
                };
            }

            AnimatorController controller = (AnimatorController)animator.runtimeAnimatorController;
            if (controller == null)
            {
                return new MetaInfo
                {
                    Error = $"No AnimatorController on {animator}",
                    AnimatorStates = Array.Empty<AnimatorStateChanged>(),
                };
            }

            List<AnimatorStateChanged> animatorStates = new List<AnimatorStateChanged>();
            foreach ((AnimatorControllerLayer animatorControllerLayer, int layerIndex) in controller.layers.Select((each, index) => (each, index)))
            {
                foreach ((UnityEditor.Animations.AnimatorState state, IReadOnlyList<string> subStateMachineNameChain) in GetAnimatorStateRecursively(animatorControllerLayer.stateMachine, animatorControllerLayer.stateMachine.stateMachines.Select(each => each.stateMachine), Array.Empty<string>()))
                {
                    animatorStates.Add(new AnimatorStateChanged
                    {
                        layer = animatorControllerLayer,
                        layerIndex = layerIndex,
                        state = state,

                        animationClip = (AnimationClip)state.motion,
                        subStateMachineNameChain = subStateMachineNameChain.ToArray(),
                    });
                }
            }

            return new MetaInfo
            {
                Animator = animator,
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

        private static string FormatStateLabel(AnimatorStateChanged animatorStateInfo, string sep) => $"{animatorStateInfo.state.name}{(animatorStateInfo.animationClip == null ? "" : $" ({animatorStateInfo.animationClip.name})")}: {animatorStateInfo.layer.name}{(animatorStateInfo.subStateMachineNameChain.Count == 0 ? "" : $"{sep}{string.Join(sep, animatorStateInfo.subStateMachineNameChain)}")}";

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameDropdownButton(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_DropdownButton";
        private static string NameProperties(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_Properties";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_HelpBox";
        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_Foldout";

        private static string NameSubStateMachineNameChain(SerializedProperty property) => $"{property.propertyPath}__AnimatorState_SubStateMachineNameChain";

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            if(property.propertyType == SerializedPropertyType.String)
            {
                return null;
            }

            Foldout foldOut = new Foldout
            {
                style =
                {
                    // backgroundColor = Color.green,
                    // left = -5,
                    position = Position.Absolute,
                    width = LabelBaseWidth - IndentWidth,
                },
                name = NameFoldout(property),
                value = false,
            };

            foldOut.RegisterValueChangedCallback(v =>
            {
                container.Q<VisualElement>(NameProperties(property)).style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return foldOut;
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            int curIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates, eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates, eachStateInfo => EqualAnimatorState(eachStateInfo, property));
            string buttonLabel = curIndex == -1? "-": FormatStateLabel(metaInfo.AnimatorStates[curIndex], "/");

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.name = NameDropdownButton(property);
            dropdownButton.userData = metaInfo;
            dropdownButton.ButtonLabelElement.text = buttonLabel;

            root.Add(dropdownButton);

            VisualElement properties = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                    backgroundColor = EColor.CharcoalGray.GetColor(),
                    paddingLeft = IndentWidth,
                },
                name = NameProperties(property),
            };

            foreach (SerializedProperty serializedProperty in new[]
                 {
                     "layerIndex",
                     "stateName",
                     "stateNameHash",
                     "stateSpeed",
                     "stateTag",
                     "animationClip",
                     // "subStateMachineNameChain",
                 }
                 .Select(each => FindPropertyRelative(property, each))
                 .Where(each => each != null))
            {
                PropertyField subField = new PropertyField(serializedProperty, ObjectNames.NicifyVariableName(serializedProperty.displayName));
                subField.SetEnabled(false);
                properties.Add(subField);
            }

            SerializedProperty subStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");
            if (subStateMachineNameChainProp != null)
            {
                TextField textField = new TextField(ObjectNames.NicifyVariableName("subStateMachineNameChain"))
                {
                    value = subStateMachineNameChainProp.arraySize == 0
                        ? ""
                        : string.Join(" > ", Enumerable
                            .Range(0, subStateMachineNameChainProp.arraySize)
                            .Select(each => subStateMachineNameChainProp.GetArrayElementAtIndex(each).stringValue)
                        ),
                    name = NameSubStateMachineNameChain(property),
                    isReadOnly = true,
                };
                textField.SetEnabled(false);
                textField.AddToClassList("unity-base-field__aligned");
                properties.Add(textField);
            }

            root.Add(properties);

            root.AddToClassList(ClassAllowDisable);

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
                    flexGrow = 1,
                    flexShrink = 0,
                },
                name = NameHelpBox(property),
            };

            helpBoxElement.AddToClassList(ClassAllowDisable);

            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
            dropdownButton.ButtonElement.clicked += () => ShowDropdown(property, saintsAttribute, container, info, parent, onValueChangedCallback);

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            int curIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates, eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates, eachStateInfo => EqualAnimatorState(eachStateInfo, property));

            if (curIndex != -1)
            {
                if (SetPropValue(property, metaInfo.AnimatorStates[curIndex]))
                {
                    onValueChangedCallback.Invoke(property.propertyType == SerializedPropertyType.String? metaInfo.AnimatorStates[curIndex].state.name : metaInfo.AnimatorStates[curIndex]);
                }
            }
        }

        private static void ShowDropdown(SerializedProperty property,ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent, Action<object> onChange)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

            int selectedIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates, eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates, eachStateInfo => EqualAnimatorState(eachStateInfo, property));
            // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
            UIToolkitUtils.DropdownButtonField buttonLabel = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
            // foreach (int index in Enumerable.Range(0, metaInfo.AnimatorStates.Count))
            foreach ((AnimatorStateChanged value, int index) in metaInfo.AnimatorStates.WithIndex())
            {
                AnimatorStateChanged curItem = value;
                string curName = FormatStateLabel(curItem, "/");

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {
                    SetPropValue(property, curItem);
                    // Util.SignFieldValue(property.serializedObject.targetObject, curItem, parent, info);
                    // Util.SignPropertyValue(property, curItem);
                    property.serializedObject.ApplyModifiedProperties();
                    // Debug.Log($"onChange {curItem}");
                    onChange(property.propertyType == SerializedPropertyType.String? curItem.state.name : curItem);
                    buttonLabel.ButtonLabelElement.text = curName;
                    // property.serializedObject.ApplyModifiedProperties();
                });
            }

            if(metaInfo.Animator != null)
            {
                if (metaInfo.AnimatorStates.Count > 0)
                {
                    genericDropdownMenu.AddSeparator("");
                }

                genericDropdownMenu.AddItem($"Edit {metaInfo.Animator.runtimeAnimatorController.name}...", false,
                    () => OpenAnimator(metaInfo.Animator.runtimeAnimatorController));
            }

            UIToolkitUtils.DropdownButtonField root = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));

            genericDropdownMenu.DropDown(root.ButtonElement.worldBound, root, true);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            // ReSharper disable once InvertIf
            if (metaInfo.Error != helpBox.text)
            {
                helpBox.text = metaInfo.Error;
                helpBox.style.display = metaInfo.Error == ""? DisplayStyle.None: DisplayStyle.Flex;
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            TextField subStateMachineNameChainTextField =
                container.Q<TextField>(name: NameSubStateMachineNameChain(property));
            // ReSharper disable once InvertIf
            if (subStateMachineNameChainTextField != null)
            {
                AnimatorStateChanged subs = (AnimatorStateChanged)newValue;
                subStateMachineNameChainTextField.value = subs.subStateMachineNameChain.Count == 0
                    ? ""
                    : string.Join(" > ", subs.subStateMachineNameChain);
            }
        }
        #endregion

#endif
    }
}
