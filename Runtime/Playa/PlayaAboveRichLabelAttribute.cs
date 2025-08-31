using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class PlayaAboveRichLabelAttribute: PlayaBelowRichLabelAttribute
    {
        public PlayaAboveRichLabelAttribute(string content = "<color=gray><label/>"): base(content)
        {
            Below = false;
        }
    }
}
