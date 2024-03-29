using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DisableIfAttribute: ReadOnlyAttribute
    {
        // public DisableIfAttribute(EMode editorMode, string groupBy = "") : base(editorMode, groupBy)
        // {
        // }

        public DisableIfAttribute(EMode editorMode, params string[] by) : base(editorMode, by)
        {
        }

        public DisableIfAttribute(params string[] by) : base(by)
        {
        }
    }
}
