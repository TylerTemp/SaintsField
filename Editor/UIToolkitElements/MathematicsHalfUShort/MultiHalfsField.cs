#if SAINTSFIELD_UNITY_MATHEMATICS && !SAINTSFIELD_UNITY_MATHEMATICS_DISABLE
using System;
using SaintsField.Editor.UIToolkitElements.EditWrapper;
using UnityEngine.UIElements;


namespace SaintsField.Editor.UIToolkitElements.MathematicsHalfUShort
{
    public class MultiHalfsField: BaseField<int[]>
    {
        private readonly RowInputsElement<MathematicsHalfUShortField, int> _rowInputsElement;
        public int Count => _rowInputsElement.Count;
        public MultiHalfsField(string label, RowInputsElement<MathematicsHalfUShortField, int> visualInput) : base(label, visualInput)
        {
            _rowInputsElement = visualInput;
        }

        public MultiHalfsField(string label, int count) : this(label, new RowInputsElement<MathematicsHalfUShortField, int>(count, Creator))
        {
        }

        public override int[] value
        {
            get => _rowInputsElement.value;
            set => _rowInputsElement.value = value;
        }

        public override void SetValueWithoutNotify(int[] newValue)
        {
            _rowInputsElement.SetValueWithoutNotify(newValue);
        }

        private static MathematicsHalfUShortField Creator(int index)
        {
            string label = index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "W",
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
            MathematicsHalfUShortField result = new MathematicsHalfUShortField(label)
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

        protected override void UpdateMixedValueContent()
        {
            _rowInputsElement.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
