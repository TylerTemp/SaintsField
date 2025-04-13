using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    // [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute: Attribute, ISaintsAttribute, IPlayaAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string Title;

        public TableColumnAttribute(string title)
        {
            Title = title;
        }
    }
}
