using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Parameter)]
    public class OptionsTreeDropdownAttribute: OptionsDropdownAttribute
    {
        public OptionsTreeDropdownAttribute(params object[] options) : base(options)
        {
        }
    }
}
