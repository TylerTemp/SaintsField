using System;
using System.Diagnostics;
using SaintsField.Playa;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class OnInspectorInitAttribute: Attribute, IPlayaAttribute
    {
    }
}
