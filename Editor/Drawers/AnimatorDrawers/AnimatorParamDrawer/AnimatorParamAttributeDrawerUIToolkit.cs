#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorParamDrawer
{
    public partial class AnimatorParamAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorParam_DropdownField";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorParam_HelpBox";

        private IReadOnlyList<AnimatorControllerParameter> _cachedAnimatorControllerParams = Array.Empty<AnimatorControllerParameter>();
        private Animator _cachedAnimator = null;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    AnimatorParamStringElement bindableElement = new AnimatorParamStringElement();
                    // bindableElement.BindAnimatorParameters(metaInfo.AnimatorParameters);
                    bindableElement.BindProperty(property);
                    return new StringDropdownField(GetPreferredLabel(property), bindableElement)
                    {
                        name = NameDropdownField(property),
                    };
                }
                case SerializedPropertyType.Integer:
                {
                    AnimatorParamIntElement bindableElement = new AnimatorParamIntElement();
                    // bindableElement.BindAnimatorParameters(metaInfo.AnimatorParameters);
                    bindableElement.BindProperty(property);
                    return new IntDropdownField(GetPreferredLabel(property), bindableElement)
                    {
                        name = NameDropdownField(property),
                    };
                }
                default:
                    return new VisualElement();
            }
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement;
            if (property.propertyType is SerializedPropertyType.String or SerializedPropertyType.Integer)
            {
                helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                        flexGrow = 1,
                    },
                    name = NameHelpBox(property),
                };
            }
            else
            {
                helpBoxElement = new HelpBox(
                    $"Type {property.propertyType} is not string or int",
                    HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.Flex,
                        flexGrow = 1,
                    },
                    name = NameHelpBox(property),
                };
            }

            helpBoxElement.AddToClassList(ClassAllowDisable);
            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement fieldElement;
            Button dropdownButton;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    fieldElement = container.Q<VisualElement>(NameDropdownField(property));
                    dropdownButton = fieldElement.Q<AnimatorParamStringElement>().Button;
                }
                    break;
                case SerializedPropertyType.Integer:
                {
                    fieldElement = container.Q<VisualElement>(NameDropdownField(property));
                    dropdownButton = fieldElement.Q<AnimatorParamIntElement>().Button;
                }
                    break;
                default:
                    return;
            }


            // UIToolkitUtils.AddContextualMenuManipulator(bindableElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            RefreshDisplay();

            AddContextualMenuManipulator(fieldElement, property, onValueChangedCallback, info, parent);
            dropdownButton.clicked += () => ShowDropdown(fieldElement, property, info, parent, onValueChangedCallback);

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshDisplay);
            fieldElement.RegisterCallback<DetachFromPanelEvent>(_ =>
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshDisplay));

            return;

            void RefreshDisplay() => CheckAnimatorChanges(container, property, saintsAttribute, info, parent);
        }

        private void AddContextualMenuManipulator(VisualElement bindableElement, SerializedProperty property, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(bindableElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            bool isString = property.propertyType == SerializedPropertyType.String;

            bindableElement.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                if (_cachedAnimatorControllerParams.Count == 0)
                {
                    return;
                }

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);

                foreach (AnimatorControllerParameter animParam in _cachedAnimatorControllerParams)
                {
                    if (animParam.name == clipboardText
                        || (canBeInt && animParam.nameHash == clipboardInt))
                    {
                        evt.menu.AppendAction($"Paste \"{animParam.name}\"({animParam.type})", _ =>
                        {
                            object newValue;
                            if(isString)
                            {
                                newValue = property.stringValue = animParam.name;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject,
                                    info, parent, animParam.name);
                            }
                            else
                            {
                                newValue = property.intValue = animParam.nameHash;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject,
                                    info, parent, animParam.nameHash);
                            }
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(newValue);
                        });
                    }
                }
            }));
        }

        private void CheckAnimatorChanges(VisualElement container, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            UpdateHelpBox(container.Q<HelpBox>(NameHelpBox(property)), metaInfo.Error);

            if (metaInfo.AnimatorParameters.SequenceEqual(_cachedAnimatorControllerParams))
            {
                return;
            }

            _cachedAnimatorControllerParams = metaInfo.AnimatorParameters;
            _cachedAnimator = metaInfo.Animator;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    AnimatorParamStringElement bindableElement = container.Q<AnimatorParamStringElement>();
                    bindableElement.BindAnimatorParameters(metaInfo.AnimatorParameters);
                }
                    return;
                case SerializedPropertyType.Integer:
                {
                    AnimatorParamIntElement bindableElement = container.Q<AnimatorParamIntElement>();
                    bindableElement.BindAnimatorParameters(metaInfo.AnimatorParameters);
                }
                    return;
                default:
                    return;
            }
        }

        private static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }

        private string _brownColor;

        private void ShowDropdown(VisualElement root, SerializedProperty property, FieldInfo info, object parent, Action<object> onValueChangedCallback)
        {
            if(_cachedAnimatorControllerParams.Count == 0 && _cachedAnimator == null)
            {
                return;
            }

            bool isString = property.propertyType == SerializedPropertyType.String;

            _brownColor ??= $"#{ColorUtility.ToHtmlStringRGB(EColor.Brown.GetColor())}";

            AnimatorControllerParameter selectedParam = null;

            AdvancedDropdownList<AnimatorControllerParameter> lis =
                new AdvancedDropdownList<AnimatorControllerParameter>();
            foreach (AnimatorControllerParameter cachedAnimatorControllerParam in _cachedAnimatorControllerParams)
            {
                lis.Add(
                    $"{cachedAnimatorControllerParam.name} <color={_brownColor}>{cachedAnimatorControllerParam.type}</color> <color=#808080>({cachedAnimatorControllerParam.nameHash})</color>",
                    cachedAnimatorControllerParam);

                if (isString && cachedAnimatorControllerParam.name == property.stringValue
                    || !isString && cachedAnimatorControllerParam.nameHash == property.intValue)
                {
                    selectedParam = cachedAnimatorControllerParam;
                }
            }

            if(_cachedAnimator != null)
            {
                if (_cachedAnimatorControllerParams.Count > 0)
                {
                    lis.AddSeparator();
                }

                lis.Add("Edit Animator...", null);
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedParam is null ? Array.Empty<object>(): new object[] { selectedParam },
                DropdownListValue = lis,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    AnimatorControllerParameter newV = (AnimatorControllerParameter)curItem;
                    if (newV is null)
                    {
                        if(_cachedAnimator != null)
                        {
                            OpenAnimator(_cachedAnimator);
                        }

                        return;
                    }

                    object newValue;
                    if (isString)
                    {
                        newValue = property.stringValue = newV.name;
                    }
                    else
                    {
                        newValue = property.intValue = newV.nameHash;
                    }

                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(newValue);
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private static string GetParameterLabel(AnimatorControllerParameter each) => $"{each.name} [{each.type}]";
    }
}
#endif
