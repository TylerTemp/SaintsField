using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FindComponentAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string[] Paths;

        public FindComponentAttribute(string path, params string[] paths)
        {
            Paths = new[]{path}.Concat(paths).ToArray();
        }
    }
}
