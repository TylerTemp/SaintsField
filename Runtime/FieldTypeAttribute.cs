using System;
using UnityEngine;

namespace SaintsField
{
    public class FieldTypeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly Type CompType;

        public FieldTypeAttribute(Type compType)
        {
            CompType = compType;
        }
    }
}
