using System.Linq;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.EditWrapper
{
    public class MatrixBooleanField: BindableElement, INotifyValueChanged<bool[][]>
    {
        private bool[][] _value;
        private readonly RowBooleansElement[] _valueFields;
        public readonly Label LabelElement;
        private readonly Foldout _foldout;

        public readonly int RowCount;
        public readonly int ColumnCount;

        public MatrixBooleanField(string label, int row, int column)
        {
            RowCount = row;
            ColumnCount = column;

            _foldout = new Foldout
            {
                text = label,
            };
            LabelElement = _foldout.Q<Label>();

            hierarchy.Add(_foldout);

            _valueFields = new RowBooleansElement[row];
            _value = new bool[row][];

            for (int rowIndex = 0; rowIndex < row; rowIndex++)
            {
                int thisRowIndex = rowIndex;
                RowBooleansElement subField = _valueFields[rowIndex] = new RowBooleansElement(column, false);
                subField.RegisterValueChangedCallback(evt =>
                {
                    evt.StopPropagation();

                    bool[][] copyValue = (bool[][])_value.Clone();
                    _value[thisRowIndex] = evt.newValue;

                    SendChangeEvent(copyValue, _value);
                    // _value[thisRowIndex] = evt.newValue;
                    // value = _value;
                });
                _foldout.Add(subField);
                _valueFields[rowIndex] = subField;
                _value[rowIndex] = new bool[column];
            }
        }

        // ReSharper disable once InconsistentNaming
        public new string viewDataKey
        {
            // ReSharper disable once UnusedMember.Global
            get => _foldout.viewDataKey;
            set => _foldout.viewDataKey = value;
        }

        public void SetValueWithoutNotify(bool[][] newValue)
        {
            _value = newValue;
            for (int index = 0; index < _valueFields.Length; index++)
            {
                bool[] newRow = newValue[index];
                RowBooleansElement field = _valueFields[index];
                // Debug.Log($"Matrix SetValueWithoutNotify {index} = {string.Join(",", _value[index])}");
                field.SetValueWithoutNotify(newRow);
                // Debug.Log($"set {index}={string.Join(",", newRow)}");
            }
        }

        // private bool[][] GetCurValue() => _valueFields.Select(each => each.value).ToArray();

        public bool[][] value
        {
            get => _value;
            set
            {
                // bool[][] curValue = GetCurValue();
                if (SeqEqual(_value, value))
                {
                    return;
                }

                bool[][] previous = (bool[][])_value.Clone();

                SetValueWithoutNotify(value);

                SendChangeEvent(previous, value);
            }
        }

        private void SendChangeEvent(bool[][] previous, bool[][] newValue)
        {
            using ChangeEvent<bool[][]> evt = ChangeEvent<bool[][]>.GetPooled(previous, newValue);
            evt.target = this;
            SendEvent(evt);
        }

        private static bool SeqEqual(bool[][] curValue, bool[][] newValue)
        {
            if(curValue.Length != newValue.Length)
            {
                return false;
            }

            for (int index = 0; index < curValue.Length; index++)
            {
                bool[] curSub = curValue[index];
                bool[] newSub = newValue[index];
                if (!curSub.SequenceEqual(newSub))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
