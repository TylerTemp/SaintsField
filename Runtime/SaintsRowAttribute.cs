using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsRowAttribute: PropertyAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly bool Inline;
        public readonly bool TryFixUIToolkit;
        // ReSharper enable InconsistentNaming

        public SaintsRowAttribute(bool inline=false, bool tryFixUIToolkit=
#if SAINTSFIELD_SAINTS_EDITOR_UI_TOOLKIT_LABEL_FIX_DISABLE
                false
#else
                true
#endif
            )
        {
            Inline = inline;
            TryFixUIToolkit = tryFixUIToolkit;
        }
    }
}
