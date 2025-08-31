using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Addressable
{
    [Conditional("UNITY_EDITOR")]
    public class AddressableLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
    }
}
