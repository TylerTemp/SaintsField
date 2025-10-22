using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class DateTimeAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";
    }
}
