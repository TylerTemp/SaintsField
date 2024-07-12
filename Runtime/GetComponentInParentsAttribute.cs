using System;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentInParentsAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute, IPlayaArraySizeAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        // ReSharper disable once InconsistentNaming
        public readonly Type CompType;

        public virtual int Limit => 0;

        public GetComponentInParentsAttribute(Type compType = null, string groupBy = "")
        {
            CompType = compType;
            GroupBy = groupBy;
        }
    }
}
