using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderGhostButtonAttribute: HeaderButtonAttribute
    {
        public override bool IsGhost => true;

        public HeaderGhostButtonAttribute(string title = null, string tooltip = null) : base(title, tooltip)
        {
        }
    }
}
