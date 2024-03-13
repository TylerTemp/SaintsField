using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DisableIfAttribute: ReadOnlyAttribute
    {
        public DisableIfAttribute(bool directValue = true, string groupBy = "") : base(directValue, groupBy)
        {
        }

        public DisableIfAttribute(params string[] by) : base(by)
        {
        }
    }
}
