using UnityEngine;

namespace ExtInspector
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AnimatorStateAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        public readonly string AnimFieldName;

        public AnimatorStateAttribute(string animator, string groupBy="")
        {
            AnimFieldName = animator;
            GroupBy = groupBy;
        }
    }
}
