using System;
using System.Diagnostics;
using SaintsField.Interfaces;

namespace SaintsField
{
    [Obsolete("Use Unit instead")]
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class AdaptAttribute: UnitAttribute
    {
        // public override SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        // public override string GroupBy => "";

        public AdaptAttribute(EUnit unit): base(unit)
        {
        }

        public AdaptAttribute(EUnit baseUnit, EUnit displayUnit): base(baseUnit, displayUnit)
        {
        }

        public AdaptAttribute(string unit): base(unit)
        {
        }

        public AdaptAttribute(string baseUnit, string displayUnit): base(baseUnit, displayUnit)
        {
        }

        public AdaptAttribute(EUnit baseUnit, string displayUnit): base(baseUnit, displayUnit)
        {
        }

        public AdaptAttribute(string baseUnit, EUnit displayUnit): base(baseUnit, displayUnit)
        {
        }
    }
}
