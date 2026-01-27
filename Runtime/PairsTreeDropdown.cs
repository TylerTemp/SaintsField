using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Parameter)]
    public class PairsTreeDropdown: PairsDropdown
    {
        public PairsTreeDropdown(params object[] tuples) : base(tuples)
        {
        }
    }
}
