using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfAttribute: ReadOnlyAttribute
    {
        public EnableIfAttribute(EMode editorMode, params string[] by) : base(editorMode, by)
        {
        }

        public EnableIfAttribute(params string[] by) : base(by)
        {
        }
    }
}
