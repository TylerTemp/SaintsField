using System.Diagnostics;
using UnityEngine;

namespace SaintsField.DebugTool
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class WhichFrameworkAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";
    }
}
