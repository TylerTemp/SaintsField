using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MarkPlayaMethodAttribute : Attribute, IPlayaAttribute, IPlayaMethodAttribute
    {
    }
}