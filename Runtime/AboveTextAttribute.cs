using System;
using System.Diagnostics;
using SaintsField.Playa;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class AboveTextAttribute: BelowTextAttribute
    {
        public AboveTextAttribute(string content = "<color=gray><label/>"): base(content)
        {
            Below = false;
        }
    }
}
