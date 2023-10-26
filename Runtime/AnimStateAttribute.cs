using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspector
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AnimStateAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        public readonly string AnimFieldName;

        public AnimStateAttribute(string animator, string groupBy="")
        {
            AnimFieldName = animator;
            GroupBy = groupBy;
        }
    }
}
