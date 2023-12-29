using System;

namespace SaintsField.Saintless
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ButtonAttribute: Attribute
    {
        // public readonly string FuncName;
        public readonly string Label;

        public ButtonAttribute(string buttonLabel = null)
        {
            // FuncName = funcName;
            Label = buttonLabel;
        }
    }
}
