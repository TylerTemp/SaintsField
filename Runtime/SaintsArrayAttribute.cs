using UnityEngine;

namespace SaintsField
{
    public class SaintsArrayAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        // ReSharper disable once InconsistentNaming
        public readonly string PropertyName;

        public SaintsArrayAttribute(string propertyName=null, string groupBy="")
        {
            PropertyName = propertyName;
            GroupBy = groupBy;
        }
    }
}
