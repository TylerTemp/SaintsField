using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutAttribute: Attribute, IPlayaAttribute, ISaintsLayout
    {
        public string LayoutBy { get; }
        public ELayout Layout { get; }
        public bool KeepGrouping { get; }

        public float MarginTop { get; }
        public float MarginBottom { get; }
        public float PaddingLeft { get; }
        public float PaddingRight { get; }

        public LayoutAttribute(string layoutBy, ELayout layout = 0, bool keepGrouping = false,
            float marginTop = -1f, float marginBottom = -1f, float paddingLeft = 0, float paddingRight = 0)
        {
            LayoutBy = layoutBy.Trim('/');
            Layout = layout;
            // if (Layout.HasFlagFast(ELayout.LabelField))
            // {
            //     Layout |= ELayout.Horizontal;
            // }
            KeepGrouping = keepGrouping;

            MarginTop = marginTop;
            MarginBottom = marginBottom;
            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
        }
    }
}
