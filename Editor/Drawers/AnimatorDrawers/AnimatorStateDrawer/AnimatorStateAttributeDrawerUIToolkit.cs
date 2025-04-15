#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Animate;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorStateDrawer
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

        private static readonly Type[] InterfaceTypes = {
            // typeof(IAnimationClip),
            typeof(ILayerIndex),
            typeof(IStateNameHash),
            typeof(IStateName),
            typeof(IStateSpeed),
            typeof(IStateTag),
            typeof(ISubStateMachineNameChain),
        };

        // [InitializeOnLoadMethod]
        // private static void AddSaintsPropertyInfoInjectAnimatorState()
        // {
        //     AddSaintsPropertyInfoInject((property, info, allAttributes) =>
        //     {
        //         if (allAttributes.Any(each => each is AnimatorStateAttribute))
        //         {
        //             return (null, null);
        //         }
        //
        //         if (property.propertyType != SerializedPropertyType.Generic)
        //         {
        //             return (null, null);
        //         }
        //         Type infoType = ReflectUtils.GetElementType(info.FieldType);
        //
        //
        //         if (!InterfaceTypes.All(interfaceType => interfaceType.IsAssignableFrom(infoType)))
        //         {
        //             return (null, null);
        //         }
        //
        //         AnimatorStateAttribute fakeAttribute = new AnimatorStateAttribute();
        //         return (fakeAttribute, typeof(AnimatorStateAttributeDrawer));
        //     });
        // }

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            if (property.propertyType == SerializedPropertyType.String)
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
            Toggle toggle = foldOut.Q<Toggle>();
            if (toggle != null)
            {
                toggle.userData = container;
            }

            foldOut.RegisterValueChangedCallback(v =>
            {
                container.Q<VisualElement>(NameProperties(property)).style.display =
                    v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return foldOut;
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
                ? animatorStateAttribute.AnimFieldName
                : null;

            MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);
            int curIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachStateInfo => EqualAnimatorState(eachStateInfo, property));
            string buttonLabel = curIndex == -1 ? "-" : FormatStateLabel(metaInfo.AnimatorStates[curIndex], "/");

            UIToolkitUtils.DropdownButtonField dropdownButton =
                UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
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
                PropertyField subField = new PropertyField(serializedProperty,
                    ObjectNames.NicifyVariableName(serializedProperty.displayName));
                subField.SetEnabled(false);
                properties.Add(subField);
            }

            SerializedProperty subStateMachineNameChainProp =
                FindPropertyRelative(property, "subStateMachineNameChain");
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
                textField.AddToClassList(BaseField<object>.alignedFieldUssClassName);
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
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
            dropdownButton.ButtonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, info, parent, onValueChangedCallback);

            string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
                ? animatorStateAttribute.AnimFieldName
                : null;

            MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);
            int curIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachStateInfo => EqualAnimatorState(eachStateInfo, property));

            if (curIndex != -1)
            {
                if (SetPropValue(property, metaInfo.AnimatorStates[curIndex]))
                {
                    onValueChangedCallback.Invoke(property.propertyType == SerializedPropertyType.String
                        ? metaInfo.AnimatorStates[curIndex].state.name
                        : metaInfo.AnimatorStates[curIndex]);
                }
            }
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent, Action<object> onChange)
        {
            string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
                ? animatorStateAttribute.AnimFieldName
                : null;

            MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

            int selectedIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachStateInfo => EqualAnimatorState(eachStateInfo, property));
            // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
            UIToolkitUtils.DropdownButtonField buttonLabel =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));
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
                    onChange(property.propertyType == SerializedPropertyType.String ? curItem.state.name : curItem);
                    buttonLabel.ButtonLabelElement.text = curName;
                    // property.serializedObject.ApplyModifiedProperties();
                });
            }

            if (metaInfo.RuntimeAnimatorController != null)
            {
                if (metaInfo.AnimatorStates.Count > 0)
                {
                    genericDropdownMenu.AddSeparator("");
                }

                genericDropdownMenu.AddItem($"Edit {metaInfo.RuntimeAnimatorController.name}...", false,
                    () => OpenAnimator(metaInfo.RuntimeAnimatorController));
            }

            UIToolkitUtils.DropdownButtonField root =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButton(property));

            genericDropdownMenu.DropDown(root.ButtonElement.worldBound, root, true);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            string animFieldName = saintsAttribute is AnimatorStateAttribute animatorStateAttribute
                ? animatorStateAttribute.AnimFieldName
                : null;

            MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            // ReSharper disable once InvertIf
            if (metaInfo.Error != helpBox.text)
            {
                helpBox.text = metaInfo.Error;
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
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
    }
}
#endif
