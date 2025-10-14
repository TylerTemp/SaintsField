using System;
using System.Collections.Generic;
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

        private static bool IsChinese()
        {
            // return true;
            return Application.systemLanguage == SystemLanguage.Chinese ||
                   Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                   Application.systemLanguage == SystemLanguage.ChineseTraditional;
        }

        public static string GetYearLabel() => IsChinese() ? "年" : "Y";
        public static string GetMonthLabel() => IsChinese() ? "月" : "M";
        public static string GetDayLabel() => IsChinese() ? "日" : "D";
        public static string GetHourLabel() => IsChinese() ? "时" : "H";
        public static string GetMinuteLabel() => IsChinese() ? "分" : "M";
        public static string GetSecondLabel() => IsChinese() ? "秒" : "S";
        public static string GetMillisecondLabel() => IsChinese() ? "毫" : "Ms";
        public static IReadOnlyList<string> GetWeekLabels() => IsChinese()
            ? new[]{"日", "一", "二", "三", "四", "五", "六"}
            : new[]{"S", "M", "T", "W", "T", "F", "S"};

        public static string GetThisYearLabel() => IsChinese()
            ? "今年"
            : "This Year";
        public static string GetTodayLabel() => IsChinese()
            ? "今天"
            : "Today";
    }
}
