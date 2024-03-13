using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute: PropertyAttribute, ISaintsAttribute, IImGuiVisibilityAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";

        public readonly string[] andCallbacks;

        // ReSharper disable once MemberCanBeProtected.Global
        public ShowIfAttribute(params string[] andCallbacks)
        {
            this.andCallbacks = andCallbacks;
        }
    }
}
