using System;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.AdaptDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
        }

        public static (string error, double value) GetDoubleValuePre(double originValue)
        {
            return ("", originValue * 100);
        }

        public static (string error, double value) GetDoubleValuePost(double adaptedValue)
        {
            return ("", adaptedValue / 100);
        }
    }
}
