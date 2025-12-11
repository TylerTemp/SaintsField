using System.Diagnostics;
using System;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentInSceneAttribute: FindObjectsByTypeAttribute
    {
        public GetComponentInSceneAttribute(bool includeInactive = false, Type compType = null, string groupBy = "")
            : base(compType, includeInactive, groupBy)
        {
        }
    }
}
