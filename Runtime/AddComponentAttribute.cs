using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class AddComponentAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly Type CompType;

        public AddComponentAttribute(Type compType = null, string groupBy = "")
        {
            CompType = compType;
            GroupBy = groupBy;
        }
    }
}
