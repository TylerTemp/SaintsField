using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]

    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayStartAttribute: DOTweenPlayAttribute
    {
        public DOTweenPlayStartAttribute(string label = null, ETweenStop stopAction = ETweenStop.Rewind, string groupBy="")
            : base(label, stopAction, groupBy, true)
        {
        }
    }
}
