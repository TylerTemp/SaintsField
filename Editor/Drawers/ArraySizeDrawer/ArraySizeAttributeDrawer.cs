using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ArraySizeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ArraySizeAttribute), true)]
    public partial class ArraySizeAttributeDrawer: SaintsPropertyDrawer
    {
        public static (string error, bool dynamic, int min, int max) GetMinMax(ArraySizeAttribute arraySizeAttribute, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(arraySizeAttribute.Callback))
            {
                return ("", false, arraySizeAttribute.Min, arraySizeAttribute.Max);
            }

            (string error, object result) = Util.GetOf<object>(arraySizeAttribute.Callback, null, property, info, parent);
            if (error != "")
            {
                return ("", true, -1, -1);
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (result)
            {
                case int fixedSize:
                    return ("", true, fixedSize, fixedSize);
                case ValueTuple<int, int> vt:
                    return AdjustDynamicMinMax(vt.Item1, vt.Item2);
                case Vector2 v2:
                    return AdjustDynamicMinMax((int)v2.x, (int)v2.y);
                case Vector2Int v2Int:
                    return AdjustDynamicMinMax(v2Int.x, v2Int.y);
                case Vector3 v3:
                    return AdjustDynamicMinMax((int)v3.x, (int)v3.y);
                case Vector3Int v3Int:
                    return AdjustDynamicMinMax(v3Int.x, v3Int.y);
                default:
                    return ($"Unsupported callback return type {result?.GetType()}({result})", true, -1, -1);
            }
        }

        private static (string error, bool dynamic, int min, int max) AdjustDynamicMinMax(int min, int max)
        {
            if (max == -1)
            {
                return ("", true, min, max);
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (max < min)
            {
                return ("", true, min, -1);
            }

            return ("", true, min, max);
        }
    }
}
