using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ResizableTextAreaAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;

        public string GroupBy => "__LABEL_FIELD__";

        // public string GroupBy { get; }

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool Inline;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int MinRow;

        public ResizableTextAreaAttribute(bool inline=false, int minRow=0)
        {
            Inline = inline;
            MinRow = minRow;
        }
    }
}
