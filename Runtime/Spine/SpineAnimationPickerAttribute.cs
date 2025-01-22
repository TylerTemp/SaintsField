using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Spine
{
    [Conditional("UNITY_EDITOR")]
    public class SpineAnimationPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string SkeletonTarget;

        public SpineAnimationPickerAttribute(string skeletonTarget = null)
        {
            SkeletonTarget = RuntimeUtil.ParseCallback(skeletonTarget).content;
        }
    }
}
