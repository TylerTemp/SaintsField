using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class SearchableAttribute: Attribute, IPlayaAttribute, IPlayaClassAttribute
    {

    }
}
