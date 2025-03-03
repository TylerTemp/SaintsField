using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Spine
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SpineSlotPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly bool ContainsBoundingBoxes;

        public readonly string SkeletonTarget;

        public SpineSlotPickerAttribute(bool containsBoundingBoxes = false, string skeletonTarget = null)
        {
            ContainsBoundingBoxes = containsBoundingBoxes;
            SkeletonTarget = RuntimeUtil.ParseCallback(skeletonTarget).content;
        }

        public SpineSlotPickerAttribute(string skeletonTarget): this(false, skeletonTarget)
        {
        }
    }
}
