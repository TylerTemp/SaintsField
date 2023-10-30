namespace ExtInspector
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AboveButtonAttribute : DecButtonAttribute
    {
        public AboveButtonAttribute(string funcName, string buttonLabel, bool buttonLabelIsCallback = false, string groupBy = "") : base(funcName, buttonLabel, buttonLabelIsCallback, groupBy)
        {
        }
    }
}
