using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly bool ReadOnlyDirectValue;
        public readonly string ReadOnlyBy;

        public ReadOnlyAttribute(bool directValue, string groupBy=null)
        {
            ReadOnlyDirectValue = directValue;
            ReadOnlyBy = null;

            GroupBy = groupBy;
        }

        public ReadOnlyAttribute(string by, string groupBy="")
        {
            Debug.Assert(!string.IsNullOrEmpty(by));
            ReadOnlyDirectValue = default;
            ReadOnlyBy = by;

            GroupBy = groupBy;
        }
    }
}
