#if SAINTSFIELD_ADDRESSABLE
using UnityEngine;

namespace SaintsField.Addressable
{
    public class AddressableLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
    }
}
#endif
