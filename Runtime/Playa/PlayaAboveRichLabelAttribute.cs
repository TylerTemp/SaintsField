using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class PlayaAboveRichLabelAttribute: AboveTextAttribute
    {
        public PlayaAboveRichLabelAttribute(string content = "<color=gray><label/>") : base(content)
        {
        }
    }
}
