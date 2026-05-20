#if SAINTSFIELD_UNITY_MATHEMATICS && !SAINTSFIELD_UNITY_MATHEMATICS_DISABLE
using System;
using SaintsField.Editor.UIToolkitElements.EditWrapper;

namespace SaintsField.Editor.UIToolkitElements.MathematicsHalfUShort
{
    public class RowHalfsElement: RowInputsElement<MathematicsHalfUShortField, int>
    {
        public RowHalfsElement(int count) : base(count, CreateElement)
        {
        }

        private static MathematicsHalfUShortField CreateElement(int index)
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
    }
}
#endif
