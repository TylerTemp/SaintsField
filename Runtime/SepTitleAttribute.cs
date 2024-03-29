using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class SepTitleAttribute : PropertyAttribute
    {
        public readonly float height;
        public readonly EColor color;
        public readonly string title;
        public readonly float gap;

        public SepTitleAttribute(string title, EColor color, float gap = 2f, float height = 2f)
        {
            this.title = title;
            this.color = color;
            this.height = height;
            this.gap = gap;
        }

        public SepTitleAttribute(EColor color, float gap = 2f, float height = 2f)
        {
            title = null;
            this.color = color;
            this.height = height;
            this.gap = gap;
        }
    }
}
