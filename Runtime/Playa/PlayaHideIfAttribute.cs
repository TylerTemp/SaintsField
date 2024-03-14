using System;

namespace SaintsField.Playa
{
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
