using System;
using System.Diagnostics;
using UnityEngine;

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

        [Obsolete]
        public SaintsRowAttribute(bool inline, bool tryFixUIToolkit): this(inline)
        {
        }
    }
}
