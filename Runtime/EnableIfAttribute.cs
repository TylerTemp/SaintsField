using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfAttribute: ReadOnlyAttribute
    {
        public EnableIfAttribute(bool directValue=true, string groupBy=""): base(!directValue, groupBy)
        {
        }

        public EnableIfAttribute(params string[] by): base(by)
        {
            if (by.Length == 0)
            {
                readOnlyDirectValue = false;
            }
        }
    }
}
