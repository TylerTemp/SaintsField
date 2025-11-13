#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AnimatorParamDrawer
{
    public partial class AnimatorParamAttributeDrawer
    {
        // private static string NameDropdownField(SerializedProperty property) =>
        //     $"{property.propertyPath}__AnimatorParam_DropdownField";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorParam_HelpBox";

        // private IReadOnlyList<AnimatorControllerParameter> _cachedAnimatorControllerParams = Array.Empty<AnimatorControllerParameter>();
        // private Animator _cachedAnimator = null;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    AnimatorParamStringElement bindableElement = new AnimatorParamStringElement
                    {
                        bindingPath = property.propertyPath,
                    };
                    AnimatorParamStringField field = new AnimatorParamStringField(GetPreferredLabel(property), bindableElement);
                    field.AddToClassList(ClassAllowDisable);
                    field.AddToClassList(AnimatorParamStringField.alignedFieldUssClassName);
                    return field;
                }
                case SerializedPropertyType.Integer:
                {
                    AnimatorParamIntElement bindableElement = new AnimatorParamIntElement
                    {
                        bindingPath = property.propertyPath,
                    };
                    AnimatorParamIntField field = new AnimatorParamIntField(GetPreferredLabel(property), bindableElement);
                    field.AddToClassList(ClassAllowDisable);
                    field.AddToClassList(AnimatorParamIntField.alignedFieldUssClassName);
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
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    AnimatorParamStringField field = container.Q<AnimatorParamStringField>();

                    void Check()
                    {
                        MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
                        UIToolkitUtils.SetHelpBox(helpBox, metaInfo.Error);
                        if (metaInfo.Error == "")
                        {
                            field.AnimatorParamStringElement.BindAnimatorParameters(metaInfo.Animator, metaInfo.AnimatorParameters);
                        }
                    }

                    Check();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(Check);
                    field.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(Check));
                    field.TrackSerializedObjectValue(property.serializedObject, _ => Check());

                    AddContextualMenuManipulator(field, animatorParamAttribute, property, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.Integer:
                {
                    AnimatorParamIntField field = container.Q<AnimatorParamIntField>();

                    void Check()
                    {
                        MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
                        UIToolkitUtils.SetHelpBox(helpBox, metaInfo.Error);
                        if (metaInfo.Error == "")
                        {
                            field.AnimatorParamIntElement.BindAnimatorParameters(metaInfo.Animator, metaInfo.AnimatorParameters);
                        }
                    }

                    Check();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(Check);
                    field.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(Check));
                    field.TrackSerializedObjectValue(property.serializedObject, _ => Check());

                    AddContextualMenuManipulator(field, animatorParamAttribute, property, onValueChangedCallback, info, parent);
                }
                    break;
                default:
                    return;
            }
        }

        private static void AddContextualMenuManipulator(VisualElement bindableElement, AnimatorParamAttribute animatorParamAttribute, SerializedProperty property, Action<object> onValueChangedCallback, FieldInfo info, object parent)
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

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);

                MetaInfo metaInfo = GetMetaInfo(property, animatorParamAttribute, info, parent);
                if (metaInfo.Error != "")
                {
                    return;
                }

                foreach (AnimatorControllerParameter animParam in metaInfo.AnimatorParameters)
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
        
        private static MetaInfo GetMetaInfoShowInInspector(AnimatorParamAttribute animatorParamAttribute, object parent)
        {
            Animator animator;
            if (string.IsNullOrEmpty(animatorParamAttribute.AnimatorName))
            {
                if(parent is Object uObj)
                {
                    IReadOnlyList<Object> r = Util.GetTargetsTypeFromObj(uObj, typeof(Animator));
                    if (r.Count == 0)
                    {
                        return new MetaInfo
                        {
                            Error = $"No animator found on {uObj}",
                        };
                    }

                    animator = r[0] as Animator;
                }
                else
                {
                    return new MetaInfo
                    {
                        Error = $"{parent} is not a unity object",
                    };
                }
            }
            else
            {
                (string error, Animator value) = Util.GetOfNoParams<Animator>(parent, animatorParamAttribute.AnimatorName, null);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                    };
                }

                animator = value;
            }

            if (animator == null)
            {
                return new MetaInfo
                {
                    Error = $"No animator found in {parent}",
                };
            }

            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;

            if (runtimeController == null)
            {
                return new MetaInfo
                {
                    Error = $"RuntimeAnimatorController must not be null in {animator.name}",
                    AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                };
            }

            string loadPath;
            if(runtimeController is AnimatorOverrideController aoc)
            {
                loadPath = AssetDatabase.GetAssetPath(aoc.runtimeAnimatorController);
            }
            else
            {
                loadPath = AssetDatabase.GetAssetPath(runtimeController);
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(loadPath);
            // AnimatorOverrideController oc = (AnimatorOverrideController);
            // if (runtimeController is AnimatorOverrideController aoc)
            // {
            //     Debug.Log(aoc.runtimeAnimatorController);
            //     Debug.Log(AssetDatabase.GetAssetPath(aoc.runtimeAnimatorController));
            // }
            // AnimatorController controller = (AnimatorController)runtimeController;
            // Debug.Log($"runtimeController={runtimeController}/controller={controller}/{AssetDatabase.GetAssetPath(runtimeController)}");
            // for override controller, this hack won't work.
            // TODO: if the target is inside a prefab which is not loaded yet, does it works?
            if (controller == null)
            {
                // Debug.Log(runtimeController.GetType());
                // controller = (AnimatorController)runtimeController;
                return new MetaInfo
                {
                    Error = $"Can not obtain AnimatorController from {animator.name}: {runtimeController.GetType()}",
                    AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                };
            }

            List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (animatorParamAttribute.AnimatorParamType == null ||
                    parameter.type == animatorParamAttribute.AnimatorParamType)
                {
                    animatorParameters.Add(parameter);
                }
            }

            return new MetaInfo
            {
                Error = "",
                Animator = animator,
                AnimatorParameters = animatorParameters,
            };
        }


        private class AnimatorParamStringHelpBox : VisualElement
        {
            public readonly AnimatorParamStringField Field;
            public readonly HelpBox HelpBox;

            public AnimatorParamStringHelpBox(AnimatorParamStringField field)
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

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, AnimatorParamAttribute animatorParamAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            MetaInfo metaInfo = GetMetaInfoShowInInspector(
                animatorParamAttribute,
                targets[0]);

            if (oldElement is AnimatorParamStringHelpBox oldContainer)
            {
                oldContainer.Field.SetValueWithoutNotify(value);
                if (metaInfo.Error == "")
                {
                    oldContainer.Field.AnimatorParamStringElement.BindAnimatorParameters(metaInfo.Animator, metaInfo.AnimatorParameters);
                }
                UIToolkitUtils.SetHelpBox(oldContainer.HelpBox, metaInfo.Error);
                return null;
            }

            AnimatorParamStringElement visualInput = new AnimatorParamStringElement()
            {
                value = value,
            };
            if (metaInfo.Error == "")
            {
                visualInput.BindAnimatorParameters(metaInfo.Animator, metaInfo.AnimatorParameters);
            }
            AnimatorParamStringField field =
                new AnimatorParamStringField(label, visualInput)
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
            return new AnimatorParamStringHelpBox(field);
        }

        private class AnimatorParamIntHelpBox : VisualElement
        {
            public readonly AnimatorParamIntField Field;
            public readonly HelpBox HelpBox;

            public AnimatorParamIntHelpBox(AnimatorParamIntField field)
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
        
        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, AnimatorParamAttribute animatorParamAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            MetaInfo metaInfo = GetMetaInfoShowInInspector(
                animatorParamAttribute,
                targets[0]);

            if (oldElement is AnimatorParamIntHelpBox oldContainer)
            {
                oldContainer.Field.SetValueWithoutNotify(value);
                if (metaInfo.Error == "")
                {
                    oldContainer.Field.AnimatorParamIntElement.BindAnimatorParameters(metaInfo.Animator, metaInfo.AnimatorParameters);
                }
                UIToolkitUtils.SetHelpBox(oldContainer.HelpBox, metaInfo.Error);
                return null;
            }

            AnimatorParamIntElement visualInput = new AnimatorParamIntElement
            {
                value = value,
            };

            if (metaInfo.Error == "")
            {
                visualInput.BindAnimatorParameters(metaInfo.Animator, metaInfo.AnimatorParameters);
            }
            AnimatorParamIntField field =
                new AnimatorParamIntField(label, visualInput)
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
            return new AnimatorParamIntHelpBox(field);
        }
    }
}
#endif
