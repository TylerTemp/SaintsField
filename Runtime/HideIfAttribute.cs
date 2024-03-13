using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public HideIfAttribute(params string[] andCallbacks) : base(andCallbacks)
        {
        }
    }
}
