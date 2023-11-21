using UnityEngine;

namespace SaintsField
{
    public class InputAxisAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;

        public string GroupBy => "__LABEL__FIELD__";
    }
}
