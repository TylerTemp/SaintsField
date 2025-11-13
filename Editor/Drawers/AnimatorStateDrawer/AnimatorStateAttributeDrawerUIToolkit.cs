#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorStateDrawer
{
    public partial class AnimatorStateAttributeDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static string NameDropdownButton(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorState_DropdownButton";

        private static string NameProperties(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorState_Properties";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorState_HelpBox";

        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorState_Foldout";

        private static string NameSubStateMachineNameChain(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorState_SubStateMachineNameChain";

        // protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, object parent)
        // {
        //     if (property.propertyType == SerializedPropertyType.String)
        //     {
        //         return null;
        //     }
        //
        //     Foldout foldOut = new Foldout
        //     {
        //         style =
        //         {
        //             // backgroundColor = Color.green,
        //             // left = -5,
        //             position = Position.Absolute,
        //             width = LabelBaseWidth - IndentWidth,
        //         },
        //         name = NameFoldout(property),
        //         value = false,
        //     };
        //     Toggle toggle = foldOut.Q<Toggle>();
        //     if (toggle != null)
        //     {
        //         toggle.userData = container;
        //     }
        //
        //     foldOut.RegisterValueChangedCallback(v =>
        //     {
        //         container.Q<VisualElement>(NameProperties(property)).style.display =
        //             v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        //     });
        //
        //     return foldOut;
        // }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    AnimatorStateElementString element = new AnimatorStateElementString
                    {
                        bindingPath = property.propertyPath,
                    };
                    AnimatorStateFieldString field = new AnimatorStateFieldString(GetPreferredLabel(property), element);
                    field.AddToClassList(ClassAllowDisable);
                    field.AddToClassList(AnimatorStateFieldString.alignedFieldUssClassName);
                    return field;
                }
                case SerializedPropertyType.Generic:
                {
                    AnimatorStateElementStruct element = new AnimatorStateElementStruct
                    {
                        bindingPath = property.propertyPath,
                    };
                    AnimatorStateFieldStruct field = new AnimatorStateFieldStruct(GetPreferredLabel(property), element);
                    field.AddToClassList(ClassAllowDisable);
                    field.AddToClassList(AnimatorStateFieldString.alignedFieldUssClassName);
                    return field;
                }
                default:
                {
                    PropertyField fallback = PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
                    fallback.AddToClassList(ClassFieldUIToolkit(property));
                    fallback.AddToClassList(ClassAllowDisable);
                    return fallback;
                }
            }

            // VisualElement root = new VisualElement
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexShrink = 1,
            //     },
            // };
            //
            // string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
            //     ? animatorStateAttribute.AnimFieldName
            //     : null;
            //
            // MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);
            // int curIndex = property.propertyType == SerializedPropertyType.String
            //     ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
            //         eachInfo => eachInfo.state.name == property.stringValue)
            //     : Util.ListIndexOfAction(metaInfo.AnimatorStates,
            //         eachStateInfo => EqualAnimatorState(eachStateInfo, property));
            // string buttonLabel = curIndex == -1 ? "-" : FormatStateLabel(metaInfo.AnimatorStates[curIndex], "/");
            //
            // UIToolkitUtils.DropdownButtonField dropdownButton =
            //     UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            // dropdownButton.name = NameDropdownButton(property);
            // dropdownButton.userData = metaInfo;
            // dropdownButton.ButtonLabelElement.text = buttonLabel;
            //
            // root.Add(dropdownButton);
            //
            // VisualElement properties = new VisualElement
            // {
            //     style =
            //     {
            //         display = DisplayStyle.None,
            //         backgroundColor = EColor.CharcoalGray.GetColor(),
            //         paddingLeft = IndentWidth,
            //     },
            //     name = NameProperties(property),
            // };
            //
            // foreach (SerializedProperty serializedProperty in new[]
            //              {
            //                  "layerIndex",
            //                  "stateName",
            //                  "stateNameHash",
            //                  "stateSpeed",
            //                  "stateTag",
            //                  "animationClip",
            //                  // "subStateMachineNameChain",
            //              }
            //              .Select(each => FindPropertyRelative(property, each))
            //              .Where(each => each != null))
            // {
            //     PropertyField subField = new PropertyField(serializedProperty,
            //         ObjectNames.NicifyVariableName(serializedProperty.displayName));
            //     subField.SetEnabled(false);
            //     properties.Add(subField);
            // }
            //
            // SerializedProperty subStateMachineNameChainProp =
            //     FindPropertyRelative(property, "subStateMachineNameChain");
            // if (subStateMachineNameChainProp != null)
            // {
            //     TextField textField = new TextField(ObjectNames.NicifyVariableName("subStateMachineNameChain"))
            //     {
            //         value = subStateMachineNameChainProp.arraySize == 0
            //             ? ""
            //             : string.Join(" > ", Enumerable
            //                 .Range(0, subStateMachineNameChainProp.arraySize)
            //                 .Select(each => subStateMachineNameChainProp.GetArrayElementAtIndex(each).stringValue)
            //             ),
            //         name = NameSubStateMachineNameChain(property),
            //         isReadOnly = true,
            //     };
            //     textField.SetEnabled(false);
            //     textField.AddToClassList(BaseField<object>.alignedFieldUssClassName);
            //     properties.Add(textField);
            // }
            //
            // root.Add(properties);
            //
            // root.AddToClassList(ClassAllowDisable);
            //
            // EmptyPrefabOverrideElement emptyPrefabOverrideElement =
            //     new EmptyPrefabOverrideElement(property);
            // emptyPrefabOverrideElement.Add(root);
            // return emptyPrefabOverrideElement;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            AnimatorStateDetailPanel detailPanel = new AnimatorStateDetailPanel
            {
                bindingPath = property.propertyPath,
            };
            root.Add(detailPanel);
            detailPanel.AddToClassList(ClassAllowDisable);

            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameHelpBox(property),
            };
            root.Add(helpBoxElement);
            helpBoxElement.AddToClassList(ClassAllowDisable);

            return root;
        }

        private static AnimatorState GetStateFromProp(SerializedProperty property)
        {
            SerializedProperty curLayerIndexProp = FindPropertyRelative(property, "layerIndex");
            SerializedProperty curStateNameHashProp = FindPropertyRelative(property, "stateNameHash");
            SerializedProperty curStateNameProp = FindPropertyRelative(property, "stateName");
            SerializedProperty curStateSpeedProp = FindPropertyRelative(property, "stateSpeed");
            SerializedProperty curTagProp = FindPropertyRelative(property, "stateTag");
            SerializedProperty curAnimationClipProp = FindPropertyRelative(property, "animationClip");
            SerializedProperty curSubStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");
            return new AnimatorState(
                layerIndex: curLayerIndexProp?.intValue ?? 0,
                stateNameHash: curStateNameHashProp?.intValue ?? 0,
                stateName: curStateNameProp?.stringValue ?? "",
                stateSpeed: curStateSpeedProp?.floatValue ?? 0f,
                stateTag: curTagProp?.stringValue ?? "",
                animationClip: curAnimationClipProp?.objectReferenceValue as AnimationClip,
                subStateMachineNameChain: curSubStateMachineNameChainProp == null
                    ? Array.Empty<string>()
                    : Enumerable.Range(0, curSubStateMachineNameChainProp.arraySize).Select(index =>
                        curSubStateMachineNameChainProp.GetArrayElementAtIndex(index).stringValue ?? "").ToArray()
            );
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AnimatorStateAttribute animatorStateAttribute =
                saintsAttribute as AnimatorStateAttribute ?? new AnimatorStateAttribute();
            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    AnimatorStateFieldString field = container.Q<AnimatorStateFieldString>();
                    UIToolkitUtils.AddContextualMenuManipulator(field, property,
                        () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

                    void Refresh()
                    {
                        MetaInfo metaInfo = GetMetaInfo(property, animatorStateAttribute.AnimFieldName, info, parent);
                        if (metaInfo.Error == "")
                        {
                            field.AnimatorStateElementString.BindAnimatorInfo(metaInfo.AnimatorStates, metaInfo.RuntimeAnimatorController);
                        }
                        UIToolkitUtils.SetHelpBox(helpBox, metaInfo.Error);
                    }

                    Refresh();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(Refresh);
                    field.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(Refresh));
                    field.TrackSerializedObjectValue(property.serializedObject, _ => Refresh());
                }
                    break;
                case SerializedPropertyType.Generic:
                {
                    SerializedProperty curLayerIndexProp = FindPropertyRelative(property, "layerIndex");
                    SerializedProperty curStateNameHashProp = FindPropertyRelative(property, "stateNameHash");
                    SerializedProperty curStateNameProp = FindPropertyRelative(property, "stateName");
                    SerializedProperty curStateSpeedProp = FindPropertyRelative(property, "stateSpeed");
                    SerializedProperty curStateTagProp = FindPropertyRelative(property, "stateTag");
                    SerializedProperty curAnimationClipProp = FindPropertyRelative(property, "animationClip");
                    SerializedProperty curSubStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");

                    AnimatorStateDetailPanel detailPanel = container.Q<AnimatorStateDetailPanel>();

                    AnimatorStateFieldStruct field = container.Q<AnimatorStateFieldStruct>();
                    UIToolkitUtils.AddContextualMenuManipulator(field, property,
                        () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

                    void Refresh()
                    {
                        MetaInfo metaInfo = GetMetaInfo(property, animatorStateAttribute.AnimFieldName, info, parent);
                        if (metaInfo.Error == "")
                        {
                            field.AnimatorStateElementStruct.BindAnimatorInfo(metaInfo.AnimatorStates, metaInfo.RuntimeAnimatorController);
                        }
                        UIToolkitUtils.SetHelpBox(helpBox, metaInfo.Error);
                    }

                    Refresh();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(Refresh);
                    field.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(Refresh));
                    field.TrackSerializedObjectValue(property.serializedObject, _ => Refresh());
                    detailPanel.BindStructProperty(property);
                    field.AnimatorStateElementStruct.BindDetailPanel(detailPanel);
                    field.AnimatorStateElementStruct.ExpandButton.SetCustomViewDataKey(property.propertyPath);

                    field.AnimatorStateElementStruct.value = GetStateFromProp(property);

                    field.AnimatorStateElementStruct.RegisterValueChangedCallback(evt =>
                    {
                        AnimatorState newValue = evt.newValue;
                        bool changed = false;

                        if (curLayerIndexProp != null && curLayerIndexProp.intValue != newValue.layerIndex)
                        {
                            changed = true;
                            curLayerIndexProp.intValue = newValue.layerIndex;
                        }

                        if (curStateNameHashProp != null && curStateNameHashProp.intValue != newValue.stateNameHash)
                        {
                            changed = true;
                            curStateNameHashProp.intValue = newValue.stateNameHash;
                        }

                        if (curStateNameProp != null && curStateNameProp.stringValue != newValue.stateName)
                        {
                            changed = true;
                            curStateNameProp.stringValue = newValue.stateName ?? "";
                        }

                        if (curStateSpeedProp != null &&
                            Math.Abs(curStateSpeedProp.floatValue - newValue.stateSpeed) > 1e-6f)
                        {
                            changed = true;
                            curStateSpeedProp.floatValue = newValue.stateSpeed;
                        }

                        if (curStateTagProp != null && curStateTagProp.stringValue != newValue.stateTag)
                        {
                            changed = true;
                            curStateTagProp.stringValue = newValue.stateTag ?? "";
                        }

                        if (curAnimationClipProp != null &&
                            (curAnimationClipProp.objectReferenceValue as AnimationClip) != newValue.animationClip)
                        {
                            changed = true;
                            curAnimationClipProp.objectReferenceValue = newValue.animationClip;
                        }

                        if (curSubStateMachineNameChainProp != null)
                        {
                            string[] chain = newValue.subStateMachineNameChain ?? Array.Empty<string>();
                            if (curSubStateMachineNameChainProp.arraySize != chain.Length)
                            {
                                curSubStateMachineNameChainProp.arraySize = chain.Length;
                                changed = true;
                            }

                            for (int i = 0; i < chain.Length; i++)
                            {
                                SerializedProperty elem = curSubStateMachineNameChainProp.GetArrayElementAtIndex(i);
                                if (elem.stringValue != chain[i])
                                {
                                    elem.stringValue = chain[i] ?? "";
                                    changed = true;
                                }
                            }
                        }

                        if (changed)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(newValue);
                        }

                    });
                }
                    break;
                default:
                    UIToolkitUtils.SetHelpBox(helpBox, $"Expect string/AnimatorState, get {property.propertyType}");
                    break;
            }
        }

        private class AnimatorStateStringHelpBox : VisualElement
        {
            public readonly AnimatorStateFieldString Field;
            public readonly HelpBox HelpBox;

            public AnimatorStateStringHelpBox(AnimatorStateFieldString field)
            {
                Add(Field = field);
                Add(HelpBox = new HelpBox
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                        display = DisplayStyle.None,
                    },
                });
            }
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, AnimatorStateAttribute animatorStateAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            MetaInfo metaInfo = GetMetaInfoShowInInspector(
                animatorStateAttribute,
                targets[0]);

            if (oldElement is AnimatorStateStringHelpBox oldContainer)
            {
                oldContainer.Field.SetValueWithoutNotify(value);
                if (metaInfo.Error == "")
                {
                    oldContainer.Field.AnimatorStateElementString.BindAnimatorInfo(metaInfo.AnimatorStates, metaInfo.RuntimeAnimatorController);
                }
                UIToolkitUtils.SetHelpBox(oldContainer.HelpBox, metaInfo.Error);
                return null;
            }

            AnimatorStateElementString visualInput = new AnimatorStateElementString
            {
                value = value,
            };

            if (metaInfo.Error == "")
            {
                visualInput.BindAnimatorInfo(metaInfo.AnimatorStates, metaInfo.RuntimeAnimatorController);
            }
            AnimatorStateFieldString field =
                new AnimatorStateFieldString(label, visualInput)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                visualInput.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return new AnimatorStateStringHelpBox(field);
        }

        private class AnimatorStateStructCombo : VisualElement
        {
            public readonly AnimatorStateFieldStruct Field;
            public readonly AnimatorStateDetailPanel DetailPanel;
            public readonly HelpBox HelpBox;

            public AnimatorStateStructCombo(AnimatorStateFieldStruct field)
            {
                Add(Field = field);
                Add(DetailPanel = new AnimatorStateDetailPanel());
                Add(HelpBox = new HelpBox
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                        display = DisplayStyle.None,
                    },
                });
            }
        }

        public static VisualElement UIToolkitValueEditAnimatorState(VisualElement oldElement, AnimatorStateAttribute animatorStateAttribute, string label, AnimatorState value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            MetaInfo metaInfo = GetMetaInfoShowInInspector(
                animatorStateAttribute,
                targets[0]);

            if (oldElement is AnimatorStateStructCombo oldContainer)
            {
                oldContainer.Field.SetValueWithoutNotify(value);
                if (metaInfo.Error == "")
                {
                    oldContainer.Field.AnimatorStateElementStruct.BindAnimatorInfo(metaInfo.AnimatorStates, metaInfo.RuntimeAnimatorController);
                }
                oldContainer.DetailPanel.SetValueWithoutNotify(value);
                UIToolkitUtils.SetHelpBox(oldContainer.HelpBox, metaInfo.Error);
                return null;
            }

            AnimatorStateElementStruct visualInput = new AnimatorStateElementStruct
            {
                value = value,
            };

            if (metaInfo.Error == "")
            {
                visualInput.BindAnimatorInfo(metaInfo.AnimatorStates, metaInfo.RuntimeAnimatorController);
            }
            AnimatorStateFieldStruct field =
                new AnimatorStateFieldStruct(label, visualInput)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                visualInput.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }

            AnimatorStateStructCombo combo = new AnimatorStateStructCombo(field);
            combo.Field.AnimatorStateElementStruct.BindDetailPanel(combo.DetailPanel);
            combo.DetailPanel.UpdateStruct(value);
            return combo;
        }

        public static VisualElement UIToolkitValueEditAnimatorStateBase(VisualElement oldElement, AnimatorStateAttribute animatorStateAttribute, string label, AnimatorStateBase value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            AnimatorState animatorState = new AnimatorState(
                value.layerIndex,
                value.stateNameHash,
                value.stateName ?? "",
                value.stateSpeed,
                value.stateTag ?? "",
                null,
                value.subStateMachineNameChain ?? Array.Empty<string>()
            );
            Action<object> wrapSetter = null;
            if (setterOrNull != null)
            {
                wrapSetter = (obj) =>
                {
                    setterOrNull.Invoke(GetBaseFromAnimatorState((AnimatorState)obj));
                };
            }

            return UIToolkitValueEditAnimatorState(oldElement, animatorStateAttribute, label, animatorState, beforeSet,
                wrapSetter, labelGrayColor, inHorizontalLayout, allAttributes, targets);
        }

        private static AnimatorStateBase GetBaseFromAnimatorState(AnimatorState animatorState) =>
            new AnimatorStateBase(
                    animatorState.layerIndex,
                    animatorState.stateNameHash,
                    animatorState.stateName ?? "",
                    animatorState.stateSpeed,
                    animatorState.stateTag ?? "",
                    animatorState.subStateMachineNameChain ?? Array.Empty<string>()
                );
    }
}
#endif
