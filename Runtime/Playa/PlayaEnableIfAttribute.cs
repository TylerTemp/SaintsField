using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaEnableIfAttribute: PlayaDisableIfAttribute
    {
        public PlayaEnableIfAttribute(EMode editorMode, params object[] by) : base(editorMode, by)
        {
        }

        public PlayaEnableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
