using System;
using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;

        public readonly bool ReadOnlyDirectValue;
        public readonly string ReadOnlyBy;

        public ReadOnlyAttribute(bool directValue)
        {
            ReadOnlyDirectValue = directValue;
            ReadOnlyBy = null;
        }

        public ReadOnlyAttribute(string by)
        {
            Debug.Assert(!string.IsNullOrEmpty(by));
            ReadOnlyDirectValue = default;
            ReadOnlyBy = by;
        }
    }
}
