using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute: PropertyAttribute, ISaintsAttribute, IImGuiVisibilityAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";

        public readonly string[] orCallbacks;

        // ReSharper disable once MemberCanBeProtected.Global
        public ShowIfAttribute(params string[] orCallbacks)
        {
            this.orCallbacks = orCallbacks;
        }
    }
}
