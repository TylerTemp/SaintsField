using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public override bool IsShow => false;
        public HideIfAttribute(EMode editorMode, params object[] orCallbacks) : base(editorMode, orCallbacks)
        {
        }

        public HideIfAttribute(params object[] orCallbacks) : base(0, orCallbacks)
        {
        }
    }
}
