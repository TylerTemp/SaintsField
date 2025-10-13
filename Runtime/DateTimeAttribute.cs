using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    public class DateTimeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";
    }
}
