using System;
using UnityEngine;

namespace SaintsField
{
    public class GetComponentInChildrenAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly Type CompType;
        public readonly bool IncludeInactive;

        public GetComponentInChildrenAttribute(bool includeInactive = false, Type compType = null, string groupBy = "")
        {
            CompType = compType;
            IncludeInactive = includeInactive;
            GroupBy = groupBy;
        }
    }
}
