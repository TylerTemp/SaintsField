using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsRowAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly bool Inline;

        public SaintsRowAttribute() : this(false)
        {
        }

        public SaintsRowAttribute(bool inline)
        {
            Inline = inline;
        }
    }
}
