using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class SaintsHashSetAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;

        public SaintsHashSetAttribute(bool searchable = true, int numberOfItemsPerPage = 0)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Searchable = searchable;
        }
    }
}
