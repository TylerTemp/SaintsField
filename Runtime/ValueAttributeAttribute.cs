using System;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    public class ValueAttributeAttribute: InjectAttributeBase
    {
        public ValueAttributeAttribute(Type decorator, params object[] parameters) : base(decorator, parameters)
        {
        }
    }
}
