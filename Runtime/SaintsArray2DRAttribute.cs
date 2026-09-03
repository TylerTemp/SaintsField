using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    // ReSharper disable once InconsistentNaming
    public class SaintsArray2DRAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool Transpose;

        public SaintsArray2DRAttribute(bool transpose = false)
        {
            Transpose = transpose;
        }
    }
}
