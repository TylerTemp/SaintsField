using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;


namespace SaintsField.Editor.Drawers.MinMaxSliderDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute), true)]
    public partial class MinMaxSliderAttributeDrawer : SaintsPropertyDrawer
    {
        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public float MinValue;
            public float MaxValue;
            // ReSharper enable InconsistentNaming

            public override string ToString() => $"Meta(min={MinValue}, max={MaxValue}, error={Error ?? "null"})";
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parentTarget)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return new MetaInfo
                {
                    Error = $"Expect Vector2 or Vector2Int, get {property.propertyType}",
                    MinValue = 0,
                    MaxValue = 1,
                };
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue;
            if (minMaxSliderAttribute.MinCallback == null)
            {
                minValue = minMaxSliderAttribute.Min;
            }
            else
            {
                (string getError, float getValue) =
                    Util.GetOf(minMaxSliderAttribute.MinCallback, 0f, property, info, parentTarget);
                // Debug.Log($"get min {getValue} with error {getError}, name={minMaxSliderAttribute.MinCallback} target={parentTarget}/directGet={parentTarget.GetType().GetField(minMaxSliderAttribute.MinCallback).GetValue(parentTarget)}");
                if (!string.IsNullOrEmpty(getError))
                {
                    return new MetaInfo
                    {
                        Error = getError,
                        MinValue = 0,
                        MaxValue = 1,
                    };
                }
                minValue = getValue;
            }

            float maxValue;
            if (minMaxSliderAttribute.MaxCallback == null)
            {
                maxValue = minMaxSliderAttribute.Max;
            }
            else
            {
                (string getError, float getValue) = Util.GetOf(minMaxSliderAttribute.MaxCallback, 0f, property, info, parentTarget);
                if (!string.IsNullOrEmpty(getError))
                {
                    return new MetaInfo
                    {
                        Error = getError,
                        MinValue = 0,
                        MaxValue = 1,
                    };
                }
                maxValue = getValue;
            }

            if (minValue > maxValue)
            {
                return new MetaInfo
                {
                    Error = $"invalid min ({minValue}) max ({maxValue}) value",
                    MinValue = 0,
                    MaxValue = 1,
                };
            }

            if (minMaxSliderAttribute.FreeInput)
            {
                if(property.propertyType == SerializedPropertyType.Vector2)
                {
                    Vector2 curValue = property.vector2Value;
                    minValue = Mathf.Min(minValue, curValue.x);
                    maxValue = Mathf.Max(maxValue, curValue.y);
                }
                else
                {
                    Vector2Int curValue = property.vector2IntValue;
                    minValue = Mathf.Min(minValue, curValue.x);
                    maxValue = Mathf.Max(maxValue, curValue.y);
                }
            }

            return new MetaInfo
            {
                Error = "",
                MinValue = minValue,
                MaxValue = maxValue,
            };
        }


        private static Vector2Int AdjustIntSliderInput(Vector2 changedNewValue, float step, float min, float max)
        {
            if (step <= 0f)
            {
                return new Vector2Int(Mathf.RoundToInt(changedNewValue.x), Mathf.RoundToInt(changedNewValue.y));
            }

            int startStep = Mathf.RoundToInt((changedNewValue.x - min) / step);
            int startValue = Mathf.RoundToInt(min + startStep * Mathf.RoundToInt(step));

            float distance = changedNewValue.y - changedNewValue.x;

            int endValue = Mathf.RoundToInt(startValue + Mathf.RoundToInt(distance / step) * step);
            if (endValue > max)
            {
                endValue = Mathf.RoundToInt(endValue - step);
            }

            return new Vector2Int(startValue, endValue);
        }

        private static Vector2 AdjustFloatSliderInput(Vector2 changedNewValue, float step, float min, float max)
        {
            if (step <= 0f)
            {
                return changedNewValue;
            }

            float startValue = min + Mathf.RoundToInt((changedNewValue.x - min) / step) * step;

            float distance = changedNewValue.y - changedNewValue.x;

            float endValue = startValue + Mathf.RoundToInt(distance / step) * step;
            if (endValue > max)
            {
                endValue -= step;
            }

            return new Vector2(startValue, endValue);
        }

        private static Vector2Int AdjustIntInput(int newValue, int value, float step, float minValue, float maxValue, bool free)
        {
            int startValue = Mathf.Min(newValue, value);
            int endValue = Mathf.Max(newValue, value);
            if (step < 0)
            {
                return free
                    ? new Vector2Int(startValue, endValue)
                    : new Vector2Int(Mathf.RoundToInt(Mathf.Max(startValue, minValue)), Mathf.RoundToInt(Mathf.Min(endValue, maxValue)));
            }

            int startSteppedValue =
                Mathf.RoundToInt(minValue + Mathf.RoundToInt(Mathf.RoundToInt((startValue - minValue) / step) * step));
            if (!free && startSteppedValue < minValue)
            {
                startSteppedValue = Mathf.RoundToInt(minValue);
            }
            int endSteppedValue = startSteppedValue + Mathf.RoundToInt(Mathf.RoundToInt((endValue - startValue) / step) * step);
            if (!free && endSteppedValue > maxValue)
            {
                endSteppedValue = startSteppedValue + Mathf.FloorToInt(Mathf.FloorToInt((maxValue - startSteppedValue) / step) * step);
            }

            return new Vector2Int(startSteppedValue, endSteppedValue);
        }

        private static Vector2 AdjustFloatInput(float newValue, float value, float step, float minValue, float maxValue, bool free)
        {
            float startValue = Mathf.Min(newValue, value);
            float endValue = Mathf.Max(newValue, value);
            if (step < 0)
            {
                return free
                    ? new Vector2(startValue, endValue)
                    : new Vector2(Mathf.Max(startValue, minValue), Mathf.Min(endValue, maxValue));
            }

            float startSteppedValue = minValue + Mathf.RoundToInt((startValue - minValue) / step) * step;
            if (!free && startSteppedValue < minValue)
            {
                startSteppedValue = minValue;
            }
            float endSteppedValue = startSteppedValue + (Mathf.RoundToInt((endValue - startValue) / step) * step);
            if (!free && endSteppedValue > maxValue)
            {
                endSteppedValue = startSteppedValue + (Mathf.FloorToInt((maxValue - startSteppedValue) / step) * step);
            }

            return new Vector2(startSteppedValue, endSteppedValue);
        }

    }
}
