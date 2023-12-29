using System;

namespace SaintsField.Unsaintly
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ShowInInspectorAttribute: Attribute
    {
    }
}
