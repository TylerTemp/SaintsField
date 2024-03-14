using System;
using System.Runtime.CompilerServices;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class OrderedAttribute: Attribute, IPlayaAttribute
    {
        public readonly int Order;

        public OrderedAttribute([CallerLineNumber] int order = 0)
        {
            Order = order;
        }
    }
}
