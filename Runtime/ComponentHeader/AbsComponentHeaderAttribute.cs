using System;
using SaintsField.Playa;

namespace SaintsField.ComponentHeader
{
    public abstract class AbsComponentHeaderAttribute: Attribute, IComponentHeaderAttribute, IPlayaAttribute
    {
        public abstract string GroupBy { get; }
        public abstract bool IsLeft { get; }
    }
}
