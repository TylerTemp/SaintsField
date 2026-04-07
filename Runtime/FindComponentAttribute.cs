using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FindComponentAttribute: GetByXPathAttribute
    {
        public readonly string[] Paths;

        // ReSharper disable once MemberCanBePrivate.Global
        public FindComponentAttribute(EXP config, string path, params string[] paths): base(config, paths.Prepend(path).ToArray())
        {
            Paths = new[]{path}.Concat(paths).ToArray();
        }

        public FindComponentAttribute(string path, params string[] paths): this(
#if UNITY_EDITOR
            SaintsFieldConfigUtil.FindComponentExp(EXP.NoPicker | EXP.NoAutoResignToNull)
#else
            EXP.NoPicker | EXP.NoAutoResignToNull
#endif
            , path, paths)
        {
            Paths = new[]{path}.Concat(paths).ToArray();
        }
    }
}
