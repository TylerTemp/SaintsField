using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class AdaptAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly EUnit EUnit;

        public AdaptAttribute(EUnit eUnit)
        {
            Debug.Assert(eUnit != EUnit.Custom, "Custom is not allowed");
            EUnit = eUnit;
        }
    }
}
