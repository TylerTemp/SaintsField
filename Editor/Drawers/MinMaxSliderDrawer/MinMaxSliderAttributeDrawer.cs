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
                (string getError, MemberInfo _, float getValue) =
                    Util.GetOf(minMaxSliderAttribute.MinCallback, 0f, property, info, parentTarget, null);
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
                (string getError, MemberInfo _, float getValue) = Util.GetOf(minMaxSliderAttribute.MaxCallback, 0f, property, info, parentTarget, null);
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

            return new MetaInfo
            {
                Error = "",
                MinValue = minValue,
                MaxValue = maxValue,
            };
        }


        private static Vector2Int RemapIntValue(Vector2Int newValue, float step, float min, float max)
        {
            int minValue = Mathf.RoundToInt(min);
            int maxValue = Mathf.RoundToInt(max);
            int x = Mathf.Clamp(newValue.x, minValue, maxValue);
            int intStep = Mathf.RoundToInt(step);

            return intStep > 1
                ? new Vector2Int(x, Util.BoundIntStep(newValue.y, x, maxValue, intStep))
                : new Vector2Int(x, Mathf.Clamp(newValue.y, x, maxValue));
        }

        private static Vector2 RemapFloatValue(Vector2 newValue, float step, float min, float max)
        {
            float x = Mathf.Clamp(newValue.x, min, max);

            return step > float.Epsilon
                ? new Vector2(x, Util.BoundFloatStep(newValue.y, x, max, step))
                : new Vector2(x, Mathf.Clamp(newValue.y, x, max));
        }

    }
}
