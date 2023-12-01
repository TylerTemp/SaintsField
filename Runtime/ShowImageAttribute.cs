using UnityEngine;

namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class ShowImageAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly string ImageCallback;
        public readonly bool Above;
        public readonly int MaxWidth;
        public readonly int MaxHeight;

        public ShowImageAttribute(string image, int maxWidth = -1, int maxHeight = -1, bool above = false,
            string groupBy = "")
        {
            GroupBy = groupBy;

            ImageCallback = image;
            Above = above;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
        }
    }
}
