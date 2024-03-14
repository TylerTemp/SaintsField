using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public HideIfAttribute(EMode editorMode, params string[] orCallbacks) : base(editorMode, orCallbacks)
        {
        }

        public HideIfAttribute(params string[] orCallbacks) : base(orCallbacks)
        {
        }
    }
}
