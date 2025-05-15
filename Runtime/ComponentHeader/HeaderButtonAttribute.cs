using System;
using System.Diagnostics;
using JetBrains.Annotations;
using SaintsField.Utils;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderButtonAttribute: AbsComponentHeaderAttribute
    {
        public readonly string Title;
        public readonly bool IsCallback;
        public readonly string Tooltip;

        public virtual bool IsGhost => false;

        public override string GroupBy => null;
        public override bool IsLeft => false;

        public HeaderButtonAttribute(string title = null, string tooltip = null)
        {
            (Title, IsCallback) = RuntimeUtil.ParseCallback(title);
            Tooltip = tooltip;
        }
    }
}
