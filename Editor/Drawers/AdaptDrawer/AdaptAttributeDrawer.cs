using System;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.AdaptDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AdaptAttribute), true)]
    public partial class AdaptAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, string display) GetDisplay(SerializedProperty property, AdaptAttribute adaptAttribute)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return ("", $"{property.longValue:P}");
                case SerializedPropertyType.Float:
                    return ("", $"{property.doubleValue:P}");
                case SerializedPropertyType.Vector2:
                    return ("", $"Vector2({property.vector2Value.x}, {property.vector2Value.y})");
                case SerializedPropertyType.Vector2Int:
                    return ("", $"Vector2Int({property.vector2IntValue.x}, {property.vector2IntValue.y})");
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
        }

        public static (string error, int value) GetIntValuePre(int originValue)
        {
            if (int.MaxValue / 100 < originValue)
            {
                return ($"int overflow {originValue}", int.MaxValue);
            }

            if (int.MinValue / 100 > originValue)
            {
                return ($"int overflow {originValue}", int.MinValue);
            }
            return ("", originValue * 100);
        }
        public static (string error, uint value) GetUIntValuePre(uint originValue)
        {
            if (uint.MaxValue / 100 < originValue)
            {
                return ($"uint overflow {originValue}", uint.MaxValue);
            }
            return ("", originValue * 100);
        }

        public static (string error, double value) GetDoubleValuePre(double originValue)
        {
            if (double.MaxValue / 100 < originValue)
            {
                return ($"double overflow {originValue}", double.MaxValue);
            }

            if (double.MinValue / 100 > originValue)
            {
                return ($"double overflow {originValue}", double.MinValue);
            }
            return ("", originValue * 100);
        }

        public static (string error, float value) GetFloatValuePre(float originValue)
        {
            if (float.MaxValue / 100 < originValue)
            {
                return ($"float overflow {originValue}", float.MaxValue);
            }

            if (float.MinValue / 100 > originValue)
            {
                return ($"float overflow {originValue}", float.MinValue);
            }
            return ("", originValue * 100);
        }

        public static (string error, long value) GetLongValuePre(long originValue)
        {
            if (long.MaxValue / 100 < originValue)
            {
                return ($"long overflow {originValue}", long.MaxValue);
            }

            if (long.MinValue / 100 > originValue)
            {
                return ($"long overflow {originValue}", long.MinValue);
            }

            return ("", originValue * 100);
        }

        public static (string error, ulong value) GetULongValuePre(ulong originValue)
        {
            if (ulong.MaxValue / 100 < originValue)
            {
                return ($"long overflow {originValue}", ulong.MaxValue);
            }
            return ("", originValue * 100);
        }

        public static (string error, double value) GetDoubleValuePost(double adaptedValue)
        {
            return ("", adaptedValue / 100);
        }
        public static (string error, float value) GetFloatValuePost(float adaptedValue)
        {
            return ("", adaptedValue / 100);
        }

        public static (string error, int value) GetIntValuePost(int value)
        {
            return ("", value / 100);
        }

        public static (string error, long value) GetLongValuePost(long value)
        {
            return ("", value / 100);
        }

        public static (string error, uint value) GetUIntValuePost(uint value)
        {
            return ("", value / 100);
        }

        public static (string error, ulong value) GetULongValuePost(ulong value)
        {
            return ("", value / 100);
        }
    }
}
