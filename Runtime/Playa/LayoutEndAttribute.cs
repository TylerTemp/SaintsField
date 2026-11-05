using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    public class LayoutEndAttribute: Attribute, IPlayaSaintsEditorOnlyAttribute, ISaintsLayout
    {
        public string LayoutBy { get; }
        public ELayout Layout => 0;

        public bool KeepGrouping => false;

        public float MarginTop { get; }
        public float MarginBottom { get; }
        public float PaddingLeft { get; }
        public float PaddingRight { get; }

        public LayoutEndAttribute(string layoutBy = null, float marginTop = -1f, float marginBottom = -1f,
            float paddingLeft = 0, float paddingRight = 0)
        {
            LayoutBy = layoutBy?.Trim('/');
            MarginTop = marginTop;
            MarginBottom = marginBottom;
            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
        }
    }
}
