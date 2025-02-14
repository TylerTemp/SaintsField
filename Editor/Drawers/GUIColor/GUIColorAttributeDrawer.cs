using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.GUIColor
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GUIColorAttribute), true)]
    public partial class GUIColorAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, Color color) GetColor(GUIColorAttribute guiColorAttribute, SerializedProperty property, FieldInfo info, object target)
        {
            if (!guiColorAttribute.IsCallback)
            {
                return ("", guiColorAttribute.Color);
            }
            (string error, object result) = Util.GetOf<object>(guiColorAttribute.Callback, null, property, info, target);
            if (error != "")
            {
                return (error, default);
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (result)
            {
                case Color color:
                    return ("", color);
                case string hex:
                {
                    return ColorUtility.TryParseHtmlString(hex, out Color color)
                        ? ("", color)
                        : ($"Could not parse color from hex string: {hex}", default);
                }
                default:
                    return ($"{result} ({result?.GetType()}) is not a valid color", default);
            }

        }
    }
}
