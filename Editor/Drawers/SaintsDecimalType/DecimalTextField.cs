using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public class DecimalTextField : TextValueField<decimal>
    {
        /// <summary>
        ///        <para>
        /// USS class name of elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string ussClassName = "saintsfield-decimal-field";

        /// <summary>
        ///        <para>
        /// USS class name of labels in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        ///        <para>
        /// USS class name of input elements in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        protected DecimalInput decimalInput => (DecimalInput)textInputBase;
        // public TextValueInput TextValueInput => (TextValueInput)textInputBase;

        /// <summary>
        ///        <para>
        /// Converts the given long integer to a string.
        /// </para>
        ///      </summary>
        /// <param name="v">The long integer to be converted to string.</param>
        /// <returns>
        ///   <para>The long integer as string.</para>
        /// </returns>
        protected override string ValueToString(decimal v)
        {
            return v.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
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
        protected override decimal StringToValue(string str)
        {
            if (decimal.TryParse(str, out decimal v))
            {
                return v;
            }

            return rawValue;
            // long num;
            // ExpressionEvaluator.Expression expression;
            // bool flag = UINumericFieldsUtils.TryConvertStringToLong(str, this.textInputBase.originalText, out num, out expression);
            // Action<ExpressionEvaluator.Expression> expressionEvaluated = this.expressionEvaluated;
            // if (expressionEvaluated != null)
            //     expressionEvaluated(expression);
            // return flag ? num : this.rawValue;
        }

        // public override decimal value
        // {
        //     get;
        //     set;
        // }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        public DecimalTextField()
            : this(null)
        {
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        public DecimalTextField(int maxLength)
            : this(null, maxLength)
        {
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        /// <param name="label"></param>
        public DecimalTextField(string label, int maxLength = 1000)
            : base(label, maxLength, new DecimalInput())
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            // TODO
            // this.visualInput.AddToClassList(LongField.inputUssClassName);

            AddLabelDragger<decimal>();
        }

        // TODO
        // internal override bool CanTryParse(string textString) => long.TryParse(textString, out long _);
        protected bool CanTryParse(string textString) => decimal.TryParse(textString, out decimal _);

        /// <summary>
        ///        <para>
        /// Applies the values of a 3D delta and a speed from an input device.
        /// </para>
        ///      </summary>
        /// <param name="delta">A vector used to compute the value change.</param>
        /// <param name="speed">A multiplier for the value change.</param>
        /// <param name="startValue">The start value.</param>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, decimal startValue)
        {
            decimalInput.ApplyInputDeviceDelta(delta, speed, startValue);
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

        protected class DecimalInput : TextValueInput
        {
            private DecimalTextField parentDecimalTextField => (DecimalTextField)parent;

            // internal ULongInput() => this.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            internal DecimalInput() => formatString = "G";

            // protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;
            protected override string allowedCharacters => "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, decimal startValue)
            {
                decimal intDragSensitivity = CalculateIntDragSensitivity(startValue);
                // Debug.Log(intDragSensitivity);
                float acceleration = Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                // Debug.Log($"acceleration={acceleration}");
                decimal num1 = StringToValue(text);
                float niceDelta = NiceDelta(delta, acceleration);
                float sensitiveDelta = niceDelta * (float)intDragSensitivity;
                float truncDelta = MathF.Truncate(sensitiveDelta * 1000f) / 1000f;
                // Debug.Log($"{sensitiveDelta}/{niceDelta}/{intDragSensitivity}");
                decimal num2 = ClampMinMaxDecimalValue(
                    truncDelta,
                    num1);
                // Debug.Log($"num1={num1}, roundDelta={sensitiveDelta}");
                if (parentDecimalTextField.isDelayed)
                    text = ValueToString(num2);
                else
                    parentDecimalTextField.value = num2;
            }

            public static decimal CalculateIntDragSensitivity(decimal value)
            {
                // decimal d = SqrtDecimal(value) * new decimal(0.029999999329447746d);
                decimal d = value * new decimal(0.029999999329447746d);
                return d > decimal.One ? d : decimal.One;
            }


            // public static decimal SqrtDecimal(decimal x)
            // {
            //     if (x < 0)
            //         throw new ArgumentException("Cannot take square root of a negative decimal.");
            //
            //     if (x == 0)
            //         return 0;
            //
            //     // Normalize x into the form m * 10^(2k)
            //     int scale = 0;
            //     decimal m = x;
            //
            //     // Reduce scale so exponent is even (for sqrt)
            //     // Example: 123.45 -> 12345 (scale +2, exponent becomes even)
            //     while (m != decimal.Truncate(m))
            //     {
            //         m *= 10;
            //         scale++;
            //     }
            //
            //     if (scale % 2 == 1)
            //     {
            //         m *= 10;
            //         scale++;
            //     }
            //
            //     int k = scale / 2;
            //
            //     // Newton iteration on m (integer-like), but still decimal
            //     decimal guess = m / 2; // decent first guess
            //     for (int i = 0; i < 50; i++)
            //     {
            //         decimal next = (guess + m / guess) / 2;
            //         if (next == guess)
            //             break;
            //         guess = next;
            //     }
            //
            //     // Now re-apply the exponent
            //     // sqrt(x) = sqrt(m) / 10^k
            //     for (int i = 0; i < k; i++)
            //         guess /= 10;
            //
            //     return guess;
            // }

            public static float Acceleration(bool shiftPressed, bool altPressed)
            {
                if (shiftPressed && altPressed)
                {
                    return 0.01f;
                }
                else if (shiftPressed)
                {
                    return 0.4f;
                }
                else if (altPressed)
                {
                    return 0.1f;
                }
                else
                {
                    return 0.01f;
                }
                // return (shiftPressed ? 0.4f : 0.1f) * (altPressed ? 0.025f : 0.1f);
            }

            private static bool s_UseYSign;

            public static float NiceDelta(Vector2 deviceDelta, float acceleration)
            {
                deviceDelta.y = -deviceDelta.y;
                if (Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) /
                    (double)Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > 0.10000000149011612)
                    s_UseYSign = Mathf.Abs(deviceDelta.x) <= (double)Mathf.Abs(deviceDelta.y);
                return s_UseYSign
                    ? Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration
                    : Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
            }

            private static decimal ClampMinMaxDecimalValue(float niceDelta, decimal value)
            {
                decimal decimalDelta = (decimal)niceDelta;
                if (niceDelta < 0)
                {
                    if (value < decimal.MinValue - decimalDelta)
                    {
                        return decimal.MinValue;
                    }
                    return value + decimalDelta;
                }

                if (decimal.MaxValue - decimalDelta < value)
                {
                    return decimal.MaxValue;
                }

                return decimalDelta + value;
            }

            protected override string ValueToString(decimal v) => v.ToString(formatString);

            protected override decimal StringToValue(string str) => parentDecimalTextField.StringToValue(str);
        }
    }

    // public class SaintsDecimalField: DecimalTextField
    // {
    //     // public SaintsDecimalField(string label, SaintsDecimalElement visualInput) : base(label, visualInput)
    //     // {
    //     // }
    //
    //     public SaintsDecimalField(int maxLength, TextValueField<decimal>.TextValueInput textValueInput) : base(maxLength, textValueInput.decimalInput)
    //     {
    //     }
    //
    //     public SaintsDecimalField(string label, int maxLength, DecimalValueInput textValueInput) : base(label, maxLength, textValueInput)
    //     {
    //     }
    //
    //     protected override string ValueToString(SaintsDecimal value)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    //
    //     protected override SaintsDecimal StringToValue(string str)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    //
    //     public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, SaintsDecimal startValue)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    // }
}
