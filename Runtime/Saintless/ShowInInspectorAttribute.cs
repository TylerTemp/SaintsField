using System;

namespace SaintsField.Saintless
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ShowInInspectorAttribute: Attribute
    {
    }
}
