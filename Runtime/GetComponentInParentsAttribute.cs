using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    public class GetComponentInParentsAttribute: PropertyAttribute, ISaintsAttribute
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
