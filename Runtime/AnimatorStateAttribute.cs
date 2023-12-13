using UnityEngine;

namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AnimatorStateAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string AnimFieldName;

        public AnimatorStateAttribute(string animator=null)
        {
            AnimFieldName = animator;
        }
    }
}
