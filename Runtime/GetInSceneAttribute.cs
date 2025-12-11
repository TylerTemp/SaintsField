using System.Diagnostics;
using System;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetInSceneAttribute: FindObjectsByTypeAttribute
    {
        public GetInSceneAttribute(bool includeInactive = true, Type compType = null, string groupBy = "")
            : base(EXP.None, compType, includeInactive, groupBy)
        {
        }

        public GetInSceneAttribute(EXP exp, bool includeInactive = true, Type compType = null, string groupBy = "")
            : base(exp, compType, includeInactive, groupBy)
        {
        }
    }
}
