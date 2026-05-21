using System;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.EditWrapper
{
    public class MultiIntegerField : BaseField<int[]>
    {
        private readonly RowInputsElement<IntegerField, int> _rowInputsElement;

        public int Count => _rowInputsElement.Count;

        public MultiIntegerField(string label, RowInputsElement<IntegerField, int> visualInput) : base(label, visualInput)
        {
            _rowInputsElement = visualInput;
        }

        public MultiIntegerField(string label, int count) : this(label, new RowInputsElement<IntegerField, int>(count, Creator))
        {
        }

        public override int[] value
        {
            get => _rowInputsElement.value;
            set
            {
                if (value.Length == 0)  // BaseField internal call
                {
                    return;
                }
                _rowInputsElement.value = value;
            }
        }

        public override void SetValueWithoutNotify(int[] newValue)
        {
            // Debug.Log($"Multi set {Count} values to {string.Join(", ", newValue)}(length={newValue.Length})");
            if (newValue.Length == 0)  // BaseField internal call
            {
                return;
            }
            _rowInputsElement.SetValueWithoutNotify(newValue);
        }

        private static IntegerField Creator(int index)
        {
            string label = index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "W",
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
            IntegerField result = new IntegerField(label)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexBasis = 0,
                    minWidth = 0,
                },
            };
            result.labelElement.style.minWidth = 0;
            return result;
        }
    }
}
