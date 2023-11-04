namespace SaintsField
{
    public class HideIfAttribute: VisibilityAttribute
    {
        public HideIfAttribute(params string[] orCallbacks) : base(true, orCallbacks)
        {
        }
    }
}
