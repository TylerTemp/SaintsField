namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class BelowButtonAttribute : DecButtonAttribute
    {
        public BelowButtonAttribute(string funcName, string buttonLabel, bool buttonLabelIsCallback = false, string groupBy = "") : base(funcName, buttonLabel, buttonLabelIsCallback, groupBy)
        {
        }
    }
}
