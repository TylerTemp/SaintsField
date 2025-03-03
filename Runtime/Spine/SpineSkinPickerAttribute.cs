using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Spine
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SpineSkinPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string SkeletonTarget;

        public SpineSkinPickerAttribute(string skeletonTarget = null)
        {
            SkeletonTarget = RuntimeUtil.ParseCallback(skeletonTarget).content;
        }
    }
}
