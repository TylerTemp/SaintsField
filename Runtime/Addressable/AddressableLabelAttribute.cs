using UnityEngine;

namespace SaintsField.Addressable
{
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLED
    public class AddressableLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
    }
#endif
}
