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

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(ValueButtonsAttribute), true)]
    [CustomPropertyDrawer(typeof(OptionsValueButtonsAttribute), true)]
    [CustomPropertyDrawer(typeof(PairsValueButtonsAttribute), true)]
    public partial class ValueButtonsAttributeDrawer: SaintsPropertyDrawer
    {
        private static string NameField(SerializedProperty sp) => $"{sp.propertyPath}__ValueButtons_Field";
        private static string NameArrange(SerializedProperty sp) => $"{sp.propertyPath}__ValueButtons_Arrange";
        private static string NameExpand(SerializedProperty sp) => $"{sp.propertyPath}__ValueButtons_Expand";
        public static string NameSubPanel(SerializedProperty sp) => $"{sp.propertyPath}__ValueButtons_SubPanel";
        public static string NameHelpBox(SerializedProperty sp) => $"{sp.propertyPath}__ValueButtons_HelpBox";

        // private RichTextDrawer _richTextDrawer;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent) =>
            UtilCreateFieldUIToolKit(GetPreferredLabel(property), property);

        public static  VisualElement UtilCreateFieldUIToolKit(string label, SerializedProperty property)
        {
            VisualElement visualInput = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            string expandName = NameExpand(property);
            LeftExpandButton leftExpandButton = new LeftExpandButton
            {
                name = expandName,
            };
            visualInput.Add(leftExpandButton);

            VisualElement valueButtonsArrangeElementWrapper = new VisualElement
            {
                style =
                {
                    // flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            visualInput.Add(valueButtonsArrangeElementWrapper);

            // root.Add(leftExpandButton);
            ValueButtonsArrangeElement valueButtonsArrangeElement = new ValueButtonsArrangeElement(new ValueButtonsCalcElement())
            {
                name = NameArrange(property),
                style =
                {
                    // height = 22,
                    marginRight = 2,
                },
            };
            valueButtonsArrangeElementWrapper.Add(valueButtonsArrangeElement);

            EmptyPrefabOverrideField r = new EmptyPrefabOverrideField(label, visualInput, property)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameField(property),
                // bindingPath = property.propertyPath,
            };
            // r.BindProperty(property);
            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(EmptyPrefabOverrideField.alignedFieldUssClassName);
            return r;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent) =>
            UtilCreateBelowUIToolkit(property);

        public static VisualElement UtilCreateBelowUIToolkit(SerializedProperty property)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            VisualElement sub = new VisualElement
            {
                name = NameSubPanel(property),
            };
            root.Add(sub);
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    // flexGrow = 1,
                    // flexShrink = 1,
                },
                name = NameHelpBox(property),
            };
            root.Add(helpBox);

            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent) =>
            UtilOnAwakeUIToolkit(this, property, saintsAttribute, container, onValueChangedCallback,
                info, parent);

        public static void UtilOnAwakeUIToolkit(IRichTextTagProvider richTextTagProvider, SerializedProperty property, ISaintsAttribute saintsAttribute, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            EmptyPrefabOverrideField field = container.Q<EmptyPrefabOverrideField>(NameField(property));
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));
            ValueButtonsArrangeElement valueButtonsArrangeElement =
                container.Q<ValueButtonsArrangeElement>(name: NameArrange(property));
            VisualElement subPanel = container.Q<VisualElement>(name: NameSubPanel(property));
            LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));

            valueButtonsArrangeElement.BindSubContainer(subPanel);

            PathedDropdownAttribute valueButtonsAttribute = (PathedDropdownAttribute)saintsAttribute;

            RefreshButtons();
            if (!string.IsNullOrEmpty(valueButtonsAttribute.FuncName))
            {
                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshButtons);
                valueButtonsArrangeElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshButtons));
                valueButtonsArrangeElement.TrackSerializedObjectValue(property.serializedObject, _ => RefreshButtons());
            }
            valueButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());

            valueButtonsArrangeElement.schedule.Execute(() =>
            {
                subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                leftExpandButton.RegisterValueChangedCallback(evt =>
                    subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                valueButtonsArrangeElement.OnCalcArrangeDone.AddListener(hasSubRow =>
                {
                    // leftExpandButton.SetEnabled(hasSubRow);
                    DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                    if (leftExpandButton.style.display != display)
                    {
                        leftExpandButton.style.display = display;
                    }
                    RefreshCurValue();
                });
                valueButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                {
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                        parent, value);
                    Util.SignPropertyValue(property, info, parent, value);
                    property.serializedObject.ApplyModifiedProperties();
                    RefreshCurValue();
                });
                RefreshCurValue();
            });

            return;

            void RefreshCurValue()
            {
                (string curError, int _, object curValue)  = Util.GetValue(property, info, parent);
                if (curError == "")
                {
                    valueButtonsArrangeElement.RefreshCurValue(curValue);
                    bool leftExpandButtonEnabled = leftExpandButton.enabledSelf;
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (leftExpandButtonEnabled)
                    {
                        leftExpandButton.tooltip = $"{curValue} (Click to see all buttons)";
                    }
                    else
                    {
                        leftExpandButton.tooltip = $"{curValue}";
                    }
                }
                else
                {
                    UIToolkitUtils.SetHelpBox(helpBox, curError);
                }
            }

            void RefreshButtons()
            {
                AdvancedDropdownMetaInfo initMetaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, valueButtonsAttribute, info, parent, false, true);
                UIToolkitUtils.SetHelpBox(helpBox, initMetaInfo.Error);
                // ReSharper disable once InvertIf
                if(initMetaInfo.Error == "")
                {
                    valueButtonsArrangeElement.UpdateButtons(
                        initMetaInfo.DropdownListValue
                            .Select(each =>
                                new ValueButtonRawInfo(
                                    RichTextDrawer.ParseRichXmlWithProvider(each.displayName, richTextTagProvider).ToArray(),
                                    each.disabled,
                                    each.value))
                            .ToArray()
                    );
                    RefreshCurValue();
                }
            }
        }
    }
}
