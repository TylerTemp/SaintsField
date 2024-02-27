using UnityEngine;

namespace SaintsField.AiNavigation
{
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
    public class NavMeshAreaAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        // ReSharper disable once InconsistentNaming
        public readonly bool IsMask;

        public NavMeshAreaAttribute(bool isMask=true, string groupBy = "")
        {
            GroupBy = groupBy;
            IsMask = isMask;
        }
    }
#endif
}
