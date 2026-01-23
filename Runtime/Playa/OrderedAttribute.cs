using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SaintsField.Playa
{
    [Obsolete("SaintsField now can detect order correctly, this attribute is no longer required")]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    // ReSharper disable once UnusedType.Global
    public class OrderedAttribute: Attribute  //, IPlayaAttribute
    {
        // ReSharper disable once NotAccessedField.Global
        public readonly int Order;

        public OrderedAttribute([CallerLineNumber] int order = 0)
        {
            Order = order;
        }
    }
}
