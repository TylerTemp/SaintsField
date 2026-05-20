#if SAINTSFIELD_UNITY_MATHEMATICS && !SAINTSFIELD_UNITY_MATHEMATICS_DISABLE
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.MathematicsHalfUShort
{
    public class MathematicsHalfUShortField : TextValueField<int>
    {
        public new static readonly string ussClassName = "saintsfield-half-ushort-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        private HalfUShortInput halfInput => (HalfUShortInput)textInputBase;

        public MathematicsHalfUShortField()
            : this(null)
        {
        }

        public MathematicsHalfUShortField(int maxLength)
            : this(null, maxLength)
        {
        }

        public MathematicsHalfUShortField(string label, int maxLength = 1000)
            : base(label, maxLength, new HalfUShortInput())
        {
            formatString = "G9";

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);

            AddLabelDragger<int>();
        }

        public static float HalfToFloat(int value)
        {
            return math.f16tof32((uint)(value & 0xffff));
        }

        public static int FloatToHalf(float value)
        {
            return (int)(math.f32tof16(value) & 0xffff);
        }

        protected override string ValueToString(int value)
        {
            return HalfToFloat(value).ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        protected override int StringToValue(string str)
        {
            if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out float value))
            {
                return FloatToHalf(value);
            }

            return rawValue;
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
        {
            halfInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }

        private class HalfUShortInput : TextValueInput
        {
            private MathematicsHalfUShortField parentHalfField => (MathematicsHalfUShortField)parent;

            internal HalfUShortInput()
            {
                formatString = "G9";
            }

            protected override string allowedCharacters => "0123456789-+.,eENaInfiny";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
            {
                float acceleration = Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                float currentValue = HalfToFloat(StringToValue(text));
                float startFloat = HalfToFloat(startValue);
                float sensitivity = CalculateFloatDragSensitivity(startFloat);
                float newFloatValue = currentValue + NiceDelta(delta, acceleration) * sensitivity;
                int newValue = FloatToHalf(newFloatValue);

                if (parentHalfField.isDelayed)
                {
                    text = ValueToString(newValue);
                }
                else
                {
                    parentHalfField.value = newValue;
                }
            }

            private static float CalculateFloatDragSensitivity(float value)
            {
                return Mathf.Max(Mathf.Pow(Mathf.Abs(value), 0.5f) * 0.03f, 0.0001f);
            }

            private static float Acceleration(bool shiftPressed, bool altPressed)
            {
                return (shiftPressed ? 4f : 1f) * (altPressed ? 0.25f : 1f);
            }

            private static bool s_UseYSign;

            private static float NiceDelta(Vector2 deviceDelta, float acceleration)
            {
                deviceDelta.y = -deviceDelta.y;
                if (Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > 0.1f)
                {
                    s_UseYSign = Mathf.Abs(deviceDelta.x) <= Mathf.Abs(deviceDelta.y);
                }

                return (s_UseYSign ? Mathf.Sign(deviceDelta.y) : Mathf.Sign(deviceDelta.x)) * deviceDelta.magnitude * acceleration;
            }

            protected override string ValueToString(int value)
            {
                return HalfToFloat(value).ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
            }

            protected override int StringToValue(string str)
            {
                return parentHalfField.StringToValue(str);
            }
        }
    }
}
#endif
