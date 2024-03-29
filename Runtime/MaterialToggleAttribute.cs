using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class MaterialToggleAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string CompName;
        public readonly int Index;

        public MaterialToggleAttribute(string rendererName=null, int index=0)
        {
            CompName = rendererName;
            Index = index;
        }
    }
}
