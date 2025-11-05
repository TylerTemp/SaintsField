#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditorInternal;

namespace SaintsField.Editor.Drawers.TagDrawer
{
    public partial class TagAttributeDrawer
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

            TagElement tagElement = new TagElement
            {
                bindingPath = property.propertyPath,
            };
            TagField r = new TagField(GetPreferredLabel(property), tagElement);
            r.AddToClassList(TagField.alignedFieldUssClassName);
            r.AddToClassList(ClassAllowDisable);
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

            TagField layerStringField = container.Q<TagField>();
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

                foreach (string tag in InternalEditorUtility.tags)
                {
                    // ReSharper disable once InvertIf
                    if (tag == clipboardText)
                    {
                        evt.menu.AppendAction($"Paste \"{tag}\"", _ =>
                        {
                            property.stringValue = tag;
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, tag);
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(tag);
                        });
                        return;
                    }
                }
            }));
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, TagAttribute tagAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is TagField sls)
            {
                sls.SetValueWithoutNotify(value);
                return null;
            }

            TagElement visualInput = new TagElement
            {
                value = value,
            };
            TagField element =
                new TagField(label, visualInput)
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
