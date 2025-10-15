using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class NoLabelAttribute: FieldRichLabelAttribute
    {
        public NoLabelAttribute() : base(null, false)
        {
        }
    }
}
