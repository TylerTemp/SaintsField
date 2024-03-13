using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public bool readOnlyDirectValue;
        // ReSharper disable once InconsistentNaming
        public readonly string[] ReadOnlyBys;

        public ReadOnlyAttribute(bool directValue=true, string groupBy="")
        {
            readOnlyDirectValue = directValue;
            ReadOnlyBys = null;

            GroupBy = groupBy;
        }

        public ReadOnlyAttribute(params string[] by)
        {
            if (by.Length == 0)
            {
                readOnlyDirectValue = true;
                ReadOnlyBys = null;
            }
            else
            {
                // Debug.Assert(!string.IsNullOrEmpty(by));
                readOnlyDirectValue = default;
                ReadOnlyBys = by;
            }

            GroupBy = "";
        }
    }
}
