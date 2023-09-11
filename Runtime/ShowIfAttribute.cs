using System;

namespace ExtInspector
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
