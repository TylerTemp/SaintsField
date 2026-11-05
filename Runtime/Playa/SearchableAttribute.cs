using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    [Obsolete("Use SaintsField config to toggle this")]
    // ReSharper disable once RedundantExtendsListEntry
    public class SearchableAttribute: Attribute, IPlayaSaintsEditorOnlyAttribute, IPlayaClassAttribute
    {
        public bool EndDecorator => false;
    }
}
