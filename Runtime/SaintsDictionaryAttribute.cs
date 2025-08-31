using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class SaintsDictionaryAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        public readonly string KeyLabel;
        public readonly string ValueLabel;

        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;

        public SaintsDictionaryAttribute(string keyLabel = "Keys", string valueLabel = "Values", bool searchable = true, int numberOfItemsPerPage = 0)
        {
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Searchable = searchable;
        }
    }
}
