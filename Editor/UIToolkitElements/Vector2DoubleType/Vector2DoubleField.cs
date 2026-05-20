using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.Vector2DoubleType
{
    public class Vector2DoubleField: BaseField<Vector2Double>
    {
        private readonly Vector2DoubleElement _element;

        public Vector2DoubleField(string label) : this(label, new Vector2DoubleElement())
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public Vector2DoubleField(string label, Vector2DoubleElement visualInput) : base(label, visualInput)
        {
            _element = visualInput;
        }

        public override void SetValueWithoutNotify(Vector2Double newValue)
        {
            _element.SetValueWithoutNotify(newValue);
        }

        public override Vector2Double value
        {
            get => _element.value;
            set => _element.value = value;
        }
    }
}
