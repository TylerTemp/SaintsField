using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaEnableIfAttribute: PlayaDisableIfAttribute
    {
        public PlayaEnableIfAttribute(EMode editorMode, params string[] by) : base(editorMode, by)
        {
        }

        public PlayaEnableIfAttribute(params string[] by) : base(by)
        {
        }
    }
}
