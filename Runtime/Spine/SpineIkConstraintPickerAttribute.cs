using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Spine
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SpineIkConstraintPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string SkeletonTarget;

        public SpineIkConstraintPickerAttribute(string skeletonTarget = null)
        {
            SkeletonTarget = RuntimeUtil.ParseCallback(skeletonTarget).content;
        }
    }
}
