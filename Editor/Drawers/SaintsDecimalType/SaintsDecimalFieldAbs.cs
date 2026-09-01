#if UNITY_2021_2_OR_NEWER
using System;
using SaintsField.Editor.Drawers.UnitDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public abstract class SaintsDecimalFieldAbs: BaseField<SaintsDecimal>
    {
        public readonly DecimalTextField DecimalTextField;
        private readonly AdaptAttribute _adaptAttribute;

        private SaintsDecimalFieldAbs(string label, AdaptAttribute adaptAttribute, DecimalTextField visualInput)
            : base(label, visualInput)
        {
            _adaptAttribute = adaptAttribute;
            DecimalTextField = visualInput;
            DecimalTextField.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                decimal baseValue = evt.newValue;
                if (_adaptAttribute != null)
                {
                    (string error, decimal converted) =
                        UnitAttributeDrawer.GetDecimalValuePost(evt.newValue, _adaptAttribute);
                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError(error);
                        SetValueWithoutNotify(value);
                        return;
                    }
                    baseValue = converted;
                }

                value = baseValue;
            });

            UnitAttributeDrawer.AddDisplayUnitChangedListener(_adaptAttribute,
                () => SetValueWithoutNotify(value));
        }

        protected SaintsDecimalFieldAbs(string label, AdaptAttribute adaptAttribute = null)
            : this(label, adaptAttribute, new DecimalTextField(""))
        {
        }

        protected override void UpdateMixedValueContent()
        {
            DecimalTextField.showMixedValue = showMixedValue;
        }

        public override void SetValueWithoutNotify(SaintsDecimal newValue)
        {
            base.SetValueWithoutNotify(newValue);
            decimal displayValue = newValue;
            if (_adaptAttribute != null)
            {
                (string error, decimal converted) =
                    UnitAttributeDrawer.GetDecimalValuePre(newValue, _adaptAttribute);
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError(error);
                }
                else
                {
                    displayValue = converted;
                }
            }
            DecimalTextField.SetValueWithoutNotify(displayValue);
        }

        private void WriteBackValue(decimal d)
        {
            int[] bits = decimal.GetBits(d);
            bool up0 = SaintsDecimalDrawer.UpdateIntValue(_loProp, bits[0]);
            bool up1 = SaintsDecimalDrawer.UpdateIntValue(_midProp, bits[1]);
            bool up2 = SaintsDecimalDrawer.UpdateIntValue(_hiProp, bits[2]);
            bool up3 = SaintsDecimalDrawer.UpdateIntValue(_flagsProp, bits[3]);

            // ReSharper disable once InvertIf
            if(up0 || up1 || up2 || up3)
            {
                _flagsProp.serializedObject.ApplyModifiedProperties();
                _propUpdated(d);
            }
        }

        private SerializedProperty _flagsProp;
        private SerializedProperty _hiProp;
        private SerializedProperty _loProp;
        private SerializedProperty _midProp;
        private Action<decimal> _propUpdated;

        public void ManuallyBindProperty(SerializedProperty property, Action<decimal> propUpdated)
        {
            _flagsProp =  GetFlagsProp(property);
            _hiProp =  GetHiProp(property);
            _loProp =  GetLoProp(property);
            _midProp =  GetMidProp(property);

            WriteSerValueNoNotify();

            _propUpdated = propUpdated;

            AddPropertyTracker(_flagsProp);
            AddPropertyTracker(_hiProp);
            AddPropertyTracker(_loProp);
            AddPropertyTracker(_midProp);
        }

        protected abstract SerializedProperty GetFlagsProp(SerializedProperty property);
        protected abstract SerializedProperty GetHiProp(SerializedProperty property);
        protected abstract SerializedProperty GetLoProp(SerializedProperty property);
        protected abstract SerializedProperty GetMidProp(SerializedProperty property);

        private void WriteSerValueNoNotify()
        {
            showMixedValue = _flagsProp.hasMultipleDifferentValues
                             || _hiProp.hasMultipleDifferentValues
                             || _loProp.hasMultipleDifferentValues
                             || _midProp.hasMultipleDifferentValues;

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

        public override SaintsDecimal value
        {
            get => base.value;
            set
            {
                if (base.value.Equals(value))
                {
                    return;
                }

                SaintsDecimal previous = this.value;
                SetValueWithoutNotify(value);
                if (_propUpdated != null)
                {
                    WriteBackValue(value.GetValue());
                }

                using ChangeEvent<SaintsDecimal> evt = ChangeEvent<SaintsDecimal>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }


}
#endif
