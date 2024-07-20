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

        public readonly bool IncludeInactive;
        public readonly Type CompType;
        public readonly bool ExcludeSelf;

        public virtual int Limit => 0;

        public GetComponentInParentsAttribute(bool includeInactive = false, Type compType = null, bool excludeSelf = false, string groupBy = "")
        {
            IncludeInactive = includeInactive;
            CompType = compType;
            ExcludeSelf = excludeSelf;
            GroupBy = groupBy;
        }
    }
}
