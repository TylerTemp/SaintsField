using UnityEngine;

namespace SaintsField.AiNavigation
{
#if SAINTSFIELD_AI_NAVIGATION
    public class NavMeshAreaAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        public NavMeshAreaAttribute(string groupBy = "")
        {
            GroupBy = groupBy;
        }
    }
#endif
}
