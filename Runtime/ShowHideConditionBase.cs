using System;

namespace SaintsField
{
    public class ShowHideConditionBase: Attribute
    {
        public readonly bool inverted;

        public readonly string propOrMethodName;

        protected ShowHideConditionBase(bool inverted, string condition)
        {
            this.inverted = inverted;
            propOrMethodName = condition;
        }
    }
}
