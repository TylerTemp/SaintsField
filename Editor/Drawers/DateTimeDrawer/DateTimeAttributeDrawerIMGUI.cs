using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public partial class DateTimeAttributeDrawer
    {
        private static GUIStyle _imageButtonStyle;
        private static GUIStyle ImageButtonStyle => _imageButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(2, 2, 2, 2),
            // margin = new RectOffset(0, 0, 0, 0),
            // border = new RectOffset(0, 0, 0, 0),
            // overflow = new RectOffset(0, 0, 0, 0),
            imagePosition = ImagePosition.ImageOnly,
            alignment = TextAnchor.MiddleCenter
        };
        private static GUIStyle _fieldOverlayStyle;
        private static GUIStyle FieldOverlayStyle => _fieldOverlayStyle ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(0, 4, 0, 0),
        };
        private const float RowGap = 2f;
        private const float InlineGap = 2f;
        private const float SeparatorWidth = 6f;

        private static Texture2D _calendarIcon;
        private static Texture2D _clockIcon;
        private static GUIContent _calendarContent;
        private static GUIContent _clockContent;

        protected override bool UseCreateFieldIMGUI => true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return TryGetTicksProperty(property) == null
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : GetImGuiFieldHeight();
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            SerializedProperty ticksProperty = TryGetTicksProperty(property);
            if (ticksProperty == null)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(new Rect(position)
                {
                    height = SingleLineHeight,
                }, label, overrideRichTextChunks);
                return;
            }

            bool isSerializedActual = !ReferenceEquals(ticksProperty, property);
            Rect fieldRect = DrawTicksField(position, label, ticksProperty.longValue, newTicks =>
            {
                ticksProperty.longValue = newTicks;
                property.serializedObject.ApplyModifiedProperties();
                object changedValue = isSerializedActual ? new DateTime(newTicks) : newTicks;
                TriggerChangedIMGUI(property, changedValue);
            });
            Rect labelRect = new Rect(position)
            {
                height = SingleLineHeight,
                width = position.width - fieldRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
        }

        internal static float GetImGuiFieldHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2f + RowGap;
        }

        internal static SerializedProperty TryGetTicksProperty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                return property;
            }

            return property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
        }

        internal static Rect DrawTicksField(Rect position, GUIContent label, long ticks, Action<long> onValueChanged)
        {
            EnsureIconContent();

            long originalTicks = ClampTicks(ticks);
            long newTicks = originalTicks;

            Rect fieldRect = string.IsNullOrEmpty(label?.text)
                ? position
                : EditorGUI.PrefixLabel(position, label);

            Rect dateRow = new Rect(fieldRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect timeRow = new Rect(fieldRect)
            {
                y = dateRow.yMax + RowGap,
                height = EditorGUIUtility.singleLineHeight,
            };

            newTicks = DrawDateRow(dateRow, newTicks, onValueChanged);
            newTicks = DrawTimeRow(timeRow, newTicks);

            if (newTicks != originalTicks)
            {
                onValueChanged?.Invoke(newTicks);
            }

            return fieldRect;
        }

        private static long DrawDateRow(Rect position, long ticks, Action<long> onValueChanged)
        {
            float buttonWidth = Mathf.Min(EditorGUIUtility.singleLineHeight, position.width);
            float fieldWidth = Mathf.Max(1f,
                position.width - buttonWidth - InlineGap - SeparatorWidth * 2f - InlineGap * 4f);

            float[] widths = ResolveWidths(fieldWidth, new[] { 50f, 32f, 32f }, new[] { 3.5f, 1.25f, 1.25f });

            float x = position.x;
            Rect yearRect = new Rect(x, position.y, widths[0], position.height);
            x = yearRect.xMax + InlineGap;
            Rect yearMonthSeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = yearMonthSeparatorRect.xMax + InlineGap;
            Rect monthRect = new Rect(x, position.y, widths[1], position.height);
            x = monthRect.xMax + InlineGap;
            Rect monthDaySeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = monthDaySeparatorRect.xMax + InlineGap;
            Rect dayRect = new Rect(x, position.y, widths[2], position.height);
            x = dayRect.xMax + InlineGap;
            Rect buttonRect = new Rect(x, position.y, Mathf.Max(1f, position.xMax - x), position.height);

            long result = ticks;
            result = ApplyYear(result,
                DrawDelayedIntField(yearRect, GetDateTime(result).Year, DateTimeUtils.GetYearLabel()));
            DrawSeparator(yearMonthSeparatorRect, "/");
            result = ApplyMonth(result,
                DrawDelayedIntField(monthRect, GetDateTime(result).Month, DateTimeUtils.GetMonthLabel()));
            DrawSeparator(monthDaySeparatorRect, "/");
            result = ApplyDay(result,
                DrawDelayedIntField(dayRect, GetDateTime(result).Day, DateTimeUtils.GetDayLabel()));

            if (GUI.Button(buttonRect, _calendarContent, ImageButtonStyle))
            {
                ShowCalendarPopup(buttonRect, position.width, result, onValueChanged);
            }

            return result;
        }

        private static long DrawTimeRow(Rect position, long ticks)
        {
            float buttonWidth = Mathf.Min(EditorGUIUtility.singleLineHeight, position.width);
            float fieldWidth = Mathf.Max(1f,
                position.width - buttonWidth - InlineGap - SeparatorWidth * 3f - InlineGap * 6f);

            float[] widths = ResolveWidths(fieldWidth, new[] { 28f, 28f, 28f, 40f }, new[] { 1f, 1f, 1f, 1.4f });

            float x = position.x;
            Rect hourRect = new Rect(x, position.y, widths[0], position.height);
            x = hourRect.xMax + InlineGap;
            Rect hourMinuteSeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = hourMinuteSeparatorRect.xMax + InlineGap;
            Rect minuteRect = new Rect(x, position.y, widths[1], position.height);
            x = minuteRect.xMax + InlineGap;
            Rect minuteSecondSeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = minuteSecondSeparatorRect.xMax + InlineGap;
            Rect secondRect = new Rect(x, position.y, widths[2], position.height);
            x = secondRect.xMax + InlineGap;
            Rect secondMillisecondSeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = secondMillisecondSeparatorRect.xMax + InlineGap;
            Rect millisecondRect = new Rect(x, position.y, widths[3], position.height);
            x = millisecondRect.xMax + InlineGap;
            Rect buttonRect = new Rect(x, position.y, Mathf.Max(1f, position.xMax - x), position.height);

            long result = ticks;
            result = ApplyHour(result,
                DrawDelayedIntField(hourRect, GetDateTime(result).Hour, DateTimeUtils.GetHourLabel()));
            DrawSeparator(hourMinuteSeparatorRect, ":");
            result = ApplyMinute(result,
                DrawDelayedIntField(minuteRect, GetDateTime(result).Minute, DateTimeUtils.GetMinuteLabel()));
            DrawSeparator(minuteSecondSeparatorRect, ":");
            result = ApplySecond(result,
                DrawDelayedIntField(secondRect, GetDateTime(result).Second, DateTimeUtils.GetSecondLabel()));
            DrawSeparator(secondMillisecondSeparatorRect, ".");
            result = ApplyMillisecond(result,
                DrawDelayedIntField(millisecondRect, GetDateTime(result).Millisecond, DateTimeUtils.GetMillisecondLabel()));

            if (GUI.Button(buttonRect, _clockContent, ImageButtonStyle))
            {
                result = ApplyCurrentTime(result);
            }

            return result;
        }

        private static void DrawSeparator(Rect position, string content)
        {
            EditorGUI.LabelField(position, content, EditorStyles.centeredGreyMiniLabel);
        }

        private static int DrawDelayedIntField(Rect position, int value, string overlay)
        {
            int newValue = EditorGUI.DelayedIntField(position, GUIContent.none, value);
            DrawFieldOverlay(position, overlay);
            return newValue;
        }

        private static void DrawFieldOverlay(Rect position, string overlay)
        {
            if (string.IsNullOrEmpty(overlay))
            {
                return;
            }

            GUI.Label(position, overlay, FieldOverlayStyle);
        }

        private static long ApplyYear(long ticks, int newYear)
        {
            DateTime current = GetDateTime(ticks);
            int safeYear = Mathf.Max(1, newYear);
            return safeYear == current.Year ? ticks : DateTimeUtils.WrapYear(current.Ticks, safeYear);
        }

        private static long ApplyMonth(long ticks, int newMonth)
        {
            DateTime current = GetDateTime(ticks);
            int safeMonth = Mathf.Clamp(newMonth, 1, 12);
            return safeMonth == current.Month ? ticks : DateTimeUtils.WrapMonth(current.Ticks, safeMonth);
        }

        private static long ApplyDay(long ticks, int newDay)
        {
            DateTime current = GetDateTime(ticks);
            int safeDay = Mathf.Clamp(newDay, 1, DateTime.DaysInMonth(current.Year, current.Month));
            return safeDay == current.Day
                ? ticks
                : new DateTime(current.Year, current.Month, safeDay, current.Hour, current.Minute, current.Second,
                    current.Millisecond, current.Kind).Ticks;
        }

        private static long ApplyHour(long ticks, int newHour)
        {
            DateTime current = GetDateTime(ticks);
            int safeHour = Mathf.Clamp(newHour, 0, 23);
            return safeHour == current.Hour
                ? ticks
                : new DateTime(current.Year, current.Month, current.Day, safeHour, current.Minute, current.Second,
                    current.Millisecond, current.Kind).Ticks;
        }

        private static long ApplyMinute(long ticks, int newMinute)
        {
            DateTime current = GetDateTime(ticks);
            int safeMinute = Mathf.Clamp(newMinute, 0, 59);
            return safeMinute == current.Minute
                ? ticks
                : new DateTime(current.Year, current.Month, current.Day, current.Hour, safeMinute, current.Second,
                    current.Millisecond, current.Kind).Ticks;
        }

        private static long ApplySecond(long ticks, int newSecond)
        {
            DateTime current = GetDateTime(ticks);
            int safeSecond = Mathf.Clamp(newSecond, 0, 59);
            return safeSecond == current.Second
                ? ticks
                : new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, safeSecond,
                    current.Millisecond, current.Kind).Ticks;
        }

        private static long ApplyMillisecond(long ticks, int newMillisecond)
        {
            DateTime current = GetDateTime(ticks);
            int safeMillisecond = Mathf.Clamp(newMillisecond, 0, 999);
            return safeMillisecond == current.Millisecond
                ? ticks
                : new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute,
                    current.Second, safeMillisecond, current.Kind).Ticks;
        }

        private static long ApplyCurrentTime(long ticks)
        {
            DateTime now = DateTime.Now;
            if (ticks == 0)
            {
                return now.Ticks;
            }

            DateTime current = GetDateTime(ticks);
            return new DateTime(current.Year, current.Month, current.Day, now.Hour, now.Minute, now.Second,
                now.Millisecond, current.Kind).Ticks;
        }

        private static DateTime GetDateTime(long ticks)
        {
            return new DateTime(ClampTicks(ticks));
        }

        private static long ClampTicks(long ticks)
        {
            if (ticks < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue.Ticks;
            }

            if (ticks > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue.Ticks;
            }

            return ticks;
        }

        private static float[] ResolveWidths(float totalWidth, float[] minWidths, float[] weights)
        {
            float[] result = new float[minWidths.Length];
            float minWidthSum = 0f;
            float weightSum = 0f;
            for (int index = 0; index < minWidths.Length; index++)
            {
                minWidthSum += minWidths[index];
                weightSum += weights[index];
            }

            if (totalWidth <= minWidthSum + 0.01f)
            {
                float scale = minWidthSum < Mathf.Epsilon ? 1f : totalWidth / minWidthSum;
                for (int index = 0; index < result.Length; index++)
                {
                    result[index] = Mathf.Max(1f, minWidths[index] * scale);
                }

                return result;
            }

            float extraWidth = totalWidth - minWidthSum;
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = minWidths[index] + extraWidth * (weights[index] / weightSum);
            }

            return result;
        }

        private static void EnsureIconContent()
        {
            _calendarIcon ??= Util.LoadResource<Texture2D>("calendar.png");
            _clockIcon ??= Util.LoadResource<Texture2D>("clock.png");
            _calendarContent ??= new GUIContent(_calendarIcon, "Open Calendar Picker");
            _clockContent ??= new GUIContent(_clockIcon, "Set Time To Now");
        }

        private static void ShowCalendarPopup(Rect buttonRect, float rowWidth, long ticks, Action<long> onValueChanged)
        {
#if UNITY_2021_3_OR_NEWER
            DateTimeElementDropdown dropdown =
                new DateTimeElementDropdown(false, Mathf.Min(Mathf.Max(rowWidth, 1f), 250f), 200f);
            dropdown.AttachedEvent.AddListener(() =>
            {
                dropdown.value = ClampTicks(ticks);
                dropdown.OnValueChanged.AddListener(v => onValueChanged?.Invoke(v));
                dropdown.DropdownYearPanel.schedule.Execute(dropdown.DropdownYearPanel.UpdateScrollTo).StartingIn(50);
            });
            PopupWindow.Show(buttonRect, dropdown);
#endif
        }
    }
}
