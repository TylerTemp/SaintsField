using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Spine
{
    [Conditional("UNITY_EDITOR")]
    public class SpineAnimationPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string SkeletonTarget;

        public SpineAnimationPickerAttribute(string skeletonTarget = null)
        {
            SkeletonTarget = RuntimeUtil.ParseCallback(skeletonTarget).content;
        }
    }
}
