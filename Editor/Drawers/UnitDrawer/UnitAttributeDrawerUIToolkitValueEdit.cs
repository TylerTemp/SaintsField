#if UNITY_2021_3_OR_NEWER

using System;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Drawers.SaintsDecimalType;
using SaintsField.Editor.Units;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.UnitDrawer
{
    public partial class UnitAttributeDrawer
    {
        public static bool SupportsUIToolkitValueEdit(Type valueType, object value)
        {
            Type type = valueType ?? value?.GetType();
            return type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) ||
                   type == typeof(ushort) || type == typeof(int) || type == typeof(uint) ||
                   type == typeof(long) || type == typeof(ulong) || type == typeof(float) ||
                   type == typeof(double) || type == typeof(decimal);
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, UnitAttribute unitAttribute,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout)
        {
            Type type = valueType ?? value.GetType();
            if (type == typeof(sbyte))
            {
                return CreateValueField<IntegerField, int, sbyte>(oldElement, unitAttribute, label, (sbyte)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new IntegerField(label),
                    each => ConvertDisplay(GetSByteValuePre(each, unitAttribute), converted => (int)converted),
                    each => ConvertSByte(each, unitAttribute));
            }
            if (type == typeof(byte))
            {
                return CreateValueField<IntegerField, int, byte>(oldElement, unitAttribute, label, (byte)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new IntegerField(label),
                    each => ConvertDisplay(GetByteValuePre(each, unitAttribute), converted => (int)converted),
                    each => ConvertByte(each, unitAttribute));
            }
            if (type == typeof(short))
            {
                return CreateValueField<IntegerField, int, short>(oldElement, unitAttribute, label, (short)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new IntegerField(label),
                    each => ConvertDisplay(GetShortValuePre(each, unitAttribute), converted => (int)converted),
                    each => ConvertShort(each, unitAttribute));
            }
            if (type == typeof(ushort))
            {
                return CreateValueField<IntegerField, int, ushort>(oldElement, unitAttribute, label, (ushort)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new IntegerField(label),
                    each => ConvertDisplay(GetUShortValuePre(each, unitAttribute), converted => (int)converted),
                    each => ConvertUShort(each, unitAttribute));
            }
            if (type == typeof(int))
            {
                return CreateValueField<IntegerField, int, int>(oldElement, unitAttribute, label, (int)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new IntegerField(label),
                    each => GetIntValuePre(each, unitAttribute),
                    each => GetIntValuePost(each, unitAttribute));
            }
            if (type == typeof(uint))
            {
                return CreateValueField<UnsignedIntegerField, uint, uint>(oldElement, unitAttribute, label, (uint)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new UnsignedIntegerField(label),
                    each => GetUIntValuePre(each, unitAttribute),
                    each => GetUIntValuePost(each, unitAttribute));
            }
            if (type == typeof(long))
            {
                return CreateValueField<LongField, long, long>(oldElement, unitAttribute, label, (long)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new LongField(label),
                    each => GetLongValuePre(each, unitAttribute),
                    each => GetLongValuePost(each, unitAttribute));
            }
            if (type == typeof(ulong))
            {
                return CreateValueField<ULongField, ulong, ulong>(oldElement, unitAttribute, label, (ulong)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new ULongField(label),
                    each => GetULongValuePre(each, unitAttribute),
                    each => GetULongValuePost(each, unitAttribute));
            }
            if (type == typeof(float))
            {
                return CreateValueField<FloatField, float, float>(oldElement, unitAttribute, label, (float)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new FloatField(label),
                    each => GetFloatValuePre(each, unitAttribute),
                    each => GetFloatValuePost(each, unitAttribute));
            }
            if (type == typeof(double))
            {
                return CreateValueField<DoubleField, double, double>(oldElement, unitAttribute, label, (double)value,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new DoubleField(label),
                    each => GetDoubleValuePre(each, unitAttribute),
                    each => GetDoubleValuePost(each, unitAttribute));
            }
            if (type == typeof(decimal))
            {
                return CreateValueField<DecimalTextField, decimal, decimal>(oldElement, unitAttribute, label,
                    (decimal)value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    () => new DecimalTextField(label),
                    each => GetDecimalValuePre(each, unitAttribute),
                    each => GetDecimalValuePost(each, unitAttribute));
            }

            return null;
        }

        private static VisualElement CreateValueField<TField, TFieldValue, TValue>(VisualElement oldElement,
            UnitAttribute unitAttribute, string label, TValue value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            Func<TField> createField, Func<TValue, (string error, TFieldValue value)> toDisplay,
            Func<TFieldValue, (string error, TValue value)> toBase)
            where TField : BaseField<TFieldValue>
        {
            if (oldElement is UnitValueField<TField, TFieldValue, TValue> oldField &&
                ReferenceEquals(oldField.Attribute, unitAttribute))
            {
                oldField.Refresh(label, value, beforeSet, setterOrNull);
                return null;
            }

            return new UnitValueField<TField, TFieldValue, TValue>(unitAttribute, label, value,
                beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, createField(), toDisplay, toBase);
        }

        private static (string error, TOutput value) ConvertDisplay<TInput, TOutput>(
            (string error, TInput value) result, Func<TInput, TOutput> convert) =>
            string.IsNullOrEmpty(result.error)
                ? ("", convert(result.value))
                : (result.error, default);

        private static (string error, sbyte value) ConvertSByte(int value, UnitAttribute attribute)
        {
            try
            {
                return GetSByteValuePost(System.Convert.ToSByte(value), attribute);
            }
            catch (Exception exception)
            {
                return ($"Unit conversion overflow: {exception.Message}", 0);
            }
        }

        private static (string error, byte value) ConvertByte(int value, UnitAttribute attribute)
        {
            try
            {
                return GetByteValuePost(System.Convert.ToByte(value), attribute);
            }
            catch (Exception exception)
            {
                return ($"Unit conversion overflow: {exception.Message}", 0);
            }
        }

        private static (string error, short value) ConvertShort(int value, UnitAttribute attribute)
        {
            try
            {
                return GetShortValuePost(System.Convert.ToInt16(value), attribute);
            }
            catch (Exception exception)
            {
                return ($"Unit conversion overflow: {exception.Message}", 0);
            }
        }

        private static (string error, ushort value) ConvertUShort(int value, UnitAttribute attribute)
        {
            try
            {
                return GetUShortValuePost(System.Convert.ToUInt16(value), attribute);
            }
            catch (Exception exception)
            {
                return ($"Unit conversion overflow: {exception.Message}", 0);
            }
        }

        private class UnitValueField<TField, TFieldValue, TValue> : VisualElement
            where TField : BaseField<TFieldValue>
        {
            public readonly UnitAttribute Attribute;

            private readonly TField _field;
            private readonly Button _button;
            private readonly HelpBox _error;
            private readonly UnitState _state;
            private readonly Func<TValue, (string error, TFieldValue value)> _toDisplay;
            private readonly Func<TFieldValue, (string error, TValue value)> _toBase;

            private TValue _baseValue;
            private Action<object> _beforeSet;
            private Action<object> _setter;
            private bool _attached;

            public UnitValueField(UnitAttribute attribute, string label, TValue value, Action<object> beforeSet,
                Action<object> setter, bool labelGrayColor, bool inHorizontalLayout, TField field,
                Func<TValue, (string error, TFieldValue value)> toDisplay,
                Func<TFieldValue, (string error, TValue value)> toBase)
            {
                Attribute = attribute;
                _field = field;
                _state = GetState(attribute);
                _toDisplay = toDisplay;
                _toBase = toBase;

                style.flexGrow = 1;
                style.flexDirection = FlexDirection.Column;

                VisualElement row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    },
                };
                _field.style.flexGrow = 1;
                UIToolkitUtils.UIToolkitValueEditAfterProcess(_field, setter != null,
                    labelGrayColor, inHorizontalLayout);
                row.Add(_field);

                _button = new Button
                {
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 0,
                        minWidth = 36,
                    },
                };
                _button.clicked += () => ShowUnitMenu(_button, _state);
                row.Add(_button);
                Add(row);

                _error = new HelpBox("", HelpBoxMessageType.Error);
                _error.style.flexGrow = 1;
                Add(_error);

                _field.RegisterValueChangedCallback(OnFieldChanged);
                RegisterCallback<AttachToPanelEvent>(_ => SetAttached(true));
                RegisterCallback<DetachFromPanelEvent>(_ => SetAttached(false));
                Refresh(label, value, beforeSet, setter);
            }

            public void Refresh(string label, TValue value, Action<object> beforeSet, Action<object> setter)
            {
                _field.label = label;
                _baseValue = value;
                _beforeSet = beforeSet;
                _setter = setter;
                _field.SetEnabled(setter != null);
                RefreshDisplay();
            }

            private void OnFieldChanged(ChangeEvent<TFieldValue> evt)
            {
                (string error, TValue value) = _toBase(evt.newValue);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                    RefreshDisplay();
                    return;
                }

                _beforeSet?.Invoke(_baseValue);
                _setter?.Invoke(value);
                _baseValue = value;
            }

            private void RefreshDisplay()
            {
                RefreshButton();
                string error = _state.Error;
                if (string.IsNullOrEmpty(error))
                {
                    (string displayError, TFieldValue display) = _toDisplay(_baseValue);
                    error = displayError;
                    if (string.IsNullOrEmpty(displayError))
                    {
                        _field.SetValueWithoutNotify(display);
                    }
                }

                _error.text = error;
                _error.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
                _button.style.display = string.IsNullOrEmpty(_state.Error) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            private void RefreshButton()
            {
                if (_state.DisplayUnit == null)
                {
                    return;
                }

                _button.text = GetDisplayText(_state.DisplayUnit);
                _button.tooltip = _state.DisplayUnit.Name;
            }

            private void SetAttached(bool attached)
            {
                if (_attached == attached)
                {
                    return;
                }

                _attached = attached;
                if (attached)
                {
                    _state.DisplayUnitChanged += RefreshDisplay;
                }
                else
                {
                    _state.DisplayUnitChanged -= RefreshDisplay;
                }
            }
        }
    }
}

#endif
