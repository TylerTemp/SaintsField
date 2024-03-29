using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class RequiredAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        // ReSharper disable once InconsistentNaming
        public readonly string ErrorMessage;

        public RequiredAttribute(string errorMessage = null)
        {
            ErrorMessage = errorMessage;
        }
    }
}
