using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ProgressBarDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(ProgressBarAttribute), true)]
    public partial class ProgressBarAttributeDrawer: SaintsPropertyDrawer
    {
        private struct MetaInfo
        {
            public string Error;

            public float Min;  // dynamic
            public float Max;  // dynamic
            public Color Color;
            public Color BackgroundColor;

            public float Value;
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute) saintsAttribute;

            float propertyValue = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : property.floatValue;

            float min = progressBarAttribute.Min;
            if(progressBarAttribute.MinCallback != null)
            {
                (string error, float value) = Util.GetOf(progressBarAttribute.MinCallback, 0f, property, info, parent);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                        Value = propertyValue,
                    };
                }
                min = value;
            }

            float max = progressBarAttribute.Max;
            if(progressBarAttribute.MaxCallback != null)
            {
                (string error, float value) = Util.GetOf(progressBarAttribute.MaxCallback, 0f, property, info, parent);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                        Value = propertyValue,
                    };
                }
                max = value;
            }

            Color color = progressBarAttribute.Color.GetColor();

            if(progressBarAttribute.ColorCallback != null)
            {
                (string error, Color value) =
                    GetCallbackColor(progressBarAttribute.ColorCallback, color, property, info, parent);

                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                        Value = propertyValue,
                    };
                }
                color = value;
            }

            Color backgroundColor = progressBarAttribute.BackgroundColor.GetColor();
            // ReSharper disable once InvertIf
            if(progressBarAttribute.BackgroundColorCallback != null)
            {
                (string error, Color value) = GetCallbackColor(progressBarAttribute.BackgroundColorCallback, backgroundColor, property, info, parent);
                if (error != "")
                {
                    return new MetaInfo
                    {
                        Error = error,
                        Max = 100f,
                        Value = propertyValue,
                    };
                }
                backgroundColor = value;
            }

            return new MetaInfo
            {
                Error = "",
                Min = min,
                Max = max,
                Color = color,
                BackgroundColor = backgroundColor,
                Value = propertyValue,
            };
        }

        private static (string error, Color value) GetCallbackColor(string by, Color defaultValue, SerializedProperty property, FieldInfo fieldInfo, object target)
        {
            (string error, object value) = Util.GetOf<object>(by, defaultValue, property, fieldInfo, target);
            return error != ""
                ? (error, defaultValue)
                : ObjToColor(value);
        }

        private static (string error, Color color) ObjToColor(object obj)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (obj)
            {
                case Color color:
                    return ("", color);
                case string str:
                    return ("", Colors.GetColorByStringPresent(str));
                case EColor eColor:
                    return ("", eColor.GetColor());
                default:
                    return ($"target is not a color: {obj}", Color.white);
            }
        }

        private static (string error, string title) GetTitle(SerializedProperty property, string titleCallback, float step, float curValue, float minValue, float maxValue, object parent)
        {
            if (titleCallback == null)
            {
                if(property.propertyType == SerializedPropertyType.Integer)
                {
                    return ("", $"{(int)curValue} / {(int)maxValue}");
                }

                if (step <= 0)
                {
                    return ("", $"{curValue} / {maxValue}");
                }

                string valueStr = step.ToString(System.Globalization.CultureInfo.InvariantCulture);
                int decimalPointIndex = valueStr.IndexOf(System.Globalization.CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);

                int decimalPlaces = 0;

                if (decimalPointIndex >= 0)
                {
                    decimalPlaces = valueStr.Length - decimalPointIndex - 1;
                }

                // string formatValue = curValue.ToString("F" + decimalPlaces);
                string formatValue = curValue.ToString($"0.{new string('#', decimalPlaces)}");
                // Debug.Log($"curValue={curValue}, format={formatValue}");

                return ("", $"{formatValue} / {maxValue}");
            }

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypes(parent))
            {
                MethodInfo methodInfo = type.GetMethod(titleCallback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                string title;
                try
                {
                    title = (string)methodInfo.Invoke(parent,
                        new object[] { curValue, minValue, maxValue, property.displayName });
                }
                catch (TargetInvocationException e)
                {
                    Debug.Assert(e.InnerException != null);
                    Debug.LogException(e);
                    return (e.InnerException.Message, null);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return (e.Message, null);
                }

                return ("", title);
            }

            return ($"Can not find method `{titleCallback}` on `{parent}`", null);
        }

        private static float BoundValue(float curValue, float minValue, float maxValue, float step, bool isInt)
        {
            float wrapCurValue = isInt
                ? Mathf.RoundToInt(curValue)
                : curValue;

            return step <= 0
                ? Mathf.Clamp(wrapCurValue, minValue, maxValue)
                : Util.BoundFloatStep(wrapCurValue, minValue, maxValue, step);
        }
    }
}
