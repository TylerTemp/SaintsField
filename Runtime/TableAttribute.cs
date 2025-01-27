using System;
using System.Diagnostics;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TableAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
    }
}
