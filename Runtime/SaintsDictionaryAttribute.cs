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

        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public string KeyLabel;
        public string ValueLabel;

        public int NumberOfItemsPerPage;
        public bool Searchable;
        public bool ObjectSearch;
        public string ExtraSearch;

        public ResponsiveLength KeyWidth;
        public ResponsiveLength ValueWidth;
        // ReSharper enable FieldCanBeMadeReadOnly.Global

        public SaintsDictionaryAttribute(
            string keyLabel = "Keys",
            string valueLabel = "Values",
            bool searchable = true,
            int numberOfItemsPerPage = 0,
            bool objectSearch = true,
            string keyWidth = null,
            string valueWidth = null,
            string extraSearch = null)
        {
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Searchable = searchable;
            ObjectSearch = objectSearch;
            ExtraSearch = RuntimeUtil.ParseCallback(extraSearch).content;

            KeyWidth = RuntimeUtil.ParseResponsiveLength(keyWidth);
            ValueWidth = RuntimeUtil.ParseResponsiveLength(valueWidth);
        }
    }
}
