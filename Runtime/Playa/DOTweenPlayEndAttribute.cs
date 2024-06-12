using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayEndAttribute: LayoutEndAttribute
    {
        public DOTweenPlayEndAttribute(string groupBy) : base(string.IsNullOrEmpty(groupBy)? DOTweenPlayAttribute.DOTweenPlayGroupBy: $"{groupBy}/{DOTweenPlayAttribute.DOTweenPlayGroupBy}")
        {
        }
    }
}
