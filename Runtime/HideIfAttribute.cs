namespace SaintsField
{
    public class HideIfAttribute: VisibilityAttribute
    {
        public HideIfAttribute(params string[] andCallbacks) : base(true, andCallbacks)
        {
        }
    }
}
