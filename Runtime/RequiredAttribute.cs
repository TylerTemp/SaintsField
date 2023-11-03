using UnityEngine;

namespace SaintsField
{
    public class RequiredAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string ErrorMessage;

        public RequiredAttribute(string errorMessage = null)
        {
            ErrorMessage = errorMessage;
        }
    }
}
