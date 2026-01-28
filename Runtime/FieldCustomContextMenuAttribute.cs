using System.Diagnostics;
using SaintsField.Interfaces;
// using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    // [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Struct | System.AttributeTargets.Class, AllowMultiple = true)]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class FieldCustomContextMenuAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string FuncName;
        public readonly string MenuName;

        public FieldCustomContextMenuAttribute(string funcName, string menuName = null)
        {
            FuncName = funcName;
            MenuName = menuName;
        }
    }
}
