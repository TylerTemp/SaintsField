using System;
using UnityEngine;

namespace SaintsField
{
    public class GetComponentInSceneAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly Type CompType;
        public readonly bool IncludeInactive;

        public GetComponentInSceneAttribute(bool includeInactive = false, Type compType = null, string groupBy = "")
        {
            CompType = compType;
            IncludeInactive = includeInactive;
            GroupBy = groupBy;
        }
    }
}
