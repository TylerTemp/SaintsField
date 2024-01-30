using System;
using System.Runtime.CompilerServices;

namespace SaintsField.Unsaintly
{
    [Obsolete("Use SaintsField.Playa namespace instead")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class OrderedAttribute: Playa.OrderedAttribute
    {
    }
}
