using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FindComponentAttribute: GetByXPathAttribute
    {
        public FindComponentAttribute(EXP config, string path, params string[] paths): base(config, paths.Prepend(path).ToArray())
        {
        }

        public FindComponentAttribute(string path, params string[] paths): this(EXP.NoPicker | EXP.NoAutoResignToNull, path, paths)
        {
        }
    }
}
