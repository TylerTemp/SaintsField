using System;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public HideIfAttribute(params string[] orCallbacks) : base(orCallbacks)
        {
        }
    }
}
