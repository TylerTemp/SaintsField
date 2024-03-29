using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
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
