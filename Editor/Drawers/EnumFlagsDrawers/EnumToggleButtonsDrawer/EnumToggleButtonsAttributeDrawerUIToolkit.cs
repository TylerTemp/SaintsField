#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer
    {
        public class EnumFlagsField : BaseField<int>
        {
            public EnumFlagsField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameExpand(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_Expand";
        private static string NameArrange(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_Arrange";
        private static string NameField(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_Field";

        private static string NameFullToggleGroup(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_FullToggleGroup";


        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);

            if (!metaInfo.HasFlags)
            {
                return ValueButtonsAttributeDrawer.UtilCreateFieldUIToolKit(GetPreferredLabel(property), property);
            }

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
            leftExpandButton.SetCustomViewDataKey(SerializedUtils.GetUniqueId(property));
            if (property.isExpanded || allAttributes.Any(each => each is FieldDefaultExpandAttribute))
            {
                leftExpandButton.value = true;
            }

            visualInput.Add(new FlagButtonFullToggleGroupElement
            {
                name = NameFullToggleGroup(property),
            });

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

            FlagButtonsArrangeElement valueButtonsArrangeElement = new FlagButtonsArrangeElement(new FlagButtonsCalcElement(false))
            {
                name = NameArrange(property),
                style =
                {
                    marginRight = 2,
                },
            };
            valueButtonsArrangeElementWrapper.Add(valueButtonsArrangeElement);

            EmptyPrefabOverrideField r = new EmptyPrefabOverrideField(GetPreferredLabel(property), visualInput, property)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameField(property),
            };
            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(EmptyPrefabOverrideField.alignedFieldUssClassName);
            return r;

        }
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            return ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property);
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
            // EnumToggleButtonsAttribute enumToggleButtonsAttribute = (EnumToggleButtonsAttribute) saintsAttribute;
            bool isFlags = Attribute.IsDefined(rawType, typeof(FlagsAttribute));
            if (!isFlags)
            {
                ValueButtonsAttributeDrawer.UtilOnAwakeUIToolkit(this, property, saintsAttribute, container,
                    onValueChangedCallback, info, parent);
                return;
            }
            EnumMetaInfo metaInfo = EnumFlagsUtil.GetEnumMetaInfo(rawType);

            FlagButtonFullToggleGroupElement flagButtonFullToggleGroupElement =
                container.Q<FlagButtonFullToggleGroupElement>(name: NameFullToggleGroup(property));
            flagButtonFullToggleGroupElement.HToggleButton.clicked += () =>
            {
                object userData = flagButtonFullToggleGroupElement.HToggleButton.userData;
                if (userData == null)
                {
                    Debug.LogWarning("userData for hToggleButton is null, skip");
                    return;
                }

                EnumFlagsUtil.SetSerializedPropertyEnumValue(metaInfo.EnumType, property, Convert.ToInt64(userData));
                property.serializedObject.ApplyModifiedProperties();
            };
            flagButtonFullToggleGroupElement.HCheckAllButton.clicked += () =>
            {
                long toggle = Convert.ToInt64(metaInfo.EverythingBit);
                EnumFlagsUtil.SetSerializedPropertyEnumValue(metaInfo.EnumType, property, toggle);
                property.serializedObject.ApplyModifiedProperties();
            };
            flagButtonFullToggleGroupElement.HEmptyButton.clicked += () =>
            {
                EnumFlagsUtil.SetSerializedPropertyEnumValue(metaInfo.EnumType, property, 0);
                property.serializedObject.ApplyModifiedProperties();
            };

            List<ValueButtonRawInfo> rawInfos = new List<ValueButtonRawInfo>();

            foreach (EnumMetaInfo.EnumValueInfo enumValueInfo in metaInfo.EnumValues)
            {
                IReadOnlyList<RichTextDrawer.RichTextChunk> chunks;
                if (enumValueInfo.OriginalLabel != enumValueInfo.Label)
                {
                    chunks = RichTextDrawer.ParseRichXmlWithProvider(enumValueInfo.Label, this).ToArray();
                }
                else
                {
                    chunks = new[]
                    {
                        new RichTextDrawer.RichTextChunk(enumValueInfo.OriginalLabel, false, enumValueInfo.OriginalLabel),
                    };
                }
                // Debug.Log($"Add {enumValueInfo.Value}");
                rawInfos.Add(new ValueButtonRawInfo(chunks, false, enumValueInfo.Value));
            }

            EmptyPrefabOverrideField field = container.Q<EmptyPrefabOverrideField>(NameField(property));
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            FlagButtonsArrangeElement flagButtonsArrangeElement =
                container.Q<FlagButtonsArrangeElement>(name: NameArrange(property));
            VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
            LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));
            leftExpandButton.RegisterValueChangedCallback(evt =>
                flagButtonFullToggleGroupElement.ToFullToggles(evt.newValue));
            flagButtonFullToggleGroupElement.ToFullToggles(leftExpandButton.value);

            flagButtonsArrangeElement.BindSubContainer(subPanel);

            flagButtonsArrangeElement.UpdateButtons(
                rawInfos
            );
            RefreshCurValue();

            flagButtonsArrangeElement.schedule.Execute(() =>
            {
                leftExpandButton.RegisterValueChangedCallback(evt =>
                    subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                flagButtonsArrangeElement.OnCalcArrangeDoneAddListener(hasSubRow =>
                {
                    subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                    // leftExpandButton.SetEnabled(hasSubRow);
                    DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                    if (leftExpandButton.style.display != display)
                    {
                        leftExpandButton.style.display = display;
                    }
                    RefreshCurValue();
                });
                flagButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                {
                    long toggle = Convert.ToInt64(value);

                    long newValue = EnumFlagsUtil.ToggleBit(property.intValue, toggle);

                    EnumFlagsUtil.SetSerializedPropertyEnumValue(metaInfo.EnumType, property, newValue);
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                        parent, Enum.ToObject(metaInfo.EnumType, newValue));
                });
                RefreshCurValue();
                flagButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());
            });

            flagButtonsArrangeElement.TrackPropertyValue(property, _ => onValueChangedCallback.Invoke(property.intValue));

            return;

            void RefreshCurValue()
            {
                int curValue = property.intValue;

                flagButtonsArrangeElement.RefreshCurValue(curValue);
                flagButtonFullToggleGroupElement.RefreshValue(curValue, metaInfo);

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
        }
    }
}
#endif
