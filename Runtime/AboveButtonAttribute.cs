namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AboveButtonAttribute : DecButtonAttribute
    {
        public AboveButtonAttribute(string funcName, string buttonLabel=null, bool isCallback = false, string groupBy = "") : base(funcName, buttonLabel, isCallback, groupBy)
        {
        }
    }
}
