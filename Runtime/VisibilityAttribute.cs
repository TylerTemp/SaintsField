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
        public readonly string[] OrCallbacks;

        public VisibilityAttribute(bool isForHide, params string[] orCallbacks)
        {
            IsForHide = isForHide;
            OrCallbacks = orCallbacks;
        }
    }
}
