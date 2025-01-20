using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShaderParamAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string TargetName;
        public readonly int Index;

        public ShaderParamAttribute(string name, int index=0)
        {
            TargetName = RuntimeUtil.ParseCallback(name).content;
            Index = index;
        }

        public ShaderParamAttribute(int index)
        {
            TargetName = null;
            Index = index;
        }

        public ShaderParamAttribute()
        {
            TargetName = null;
            Index = 0;
        }
    }
}
