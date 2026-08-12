using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SaintsDictionaryAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        public readonly string KeyLabel;
        public readonly string ValueLabel;

        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool ObjectSearch;

        public readonly ResponsiveLength KeyWidth;
        public readonly ResponsiveLength ValueWidth;

        public SaintsDictionaryAttribute(
            string keyLabel = "Keys",
            string valueLabel = "Values",
            bool searchable = true,
            int numberOfItemsPerPage = 0,
            bool objectSearch = true,
            string keyWidth = null,
            string valueWidth = null)
        {
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Searchable = searchable;
            ObjectSearch = objectSearch;

            KeyWidth = RuntimeUtil.ParseResponsiveLength(keyWidth);
            ValueWidth = RuntimeUtil.ParseResponsiveLength(valueWidth);
        }
    }
}
