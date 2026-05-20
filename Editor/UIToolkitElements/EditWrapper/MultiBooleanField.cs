using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.EditWrapper
{
    public class MultiBooleanField: BaseField<bool[]>
    {
        private readonly RowBooleansElement _rowBooleansElement;
        public int Count => _rowBooleansElement.Count;
        public MultiBooleanField(string label, RowBooleansElement visualInput) : base(label, visualInput)
        {
            _rowBooleansElement = visualInput;
        }

        public override bool[] value
        {
            get => _rowBooleansElement.value;
            set => _rowBooleansElement.value = value;
        }

        public override void SetValueWithoutNotify(bool[] newValue)
        {
            _rowBooleansElement.SetValueWithoutNotify(newValue);
        }

        public static MultiBooleanField Create(int count, string label,
            bool hasSetter,
            bool labelGrayColor,
            bool inHorizontalLayout)
        {
            MultiBooleanField element = new MultiBooleanField(label, new RowBooleansElement(count, true));
            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, hasSetter, labelGrayColor, inHorizontalLayout);
            return element;
        }
    }
}
