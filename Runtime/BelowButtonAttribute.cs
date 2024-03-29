using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class BelowButtonAttribute : DecButtonAttribute
    {
        public BelowButtonAttribute(string funcName, string buttonLabel=null, bool isCallback = false, string groupBy = "") : base(funcName, buttonLabel, isCallback, groupBy)
        {
        }
    }
}
