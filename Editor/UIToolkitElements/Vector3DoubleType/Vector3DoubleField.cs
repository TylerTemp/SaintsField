using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.Vector3DoubleType
{
    public class Vector3DoubleField: BaseField<Vector3Double>
    {
        private readonly Vector3DoubleElement _element;

        public Vector3DoubleField(string label) : this(label, new Vector3DoubleElement())
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public Vector3DoubleField(string label, Vector3DoubleElement visualInput) : base(label, visualInput)
        {
            _element = visualInput;
        }

        public override void SetValueWithoutNotify(Vector3Double newValue)
        {
            _element.SetValueWithoutNotify(newValue);
        }

        public override Vector3Double value
        {
            get => _element.value;
            set => _element.value = value;
        }
    }
}
