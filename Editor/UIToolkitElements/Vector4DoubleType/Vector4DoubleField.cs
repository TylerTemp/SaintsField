using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.Vector4DoubleType
{
    public class Vector4DoubleField: BaseField<Vector4Double>
    {
        private readonly Vector4DoubleElement _element;

        public Vector4DoubleField(string label) : this(label, new Vector4DoubleElement())
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public Vector4DoubleField(string label, Vector4DoubleElement visualInput) : base(label, visualInput)
        {
            _element = visualInput;
        }

        public override void SetValueWithoutNotify(Vector4Double newValue)
        {
            _element.SetValueWithoutNotify(newValue);
        }

        public override Vector4Double value
        {
            get => _element.value;
            set => _element.value = value;
        }
    }
}
