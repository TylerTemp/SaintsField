using System;
using UnityEngine;

namespace SaintsField
{
    public class GetPrefabWithComponentAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly Type CompType;

        public GetPrefabWithComponentAttribute(Type compType = null, string groupBy = "")
        {
            CompType = compType;
            GroupBy = groupBy;
        }
    }
}
