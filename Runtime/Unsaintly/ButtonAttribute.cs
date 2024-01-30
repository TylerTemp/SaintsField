using System;

namespace SaintsField.Unsaintly
{
    [Obsolete("Use SaintsField.Playa namespace instead")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ButtonAttribute: Playa.ButtonAttribute
    {
    }
}
