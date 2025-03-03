#if UNITY_2021_2_OR_NEWER
using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine.Rendering;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShaderKeywordAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string TargetName;
        public readonly int Index;

        public ShaderKeywordAttribute(string name, int index)
        {
            TargetName = RuntimeUtil.ParseCallback(name).content;
            Index = index;
        }

        public ShaderKeywordAttribute(string name)
        {
            TargetName = RuntimeUtil.ParseCallback(name).content;
            Index = 0;
        }

        public ShaderKeywordAttribute(int index)
        {
            TargetName = null;
            Index = index;
        }

        public ShaderKeywordAttribute()
        {
            TargetName = null;
            Index = 0;
        }
    }
}
#endif
