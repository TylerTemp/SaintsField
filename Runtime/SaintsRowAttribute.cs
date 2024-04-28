using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsRowAttribute: PropertyAttribute
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
    }
}
