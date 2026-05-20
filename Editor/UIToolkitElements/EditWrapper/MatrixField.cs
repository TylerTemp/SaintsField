using System.Linq;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.EditWrapper
{
    public class MatrixField<TField, TFieldValue>: BindableElement, INotifyValueChanged<TFieldValue[][]>
        where TField : TextValueField<TFieldValue>, new()
    {
        private TFieldValue[][] _value;
        private readonly RowInputsElement<TField, TFieldValue>[] _valueFields;
        public readonly Label LabelElement;
        private readonly Foldout _foldout;
        public readonly int RowCount;
        public readonly int ColumnCount;

        public MatrixField(string label, int row, int column)
        {
            RowCount = row;
            ColumnCount = column;

            _foldout = new Foldout
            {
                text = label,
            };
            LabelElement = _foldout.Q<Label>();

            hierarchy.Add(_foldout);

            _valueFields = new RowInputsElement<TField, TFieldValue>[row];
            _value = new TFieldValue[row][];

            for (int rowIndex = 0; rowIndex < row; rowIndex++)
            {
                int thisRowIndex = rowIndex;
                RowInputsElement<TField, TFieldValue> subField = _valueFields[rowIndex] = new RowInputsElement<TField, TFieldValue>(
                    column, _ => new TField
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                            flexBasis = 0,
                            minWidth = 0,
                        },
                    });
                subField.RegisterValueChangedCallback(evt =>
                {
                    evt.StopPropagation();

                    TFieldValue[][] copyValue = (TFieldValue[][])_value.Clone();
                    _value[thisRowIndex] = evt.newValue;

                    SendChangeEvent(copyValue, _value);
                    // _value[thisRowIndex] = evt.newValue;
                    // value = _value;
                });
                _foldout.Add(subField);
                _valueFields[rowIndex] = subField;
                _value[rowIndex] = new TFieldValue[column];
            }
        }

        // ReSharper disable once InconsistentNaming
        public new string viewDataKey
        {
            // ReSharper disable once UnusedMember.Global
            get => _foldout.viewDataKey;
            set => _foldout.viewDataKey = value;
        }

        public void SetValueWithoutNotify(TFieldValue[][] newValue)
        {
            _value = newValue;
            for (int index = 0; index < _valueFields.Length; index++)
            {
                TFieldValue[] newRow = newValue[index];
                RowInputsElement<TField, TFieldValue> field = _valueFields[index];
                // Debug.Log($"Matrix SetValueWithoutNotify {index} = {string.Join(",", _value[index])}");
                field.SetValueWithoutNotify(newRow);
                // Debug.Log($"set {index}={string.Join(",", newRow)}");
            }
        }

        // private TFieldValue[][] GetCurValue() => _valueFields.Select(each => each.value).ToArray();

        public TFieldValue[][] value
        {
            get => _value;
            set
            {
                // TFieldValue[][] curValue = GetCurValue();
                if (SeqEqual(_value, value))
                {
                    return;
                }

                TFieldValue[][] previous = (TFieldValue[][])_value.Clone();

                SetValueWithoutNotify(value);

                SendChangeEvent(previous, value);
            }
        }

        private void SendChangeEvent(TFieldValue[][] previous, TFieldValue[][] newValue)
        {
            using ChangeEvent<TFieldValue[][]> evt = ChangeEvent<TFieldValue[][]>.GetPooled(previous, newValue);
            evt.target = this;
            SendEvent(evt);
        }

        private static bool SeqEqual(TFieldValue[][] curValue, TFieldValue[][] newValue)
        {
            if(curValue.Length != newValue.Length)
            {
                return false;
            }

            for (int index = 0; index < curValue.Length; index++)
            {
                TFieldValue[] curSub = curValue[index];
                TFieldValue[] newSub = newValue[index];
                if (!curSub.SequenceEqual(newSub))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
