using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.QuaternionType
{
    public class QuaternionField: BaseField<Quaternion>, INotifyValueChanged<Quaternion>
    {
        private readonly QuaternionElement _element;
        public QuaternionField(string label, QuaternionElement visualInput) : base(label, visualInput)
        {
            _element = visualInput;
        }
        public QuaternionField(string label) : this(label, new QuaternionElement())
        {
        }

        public override void SetValueWithoutNotify(Quaternion newValue)
        {
            _element.SetValueWithoutNotify(newValue);
        }

        public override Quaternion value
        {
            get => _element.value;
            set => _element.value = value;
        }
    }
}
