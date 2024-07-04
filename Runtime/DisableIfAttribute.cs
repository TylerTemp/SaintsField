using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DisableIfAttribute: ReadOnlyAttribute
    {
        public DisableIfAttribute(EMode editorMode, params object[] by) : base(editorMode, by)
        {
        }

        public DisableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
