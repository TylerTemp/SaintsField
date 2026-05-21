using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.EditWrapper
{
    public class RowInputsElement<TField, TFieldValue>: BindableElement, INotifyValueChanged<TFieldValue[]>
        where TField : BaseField<TFieldValue>
    {
        private TFieldValue[] _value;
        private readonly TField[] _valueFields;
        public readonly int Count;

        public RowInputsElement(int count, Func<int, TField> tFieldCreator)
        {
            Count = count;

            _value = new TFieldValue[count];
            _valueFields = new TField[count];

            style.flexDirection = FlexDirection.Row;
            // style.flexGrow = 1;
            // style.flexShrink = 1;
            for (int index = 0; index < count; index++)
            {
                int curIndex = index;
                TField field1 = tFieldCreator(index);
                field1.RegisterValueChangedCallback(evt =>
                {
                    evt.StopPropagation();

                    TFieldValue[] copyValue = (TFieldValue[])_value.Clone();
                    _value[curIndex] = evt.newValue;

                    SendChangeEvent(copyValue, _value);
                    // value = copyValue;
                });
                hierarchy.Add(field1);
                _valueFields[index] = field1;
            }
        }

        public void SetValueWithoutNotify(TFieldValue[] newValue)
        {
            _value = newValue;
            for (int index = 0; index < Count; index++)
            {
                _valueFields[index].SetValueWithoutNotify(_value[index]);
            }
        }

        public TFieldValue[] value
        {
            get => _value;
            set
            {
                if (_value.SequenceEqual(value))
                {
                    // Debug.Log($"equal {string.Join(",", _value)} -> {string.Join(",", value)}");
                    return;
                }

                TFieldValue[] previous = (TFieldValue[])_value.Clone();

                SetValueWithoutNotify(value);

                SendChangeEvent(previous, value);
            }
        }

        private void SendChangeEvent(TFieldValue[] previous, TFieldValue[] newValue)
        {
            using ChangeEvent<TFieldValue[]> evt = ChangeEvent<TFieldValue[]>.GetPooled(previous, newValue);
            evt.target = this;
            SendEvent(evt);
        }
    }
}
