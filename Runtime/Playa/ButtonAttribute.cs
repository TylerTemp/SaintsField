using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ButtonAttribute: Attribute, IPlayaAttribute, ISaintsMethodAttribute
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
