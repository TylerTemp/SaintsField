using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.Vector3DoubleType
{
    public class Vector3DoubleElement: BindableElement, INotifyValueChanged<Vector3Double>
    {
        private Vector3Double _value;
        private readonly DoubleField _xField;
        private readonly DoubleField _yField;
        private readonly DoubleField _zField;

        public Vector3DoubleElement()
        {
            style.flexDirection = FlexDirection.Row;

            DoubleField doubleX = CreateDoubleField("X");
            hierarchy.Add(doubleX);
            doubleX.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newX = evt.newValue;
                value = new Vector3Double(newX, _value.y, _value.z);
            });
            _xField = doubleX;

            DoubleField doubleY = CreateDoubleField("Y");
            hierarchy.Add(doubleY);
            doubleY.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newY = evt.newValue;
                value = new Vector3Double(_value.x, newY, _value.z);
            });
            _yField = doubleY;

            DoubleField doubleZ = CreateDoubleField("Z");
            hierarchy.Add(doubleZ);
            doubleZ.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newZ = evt.newValue;
                value = new Vector3Double(_value.x, _value.y, newZ);
            });
            _zField = doubleZ;
        }

        private static DoubleField CreateDoubleField(string label)
        {
            DoubleField doubleField = new DoubleField(label)
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
            doubleField.labelElement.style.minWidth = 0;
            return doubleField;
        }

        public void SetValueWithoutNotify(Vector3Double newValue)
        {
            _value = newValue;
            _xField.SetValueWithoutNotify(newValue.x);
            _yField.SetValueWithoutNotify(newValue.y);
            _zField.SetValueWithoutNotify(newValue.z);
        }

        public Vector3Double value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                Vector3Double previous = _value;

                SetValueWithoutNotify(value);

                using ChangeEvent<Vector3Double> evt = ChangeEvent<Vector3Double>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
