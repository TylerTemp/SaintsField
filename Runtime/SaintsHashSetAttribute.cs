using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
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
        public readonly bool ObjectSearch;
        public readonly string ExtraSearch;

        public SaintsHashSetAttribute(bool searchable = true, int numberOfItemsPerPage = 0,
            bool objectSearch = true, string extraSearch = null)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Searchable = searchable;
            ObjectSearch = objectSearch;
            ExtraSearch = RuntimeUtil.ParseCallback(extraSearch).content;

            if (ExtraSearch != null)
            {
                Searchable = true;
            }
        }
    }
}
