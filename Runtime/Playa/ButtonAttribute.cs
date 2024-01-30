using System;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ButtonAttribute: Attribute, ISaintsMethodAttribute
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
