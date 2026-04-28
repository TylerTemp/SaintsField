using System.Diagnostics;

namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class AboveButtonAttribute : DecButtonAttribute
    {
        public AboveButtonAttribute(string funcName, string buttonLabel=null, bool isCallback = false, bool hideReturnValue=false, string groupBy = "") : base(funcName, buttonLabel, isCallback, hideReturnValue, groupBy)
        {
        }
    }
}
