using System;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public static class DateTimeUtils
    {
        public static long WrapYear(long oldValue, int newYear)
        {
            DateTime oldDate = new DateTime(oldValue);
            DateTime newDate = new DateTime(newYear, oldDate.Month, oldDate.Day, oldDate.Hour, oldDate.Minute, oldDate.Second, oldDate.Millisecond, oldDate.Kind);
            return newDate.Ticks;
        }

        public static long WrapMonth(long oldValue, int newMonth)
        {
            DateTime oldDate = new DateTime(oldValue);
            int day = Math.Min(oldDate.Day, DateTime.DaysInMonth(oldDate.Year, newMonth));
            DateTime newDate = new DateTime(oldDate.Year, newMonth, day, oldDate.Hour, oldDate.Minute, oldDate.Second, oldDate.Millisecond, oldDate.Kind);
            return newDate.Ticks;
        }
    }
}
