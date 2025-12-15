using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Utils
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsSerializedActualDrawerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";
    }
}
