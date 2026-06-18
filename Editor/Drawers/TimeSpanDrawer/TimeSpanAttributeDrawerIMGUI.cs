using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
    public partial class TimeSpanAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public bool ExpandedInitialized;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}[{index}]";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        private static GUIStyle _imageButtonStyle;
        private static GUIStyle ImageButtonStyle => _imageButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(0, 0, 0, 0),
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

        private static Texture2D _expandIcon;
        private static Texture2D _foldIcon;
        private static GUIContent _expandContent;
        private static GUIContent _foldContent;

        protected override bool UseCreateFieldIMGUI => true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width, int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth,
            object parent)
        {
            SerializedProperty ticksProperty = TryGetTicksProperty(property);
            if (ticksProperty == null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            EnsureExpandedInitialized(property, index, info);
            return GetImGuiFieldHeight(property.isExpanded);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SerializedProperty ticksProperty = TryGetTicksProperty(property);
            if (ticksProperty == null)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            EnsureExpandedInitialized(property, index, info);

            bool isSerializedActual = !ReferenceEquals(ticksProperty, property);
            DrawTicksField(position, label, ticksProperty.longValue, property.isExpanded,
                expanded => property.isExpanded = expanded,
                newTicks =>
                {
                    ticksProperty.longValue = newTicks;
                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, isSerializedActual ? new TimeSpan(newTicks) : newTicks);
                });
        }

        internal static float GetImGuiFieldHeight(bool expanded)
        {
            return expanded
                ? EditorGUIUtility.singleLineHeight * 3f + RowGap * 2f
                : EditorGUIUtility.singleLineHeight;
        }

        internal static SerializedProperty TryGetTicksProperty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                return property;
            }

            return property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
        }

        internal static void DrawTicksField(Rect position, GUIContent label, long ticks, bool expanded,
            Action<bool> onExpandedChanged, Action<long> onValueChanged)
        {
            EnsureIconContent();

            Rect fieldRect = string.IsNullOrEmpty(label?.text)
                ? position
                : EditorGUI.PrefixLabel(position, label);

            long originalTicks = ticks;
            long newTicks = expanded
                ? DrawExpandedRows(fieldRect, ticks, onExpandedChanged)
                : DrawCompactRow(fieldRect, ticks, onExpandedChanged);

            if (newTicks != originalTicks)
            {
                onValueChanged?.Invoke(newTicks);
            }
        }

        private static void EnsureExpandedInitialized(SerializedProperty property, int index, FieldInfo info)
        {
            InfoIMGUI infoCache = EnsureKey(property, index);
            if (infoCache.ExpandedInitialized)
            {
                return;
            }

            infoCache.ExpandedInitialized = true;
            property.isExpanded |= HasDefaultExpand(info);
        }

        private static bool HasDefaultExpand(FieldInfo info)
        {
            if (info == null)
            {
                return false;
            }

            foreach (Attribute attribute in ReflectCache.GetCustomAttributes(info))
            {
                if (attribute is FieldDefaultExpandAttribute || attribute is DefaultExpandAttribute)
                {
                    return true;
                }
            }

            return false;
        }

        private static long DrawCompactRow(Rect position, long ticks, Action<bool> onExpandedChanged)
        {
            float buttonWidth = Mathf.Min(EditorGUIUtility.singleLineHeight, position.width);
            float fieldWidth = Mathf.Max(1f,
                position.width - buttonWidth - InlineGap - SeparatorWidth * 2f - InlineGap * 4f);

            float[] widths = ResolveWidths(fieldWidth, new[] { 48f, 28f, 52f }, new[] { 2.2f, 1f, 1.4f });

            float x = position.x;
            Rect totalHourRect = new Rect(x, position.y, widths[0], position.height);
            x = totalHourRect.xMax + InlineGap;
            Rect hourMinuteSeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = hourMinuteSeparatorRect.xMax + InlineGap;
            Rect minuteRect = new Rect(x, position.y, widths[1], position.height);
            x = minuteRect.xMax + InlineGap;
            Rect minuteSecondSeparatorRect = new Rect(x, position.y, SeparatorWidth, position.height);
            x = minuteSecondSeparatorRect.xMax + InlineGap;
            Rect secondRect = new Rect(x, position.y, widths[2], position.height);
            x = secondRect.xMax + InlineGap;
            Rect buttonRect = new Rect(x, position.y, Mathf.Max(1f, position.xMax - x), position.height);

            long result = ticks;
            result = ApplyTotalHours(result,
                DrawDelayedIntField(totalHourRect, GetTotalHours(result), DateTimeUtils.GetHourLabel()));
            DrawSeparator(hourMinuteSeparatorRect, ":");
            result = ApplyMinute(result,
                DrawDelayedIntField(minuteRect, GetTimeSpan(result).Minutes, DateTimeUtils.GetMinuteLabel()));
            DrawSeparator(minuteSecondSeparatorRect, ":");
            result = ApplySecondFloat(result,
                DrawDelayedTextField(secondRect, GetSecondFloatText(result), DateTimeUtils.GetSecondLabel()));

            if (GUI.Button(buttonRect, _expandContent, ImageButtonStyle))
            {
                onExpandedChanged?.Invoke(true);
            }

            return result;
        }

        private static long DrawExpandedRows(Rect position, long ticks, Action<bool> onExpandedChanged)
        {
            Rect dayRow = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect timeRow = new Rect(position)
            {
                y = dayRow.yMax + RowGap,
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect millisecondRow = new Rect(position)
            {
                y = timeRow.yMax + RowGap,
                height = EditorGUIUtility.singleLineHeight,
            };

            long result = ticks;
            result = ApplyDay(result,
                DrawDelayedIntField(dayRow, GetTimeSpan(result).Days, DateTimeUtils.GetDayLabel()));
            result = DrawExpandedTimeRow(timeRow, result, onExpandedChanged);
            result = ApplyMillisecond(result,
                DrawDelayedIntField(millisecondRow, GetTimeSpan(result).Milliseconds, DateTimeUtils.GetMillisecondLabel()));
            return result;
        }

        private static long DrawExpandedTimeRow(Rect position, long ticks, Action<bool> onExpandedChanged)
        {
            float buttonWidth = Mathf.Min(EditorGUIUtility.singleLineHeight, position.width);
            float fieldWidth = Mathf.Max(1f,
                position.width - buttonWidth - InlineGap - SeparatorWidth * 2f - InlineGap * 4f);

            float[] widths = ResolveWidths(fieldWidth, new[] { 28f, 28f, 28f }, new[] { 1f, 1f, 1f });

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
            Rect buttonRect = new Rect(x, position.y, Mathf.Max(1f, position.xMax - x), position.height);

            long result = ticks;
            result = ApplyHour(result,
                DrawDelayedIntField(hourRect, GetTimeSpan(result).Hours, DateTimeUtils.GetHourLabel()));
            DrawSeparator(hourMinuteSeparatorRect, ":");
            result = ApplyMinute(result,
                DrawDelayedIntField(minuteRect, GetTimeSpan(result).Minutes, DateTimeUtils.GetMinuteLabel()));
            DrawSeparator(minuteSecondSeparatorRect, ":");
            result = ApplySecond(result,
                DrawDelayedIntField(secondRect, GetTimeSpan(result).Seconds, DateTimeUtils.GetSecondLabel()));

            if (GUI.Button(buttonRect, _foldContent, ImageButtonStyle))
            {
                onExpandedChanged?.Invoke(false);
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

        private static string DrawDelayedTextField(Rect position, string value, string overlay)
        {
            string newValue = EditorGUI.DelayedTextField(position, GUIContent.none, value);
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

        private static long ApplyDay(long ticks, int newDay)
        {
            TimeSpan current = GetTimeSpan(ticks);
            int safeDay = Mathf.Clamp(newDay, 0, TimeSpan.MaxValue.Days);
            return safeDay == current.Days
                ? ticks
                : new TimeSpan(safeDay, current.Hours, current.Minutes, current.Seconds, current.Milliseconds).Ticks;
        }

        private static long ApplyTotalHours(long ticks, int newTotalHours)
        {
            TimeSpan current = GetTimeSpan(ticks);
            int safeTotalHours = Mathf.Clamp(newTotalHours, 0, (int)TimeSpan.MaxValue.TotalHours);
            return safeTotalHours == (int)current.TotalHours
                ? ticks
                : new TimeSpan(0, safeTotalHours, current.Minutes, current.Seconds, current.Milliseconds).Ticks;
        }

        private static long ApplyHour(long ticks, int newHour)
        {
            TimeSpan current = GetTimeSpan(ticks);
            int safeHour = Mathf.Clamp(newHour, 0, 23);
            return safeHour == current.Hours
                ? ticks
                : new TimeSpan(current.Days, safeHour, current.Minutes, current.Seconds, current.Milliseconds).Ticks;
        }

        private static long ApplyMinute(long ticks, int newMinute)
        {
            TimeSpan current = GetTimeSpan(ticks);
            int safeMinute = Mathf.Clamp(newMinute, 0, 59);
            return safeMinute == current.Minutes
                ? ticks
                : new TimeSpan(current.Days, current.Hours, safeMinute, current.Seconds, current.Milliseconds).Ticks;
        }

        private static long ApplySecond(long ticks, int newSecond)
        {
            TimeSpan current = GetTimeSpan(ticks);
            int safeSecond = Mathf.Clamp(newSecond, 0, 59);
            return safeSecond == current.Seconds
                ? ticks
                : new TimeSpan(current.Days, current.Hours, current.Minutes, safeSecond, current.Milliseconds).Ticks;
        }

        private static long ApplyMillisecond(long ticks, int newMillisecond)
        {
            TimeSpan current = GetTimeSpan(ticks);
            int safeMillisecond = Mathf.Clamp(newMillisecond, 0, 999);
            return safeMillisecond == current.Milliseconds
                ? ticks
                : new TimeSpan(current.Days, current.Hours, current.Minutes, current.Seconds, safeMillisecond).Ticks;
        }

        private static long ApplySecondFloat(long ticks, string text)
        {
            string trimmed = (text ?? "").Trim();
            if (trimmed == "")
            {
                return ticks;
            }

            int integerPart;
            int fractionalPart;
            if (trimmed.Contains(".") && !trimmed.EndsWith("."))
            {
                string[] parts = trimmed.Split('.');
                if (parts.Length == 0 || !int.TryParse(parts[0], out integerPart))
                {
                    return ticks;
                }

                string fractionText = parts.Length > 1 ? parts[1] : "";
                if (fractionText.Length > 3)
                {
                    fractionText = fractionText[..3];
                }

                fractionalPart = 0;
                if (fractionText.Length > 0 && int.TryParse(fractionText, out int parsedFraction))
                {
                    fractionalPart = parsedFraction * (int)Mathf.Pow(10, 3 - fractionText.Length);
                }
            }
            else
            {
                if (!float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue) &&
                    !float.TryParse(trimmed, out floatValue))
                {
                    return ticks;
                }

                integerPart = Mathf.Clamp((int)floatValue, 0, 59);
                fractionalPart = Mathf.Clamp(
                    Mathf.RoundToInt((floatValue - Mathf.Floor(floatValue)) * 1000f), 0, 999);
            }

            TimeSpan current = GetTimeSpan(ticks);
            int safeSecond = Mathf.Clamp(integerPart, 0, 59);
            return safeSecond == current.Seconds && fractionalPart == current.Milliseconds
                ? ticks
                : new TimeSpan(current.Days, current.Hours, current.Minutes, safeSecond, fractionalPart).Ticks;
        }

        private static TimeSpan GetTimeSpan(long ticks)
        {
            return new TimeSpan(ticks);
        }

        private static int GetTotalHours(long ticks)
        {
            return (int)GetTimeSpan(ticks).TotalHours;
        }

        private static string GetSecondFloatText(long ticks)
        {
            TimeSpan current = GetTimeSpan(ticks);
            return current.Milliseconds == 0
                ? current.Seconds.ToString(CultureInfo.InvariantCulture)
                : $"{current.Seconds.ToString(CultureInfo.InvariantCulture)}.{current.Milliseconds:000}";
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
            _expandIcon ??= Util.LoadResource<Texture2D>("expand.png");
            _foldIcon ??= Util.LoadResource<Texture2D>("fold.png");
            _expandContent ??= new GUIContent(_expandIcon, "Expand");
            _foldContent ??= new GUIContent(_foldIcon, "Fold");
        }
    }
}
