using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    public class ULongField : TextValueField<ulong>
    {
        /// <summary>
        ///        <para>
        /// USS class name of elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string ussClassName = "unity-long-field";
        /// <summary>
        ///        <para>
        /// USS class name of labels in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string labelUssClassName = LongField.ussClassName + "__label";
        /// <summary>
        ///        <para>
        /// USS class name of input elements in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string inputUssClassName = LongField.ussClassName + "__input";

        private ULongField.ULongInput longInput => (ULongField.ULongInput) this.textInputBase;

        /// <summary>
        ///        <para>
        /// Converts the given long integer to a string.
        /// </para>
        ///      </summary>
        /// <param name="v">The long integer to be converted to string.</param>
        /// <returns>
        ///   <para>The long integer as string.</para>
        /// </returns>
        protected override string ValueToString(ulong v)
        {
            return v.ToString(this.formatString, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        ///        <para>
        /// Converts a string to a long integer.
        /// </para>
        ///      </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>
        ///   <para>The long integer parsed from the string.</para>
        /// </returns>
        protected override ulong StringToValue(string str)
        {
            if (ulong.TryParse(str, out ulong v))
            {
                return v;
            }

            return this.rawValue;
            // long num;
            // ExpressionEvaluator.Expression expression;
            // bool flag = UINumericFieldsUtils.TryConvertStringToLong(str, this.textInputBase.originalText, out num, out expression);
            // Action<ExpressionEvaluator.Expression> expressionEvaluated = this.expressionEvaluated;
            // if (expressionEvaluated != null)
            //     expressionEvaluated(expression);
            // return flag ? num : this.rawValue;
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        public ULongField()
            : this((string) null)
        {
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        public ULongField(int maxLength)
            : this((string) null, maxLength)
        {
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        /// <param name="label"></param>
        public ULongField(string label, int maxLength = 1000)
            : base(label, maxLength, (TextValueField<ulong>.TextValueInput) new ULongField.ULongInput())
        {
            this.AddToClassList(LongField.ussClassName);
            this.labelElement.AddToClassList(LongField.labelUssClassName);
            // TODO
            // this.visualInput.AddToClassList(LongField.inputUssClassName);

            this.AddLabelDragger<ulong>();
        }

        // TODO
        // internal override bool CanTryParse(string textString) => long.TryParse(textString, out long _);
        protected bool CanTryParse(string textString) => long.TryParse(textString, out long _);

        /// <summary>
        ///        <para>
        /// Applies the values of a 3D delta and a speed from an input device.
        /// </para>
        ///      </summary>
        /// <param name="delta">A vector used to compute the value change.</param>
        /// <param name="speed">A multiplier for the value change.</param>
        /// <param name="startValue">The start value.</param>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
        {
            this.longInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }

        // [ExcludeFromDocs]
        // [Serializable]
        // public new class UxmlSerializedData : TextInputBaseField<long>.UxmlSerializedData
        // {
        //     [Conditional("UNITY_EDITOR")]
        //     public new static void Register() => TextInputBaseField<long>.UxmlSerializedData.Register();
        //
        //     public override object CreateInstance() => (object) new LongField();
        // }

        /// <summary>
        ///        <para>
        /// Instantiates a LongField using the data read from a UXML file.
        /// </para>
        ///      </summary>
        // [Obsolete("UxmlFactory is deprecated and will be removed. Use UxmlElementAttribute instead.", false)]
        // public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<LongField, LongField.UxmlTraits>
        // {
        // }

        /// <summary>
        ///        <para>
        /// Defines UxmlTraits for the LongField.
        /// </para>
        ///      </summary>
        // [Obsolete("UxmlTraits is deprecated and will be removed. Use UxmlElementAttribute instead.", false)]
        // public new class UxmlTraits : TextValueFieldTraits<long, UxmlLongAttributeDescription>
        // {
        // }

        private class ULongInput : TextValueField<ulong>.TextValueInput
        {
            private ULongField parentULongField => (ULongField) this.parent;

            // internal ULongInput() => this.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            internal ULongInput() => this.formatString = "#######0";

            // protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;
            protected override string allowedCharacters => "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
            {
                double intDragSensitivity = (double) CalculateIntDragSensitivity(startValue);
                float acceleration = Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                ulong num1 = this.StringToValue(this.text);
                ulong num2 = this.ClampMinMaxLongValue((long) Math.Round((double) NiceDelta((Vector2) delta, acceleration) * intDragSensitivity), num1);
                if (this.parentULongField.isDelayed)
                    this.text = this.ValueToString(num2);
                else
                    this.parentULongField.value = num2;
            }

            public static double CalculateIntDragSensitivity(double value)
            {
                return Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5) * 0.029999999329447746);
            }
            public static float Acceleration(bool shiftPressed, bool altPressed)
            {
                return (float) ((shiftPressed ? 4.0 : 1.0) * (altPressed ? 0.25 : 1.0));
            }

            private static bool s_UseYSign;

            public static float NiceDelta(Vector2 deviceDelta, float acceleration)
            {
                deviceDelta.y = -deviceDelta.y;
                if ((double) Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / (double) Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > 0.10000000149011612)
                    s_UseYSign = (double) Mathf.Abs(deviceDelta.x) <= (double) Mathf.Abs(deviceDelta.y);
                return s_UseYSign ? Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration : Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
            }

            private ulong ClampMinMaxLongValue(long niceDelta, ulong value)
            {
                if (niceDelta < 0)
                {
                    ulong posUlongDelta = (ulong)(-niceDelta);
                    if (value < posUlongDelta)
                    {
                        return 0;
                    }

                    return value - posUlongDelta;
                }

                ulong ulongDelta = (ulong)niceDelta;
                if (ulong.MaxValue - ulongDelta < value)
                {
                    return ulong.MaxValue;
                }

                return ulongDelta + value;
            }

            protected override string ValueToString(ulong v) => v.ToString(this.formatString);

            protected override ulong StringToValue(string str) => this.parentULongField.StringToValue(str);
        }
    }
}
