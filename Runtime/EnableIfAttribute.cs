using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfAttribute: ReadOnlyAttribute
    {
        public EnableIfAttribute(EMode editorMode, params object[] by) : base(editorMode, by)
        {
        }

        public EnableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
