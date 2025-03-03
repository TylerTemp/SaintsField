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
    public class ShaderParamAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string TargetName;
        public readonly int Index;
        public readonly ShaderPropertyType? PropertyType;

        public ShaderParamAttribute(string name, ShaderPropertyType propertyType, int index=0)
        {
            TargetName = RuntimeUtil.ParseCallback(name).content;
            Index = index;
            PropertyType = propertyType;
        }

        public ShaderParamAttribute(string name, int index)
        {
            TargetName = RuntimeUtil.ParseCallback(name).content;
            Index = index;
        }

        public ShaderParamAttribute(string name)
        {
            TargetName = RuntimeUtil.ParseCallback(name).content;
            Index = 0;
        }

        public ShaderParamAttribute(int index)
        {
            TargetName = null;
            Index = index;
        }

        public ShaderParamAttribute(ShaderPropertyType propertyType)
        {
            TargetName = null;
            Index = 0;
            PropertyType = propertyType;
        }

        public ShaderParamAttribute()
        {
            TargetName = null;
            Index = 0;
        }
    }
}
#endif
