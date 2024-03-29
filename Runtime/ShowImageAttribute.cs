using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class ShowImageAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly string ImageCallback;
        public readonly bool Above;
        public readonly int MaxWidth;
        public readonly int MaxHeight;
        public readonly EAlign Align;

        // ReSharper disable once MemberCanBeProtected.Global
        public ShowImageAttribute(string image = null, int maxWidth = -1, int maxHeight = -1, EAlign align = EAlign.Start, bool above = false,
            string groupBy = "")
        {
            GroupBy = groupBy;

            ImageCallback = image;
            Above = above;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            Align = align;
        }
    }
}
