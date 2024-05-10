using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute
    {
        // public readonly string FuncName;
        public readonly string Label;

        public ButtonAttribute(string label = null)
        {
            // FuncName = funcName;
            Label = label;
        }
    }
}
