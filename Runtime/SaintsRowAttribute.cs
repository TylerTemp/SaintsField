using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsRowAttribute: PropertyAttribute, ISaintsAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly bool Inline;
        // ReSharper enable InconsistentNaming

        public SaintsRowAttribute() : this(false)
        {
        }

        public SaintsRowAttribute(bool inline)
        {
            Inline = inline;
        }


        [Obsolete]
        public SaintsRowAttribute(bool inline, bool tryFixUIToolkit): this(inline)
        {
        }

        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
    }
}
