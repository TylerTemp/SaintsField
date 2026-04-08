using System;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public class SaintsDecimalField: BindableElement, INotifyValueChanged<SaintsDecimal>
    {
        public readonly DecimalTextField DecimalTextField;
        public SaintsDecimalField(string label)
        {
            DecimalTextField = new DecimalTextField(label);

            hierarchy.Add(DecimalTextField);
        }

        public void SetValueWithoutNotify(SaintsDecimal newValue)
        {
            bool valueChanged = !DecimalTextField.value.Equals(newValue);
            DecimalTextField.SetValueWithoutNotify(newValue);
            if (valueChanged && _propUpdated != null)
            {
                WriteBackValue(newValue);
            }
        }

        private void WriteBackValue(decimal d)
        {
            int[] bits = decimal.GetBits(value);
            bool up0 = UpdateIntValue(_loProp, bits[0]);
            bool up1 = UpdateIntValue(_midProp, bits[1]);
            bool up2 = UpdateIntValue(_hiProp, bits[2]);
            bool up3 = UpdateIntValue(_flagsProp, bits[3]);

            // ReSharper disable once InvertIf
            if(up0 || up1 || up2 || up3)
            {
                _flagsProp.serializedObject.ApplyModifiedProperties();
                _propUpdated(d);
            }
        }

        private static bool UpdateIntValue(SerializedProperty property, int newValue)
        {
            if (property.intValue == newValue)
            {
                return false;
            }

            property.intValue = newValue;
            return true;
        }

        private void WriteBackValue(SaintsDecimal d) => WriteBackValue(d.GetValue());

        private SerializedProperty _flagsProp;
        private SerializedProperty _hiProp;
        private SerializedProperty _loProp;
        private SerializedProperty _midProp;
        private Action<decimal> _propUpdated;

        public void ManuallyBindProperty(SerializedProperty property, Action<decimal> propUpdated)
        {
            _flagsProp = property.FindPropertyRelative(nameof(SaintsDecimal.flags));
            _hiProp = property.FindPropertyRelative(nameof(SaintsDecimal.hi));
            _loProp = property.FindPropertyRelative(nameof(SaintsDecimal.lo));
            _midProp = property.FindPropertyRelative(nameof(SaintsDecimal.mid));

            WriteSerValueNoNotify();

            _propUpdated = propUpdated;

            DecimalTextField.RegisterValueChangedCallback(v =>
            {
                WriteBackValue(v.newValue);
            });

            AddPropertyTracker(_flagsProp);
            AddPropertyTracker(_hiProp);
            AddPropertyTracker(_loProp);
            AddPropertyTracker(_midProp);
        }

        private void WriteSerValueNoNotify()
        {
            SetValueWithoutNotify(new SaintsDecimal
            {
                hi = _hiProp.intValue,
                lo = _loProp.intValue,
                mid =  _midProp.intValue,
                flags = _flagsProp.intValue,
            });
        }

        private void AddPropertyTracker(SerializedProperty property)
        {
            VisualElement tracker = new VisualElement();
            hierarchy.Add(tracker);
            tracker.TrackPropertyValue(property, _ => WriteSerValueNoNotify());
            tracker.RegisterCallback<DetachFromPanelEvent>(_ => UIToolkitUtils.Unbind(tracker));
        }

        public SaintsDecimal value
        {
            get => DecimalTextField.value;
            set
            {
                if (DecimalTextField.value.Equals(value))
                {
                    return;
                }

                SaintsDecimal previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<SaintsDecimal> evt = ChangeEvent<SaintsDecimal>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }


}
