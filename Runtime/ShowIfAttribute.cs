using System;

namespace SaintsField
{
    public class ShowIfAttribute: VisibilityAttribute
    {
        public ShowIfAttribute(params string[] orCallbacks) : base(false, orCallbacks)
        {
        }
    }
}
