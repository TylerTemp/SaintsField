using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ShowIfAttribute: ShowHideConditionBase
    {
        public ShowIfAttribute(string condition)
            : base(false, condition)
        {
        }
    }
}
