using UnityEngine;

namespace SaintsField
{
    public class SaintsRowAttribute: PropertyAttribute
    {
        public readonly bool Inline;
        public readonly bool TryFixUIToolkit;

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
