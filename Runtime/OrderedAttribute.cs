using System;
using System.Runtime.CompilerServices;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class OrderedAttribute: Attribute
    {
        public int Order { get; }

        public OrderedAttribute([CallerLineNumber] int order = 0)
        {
            Order = order;
        }
    }
}
