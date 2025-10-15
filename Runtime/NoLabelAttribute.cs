using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class NoLabelAttribute: FieldLabelTextAttribute
    {
        public NoLabelAttribute() : base(null, false)
        {
        }
    }
}
