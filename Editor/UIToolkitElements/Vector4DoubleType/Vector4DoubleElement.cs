using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.Vector4DoubleType
{
    public class Vector4DoubleElement: BindableElement, INotifyValueChanged<Vector4Double>
    {
        private Vector4Double _value;
        private readonly DoubleField _xField;
        private readonly DoubleField _yField;
        private readonly DoubleField _zField;
        private readonly DoubleField _wField;

        public Vector4DoubleElement()
        {
            style.flexDirection = FlexDirection.Row;

            DoubleField doubleX = CreateDoubleField("X");
            hierarchy.Add(doubleX);
            doubleX.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newX = evt.newValue;
                value = new Vector4Double(newX, _value.y, _value.z, _value.w);
            });
            _xField = doubleX;

            DoubleField doubleY = CreateDoubleField("Y");
            hierarchy.Add(doubleY);
            doubleY.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newY = evt.newValue;
                value = new Vector4Double(_value.x, newY, _value.z, _value.w);
            });
            _yField = doubleY;

            DoubleField doubleZ = CreateDoubleField("Z");
            hierarchy.Add(doubleZ);
            doubleZ.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newZ = evt.newValue;
                value = new Vector4Double(_value.x, _value.y, newZ, _value.w);
            });
            _zField = doubleZ;

            DoubleField doubleW = CreateDoubleField("W");
            hierarchy.Add(doubleW);
            doubleW.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();

                double newW = evt.newValue;
                value = new Vector4Double(_value.x, _value.y, _value.z, newW);
            });
            _wField = doubleW;
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

        public void SetValueWithoutNotify(Vector4Double newValue)
        {
            _value = newValue;
            _xField.SetValueWithoutNotify(newValue.x);
            _yField.SetValueWithoutNotify(newValue.y);
            _zField.SetValueWithoutNotify(newValue.z);
            _wField.SetValueWithoutNotify(newValue.w);
        }

        public Vector4Double value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                Vector4Double previous = _value;

                SetValueWithoutNotify(value);

                using ChangeEvent<Vector4Double> evt = ChangeEvent<Vector4Double>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
