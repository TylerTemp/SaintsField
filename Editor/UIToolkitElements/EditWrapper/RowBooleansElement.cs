using System;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.EditWrapper
{
    public class RowBooleansElement: RowInputsElement<Toggle, bool>
    {
        public RowBooleansElement(int count, bool label) : base(count, label? CreateToggle: CreateToggleNoLabel)
        {
        }

        private static Toggle CreateToggle(int index)
        {
            string label = index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "W",
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
            Toggle result = new Toggle(label)
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

        private static Toggle CreateToggleNoLabel(int index)
        {
            Toggle result = new Toggle(null)
            {
                style =
                {
                    minWidth = 20,
                },
            };
            result.labelElement.style.minWidth = 0;
            return result;
        }
    }
}
