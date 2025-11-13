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

        private AnimatorState GetStateFromProp(SerializedProperty property)
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
            }



            // UIToolkitUtils.DropdownButtonField dropdownButton =
            //     container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
            //
            // UIToolkitUtils.AddContextualMenuManipulator(dropdownButton, property,
            //     () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            //
            // Foldout foldout = container.Q<Foldout>(NameFoldout(property));
            // UIToolkitUtils.AddContextualMenuManipulator(foldout, property,
            //     () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            //
            // dropdownButton.ButtonElement.clicked += () =>
            //     ShowDropdown(property, saintsAttribute, container, info, parent, onValueChangedCallback);
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
            //
            // if (curIndex != -1)
            // {
            //     if (SetPropValue(property, metaInfo.AnimatorStates[curIndex]))
            //     {
            //         onValueChangedCallback.Invoke(property.propertyType == SerializedPropertyType.String
            //             ? metaInfo.AnimatorStates[curIndex].state.name
            //             : metaInfo.AnimatorStates[curIndex]);
            //     }
            // }
        }

        // private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     VisualElement container, FieldInfo info, object parent, Action<object> onChange)
        // {
        //     string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
        //         ? animatorStateAttribute.AnimFieldName
        //         : null;
        //
        //     MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);
        //
        //     GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
        //
        //     int selectedIndex = property.propertyType == SerializedPropertyType.String
        //         ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
        //             eachInfo => eachInfo.state.name == property.stringValue)
        //         : Util.ListIndexOfAction(metaInfo.AnimatorStates,
        //             eachStateInfo => EqualAnimatorState(eachStateInfo, property));
        //     // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
        //     UIToolkitUtils.DropdownButtonField buttonLabel =
        //         container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
        //     // foreach (int index in Enumerable.Range(0, metaInfo.AnimatorStates.Count))
        //     foreach ((AnimatorStateChanged value, int index) in metaInfo.AnimatorStates.WithIndex())
        //     {
        //         AnimatorStateChanged curItem = value;
        //         string curName = FormatStateLabel(curItem, "/");
        //
        //         genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
        //         {
        //             SetPropValue(property, curItem);
        //             // Util.SignFieldValue(property.serializedObject.targetObject, curItem, parent, info);
        //             // Util.SignPropertyValue(property, curItem);
        //             property.serializedObject.ApplyModifiedProperties();
        //             // Debug.Log($"onChange {curItem}");
        //             onChange(property.propertyType == SerializedPropertyType.String ? curItem.state.name : curItem);
        //             buttonLabel.ButtonLabelElement.text = curName;
        //             // property.serializedObject.ApplyModifiedProperties();
        //         });
        //     }
        //
        //     if (metaInfo.RuntimeAnimatorController != null)
        //     {
        //         if (metaInfo.AnimatorStates.Count > 0)
        //         {
        //             genericDropdownMenu.AddSeparator("");
        //         }
        //
        //         genericDropdownMenu.AddItem($"Edit {metaInfo.RuntimeAnimatorController.name}...", false,
        //             () => OpenAnimator(metaInfo.RuntimeAnimatorController));
        //     }
        //
        //     UIToolkitUtils.DropdownButtonField root =
        //         container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
        //
        //     genericDropdownMenu.DropDown(root.ButtonElement.worldBound, root, true);
        // }

        // protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes,
        //     VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        // {
        //     object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
        //
        //     string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
        //         ? animatorStateAttribute.AnimFieldName
        //         : null;
        //
        //     MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);
        //
        //     HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
        //
        //     // ReSharper disable once InvertIf
        //     if (metaInfo.Error != helpBox.text)
        //     {
        //         helpBox.text = metaInfo.Error;
        //         helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
        //     }
        //
        //     TextField subStateMachineNameChainTextField =
        //         container.Q<TextField>(name: NameSubStateMachineNameChain(property));
        //     // ReSharper disable once InvertIf
        //     if (subStateMachineNameChainTextField != null)
        //     {
        //         SerializedProperty chain = property.FindPropertyRelative("subStateMachineNameChain");
        //         // ReSharper disable once InvertIf
        //         if (chain != null)
        //         {
        //             string chainText = string.Join(" > ", Enumerable
        //                     .Range(0, chain.arraySize)
        //                     .Select(each => chain.GetArrayElementAtIndex(each).stringValue));
        //             if (subStateMachineNameChainTextField.value != chainText)
        //             {
        //                 subStateMachineNameChainTextField.value = chainText;
        //             }
        //         }
        //     }
        //
        //     int curIndex = property.propertyType == SerializedPropertyType.String
        //         ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
        //             eachInfo => eachInfo.state.name == property.stringValue)
        //         : Util.ListIndexOfAction(metaInfo.AnimatorStates,
        //             eachStateInfo => EqualAnimatorState(eachStateInfo, property));
        //     string buttonLabel = curIndex == -1 ? "-" : FormatStateLabel(metaInfo.AnimatorStates[curIndex], "/");
        //     UIToolkitUtils.DropdownButtonField dropdownButton =
        //         container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
        //     if (dropdownButton.ButtonLabelElement.text != buttonLabel)
        //     {
        //         dropdownButton.ButtonLabelElement.text = buttonLabel;
        //     }
        // }
        //
        // protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container,
        //     FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        // {
        //     TextField subStateMachineNameChainTextField =
        //         container.Q<TextField>(name: NameSubStateMachineNameChain(property));
        //     // ReSharper disable once InvertIf
        //     if (subStateMachineNameChainTextField != null)
        //     {
        //         AnimatorStateChanged subs = (AnimatorStateChanged)newValue;
        //         subStateMachineNameChainTextField.value = subs.subStateMachineNameChain.Count == 0
        //             ? ""
        //             : string.Join(" > ", subs.subStateMachineNameChain);
        //     }
        // }
    }
}
#endif
