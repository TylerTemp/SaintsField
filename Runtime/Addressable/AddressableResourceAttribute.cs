using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Addressable
{
    public class AddressableResourceAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        // public AddressableResourceAttribute()
        // {
        //
        // }
    }
}
