using System;
using System.Runtime.CompilerServices;
using SaintsField.Editor.Core;
using SaintsField.Editor.Units;
using UnityEditor;

namespace SaintsField.Editor.Drawers.UnitDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(UnitAttribute))]
    public partial class UnitAttributeDrawer: SaintsPropertyDrawer
    {
        private static readonly ConditionalWeakTable<UnitAttribute, UnitState> States =
            new ConditionalWeakTable<UnitAttribute, UnitState>();

        public static UnitState GetState(UnitAttribute attribute) =>
            States.GetValue(attribute, each => new UnitState(each));

        public static void AddDisplayUnitChangedListener(UnitAttribute attribute, Action listener)
        {
            if (attribute != null)
            {
                GetState(attribute).DisplayUnitChanged += listener;
            }
        }

        private static (bool success, decimal result, string error) Convert(decimal value,
            UnitAttribute attribute, bool toDisplay)
        {
            UnitState state = GetState(attribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return (false, value, state.Error);
            }

            return toDisplay
                ? UnitRegistry.Convert(value, state.BaseUnit, state.DisplayUnit)
                : UnitRegistry.Convert(value, state.DisplayUnit, state.BaseUnit);
        }

        private static (string error, T value) ConvertInteger<T>(T value, UnitAttribute attribute,
            bool toDisplay, Func<decimal, T> cast)
        {
            try
            {
                (bool success, decimal converted, string error) =
                    Convert(System.Convert.ToDecimal(value), attribute, toDisplay);
                if (!success)
                {
                    return (error, value);
                }

                return ("", cast(converted));
            }
            catch (Exception exception)
            {
                return ($"Unit conversion overflow: {exception.Message}", value);
            }
        }

        private static (string error, double value) ConvertDouble(double value, UnitAttribute attribute,
            bool toDisplay)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return ("", value);
            }

            try
            {
                (bool success, decimal converted, string error) = Convert((decimal)value, attribute, toDisplay);
                if (!success)
                {
                    return (error, value);
                }

                return ("", (double)converted);
            }
            catch (Exception exception)
            {
                return ($"Unit conversion overflow: {exception.Message}", value);
            }
        }

        public static (string error, int value) GetIntValuePre(int value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (int)converted);

        public static (string error, sbyte value) GetSByteValuePre(sbyte value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (sbyte)converted);

        public static (string error, byte value) GetByteValuePre(byte value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (byte)converted);

        public static (string error, short value) GetShortValuePre(short value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (short)converted);

        public static (string error, ushort value) GetUShortValuePre(ushort value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (ushort)converted);

        public static (string error, uint value) GetUIntValuePre(uint value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (uint)converted);

        public static (string error, long value) GetLongValuePre(long value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (long)converted);

        public static (string error, ulong value) GetULongValuePre(ulong value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, true, converted => (ulong)converted);

        public static (string error, float value) GetFloatValuePre(float value, UnitAttribute attribute)
        {
            (string error, double converted) = ConvertDouble(value, attribute, true);
            return (error, (float)converted);
        }

        public static (string error, double value) GetDoubleValuePre(double value, UnitAttribute attribute) =>
            ConvertDouble(value, attribute, true);

        public static (string error, decimal value) GetDecimalValuePre(decimal value, UnitAttribute attribute)
        {
            (bool success, decimal converted, string error) = Convert(value, attribute, true);
            return success ? ("", converted) : (error, value);
        }

        public static (string error, int value) GetIntValuePost(int value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (int)converted);

        public static (string error, sbyte value) GetSByteValuePost(sbyte value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (sbyte)converted);

        public static (string error, byte value) GetByteValuePost(byte value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (byte)converted);

        public static (string error, short value) GetShortValuePost(short value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (short)converted);

        public static (string error, ushort value) GetUShortValuePost(ushort value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (ushort)converted);

        public static (string error, uint value) GetUIntValuePost(uint value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (uint)converted);

        public static (string error, long value) GetLongValuePost(long value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (long)converted);

        public static (string error, ulong value) GetULongValuePost(ulong value, UnitAttribute attribute) =>
            ConvertInteger(value, attribute, false, converted => (ulong)converted);

        public static (string error, float value) GetFloatValuePost(float value, UnitAttribute attribute)
        {
            (string error, double converted) = ConvertDouble(value, attribute, false);
            return (error, (float)converted);
        }

        public static (string error, double value) GetDoubleValuePost(double value, UnitAttribute attribute) =>
            ConvertDouble(value, attribute, false);

        public static (string error, decimal value) GetDecimalValuePost(decimal value, UnitAttribute attribute)
        {
            (bool success, decimal converted, string error) = Convert(value, attribute, false);
            return success ? ("", converted) : (error, value);
        }
    }
}
