using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.Vector2DoubleType
{
    public class Vector2DoubleElement: BindableElement, INotifyValueChanged<Vector2Double>
    {
        private Vector2Double _value;
        private readonly DoubleField _xField;
        private readonly DoubleField _yField;

        public Vector2DoubleElement()
        {
            style.flexDirection = FlexDirection.Row;
            DoubleField doubleX = new DoubleField("X")
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexBasis = 0,
                    minWidth = 0,
                    marginRight = 3,
                },
            };
            doubleX.labelElement.style.minWidth = 0;
            hierarchy.Add(doubleX);

            doubleX.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newX = evt.newValue;
                value = new Vector2Double(newX, _value.y);
            });
            _xField = doubleX;

            DoubleField doubleY = new DoubleField("Y")
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexBasis = 0,
                    minWidth = 0,
                    marginRight = 3,
                },
            };
            doubleY.labelElement.style.minWidth = 0;
            hierarchy.Add(doubleY);

            doubleY.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newY  = evt.newValue;
                value = new Vector2Double(_value.x, newY);
            });
            _yField = doubleY;
        }

        public void SetValueWithoutNotify(Vector2Double newValue)
        {
            _value = newValue;
            _xField.SetValueWithoutNotify(newValue.x);
            _yField.SetValueWithoutNotify(newValue.y);
        }

        public Vector2Double value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                Vector2Double previous = _value;

                SetValueWithoutNotify(value);

                using ChangeEvent<Vector2Double> evt = ChangeEvent<Vector2Double>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
