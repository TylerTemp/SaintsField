namespace SaintsField
{
    public class ShowIfAttribute: VisibilityAttribute
    {
        public ShowIfAttribute(params string[] andCallbacks) : base(false, andCallbacks)
        {
        }
    }
}
