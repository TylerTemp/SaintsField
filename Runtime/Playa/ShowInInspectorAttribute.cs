using System;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ShowInInspectorAttribute: Attribute
    {
    }
}
