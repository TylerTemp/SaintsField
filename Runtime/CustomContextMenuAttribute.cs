using System;
using System.Diagnostics;
using SaintsField.Playa;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
    public class CustomContextMenuAttribute: Attribute, IPlayaAttribute
    {

    }
}
