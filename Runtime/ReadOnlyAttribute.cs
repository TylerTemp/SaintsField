using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly bool ReadOnlyDirectValue;
        public readonly string[] ReadOnlyBys;

        public ReadOnlyAttribute(bool directValue=true, string groupBy=null)
        {
            ReadOnlyDirectValue = directValue;
            ReadOnlyBys = null;

            GroupBy = groupBy;
        }

        public ReadOnlyAttribute(params string[] by)
        {
            // Debug.Assert(!string.IsNullOrEmpty(by));
            ReadOnlyDirectValue = default;
            ReadOnlyBys = by;

            GroupBy = "";
        }
    }
}
