using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Addressable
{
    [Conditional("UNITY_EDITOR")]
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
