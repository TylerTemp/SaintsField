using System;
using System.Diagnostics;
#if SAINTSFIELD_IMPLICIT_USE
using JetBrains.Annotations;
#endif

namespace SaintsField.ComponentHeader
{
#if SAINTSFIELD_IMPLICIT_USE
    [MeansImplicitUse]  // https://github.com/TylerTemp/SaintsField/pull/171#issuecomment-3680042013
#endif
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderLeftButtonAttribute: HeaderButtonAttribute
    {
        public override bool IsLeft => true;

        public HeaderLeftButtonAttribute(string label = null, string toolTip = null): base(label, toolTip)
        {
        }
    }
}
