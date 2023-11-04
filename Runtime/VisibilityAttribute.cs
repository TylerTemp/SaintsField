using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class VisibilityAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";

        public readonly bool IsForHide;
        public readonly string[] andCallbacks;

        // ReSharper disable once MemberCanBeProtected.Global
        public VisibilityAttribute(bool isForHide, params string[] andCallbacks)
        {
            IsForHide = isForHide;
            this.andCallbacks = andCallbacks;
        }
    }
}
