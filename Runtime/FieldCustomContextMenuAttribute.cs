using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
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
        public readonly bool MenuNameIsCallback;

        public FieldCustomContextMenuAttribute(string funcName, string menuName = null)
        {
            FuncName = RuntimeUtil.ParseCallback(funcName).content;
            (MenuName, MenuNameIsCallback) = RuntimeUtil.ParseCallback(menuName);
        }
    }
}
