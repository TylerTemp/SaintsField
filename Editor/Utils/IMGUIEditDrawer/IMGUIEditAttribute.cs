using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    internal static class IMGUIEditAttribute
    {
        private sealed class Info
        {
            public string Error = "";
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
        }

        private static readonly Dictionary<string, Info> Infos = new Dictionary<string, Info>();

        private static Info EnsureInfo(string key)
        {
            if (Infos.ContainsKey(key))
            {
                return Infos[key];
            }

            return Infos[key] = new Info();
        }

        public static (bool ok, float height) GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            Attribute attribute = GetPrimaryAttribute(allAttributes, valueType, value);
            if (attribute == null)
            {
                return (false, 0f);
            }

            switch (attribute)
            {
                case ValueButtonsAttribute valueButtonsAttribute:
                    return (true, GetValueButtonsHeight(label, valueType, value, valueButtonsAttribute, targets,
                        richTextTagProvider, foldoutViewKey));
                case EnumToggleButtonsAttribute enumToggleButtonsAttribute
                    when IsEnum(valueType, value):
                    return (true, GetValueButtonsHeight(label, valueType, value, enumToggleButtonsAttribute, targets,
                        richTextTagProvider, foldoutViewKey));
                case DropdownAttribute:
                    return (true, GetDropdownHeight(valueType, value, attribute, targets, foldoutViewKey));
                case PropRangeAttribute:
                    if (IsNumeric(valueType, value))
                    {
                        return (true, EditorGUIUtility.singleLineHeight);
                    }
                    break;
                case LayerAttribute:
                    if (valueType == typeof(int) || value is int || valueType == typeof(LayerMask) || value is LayerMask)
                    {
                        return (true, EditorGUIUtility.singleLineHeight);
                    }
                    break;
                case GuidAttribute:
                    if (valueType == typeof(string) || value is string)
                    {
                        return (true, IMGUITextHeight(inHorizontalLayout));
                    }
                    break;
            }

            if ((valueType == typeof(long) || value is long) &&
                allAttributes.Any(each => each is DateTimeAttribute or TimeSpanAttribute))
            {
                return (true, IMGUITextHeight(inHorizontalLayout));
            }

            return (false, 0f);
        }

        public static bool TryOnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            Attribute attribute = GetPrimaryAttribute(allAttributes, valueType, value);
            if (attribute != null)
            {
                switch (attribute)
                {
                    case ValueButtonsAttribute valueButtonsAttribute:
                        DrawValueButtons(position, label, valueType, value, valueButtonsAttribute, beforeSet,
                            setterOrNull, targets, richTextTagProvider, foldoutViewKey);
                        return true;
                    case EnumToggleButtonsAttribute enumToggleButtonsAttribute
                        when IsEnum(valueType, value):
                        DrawValueButtons(position, label, valueType, value, enumToggleButtonsAttribute, beforeSet,
                            setterOrNull, targets, richTextTagProvider, foldoutViewKey);
                        return true;
                    case DropdownAttribute dropdownAttribute:
                        DrawDropdown(position, label, valueType, value, dropdownAttribute, beforeSet, setterOrNull,
                            targets, richTextTagProvider, foldoutViewKey);
                        return true;
                    case PropRangeAttribute propRangeAttribute:
                        if (TryDrawPropRange(position, label, valueType, value, propRangeAttribute, beforeSet,
                                setterOrNull, labelGrayColor, inHorizontalLayout, targets))
                        {
                            return true;
                        }
                        break;
                    case LayerAttribute:
                        if (TryDrawLayer(position, label, valueType, value, beforeSet, setterOrNull, labelGrayColor,
                                inHorizontalLayout))
                        {
                            return true;
                        }
                        break;
                    case GuidAttribute:
                        if (TryDrawGuidString(position, label, valueType, value, beforeSet, setterOrNull,
                                labelGrayColor, inHorizontalLayout))
                        {
                            return true;
                        }
                        break;
                }
            }

            if (valueType == typeof(long) || value is long)
            {
                if (allAttributes.Any(each => each is DateTimeAttribute))
                {
                    DrawDateTimeTicks(position, label, (long)value, beforeSet, setterOrNull, labelGrayColor,
                        inHorizontalLayout);
                    return true;
                }

                if (allAttributes.Any(each => each is TimeSpanAttribute))
                {
                    DrawTimeSpanTicks(position, label, (long)value, beforeSet, setterOrNull, labelGrayColor,
                        inHorizontalLayout);
                    return true;
                }
            }

            return false;
        }

        private static Attribute GetPrimaryAttribute(IReadOnlyList<Attribute> allAttributes, Type valueType, object value)
        {
            if (allAttributes == null)
            {
                return null;
            }

            foreach (Attribute attribute in allAttributes)
            {
                switch (attribute)
                {
                    case ValueButtonsAttribute:
                    case DropdownAttribute:
                        return attribute;
                    case EnumToggleButtonsAttribute when IsEnum(valueType, value):
                        return attribute;
                }
            }

            foreach (Attribute attribute in allAttributes)
            {
                switch (attribute)
                {
                    case PropRangeAttribute:
                    case LayerAttribute:
                    case GuidAttribute:
                        return attribute;
                }
            }

            return null;
        }

        private static bool IsEnum(Type valueType, object value) =>
            valueType?.BaseType == typeof(Enum) || value is Enum;

        private static bool IsNumeric(Type valueType, object value)
        {
            Type type = value?.GetType() ?? valueType;
            return type == typeof(sbyte)
                   || type == typeof(byte)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(float)
                   || type == typeof(double);
        }

        private static float IMGUITextHeight(bool inHorizontalLayout) =>
            IMGUIPlainDrawer.IMGUIText.GetHeight(inHorizontalLayout);

        private static Type GetUseType(Type valueType, object value) => value?.GetType() ?? valueType ?? typeof(object);

        private static object GetTarget(IReadOnlyList<object> targets) =>
            targets == null || targets.Count == 0 ? null : targets[0];

        private static AdvancedDropdownMetaInfo GetDropdownMeta(Type valueType, object value,
            Attribute attribute, IReadOnlyList<object> targets, bool flat)
        {
            return AdvancedDropdownAttributeDrawer.GetMetaInfoShowInInspector(
                GetUseType(valueType, value),
                (PathedDropdownAttribute)attribute,
                value,
                GetTarget(targets),
                true,
                flat);
        }

        private static float GetDropdownHeight(Type valueType, object value, Attribute attribute,
            IReadOnlyList<object> targets, string foldoutViewKey)
        {
            AdvancedDropdownMetaInfo metaInfo = GetDropdownMeta(valueType, value, attribute, targets, false);
            float result = EditorGUIUtility.singleLineHeight;
            if (metaInfo.Error != "")
            {
                result += ImGuiHelpBox.GetHeight(metaInfo.Error, EditorGUIUtility.currentViewWidth, MessageType.Error);
            }

            return result;
        }

        private static void DrawDropdown(Rect position, string label, Type valueType, object value,
            DropdownAttribute dropdownAttribute, Action<object> beforeSet, Action<object> setterOrNull,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            Info info = EnsureInfo($"{foldoutViewKey}.dropdown");
            AdvancedDropdownMetaInfo metaInfo = GetDropdownMeta(valueType, value, dropdownAttribute, targets, false);
            info.Error = metaInfo.Error;

            Rect fieldRect = EditorGUI.PrefixLabel(new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            }, new GUIContent(label));

            using (new EditorGUI.DisabledScope(setterOrNull == null))
            {
                if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup) && metaInfo.DropdownListValue != null)
                {
                    SaintsTreeDropdownIMGUI dropdown = new SaintsTreeDropdownIMGUI(
                        metaInfo,
                        fieldRect.width,
                        320f,
                        false,
                        (curItem, _) =>
                        {
                            beforeSet?.Invoke(value);
                            setterOrNull?.Invoke(curItem);
                            return null;
                        });
                    PopupWindow.Show(fieldRect, dropdown);
                }
            }

            string display = AdvancedDropdownAttributeDrawer.GetMetaStackDisplay(metaInfo);
            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            info.RichTextDrawer.DrawChunks(drawRect, RichTextDrawer.ParseRichXmlWithProvider(display, richTextTagProvider));

            if (info.Error != "")
            {
                Rect helpRect = new Rect(position)
                {
                    y = fieldRect.yMax,
                    height = Mathf.Max(0f, position.yMax - fieldRect.yMax),
                };
                ImGuiHelpBox.Draw(helpRect, info.Error, MessageType.Error);
            }
        }

        private static float GetValueButtonsHeight(string label, Type valueType, object value, Attribute attribute,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            Info info = EnsureInfo($"{foldoutViewKey}.buttons");
            AdvancedDropdownMetaInfo metaInfo = GetDropdownMeta(valueType, value, attribute, targets, true);
            info.Error = metaInfo.Error;
            ValueButtonRawInfo[] rawInfos = ValueButtonsAttributeDrawer.UtilMakeButtonRawInfos(metaInfo, richTextTagProvider);

            float inputWidth = ValueButtonsAttributeDrawer.UtilGetFieldInputWidth(EditorGUIUtility.currentViewWidth,
                new GUIContent(label));
            bool expanded = IsExpanded(foldoutViewKey, IsNoFold(attribute));
            ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(inputWidth, inputWidth, !expanded, rawInfos,
                    info.RichTextDrawer);

            return EditorGUIUtility.singleLineHeight +
                   ValueButtonsAttributeDrawer.UtilGetBelowHeight(inputWidth, expanded, info.Error, layout);
        }

        private static bool IsNoFold(Attribute attribute) =>
            attribute switch
            {
                ValueButtonsAttribute valueButtonsAttribute => valueButtonsAttribute.NoFold,
                EnumToggleButtonsAttribute enumToggleButtonsAttribute => enumToggleButtonsAttribute.NoFold,
                _ => false,
            };

        private static bool IsExpanded(string foldoutViewKey, bool noFold) =>
            noFold || IMGUIEdit.ViewKey.ContainsKey($"{foldoutViewKey}.buttons.expanded") &&
            IMGUIEdit.ViewKey[$"{foldoutViewKey}.buttons.expanded"];

        private static Rect DrawFoldout(Rect position, string foldoutViewKey)
        {
            Rect foldoutRect = new Rect(position)
            {
                width = ValueButtonsAttributeDrawer.ExpandButtonWidth,
            };
            string key = $"{foldoutViewKey}.buttons.expanded";
            bool expanded = IMGUIEdit.ViewKey.ContainsKey(key) && IMGUIEdit.ViewKey[key];
            bool newExpanded = GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);
            if (newExpanded != expanded)
            {
                IMGUIEdit.ViewKey[key] = newExpanded;
            }

            return new Rect(position)
            {
                x = foldoutRect.xMax,
                width = Mathf.Max(0f, position.width - ValueButtonsAttributeDrawer.ExpandButtonWidth),
            };
        }

        private static void DrawValueButtons(Rect position, string label, Type valueType, object value,
            Attribute attribute, Action<object> beforeSet, Action<object> setterOrNull,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            Info info = EnsureInfo($"{foldoutViewKey}.buttons");
            AdvancedDropdownMetaInfo metaInfo = GetDropdownMeta(valueType, value, attribute, targets, true);
            info.Error = metaInfo.Error;
            ValueButtonRawInfo[] rawInfos = ValueButtonsAttributeDrawer.UtilMakeButtonRawInfos(metaInfo, richTextTagProvider);

            Rect lineRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect fieldRect = EditorGUI.PrefixLabel(lineRect, new GUIContent(label));

            bool expanded = IsExpanded(foldoutViewKey, IsNoFold(attribute));
            ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(fieldRect.width, fieldRect.width, !expanded, rawInfos,
                    info.RichTextDrawer);

            Rect buttonsRect = fieldRect;
            if (!IsNoFold(attribute) && layout.HasSubRow)
            {
                buttonsRect = DrawFoldout(buttonsRect, foldoutViewKey);
            }

            if (layout.Rows.Count > 0)
            {
                ValueButtonsAttributeDrawer.UtilDrawButtonRow(buttonsRect, layout.Rows[0],
                    layout.MainAvailableWidth, info.RichTextDrawer,
                    buttonInfo => IsButtonOn(value, buttonInfo.Value, valueType, attribute),
                    buttonInfo => SetButtonValue(value, buttonInfo.Value, valueType, attribute, beforeSet, setterOrNull));
            }

            Rect belowRect = new Rect(position)
            {
                y = lineRect.yMax,
                height = Mathf.Max(0f, position.yMax - lineRect.yMax),
            };
            ValueButtonsAttributeDrawer.UtilDrawBelow(belowRect, expanded, info.Error, layout, info.RichTextDrawer,
                buttonInfo => IsButtonOn(value, buttonInfo.Value, valueType, attribute),
                buttonInfo => SetButtonValue(value, buttonInfo.Value, valueType, attribute, beforeSet, setterOrNull));
        }

        private static bool IsButtonOn(object currentValue, object buttonValue, Type valueType, Attribute attribute)
        {
            if (attribute is EnumToggleButtonsAttribute && IsFlagsEnum(valueType, currentValue))
            {
                long cur = Convert.ToInt64(currentValue);
                long target = Convert.ToInt64(buttonValue);
                return target == 0 ? cur == 0 : (cur & target) == target;
            }

            return Util.GetIsEqual(currentValue, buttonValue);
        }

        private static void SetButtonValue(object currentValue, object buttonValue, Type valueType, Attribute attribute,
            Action<object> beforeSet, Action<object> setterOrNull)
        {
            if (setterOrNull == null)
            {
                return;
            }

            object newValue = buttonValue;
            if (attribute is EnumToggleButtonsAttribute && IsFlagsEnum(valueType, currentValue))
            {
                Type enumType = GetUseType(valueType, currentValue);
                long cur = Convert.ToInt64(currentValue);
                long target = Convert.ToInt64(buttonValue);
                long result = target == 0 ? 0 : ((cur & target) == target ? cur & ~target : cur | target);
                newValue = Enum.ToObject(enumType, result);
            }

            beforeSet?.Invoke(currentValue);
            setterOrNull(newValue);
        }

        private static bool IsFlagsEnum(Type valueType, object value)
        {
            Type enumType = GetUseType(valueType, value);
            return enumType.IsEnum && Attribute.IsDefined(enumType, typeof(FlagsAttribute));
        }

        private static bool TryDrawPropRange(Rect position, string label, Type valueType, object value,
            PropRangeAttribute propRangeAttribute, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets)
        {
            if (!IsNumeric(valueType, value))
            {
                return false;
            }

            (bool minOk, double min) = GetBound(propRangeAttribute.MinCallback, propRangeAttribute.Min, targets);
            (bool maxOk, double max) = GetBound(propRangeAttribute.MaxCallback, propRangeAttribute.Max, targets);
            if (!minOk || !maxOk)
            {
                return false;
            }

            double cur = Convert.ToDouble(value);
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            double edited = IMGUIPlainDrawer.IMGUIShared.DrawStackedField(position, new GUIContent(label),
                inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.Slider(rect, content, (float)cur, (float)min, (float)max),
                rect => EditorGUI.Slider(rect, (float)cur, (float)min, (float)max));

            if (changed.changed && setterOrNull != null)
            {
                object converted = ConvertNumeric(BoundStep(edited, min, max, propRangeAttribute.Step), value?.GetType() ?? valueType);
                beforeSet?.Invoke(value);
                setterOrNull(converted);
            }

            return true;
        }

        private static (bool ok, double value) GetBound(string callback, double defaultValue,
            IReadOnlyList<object> targets)
        {
            if (string.IsNullOrEmpty(callback))
            {
                return (true, defaultValue);
            }

            object target = GetTarget(targets);
            if (target == null)
            {
                return (false, defaultValue);
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromInstance(target))
            {
                MemberInfo member = type.GetMember(callback, flags).FirstOrDefault();
                object raw = member switch
                {
                    FieldInfo fieldInfo => fieldInfo.GetValue(target),
                    PropertyInfo propertyInfo => propertyInfo.GetValue(target),
                    MethodInfo methodInfo when methodInfo.GetParameters().Length == 0 => methodInfo.Invoke(target, null),
                    _ => null,
                };
                if (raw == null)
                {
                    continue;
                }

                return (true, Convert.ToDouble(raw));
            }

            return (false, defaultValue);
        }

        private static double BoundStep(double value, double min, double max, double step)
        {
            double clamped = Math.Clamp(value, min, max);
            if (step <= 0)
            {
                return clamped;
            }

            return Math.Clamp(Math.Round((clamped - min) / step) * step + min, min, max);
        }

        private static object ConvertNumeric(double value, Type type)
        {
            if (type == typeof(sbyte)) return (sbyte)Math.Clamp(Math.Round(value), sbyte.MinValue, sbyte.MaxValue);
            if (type == typeof(byte)) return (byte)Math.Clamp(Math.Round(value), byte.MinValue, byte.MaxValue);
            if (type == typeof(short)) return (short)Math.Clamp(Math.Round(value), short.MinValue, short.MaxValue);
            if (type == typeof(ushort)) return (ushort)Math.Clamp(Math.Round(value), ushort.MinValue, ushort.MaxValue);
            if (type == typeof(int)) return (int)Math.Clamp(Math.Round(value), int.MinValue, int.MaxValue);
            if (type == typeof(uint)) return (uint)Math.Clamp(Math.Round(value), uint.MinValue, uint.MaxValue);
            if (type == typeof(long)) return (long)Math.Clamp(Math.Round(value), long.MinValue, long.MaxValue);
            if (type == typeof(ulong)) return (ulong)Math.Clamp(Math.Round(value), ulong.MinValue, ulong.MaxValue);
            if (type == typeof(float)) return (float)value;
            return value;
        }

        private static bool TryDrawLayer(Rect position, string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            int current;
            bool isLayerMask = valueType == typeof(LayerMask) || value is LayerMask;
            if (isLayerMask)
            {
                current = ((LayerMask)value).value;
            }
            else if (valueType == typeof(int) || value is int)
            {
                current = (int)value;
            }
            else
            {
                return false;
            }

            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int result = IMGUIPlainDrawer.IMGUIShared.DrawStackedField(position, new GUIContent(label),
                inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.LayerField(rect, content, current),
                rect => EditorGUI.LayerField(rect, current));
            if (changed.changed && setterOrNull != null)
            {
                beforeSet?.Invoke(value);
                setterOrNull(isLayerMask ? (object)(LayerMask)result : result);
            }

            return true;
        }

        private static bool TryDrawGuidString(Rect position, string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            if (valueType != typeof(string) && value is not string)
            {
                return false;
            }

            string current = (string)value;
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            string result = IMGUIPlainDrawer.IMGUIText.DrawDelayedField(position, new GUIContent(label), current,
                inHorizontalLayout, labelGrayColor);
            if (changed.changed && setterOrNull != null && Guid.TryParse(result, out _))
            {
                beforeSet?.Invoke(value);
                setterOrNull(result);
            }

            return true;
        }

        private static void DrawDateTimeTicks(Rect position, string label, long ticks, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            DateTime current = new DateTime(ticks);
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            string result = IMGUIPlainDrawer.IMGUIText.DrawDelayedField(position, new GUIContent(label),
                current.ToString("O", System.Globalization.CultureInfo.InvariantCulture), inHorizontalLayout,
                labelGrayColor);
            if (changed.changed && setterOrNull != null &&
                DateTime.TryParse(result, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                beforeSet?.Invoke(ticks);
                setterOrNull(parsed.Ticks);
            }
        }

        private static void DrawTimeSpanTicks(Rect position, string label, long ticks, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            TimeSpan current = new TimeSpan(ticks);
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            string result = IMGUIPlainDrawer.IMGUIText.DrawDelayedField(position, new GUIContent(label),
                current.ToString("c", System.Globalization.CultureInfo.InvariantCulture), inHorizontalLayout,
                labelGrayColor);
            if (changed.changed && setterOrNull != null &&
                TimeSpan.TryParse(result, System.Globalization.CultureInfo.InvariantCulture, out TimeSpan parsed))
            {
                beforeSet?.Invoke(ticks);
                setterOrNull(parsed.Ticks);
            }
        }
    }
}
