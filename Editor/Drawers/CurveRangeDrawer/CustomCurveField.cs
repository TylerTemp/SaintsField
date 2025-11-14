using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;

namespace SaintsField.Editor.Drawers.CurveRangeDrawer
{
    public class CustomCurveField: CurveField
    {
        public CustomCurveField(string label): base(label)
        {
        }

        public CustomCurveField(string label, Color color): base(label)
        {
            Type type = typeof(CurveField);
            FieldInfo colorFieldInfo = type.GetField("m_CurveColor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (colorFieldInfo != null)
            {
                colorFieldInfo.SetValue(this, color);
            }
        }
    }
}
