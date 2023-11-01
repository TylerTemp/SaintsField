namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class PostFieldButtonAttribute : DecButtonAttribute
    {
        public PostFieldButtonAttribute(string funcName, string buttonLabel, bool buttonLabelIsCallback = false, string groupBy = "") : base(funcName, buttonLabel, buttonLabelIsCallback, groupBy)
        {
        }
    }
}
