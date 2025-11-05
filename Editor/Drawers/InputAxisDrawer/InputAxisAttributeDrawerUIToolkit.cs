#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public partial class InputAxisAttributeDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            InputAxisElement inputAxisElement = new InputAxisElement
            {
                bindingPath = property.propertyPath,
            };

            InputAxisField r = new InputAxisField(GetPreferredLabel(property), inputAxisElement);
            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(InputAxisField.alignedFieldUssClassName);
            return r;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return null;
            }

            return new HelpBox($"Type {property.propertyType} is not string.", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            InputAxisField layerStringField = container.Q<InputAxisField>();
            AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);
        }

        private static void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                foreach (string axisName in InputAxisUtils.GetAxisNames())
                {
                    // ReSharper disable once InvertIf
                    if (axisName == clipboardText)
                    {
                        evt.menu.AppendAction($"Paste \"{axisName}\"", _ =>
                        {
                            property.stringValue = axisName;
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, axisName);
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(axisName);
                        });
                        return;
                    }
                }
            }));
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, InputAxisAttribute inputAxisAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is InputAxisField sls)
            {
                sls.SetValueWithoutNotify(value);
                return null;
            }

            InputAxisElement visualInput = new InputAxisElement
            {
                value = value,
            };
            InputAxisField element =
                new InputAxisField(label, visualInput)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                visualInput.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return element;
        }
    }
}
#endif
