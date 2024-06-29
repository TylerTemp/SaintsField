using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class AssetPreviewAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly bool Above;
        public readonly int Width;
        public readonly int Height;
        public readonly EAlign Align;

        public AssetPreviewAttribute(int width=-1, int height=-1, EAlign align=EAlign.End, bool above=false, string groupBy="")
        {
            GroupBy = groupBy;

            Above = above;
            Width = width;
            Height = height;
            Align = align;
        }
    }
}
