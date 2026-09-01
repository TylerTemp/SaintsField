#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Units;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.UnitDrawer
{
    public partial class UnitAttributeDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static string NameUnitField(SerializedProperty property) =>
            $"{property.propertyPath}__UnitField";

        private static string NameUnitButton(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__UnitButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            UnitAttribute unitAttribute = (UnitAttribute)saintsAttribute;
            UnitState state = GetState(unitAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
            }

            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
            string label = GetPreferredLabel(property);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                if (rawType == typeof(uint))
                {
                    UnsignedIntegerField field = PrepareField<UnsignedIntegerField, uint>(new UnsignedIntegerField(label), property);
                    BindField(field, property, unitAttribute,
                        () => property.uintValue,
                        value => GetUIntValuePre(value, unitAttribute),
                        value => GetUIntValuePost(value, unitAttribute),
                        value => property.uintValue = value);
                    return field;
                }

                if (rawType == typeof(long))
                {
                    LongField field = PrepareField<LongField, long>(new LongField(label), property);
                    BindField(field, property, unitAttribute,
                        () => property.longValue,
                        value => GetLongValuePre(value, unitAttribute),
                        value => GetLongValuePost(value, unitAttribute),
                        value => property.longValue = value);
                    return field;
                }

                if (rawType == typeof(ulong))
                {
                    ULongField field = PrepareField<ULongField, ulong>(new ULongField(label), property);
                    BindField(field, property, unitAttribute,
                        () => property.ulongValue,
                        value => GetULongValuePre(value, unitAttribute),
                        value => GetULongValuePost(value, unitAttribute),
                        value => property.ulongValue = value);
                    return field;
                }

                IntegerField intField = PrepareField<IntegerField, int>(new IntegerField(label), property);
                BindField(intField, property, unitAttribute,
                    () => property.intValue,
                    value => GetIntValuePre(value, unitAttribute),
                    value => GetIntValuePost(value, unitAttribute),
                    value => property.intValue = value);
                return intField;
            }

            // ReSharper disable once InvertIf
            if (property.propertyType == SerializedPropertyType.Float)
            {
                if (rawType == typeof(float))
                {
                    FloatField field = PrepareField<FloatField, float>(new FloatField(label), property);
                    BindField(field, property, unitAttribute,
                        () => property.floatValue,
                        value => GetFloatValuePre(value, unitAttribute),
                        value => GetFloatValuePost(value, unitAttribute),
                        value => property.floatValue = value);
                    return field;
                }

                DoubleField doubleField = PrepareField<DoubleField, double>(new DoubleField(label), property);
                BindField(doubleField, property, unitAttribute,
                    () => property.doubleValue,
                    value => GetDoubleValuePre(value, unitAttribute),
                    value => GetDoubleValuePost(value, unitAttribute),
                    value => property.doubleValue = value);
                return doubleField;
            }

            return PropertyFieldFallbackUIToolkit(property, label);
        }

        private static TField PrepareField<TField, TValue>(TField field, SerializedProperty property)
            where TField : BaseField<TValue>
        {
            field.name = NameUnitField(property);
            field.AddToClassList(BaseField<TValue>.alignedFieldUssClassName);
            field.AddToClassList(ClassAllowDisable);
            if (!string.IsNullOrEmpty(property.tooltip) && field.labelElement != null)
            {
                field.labelElement.tooltip = property.tooltip;
            }

            return field;
        }

        private static void BindField<T>(BaseField<T> field, SerializedProperty property,
            UnitAttribute unitAttribute, Func<T> getBaseValue,
            Func<T, (string error, T value)> toDisplay,
            Func<T, (string error, T value)> toBase,
            Action<T> setBaseValue)
        {
            Refresh();
            field.RegisterValueChangedCallback(evt =>
            {
                (string error, T value) = toBase(evt.newValue);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                    Refresh();
                    return;
                }

                setBaseValue(value);
                property.serializedObject.ApplyModifiedProperties();
            });
            field.TrackPropertyValue(property, _ => Refresh());
            AddDisplayUnitChangedListener(unitAttribute, Refresh);
            return;

            void Refresh()
            {
                field.showMixedValue = property.hasMultipleDifferentValues;
                (string error, T value) = toDisplay(getBaseValue());
                if (string.IsNullOrEmpty(error))
                {
                    field.SetValueWithoutNotify(value);
                }
            }
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            UnitAttribute unitAttribute = (UnitAttribute)saintsAttribute;
            UnitState state = GetState(unitAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return null;
            }

            Button button = CreateUnitButton(unitAttribute);
            button.name = NameUnitButton(property, index);
            return button;
        }

        public static Button CreateUnitButton(UnitAttribute unitAttribute)
        {
            UnitState state = GetState(unitAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return null;
            }

            Button button = new Button
            {
                tooltip = state.DisplayUnit.Name,
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    minWidth = 36,
                },
            };

            void RefreshButton()
            {
                button.text = string.IsNullOrEmpty(state.DisplayUnit.PrimarySymbol)
                    ? state.DisplayUnit.Name
                    : state.DisplayUnit.PrimarySymbol;
                button.tooltip = state.DisplayUnit.Name;
            }

            RefreshButton();
            state.DisplayUnitChanged += RefreshButton;
            button.clicked += () => ShowUnitMenu(button, state);
            return button;
        }

        private static void ShowUnitMenu(Button button, UnitState state)
        {
            GenericDropdownMenu menu = new GenericDropdownMenu();
            foreach (UnitInfo unitInfo in UnitRegistry.GetAllUnitInfos(state.BaseUnit.Category))
            {
                UnitInfo captured = unitInfo;
                string label = string.IsNullOrEmpty(unitInfo.PrimarySymbol)
                    ? unitInfo.Name
                    : $"{unitInfo.Name} ({unitInfo.PrimarySymbol})";
                menu.AddItem(label, ReferenceEquals(unitInfo, state.DisplayUnit),
                    () => state.SetDisplayUnit(captured));
            }

            menu.DropDown(button.worldBound, button,
#if UNITY_6000_3_OR_NEWER
                DropdownMenuSizeMode.Auto
#else
                true
#endif
            );
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            UnitState state = GetState((UnitAttribute)saintsAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return new HelpBox(state.Error, HelpBoxMessageType.Error)
                {
                    style = { flexGrow = 1 },
                };
            }

            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.Float)
            {
                return new HelpBox($"Unit does not support {property.propertyType} fields.",
                    HelpBoxMessageType.Error)
                {
                    style = { flexGrow = 1 },
                };
            }

            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement field = container.Q<VisualElement>(NameUnitField(property));
            if (field != null)
            {
                UIToolkitUtils.AddContextualMenuManipulator(field, property,
                    () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            }

            container.TrackPropertyValue(property, changed =>
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (changed.propertyType)
                {
                    case SerializedPropertyType.Float:
                        onValueChangedCallback.Invoke(changed.doubleValue);
                        break;
                    case SerializedPropertyType.Integer:
                        onValueChangedCallback.Invoke(changed.longValue);
                        break;
                }
            });
        }
    }
}

#endif
