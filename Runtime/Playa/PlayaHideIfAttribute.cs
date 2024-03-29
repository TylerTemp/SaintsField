using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaHideIfAttribute: PlayaShowIfAttribute
    {
        public PlayaHideIfAttribute(EMode editorMode, params string[] orCallbacks): base(editorMode, orCallbacks)
        {
        }

        public PlayaHideIfAttribute(params string[] orCallbacks): base(orCallbacks)
        {
        }
    }
}
